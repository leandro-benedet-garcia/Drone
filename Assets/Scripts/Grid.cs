using System.Net;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;

using MyBox;
using System;
using UnityEngine.Tilemaps;
using TMPro;

namespace DroneGame
{
  public sealed class ParsedData : Dictionary<string, Dictionary<string, float>>
  {

  }

  /// <summary>The 2D grid that will be used for the path finding</summary>
  public sealed class Grid : MonoBehaviour
  {
    public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";
    readonly Dictionary<string, Dictionary<string, string>> _cachedPaths = new();
    public ParsedData parsed;

    [Header("Prefabs")]
    [SerializeField] Tile _tilePrefab;
    [SerializeField] Connector _connectorPrefab;
    [SerializeField] Drone _drone;

    [Header("UI Inputs")]
    [SerializeField] TMP_InputField _startCoordinateInput;
    [SerializeField] TMP_InputField _pickupCoordinateInput;
    [SerializeField] TMP_InputField _dropOffCoordinateInput;


    readonly HashSet<Tile> _changedColorsTiles = new();

    /// <summary>Dict used to convert from letters to numbers</summary>
    private readonly Dictionary<char, int> _letterToInt = new(){
            {'A', 0},
            {'B', 1},
            {'C', 2},
            {'D', 3},
            {'E', 4},
            {'F', 5},
            {'G', 6},
            {'H', 7}
        };
    Dictionary<string, TileData> _allTilesData;
    Dictionary<string, Tile> _allTiles;

    public TileData this[string coordinate]
    {
      get => _allTilesData[coordinate];
    }

    public Vector3 GetTileWorldCoordinate(string coordinate) => _allTilesData[coordinate].globalCoordinates;

    /// <summary>This is a default unity function, it is called when the object is instantiated into the scene</summary>
    [ButtonMethod]
    void Awake()
    {
      parsed = DownloadAndParseGridData(url);
      GenerateGrid(parsed);
    }

    public void MoveDrone()
    {
      if (_drone.alreadyMoving) return;

      var start = _startCoordinateInput.text;
      var pickup = _pickupCoordinateInput.text;
      var dropOff = _dropOffCoordinateInput.text;

      ResetColors();
      SetColor(start, Color.green);
      SetColor(pickup, Color.yellow);
      SetColor(dropOff, Color.blue);

      StartCoroutine(_drone.FollowPath(GetShortestPathTiles(new string[] { start, pickup, dropOff })));
    }

    private void ResetColors()
    {
      foreach(var currTile in _changedColorsTiles) currTile.ResetColor();
      _changedColorsTiles.Clear();
    }

    private void SetColor(string coords, Color color)
    {
      var tileToChange = _allTiles[coords];
      tileToChange.SetColor(color);
      _changedColorsTiles.Add(tileToChange);
    }

    ParsedData DownloadAndParseGridData(string url)
    {
      using var wc = new WebClient();
      var json = wc.DownloadString(url);
      return JsonConvert.DeserializeObject<ParsedData>(json);
    }

    void GenerateGrid(ParsedData parsed)
    {
      _allTilesData = new();
      _allTiles = new();

      foreach (var currParsedTile in parsed)
      {
        var coordinate = currParsedTile.Key;
        var neighbors = currParsedTile.Value;

        var currTile = GetOrCreateTile(coordinate);

        var currInstance = Instantiate(_tilePrefab, transform);
        currInstance.Initialize(currTile);

        _allTiles[coordinate] = currInstance;

        foreach (var currNeighbor in neighbors)
        {
          var neighborDistance = currNeighbor.Value;
          var neighborCoordinate = currNeighbor.Key;
          var converted = GetOrCreateTile(neighborCoordinate);

          currTile.neighbors[neighborCoordinate] = neighborDistance;

          // Since I want to have a visualization of the grid paths,
          // I am adding a connector between the tiles and their neighbors
          var createdConnection = Instantiate(_connectorPrefab, transform);
          createdConnection.Initialize(currTile, converted, neighborDistance);
        }
      }
    }

    /// <summary>Return an already created tile or create a new one if a tile with the coordinates does not already exist</summary>
    /// <param name="key">The coordinate in the format A2</param>
    /// <returns>A brand new or already existing Tile</returns>
    private TileData GetOrCreateTile(string key)
    {
      if (_allTilesData.ContainsKey(key))
      {
        return _allTilesData[key];
      }

      var createdTile = new TileData
      {
        globalCoordinates = FromLetterNumberToVector(key),
        letterCoordinate = key,
        neighbors = new()
      };
      _allTilesData[key] = createdTile;
      return createdTile;
    }

    /// <summary>Transform letter and number coordinate system to a Vector to be used by Unity later</summary>
    private Vector3 FromLetterNumberToVector(string letterAndNumber)
    {
      var firstNumber = _letterToInt[letterAndNumber[0]];
      var secondNumber = int.Parse(letterAndNumber[1].ToString());

      return new(firstNumber, 0, secondNumber);
    }

    public List<TileData> GetShortestPathTiles(string startPosition, string endPosition)
    {
      var returnList = new List<TileData>();
      foreach (var currCoordinate in GetShortestPath(startPosition, endPosition)) returnList.Add(_allTilesData[currCoordinate]);
      return returnList;
    }

    public List<TileData> GetShortestPathTiles(string[] positions)
    {
      var returnList = new List<TileData>();
      foreach (var currCoordinate in GetShortestPath(positions)) returnList.Add(_allTilesData[currCoordinate]);
      return returnList;
    }

    // TODO: Convert this into an async operation
    public List<string> GetShortestPath(string[] positions)
    {
      var positionsLen = positions.Length;
      if (positionsLen < 2) throw new("positions need to have at least 2 items");

      var returnList = new List<string>();
      for (int pathIndex = 1; pathIndex < positionsLen; pathIndex++)
      {
        var previousCoordinate = positions[pathIndex - 1];
        var nextCoordinate = positions[pathIndex];

        var currPath = GetShortestPath(previousCoordinate, nextCoordinate);

        if (pathIndex < positionsLen - 1) currPath.RemoveAt(currPath.Count - 1);

        returnList = returnList.Concat(currPath).ToList();
      }
      return returnList;
    }


    /// <summary>Find the shortest paths between start and end using Dijkstras algorithm
    /// All paths to start node are automatically cached so if it is asked again, there's no need to recalculate it</summary>
    /// <seealso cref="FindAllPathsFrom"/>
    public List<string> GetShortestPath(string startPosition, string endPosition)
    {
      // If the API were dynamic, we would recreate the grid in here by simply called Awake after deleting the grid
      // But since it is static, we don't have to.
      Dictionary<string, string> currCachedPath;
      var path = new List<string>();

      // No need to do anything other than return start position if the start and end are the same
      if (startPosition == endPosition)
      {
        path.Add(startPosition);
        return path;
      }
      if (_cachedPaths.ContainsKey(startPosition)) currCachedPath = _cachedPaths[startPosition];
      else
      {
        currCachedPath = FindAllPathsFrom(startPosition);
        _cachedPaths[startPosition] = currCachedPath;
      }

      var node = endPosition;
      while (node != startPosition)
      {
        path.Add(node);
        node = currCachedPath[node];
      }
      path.Add(startPosition);
      path.Reverse();
      return path;
    }

    /// <summary>Find all paths to the start position using Dijkstras algorithm
    /// This is necessary because the API only have seconds instead of distance, which we can use as weight
    /// if we had distance information, we could use A* which is more effective</summary>
    Dictionary<string, string> FindAllPathsFrom(string startPosition)
    {
      var unvisitedNodes = _allTilesData.Keys.ToList();
      var shortestPath = new Dictionary<string, float>();
      var previousNodes = new Dictionary<string, string>();

      foreach (var currNode in unvisitedNodes)
      {
        shortestPath[currNode] = float.MaxValue;
      }

      shortestPath[startPosition] = 0;

      while (unvisitedNodes.Count > 0)
      {
        string minNode = null;
        foreach (var currNode in unvisitedNodes)
        {
          if (minNode == null) minNode = currNode;
          else if (shortestPath[currNode] < shortestPath[minNode]) minNode = currNode;
        }

        var neighbors = GetNeighbors(minNode);
        foreach (var currNeighbor in neighbors)
        {
          var neighborCoordinate = currNeighbor.Key;
          var totalCost = shortestPath[minNode] + currNeighbor.Value;
          if (totalCost < shortestPath[neighborCoordinate])
          {
            shortestPath[neighborCoordinate] = totalCost;
            previousNodes[neighborCoordinate] = minNode;
          }
        }
        unvisitedNodes.Remove(minNode);
      }
      return previousNodes;
    }

    Dictionary<string, float> GetNeighbors(string coordinates) => _allTilesData[coordinates].neighbors;
  }
}

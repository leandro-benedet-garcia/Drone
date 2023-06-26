using System.Net;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;

using Newtonsoft.Json;

using MyBox;

namespace DroneGame
{
  /// <summary>Mostly just a short way to type the dictionary inside a dictionary
  /// for the parsed data from the API</summary>
  public sealed class ParsedData : Dictionary<string, Dictionary<string, float>>
  {

  }

  /// <summary>The returned final path</summary>
  public struct PathReturn
  {
    /// <summary>Calculated total time that it would take the drone to travel</summary>
    public float totalTime;

    /// <summary>The path in coordinates</summary>
    public List<string> path;

    /// <summary>The tile data of the coordinates</summary>
    public List<TileData> tiles;
  }

  /// <summary>Dijkstras algorithm returns all paths to the Start position,
  /// since that operation can take a while to complete depending on the amount of nodes
  /// we cache the results </summary>
  public struct CachedPath
  {
    /// <summary>Key is the coordinate and value is
    /// how long it would take to reach the start position </summary>
    public Dictionary<string, float> shortestPath;

    /// <summary>Key is the coordinate and value is the shortest next path to Start</summary>
    public Dictionary<string, string> previousNodes;
  }

  /// <summary>The 2D grid that will be used for the path finding</summary>
  public sealed class Grid : MonoBehaviour
  {

    public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";
    readonly Dictionary<string, CachedPath> _cachedPaths = new();
    public ParsedData parsed;
    List<string> _pathHistory = new();

    [Header("Prefabs/Assets")]
    [SerializeField] Tile _tilePrefab;
    [SerializeField] Connector _connectorPrefab;
    [SerializeField] Drone _drone;

    [Header("UI Inputs")]
    [SerializeField] TMP_InputField _startCoordinateInput;
    [SerializeField] TMP_InputField _pickupCoordinateInput;
    [SerializeField] TMP_InputField _dropOffCoordinateInput;

    [Header("Other UI")]
    [SerializeField] TextMeshProUGUI _pathElement;
    [SerializeField] TextMeshProUGUI _pathHistoryUi;


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
    string _lastPath;

    public TileData this[string coordinate]
    {
      get => _allTilesData[coordinate];
    }

    void PushToHistory(string entry)
    {
      _pathHistory.Add(entry);
      var historyCount = _pathHistory.Count;
      if (historyCount > 5) _pathHistory.RemoveAt(0);
    }

    public Vector3 GetTileWorldCoordinate(string coordinate) => _allTilesData[coordinate].globalCoordinates;

    /// <summary>This is a default unity function, it is called when the object is instantiated into the scene</summary>
    [ButtonMethod]
    void Awake()
    {
      parsed = DownloadAndParseGridData(url);
      GenerateGrid(parsed);
    }

    /// <summary>This is meant to be called from a button in the UI
    /// This method does:
    /// * Set some colors for a couple tiles
    /// * Get inputs from the UI
    /// * Call the pathfinding functions
    /// * Start the drone Coroutine that makes it move
    /// </summary>
    public void MoveDrone()
    {
      if (_drone.alreadyMoving) return;

      var start = _startCoordinateInput.text.ToUpper();
      var pickup = _pickupCoordinateInput.text.ToUpper();
      var dropOff = _dropOffCoordinateInput.text.ToUpper();

      foreach (var currTile in _changedColorsTiles) currTile.ResetColor();
      _changedColorsTiles.Clear();
      SetColor(start, Color.green);
      SetColor(pickup, Color.yellow);
      SetColor(dropOff, Color.blue);

      if (_lastPath != null) PushToHistory(_lastPath);
      var shortestPath = GetShortestPath(new string[] { start, pickup, dropOff });
      _lastPath = $"Path: {string.Join(" -> ", shortestPath.path)}\nTotal Time:{shortestPath.totalTime}";
      _pathElement.text = _lastPath;
      _pathHistoryUi.text = string.Join('\n', _pathHistory);

      StartCoroutine(_drone.FollowPath(shortestPath.tiles));
    }

    /// <summary>Set color of a tile based o it's coordinates</summary>
    private void SetColor(string coords, Color color)
    {
      var tileToChange = _allTiles[coords];
      tileToChange.SetColor(color);
      _changedColorsTiles.Add(tileToChange);
    }

    /// <summary>Download and parse the API</summary>
    ParsedData DownloadAndParseGridData(string url)
    {
      using var wc = new WebClient();
      var json = wc.DownloadString(url);
      return JsonConvert.DeserializeObject<ParsedData>(json);
    }

    /// <summary>Generate grid based on the parsed API</summary>
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

    /// <summary>Return an already created tile or create a new one if a tile
    /// with the coordinates does not already exist</summary>
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

    // TODO: Convert this into an async operation
    public PathReturn GetShortestPath(string[] positions)
    {
      var positionsLen = positions.Length;
      if (positionsLen < 2) throw new("positions need to have at least 2 items");

      var totalTime = 0f;

      var path = new List<string>();
      var tiles = new List<TileData>();

      for (int pathIndex = 1; pathIndex < positionsLen; pathIndex++)
      {
        var previousCoordinate = positions[pathIndex - 1];
        var nextCoordinate = positions[pathIndex];

        var currPath = GetShortestPath(previousCoordinate, nextCoordinate);
        totalTime += currPath.totalTime;

        if (pathIndex < positionsLen - 1)
        {
          currPath.path.RemoveAt(currPath.path.Count - 1);
          currPath.tiles.RemoveAt(currPath.tiles.Count - 1);
        }

        path = path.Concat(currPath.path).ToList();
        tiles = tiles.Concat(currPath.tiles).ToList();
      }
      return new()
      {
        path = path,
        totalTime = totalTime,
        tiles = tiles
      };
    }

    /// <summary>Find the shortest paths between start and end using Dijkstras algorithm
    /// All paths to start node are automatically cached so if it is asked
    /// again, there's no need to recalculate it</summary>
    /// <seealso cref="FindAllPathsFrom"/>
    public PathReturn GetShortestPath(string startPosition, string endPosition)
    {
      // If the API were dynamic, we would recreate the grid in here by simply called Awake after deleting the grid
      // But since it is static, we don't have to.
      CachedPath currCachedPath;
      var path = new List<string>();

      // No need to do anything other than return start position if the start and end are the same
      if (startPosition == endPosition)
      {
        path.Add(startPosition);
        return new PathReturn()
        {
          path = path,
          totalTime = 0,
          tiles = new() { _allTilesData[path[0]] }
        };
      }
      if (_cachedPaths.ContainsKey(startPosition)) currCachedPath = _cachedPaths[startPosition];
      else
      {
        currCachedPath = FindAllPathsFrom(startPosition);
        _cachedPaths[startPosition] = currCachedPath;
      }

      var node = endPosition;
      var previousNodes = currCachedPath.previousNodes;
      while (node != startPosition)
      {
        path.Add(node);
        node = previousNodes[node];
      }

      path.Add(startPosition);
      path.Reverse();

      return new()
      {
        path = path,
        totalTime = currCachedPath.shortestPath[endPosition],
        tiles = (from currCoordinate in path select _allTilesData[currCoordinate]).ToList()
      };
    }

    /// <summary>Find all paths to the start position using Dijkstras algorithm
    /// This is necessary because the API only have seconds instead of distance, which we can use as weight
    /// if we had distance information, we could use A* which is more effective</summary>
    CachedPath FindAllPathsFrom(string startPosition)
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
      return new CachedPath
      {
        shortestPath = shortestPath,
        previousNodes = previousNodes
      };
    }

    /// <summary>Get all neighbors of the tile<summary>
    Dictionary<string, float> GetNeighbors(string coordinates) => _allTilesData[coordinates].neighbors;
  }
}

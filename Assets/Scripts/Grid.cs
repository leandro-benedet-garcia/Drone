using System.Net;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Newtonsoft.Json;
using MyBox;

namespace Drone
{
  struct CachedPath
  {
    public Dictionary<string, string> previousNodes;
    public Dictionary<string, float> shortestPath;
  }

  /// <summary>The 2D grid that will be used for the path finding</summary>
  public sealed class Grid : MonoBehaviour
  {
    public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";
    readonly Dictionary<string, CachedPath> _cachedPaths = new();
    public Dictionary<string, Dictionary<string, float>> parsed;
    [SerializeField] Tile _tilePrefab;
    [SerializeField] Connector _connectorPrefab;


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
    public Dictionary<string, TileData> allTiles;

    public TileData this[string coordinate]{
      get => allTiles[coordinate];
      set => allTiles[coordinate] = value;
    }

    /// <summary>This is a default unity function, it is called when the object is instantiated into the scene</summary>
    [ButtonMethod]
    void Awake()
    {
      parsed = DownloadAndParseGridData(url);
      GenerateGrid(parsed);
    }

    Dictionary<string, Dictionary<string, float>> DownloadAndParseGridData(string url)
    {
      using var wc = new WebClient();
      var json = wc.DownloadString(url);
      return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
    }

    void GenerateGrid(Dictionary<string, Dictionary<string, float>> parsed)
    {
      allTiles = new();

      foreach (var currParsedTile in parsed)
      {
        var neighbors = currParsedTile.Value;

        var currTile = GetOrCreateTile(currParsedTile.Key);

        var currInstance = Instantiate(_tilePrefab, transform);
        currInstance.Initialize(currTile);

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
      if (allTiles.ContainsKey(key))
      {
        return allTiles[key];
      }

      var createdTile = new TileData
      {
        globalCoordinates = FromLetterNumberToVector(key),
        letterCoordinate = key,
        neighbors = new()
      };
      allTiles[key] = createdTile;
      return createdTile;
    }

    /// <summary>Transform letter and number coordinate system to a Vector to be used by Unity later</summary>
    private Vector3 FromLetterNumberToVector(string letterAndNumber)
    {
      var firstNumber = _letterToInt[letterAndNumber[0]];
      var secondNumber = int.Parse(letterAndNumber[1].ToString());

      return new(firstNumber, 0, secondNumber);
    }

    /// <summary>Find the shortest paths between start and end using Dijkstras algorithm
    /// All paths to start node are automatically cached so if it is asked again, there's no need to recalculate it</summary>
    public List<string> GetShortestPath(string startPosition, string endPosition)
    {
      // If the API were dynamic, we would recreate the grid in here by simply called Awake after deleting the grid
      // But since it is static, we don't have to.
      CachedPath currCachedPath;
      var path = new List<string>();

      // No need todo anything other than return start position if the start and end are the same
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

      var previousNodes = currCachedPath.previousNodes;
      var node = endPosition;
      while (node != startPosition)
      {
        path.Add(node);
        node = previousNodes[node];
      }
      path.Add(startPosition);
      return path;
    }

    /// <summary>Find all paths to the start position using Dijkstras algorithm</summary>
    CachedPath FindAllPathsFrom(string startPosition)
    {
      var unvisitedNodes = allTiles.Keys.ToList();
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
          Debug.Log($"{neighborCoordinate}={totalCost}?{shortestPath[neighborCoordinate]}");
          if (totalCost < shortestPath[neighborCoordinate])
          {
            Debug.Log($"{neighborCoordinate}: {totalCost}");
            shortestPath[neighborCoordinate] = totalCost;
            previousNodes[neighborCoordinate] = minNode;
          }
        }
        unvisitedNodes.Remove(minNode);
      }
      foreach (var node in previousNodes)
        Debug.Log($"{node.Key}->{node.Value}");
      return new CachedPath
      {
        shortestPath = shortestPath,
        previousNodes = previousNodes
      };
    }

    Dictionary<string, float> GetNeighbors(string coordinates) => allTiles[coordinates].neighbors;
  }
}

using System.Net;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace Drone
{
  /// <summary>The 2D grid that will be used for the path finding</summary>
  public sealed class Grid: MonoBehaviour
  {
    public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";
    [SerializeField] Tile _tilePrefab;

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
    private Dictionary<string, TileData> _allTiles;

    void Awake()
    {
      var parsed = DownloadAndParseGridData(url);
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
      _allTiles = new();

      foreach (var currParsedTile in parsed)
      {
        var neighbors = currParsedTile.Value;
        var neighborLen = neighbors.Count;

        var currTile = GetOrCreateTile(currParsedTile.Key);

        var currInstance = Instantiate(_tilePrefab, transform);
        currInstance.Initialize(currTile);

        foreach (var currNeighbor in neighbors)
        {
          var converted = GetOrCreateTile(currNeighbor.Key);

          // For some reason the distances are different back and forth
          // Otherwise I would add each other as a neighbor here.
          currTile.neighbors[converted] = currNeighbor.Value;
        }
      }
    }

    /// <summary>Return an already created tile or create a new one if a tile with the coordinates does not already exist</summary>
    /// <param name="key">The coordinate in the format A2</param>
    /// <returns>A brand new or already existing Tile</returns>
    private TileData GetOrCreateTile(string key)
    {
      if (_allTiles.ContainsKey(key))
      {
        return _allTiles[key];
      }

      var createdTile = new TileData
      {
        index = FromLetterNumberToVector(key),
        letterCoordinate = key,
        neighbors = new()
      };
      _allTiles[key] = createdTile;
      return createdTile;
    }

    /// <summary>Transform letter and number coordinate system to a Vector to be used by Unity later</summary>
    private Vector3 FromLetterNumberToVector(string letterAndNumber)
    {
      var firstNumber = _letterToInt[letterAndNumber[0]];
      var secondNumber = int.Parse(letterAndNumber[1].ToString());

      Debug.Log($"{firstNumber},{secondNumber}");
      return new(firstNumber, 0, secondNumber);
    }
  }
}

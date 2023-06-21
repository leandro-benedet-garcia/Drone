using System.Net;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace Drone
{
    /// <summary>The 2D grid that will be used for the path finding</summary>
    public sealed class Grid
    {
        public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";

        /// <summary>Dict used to convert from letters to numbers</summary>
        private readonly Dictionary<char, int> _letterToInt = new(){
            {'A', 0},
            {'B', 1},
            {'C', 2},
            {'D', 3},
            {'F', 4},
            {'G', 5},
            {'H', 6}
        };
        private Dictionary<string, TileData> _allTiles;

        private Dictionary<string, Dictionary<string, float>> DownloadAndParseGridData()
        {
            using WebClient wc = new WebClient();
            string json = wc.DownloadString(url);
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
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

            TileData createdTile = new TileData
            {
                index = FromLetterNumberToVector(key),
                letterCoordinate = key,
                neighbors = new()
            };
            _allTiles[key] = createdTile;
            return createdTile;
        }

        /// <summary>Transform letter and number coordinate system to a Vector to be used by Unity later</summary>
        private Vector2Int FromLetterNumberToVector(string letterAndNumber)
        {
            var firstNumber = _letterToInt[letterAndNumber[0]];
            var secondNumber = (int)letterAndNumber[1];
            return new(firstNumber, secondNumber);
        }
    }
}

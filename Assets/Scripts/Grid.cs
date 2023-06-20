using System.Net;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;
using System;

namespace Drone
{
    public struct Tile
    {
        public Vector2Int index;
        public string letterCoordinate;
        public Dictionary<Tile, float> neighbors;
    }

    public sealed class Grid
    {
        public string url = "https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f";
        Dictionary<char, int> _letterToInt = new(){
            {'A', 0},
            {'B', 1},
            {'C', 2},
            {'D', 3},
            {'F', 4},
            {'G', 5},
            {'H', 6}
        };

        Dictionary<string, Tile> _allTiles;

        Dictionary<string, Dictionary<string, float>> DownloadAndParseGridData()
        {
            using var wc = new WebClient();
            var json = wc.DownloadString(url);
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
        }

        void GenerateGrid()
        {
            _allTiles = new();
            var parsed = DownloadAndParseGridData();

            foreach (var currParsedTile in parsed)
            {
                var neighbors = currParsedTile.Value;
                var neighborLen = neighbors.Count;

                Tile currTile = GetOrCreateTile(currParsedTile.Key);
                foreach (var currNeighbor in neighbors)
                {
                    var converted = GetOrCreateTile(currNeighbor.Key);

                    // For some reason the distances are different back and forth
                    // Otherwise I would add each other as a neighbor here.
                    currTile.neighbors[converted] = currNeighbor.Value;
                }
            }
        }

        Tile GetOrCreateTile(string key)
        {
            if (_allTiles.ContainsKey(key)) return _allTiles[key];

            var createdTile = new Tile
            {
                index = FromLetterNumberToVector(key),
                letterCoordinate = key,
                neighbors = new()
            };
            _allTiles[key] = createdTile;
            return createdTile;
        }

        /// <summary>Transform letter and number coordinate system to a Vector to be used by Unity later</summary>
        Vector2Int FromLetterNumberToVector(string letterAndNumber)
        {
            var firstNumber = _letterToInt[letterAndNumber[0]];
            var secondNumber = (int)letterAndNumber[1];
            return new(firstNumber, secondNumber);
        }
    }
}

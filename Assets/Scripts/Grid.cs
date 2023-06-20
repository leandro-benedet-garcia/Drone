using System.Net;
using System.Collections.Generic;

using UnityEngine;

using Newtonsoft.Json;

namespace Drone
{
    public struct Tile
    {
        public Vector2Int index;
        public Dictionary<Tile, float> neighbors;
    }

    public sealed class Grid
    {
        Dictionary<char, int> letterToInt = new(){
            {'A', 0},
            {'B', 1},
            {'C', 2},
            {'D', 3},
            {'F', 4},
            {'G', 5},
            {'H', 6}
        };

        Tile[] allTiles;

        Dictionary<string, Dictionary<string, float>> DownloadAndParseGridData()
        {
            using (var wc = new WebClient())
            {
                var json = wc.DownloadString("https://mocki.io/v1/10404696-fd43-4481-a7ed-f9369073252f");
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, float>>>(json);
            }
        }
    }
}
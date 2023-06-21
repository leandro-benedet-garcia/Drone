using UnityEngine;
using System.Collections.Generic;


namespace Drone
{
    public struct TileData
    {
        public Vector2Int index;
        public string letterCoordinate;
        public Dictionary<TileData, float> neighbors;
    }

    public class Tile: MonoBehaviour{
        TileData data;
    }
}

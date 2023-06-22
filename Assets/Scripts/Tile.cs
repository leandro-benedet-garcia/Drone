using UnityEngine;
using System.Collections.Generic;


namespace Drone
{
  public struct TileData
  {
    public Vector3 index;
    public string letterCoordinate;
    public Dictionary<TileData, float> neighbors;
  }

  public class Tile : MonoBehaviour
  {
    TileData _data;

    public void Initialize(TileData data)
    {
      _data = data;
      transform.localPosition = data.index;
    }
  }
}

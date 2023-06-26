using UnityEngine;
using System.Collections.Generic;

using TMPro;

namespace DroneGame
{
  [SerializeField]
  public struct TileData
  {
    public Vector3 globalCoordinates;
    public string letterCoordinate;
    public Dictionary<string, float> neighbors;
  }

  public class Tile : MonoBehaviour
  {
    TileData _data;

    public void Initialize(TileData data)
    {
      _data = data;
      transform.localPosition = data.globalCoordinates;
      var text = transform.Find("Text").GetComponent<TextMeshPro>();
      text.text = data.letterCoordinate;
    }
  }
}

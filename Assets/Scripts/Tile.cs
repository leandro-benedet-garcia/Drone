using UnityEngine;
using System.Collections.Generic;

using TMPro;
using System;

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
    Material _tileMaterial;
    Color _startColor;


    public void ResetColor(){
      SetColor(_startColor);
    }

    public void Initialize(TileData data)
    {
      _tileMaterial = transform.Find("TileModel").GetComponent<MeshRenderer>().material ?? throw new("TileMaterial not found");
      _startColor = _tileMaterial.color;
      _data = data;
      transform.localPosition = data.globalCoordinates;
      var text = transform.Find("Text").GetComponent<TextMeshPro>();
      text.text = data.letterCoordinate;
    }

    public void SetColor(Color color)
    {
      _tileMaterial.color = color;
    }
  }
}

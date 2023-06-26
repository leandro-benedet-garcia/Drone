using UnityEngine;
using System.Collections.Generic;

using TMPro;

namespace DroneGame
{
  [SerializeField]
  /// <summary>struct that holds information about the tiles from the API</summary>
  public struct TileData
  {
  /// <summary>World coordinates of the tile</summary>
    public Vector3 globalCoordinates;

    /// <summary>Original letter and number coordinate</summary>
    public string letterCoordinate;

  /// <summary>Dict with the tile neighbors, key is letterCoordinate and value is  distance to the neighbor</summary>
    public Dictionary<string, float> neighbors;
  }

  /// <summary>Component of the tile, mainly used for Unity</summary>
  public class Tile : MonoBehaviour
  {
    Material _tileMaterial;
    Color _startColor;

    /// <summary>Set color of the tile</summary>
    public void SetColor(Color color) =>_tileMaterial.color = color;

    /// <summary>Set color of the tile back to the original</summary>
    public void ResetColor() => SetColor(_startColor);

    /// <summary>Since Unity does not allow arguments trough Instantiate we create a startup function
    /// Position and initialize tile from the TileData</summary>
    public void Initialize(TileData data)
    {
      _tileMaterial = transform.Find("TileModel").GetComponent<MeshRenderer>().material ?? throw new("TileMaterial not found");
      _startColor = _tileMaterial.color;
      transform.localPosition = data.globalCoordinates;
      var text = transform.Find("Text").GetComponent<TextMeshPro>();
      text.text = data.letterCoordinate;
    }


  }
}

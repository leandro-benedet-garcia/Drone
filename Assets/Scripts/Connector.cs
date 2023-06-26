using UnityEngine;

namespace DroneGame
{
  /// <summary>Mostly for debug purposes, it is an arrow that points towards a neighbor tile</summary>
  class Connector : MonoBehaviour
  {
    public TileData from;
    public TileData to;
    public float distance;

    /// <summary>Since Unity does not allow arguments trough Instantiate we create a startup function</summary>
    /// <param name="from">Node that contains the neighbor</param>
    /// <param name="to">Neighbor node that the arrow shall point to</param>
    public void Initialize(TileData from, TileData to, float distance)
    {
      this.from = from;
      this.to = to;
      this.distance = distance;
      var toCoordinate = to.globalCoordinates;

      // Place Connector between two tiles
      var connectorPosition = Vector3.Lerp(from.globalCoordinates, toCoordinate, 0.5f);
      transform.localPosition = connectorPosition;
      transform.LookAt(toCoordinate);
    }
  }
}

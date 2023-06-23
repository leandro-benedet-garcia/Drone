using UnityEngine;

using TMPro;

namespace Drone
{
  class Connector : MonoBehaviour
  {
    public TileData from;
    public TileData to;
    public float distance;

    public void Initialize(TileData from, TileData to, float distance)
    {
      this.from = from;
      this.to = to;
      this.distance = distance;
      var toCoordinate = to.globalCoordinates;

      var connectorPosition = Vector3.Lerp(from.globalCoordinates, toCoordinate, 0.5f);
      transform.localPosition = connectorPosition;
      transform.LookAt(toCoordinate);
    }
  }
}

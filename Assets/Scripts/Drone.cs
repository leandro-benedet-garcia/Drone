using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DroneGame
{
  class Drone : MonoBehaviour
  {
    Grid _grid;
    bool _alreadyMoving;

    /// <summary>Set the move speed of the drone</summary>
    [SerializeField]
    [Range(0.1f, 1.0f)]
    float _moveSpeed;

    /// <summary>This is here to mostly just setup the height, but it's a vector3 to facilitate to use in math</summary>
    [SerializeField] Vector3 _coordinateAdjustment;
    void Start()
    {
      _grid = GameObject.Find("Grid").GetComponent<Grid>() ?? throw new("Grid not found");
      transform.position = _grid.GetTileWorldCoordinate("A1") + _coordinateAdjustment;
    }

    /// <summary>This function is meant to be executed as a coroutine
    /// Make drone move along the specified path.</summary>
    public IEnumerator FollowPath(List<TileData> path)
    {
      if (_alreadyMoving) throw new("Drone is already moving");

      _alreadyMoving = true;

      for (int tileIndex = 1; tileIndex < path.Count; tileIndex++)
      {
        var currLerp = 0f;
        var previousTile = path[tileIndex - 1].globalCoordinates + _coordinateAdjustment;
        var nextTile = path[tileIndex].globalCoordinates + _coordinateAdjustment;

        while (currLerp <= 1f)
        {
          var currSpeed = _moveSpeed * Time.deltaTime;
          transform.position = Vector3.Lerp(previousTile, nextTile, currLerp);

          // And this is the reason why this works
          // Wait for the next frame to continue the code execution.
          yield return null;
          currLerp += currSpeed;
        }
      }
      _alreadyMoving = false;
    }
  }
}

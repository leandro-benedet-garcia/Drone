using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DroneGame
{
  public class Drone : MonoBehaviour
  {
    Grid _grid;
    Button _button;

    public bool alreadyMoving;

    /// <summary>Set the move speed of the drone</summary>
    [SerializeField]
    [Range(0.1f, 10.0f)]
    float _moveSpeed;

    /// <summary>This is here to mostly just setup the height, but it's a vector3 to facilitate to use in math</summary>
    [SerializeField] Vector3 _coordinateAdjustment;

    /// <summary>Default Unity event, Start executes when th object is instantiated and happens after Awake is executed</summary>
    public void Initialize(Grid grid)
    {
      _grid = grid;
      _button = GameObject.Find("CalculateAndMoveButton").GetComponent<Button>() ?? throw new("Button not found");
      transform.position = _grid.GetTileWorldCoordinate("A1") + _coordinateAdjustment;
    }

    /// <summary>This function is meant to be executed as a coroutine
    /// Make drone move along the specified path.</summary>
    public IEnumerator FollowPath(List<TileData> path)
    {
      if (alreadyMoving) throw new("Drone is already moving");

      alreadyMoving = true;
#if !UNITY_INCLUDE_TESTS
      _button.interactable = false;
#endif
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
      alreadyMoving = false;
#if !UNITY_INCLUDE_TESTS
      _button.interactable = true;
#endif
    }
  }
}

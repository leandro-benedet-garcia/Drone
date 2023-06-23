using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;

namespace Drone.Tests
{
  public class TestGrid
  {
    Grid _grid;

    [UnitySetUp]
    public IEnumerator GlobalSetup()
    {
      yield return new EnterPlayMode();
      var gridPrefab = Resources.Load<Grid>("Grid") ?? throw new("Resource Grid Not found");
      _grid = Object.Instantiate(gridPrefab);
    }

    [Test]
    public void TestGridCreation()
    {
      foreach (var currTile in _grid.allTiles)
      {
        var coordinate = currTile.Key;

        Assert.True(char.IsLetter(coordinate[0]));
        Assert.True(char.IsNumber(coordinate[1]));

        var neighbors = currTile.Value
                                                            .neighbors;

        Assert.IsNotEmpty(neighbors);

        foreach (var neighbor in neighbors)
        {
          var neighborCoordinate = neighbor.Key.letterCoordinate;

          // no need to test for the time because the parser automatically raises an error if it were to be invalid
          Assert.True(char.IsLetter(neighborCoordinate[0]));
          Assert.True(char.IsNumber(neighborCoordinate[1]));
        }
      }
    }

    [Test]
    public void TestShortestPath()
    {

    }
  }
}

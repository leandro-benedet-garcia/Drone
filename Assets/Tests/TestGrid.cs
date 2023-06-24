using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;
using System.Collections.Generic;

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
      foreach (var currTile in _grid.parsed)
      {
        var currCoordinate = currTile.Key;
        var tileInGrid = _grid[currCoordinate];

        Assert.True(char.IsLetter(currCoordinate[0]));
        Assert.True(char.IsNumber(currCoordinate[1]));

        var neighbors = tileInGrid.neighbors;

        Assert.IsNotEmpty(neighbors);
        Assert.False(neighbors.ContainsKey(currCoordinate));

        foreach (var neighbor in currTile.Value)
        {
          var neighborCoordinate = neighbor.Key;

          Assert.Contains(neighborCoordinate, neighbors.Keys);

          // no need to test for the time because the parser automatically raises an error if it were to be invalid
          Assert.True(char.IsLetter(neighborCoordinate[0]));
          Assert.True(char.IsNumber(neighborCoordinate[1]));
        }
      }
    }

    [Test]
    public void TestShortestPath()
    {
      var A1ToA1 = new List<string>(){"A1"};
      var A1ToA3 = new List<string>(){"A1", "A2", "A3"};
      var A1ToA4 = new List<string>(){"A1", "A2", "A3", "A4"};

      var calculatedPath = _grid.GetShortestPath("A1", "A1");
      Assert.AreEqual(A1ToA1, calculatedPath);

      calculatedPath = _grid.GetShortestPath("A1", "A3");
      Assert.AreEqual(A1ToA3, calculatedPath);

      calculatedPath = _grid.GetShortestPath("A1", "A4");
      Assert.AreEqual(A1ToA4, calculatedPath);
    }
  }
}

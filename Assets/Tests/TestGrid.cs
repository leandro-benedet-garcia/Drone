using NUnit.Framework;

using UnityEngine;

using System.Collections.Generic;
using System.Diagnostics;

namespace Drone.Tests
{
  public class TestGrid
  {
    Grid _grid;

    [OneTimeSetUp]
    public void GlobalSetup()
    {
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
      var A1ToA1 = new List<string>() { "A1" };
      var A1ToA3 = new List<string>() { "A1", "A2", "A3" };
      var A1ToA4 = new List<string>() { "A1", "A2", "A3", "A4" };

      // Where we going, we don't need diagonals
      // At first, I tried with diagonals, but my own code reminded that there were no diagonals in the API
      var A1ToH8 = new List<string>(){"A1", "B1", "C1", "C2", "C3", "D3", "E3",
                                      "F3", "F4", "F5", "F6", "G6", "G7", "H7",
                                      "H8"};

      // Nothing is done, A1 needs to be returned immediately
      var calculatedPath = _grid.GetShortestPath("A1", "A1");
      Assert.AreEqual(A1ToA1, calculatedPath);

      var watch = Stopwatch.StartNew();
      // Now the algorithm needs to work.
      calculatedPath = _grid.GetShortestPath("A1", "A3");
      Assert.AreEqual(A1ToA3, calculatedPath);
      watch.Stop();

      var firstRunTime = watch.ElapsedMilliseconds;

      watch = Stopwatch.StartNew();
      // This one must be executed faster than the first time because of cache
      calculatedPath = _grid.GetShortestPath("A1", "A3");
      Assert.AreEqual(A1ToA3, calculatedPath);
      watch.Stop();
      Assert.Less(watch.ElapsedMilliseconds, firstRunTime);

      calculatedPath = _grid.GetShortestPath("A1", "A4");
      Assert.AreEqual(A1ToA4, calculatedPath);

      calculatedPath = _grid.GetShortestPath("A1", "H8");
      Assert.AreEqual(A1ToH8, calculatedPath);
    }
  }
}

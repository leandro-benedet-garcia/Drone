using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using Drone;

public class TestGrid
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestDownloadAndParse()
    {
        var grid = new Drone.Grid();
        var parsed = Utils.CallPrivateMethod(grid, "DownloadAndParseGridData", new object[0]) as Dictionary<string, Dictionary<string, float>>;
        foreach(var currTile in parsed)
            foreach(var neighbor in currTile.Value)
                Debug.Log($"Neighbors of {currTile.Key}: {neighbor.Key}, {neighbor.Value}");
    }
}

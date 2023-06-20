using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using Drone;

public class TestGrid
{
    [Test]
    public void TestDownloaderAndParser()
    {
        var grid = new Drone.Grid();
        var parsed = Utils.CallPrivateMethod(grid, "DownloadAndParseGridData", new object[0]) as Dictionary<string, Dictionary<string, float>>;

        foreach (var currTile in parsed)
        {
            var coordinate = currTile.Key;

            Assert.True(char.IsLetter(coordinate[0]));
            Assert.True(char.IsNumber(coordinate[1]));

            foreach (var neighbor in currTile.Value)
            {
                var neighborCoordinate = neighbor.Key;

                // no need to test for the times because the parser automatically raises an error if it were to be invalid
                Assert.True(char.IsLetter(neighborCoordinate[0]));
                Assert.True(char.IsNumber(neighborCoordinate[1]));
            }
        }
    }
}

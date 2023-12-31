using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;

namespace DroneGame.Tests
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

    [UnityTest]
    public IEnumerator TestGridCreation()
    {
      while(_grid.parsed == null) yield return null;

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

    [UnityTest]
    public IEnumerator TestShortestPath()
    {
      while(_grid.parsed == null) yield return null;

      var A1ToA1 = new List<string>() { "A1" };
      var A1ToA3 = new List<string>() { "A1", "A2", "A3" };
      var A1ToA4 = new List<string>() { "A1", "A2", "A3", "A4" };

      // Where we going, we don't need diagonals
      // At first, I tried with diagonals, but my own code reminded that there were no diagonals in the API
      var A1ToH8 = new List<string>(){"A1", "B1", "C1", "C2", "C3", "D3", "E3",
                                      "F3", "F4", "F5", "F6", "G6", "G7", "H7",
                                      "H8"};

      // Nothing is done, A1 needs to be returned immediately
      var calculatedPath = _grid.GetShortestPath("A1", "A1").path;
      Assert.AreEqual(A1ToA1, calculatedPath);

      var watch = Stopwatch.StartNew();
      // Now the algorithm needs to work.
      calculatedPath = _grid.GetShortestPath("A1", "A3").path;
      Assert.AreEqual(A1ToA3, calculatedPath);
      watch.Stop();

      var firstRunTime = watch.ElapsedMilliseconds;

      watch = Stopwatch.StartNew();
      // This one must be executed faster than the first time because of cache
      calculatedPath = _grid.GetShortestPath("A1", "A3").path;
      Assert.AreEqual(A1ToA3, calculatedPath);
      watch.Stop();
      Assert.Less(watch.ElapsedMilliseconds, firstRunTime);

      calculatedPath = _grid.GetShortestPath("A1", "A4").path;
      Assert.AreEqual(A1ToA4, calculatedPath);

      calculatedPath = _grid.GetShortestPath("A1", "H8").path;
      Assert.AreEqual(A1ToH8, calculatedPath);

      calculatedPath = _grid.GetShortestPath(new string[] { "A1", "H8" }).path;
      Assert.AreEqual(A1ToH8, calculatedPath);

      calculatedPath = _grid.GetShortestPath(new string[] { "A1", "F5", "H8" }).path;
      Assert.AreEqual(A1ToH8, calculatedPath);

      calculatedPath = _grid.GetShortestPath(new string[] { "A1", "F5", "G6", "H8" }).path;
      Assert.AreEqual(A1ToH8, calculatedPath);
    }

    [UnityTest]
    public IEnumerator TestDroneMove()
    {
      while(_grid.parsed == null) yield return null;

      var dronePrefab = Resources.Load<Drone>("Drone/Drone") ?? throw new("Resource Drone Not found");
      var drone = Object.Instantiate(dronePrefab);


      var first = _grid["A1"];
      var second = _grid["A2"];

      drone.StartCoroutine(drone.FollowPath(new(){first, second}));
    }
  }
}

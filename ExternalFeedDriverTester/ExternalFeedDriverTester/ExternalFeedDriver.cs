/// ExternalFeedDriver
/// Driver for Hololens based tests of External Feed pathway of Sensor Integrator Application.
/// Mark Scherer, June 2018

using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Driver of External Feed pathway of Sensor Integrator Application.
/// </summary>
[RequireComponent(typeof(MeshManager))]
[RequireComponent(typeof(VoxelGridManager))]
public class ExternalFeedDriver : MonoBehaviour
{
    /// <summary>
    /// Tracks speed of execution of pathway.
    /// </summary>
    public double frequency { get; private set; }

    /// <summary>
    /// Central control for VoxelGridManager.
    /// </summary>
    public bool updateVoxelStructure = true;

    private Stopwatch stopWatch = new Stopwatch();

    System.Random rand = new System.Random();

    /// <summary>
    /// Called once at startup.
    /// </summary>
    void Start()
    {
        VoxelGridManager.Instance.updateStruct = updateVoxelStructure;
    }
    
    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        stopWatch.Start();
        List<(Vector3, byte)> updates = new List<(Vector3, byte)>();

        /// get mesh vertex list from MeshManager
        List<Vector3> vertices = MeshManager.Instance.getVertices();

        foreach (Vector3 point in vertices)
            updates.Add((point, (byte)rand.Next(0, 255)));

        /// add all vertices to voxel grid with VoxelGridManager
        VoxelGridManager.Instance.set(updates);

        stopWatch.Stop();
        double millisecs = stopWatch.ElapsedMilliseconds;
        frequency = 1000.0 / millisecs;
    }
}
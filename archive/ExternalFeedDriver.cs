/// ExternalFeedDriver
/// Driver for Hololens based tests of External Feed pathway of Sensor Integrator Application.
/// Mark Scherer, June 2018

using System;
using System.Linq;
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
    public double speed { get; private set; }

    /// <summary>
    /// Central control for VoxelGridManager.
    /// </summary>
    public bool updateVoxelStructure = true;

    private Stopwatch stopWatch = new Stopwatch();

    /// get dependent classes
    private MeshManager meshManager;
    private VoxelGridManager voxManager;


    /// <summary>
    /// Called once at startup.
    /// </summary>
    void Start()
    {
        meshManager = MeshManager.Instance;
        voxManager = VoxelGridManager.Instance;

        voxManager.updateStruct = updateVoxelStructure;
    }
    
    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        /*
        // Debug
        UnityEngine.Debug.Log("Starting ExternalFeedDriver:Update()... starting stopwatch...");

        stopWatch.Reset();
        stopWatch.Start();

        // Debug
        UnityEngine.Debug.Log("Trying MeshManager:getVertices()...");
        /// get mesh vertex list from MeshManager
        List<Vector3> vertices = meshManager.getVertices();

        // Debug
        UnityEngine.Debug.Log("Creating List<byte> updateValues...");
        // create list of update values
        byte defaultValue = 0;
        List<byte> updateValues = Enumerable.Repeat(defaultValue, vertices.Count).ToList();

        // Debug
        UnityEngine.Debug.Log("Trying VoxelGridManager:set()...");
        // push updates to VoxelGridManager
        voxManager.set(vertices, updateValues);

        // Debug
        UnityEngine.Debug.Log("Ending stopwatch...");
        stopWatch.Stop();
        speed = (double)Stopwatch.Frequency / (double)stopWatch.ElapsedTicks;

        // Debug
        UnityEngine.Debug.Log("Trying GC.GetTotalMemory...");
        long Memory = GC.GetTotalMemory(false);
        UnityEngine.Debug.Log(String.Format("Finished ExternalFeedDriver:Update(). Cycle Time (ms): {0}. Total Program Memory Use: {1}",
            Math.Round(speed, 1), MemToStr(Memory)));
            
        */
    }

    /// <summary>
    /// Returns presentable string of memory size.
    /// </summary>
    static string MemToStr(long bytes)
    {
        if (bytes < 1000) // less than 1 kB
            return String.Format("{0} B", bytes);
        if (bytes < 1000 * 1000) // less than 1 MB
            return String.Format("{0} kB", Math.Round((double)bytes / 1000.0, 2));
        if (bytes < 1000 * 1000 * 1000) // less than 1 GB
            return String.Format("{0} MB", Math.Round((double)bytes / (1000.0 * 1000.0), 2));
        return String.Format("{0} GB", Math.Round((double)bytes / (1000.0 * 1000.0 * 1000.0), 2));
    }
}
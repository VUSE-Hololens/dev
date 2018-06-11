/// VoxelGridManager
/// Interface for voxelated data structure. 
/// Mark Scherer, June 2018

/// NOTE: Tested via VoxelGridTester/Program.cs/TestVoxelGridManager() (6/11/2018)

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for voxelated data structure. Currently setup to store byte data.
/// NOTE: Because singleton, voxGrid constructor values must be set with static control variables within this class.
/// </summary>
public class VoxelGridManager : Singleton<VoxelGridManager>
{
    /// <summary>
    /// Control for starting point of voxGrid.
    /// </summary>
    private static Vector3 startingPoint = new Vector3(0f, 0f, 0f); // meters

    /// <summary>
    /// Control for minSize for voxGrid.
    /// </summary>
    private static float minSize = 0.1f; // meters

    /// <summary>
    /// Control for defaultSize for voxGrid.
    /// </summary>
    private static float defaultSize = 1.0f; // meters

    /// <summary>
    /// Voxel grid data structure.
    /// </summary>
    private Octree<byte> voxGrid;

    /// <summary>
    /// Runtime control for updateStruct of voxGrid set method.
    /// </summary>
    public bool updateStruct = true;

    /// <summary>
    /// DateTime of last voxGrid update.
    /// </summary>
    public DateTime lastUpdate { get; private set; }

    /// <summary>
    /// Constructor.
    /// NOTE: Singleton<T> enforces new constraint on T... must have public, parameterless constructor.
    /// </summary>
    public VoxelGridManager()
    {
        voxGrid = new Octree<byte>(startingPoint, minSize, defaultSize);
        lastUpdate = DateTime.Now;
    }

    /// <summary>
    /// Accessor for voxGrid metadata.
    /// </summary>
    public (int components, int voxels, int nonNullVoxels, double volume, double nonNullVolume,
        Vector3 min, Vector3 max, DateTime lastUpdated) about()
    {
        (int components, int voxels, int nonNullVoxels, double volume, double nonNullVolume,
            Vector3 min, Vector3 max, DateTime lastUpdated) metadata;
        metadata.components = voxGrid.numComponents;
        metadata.voxels = voxGrid.numVoxels;
        metadata.nonNullVoxels = voxGrid.numNonNullVoxels;
        metadata.volume = voxGrid.volume;
        metadata.nonNullVolume = voxGrid.nonNullVolume;
        metadata.min = voxGrid.root.min;
        metadata.max = voxGrid.root.max;
        metadata.lastUpdated = lastUpdate;
        return metadata;
    }

    /// <summary>
    /// Public interface for voxGrid set method.
    /// </summary>
    /// <param name="updates">
    /// List of tuples of points, values to set.
    /// </param>
    public void set(List<(Vector3 point, byte value)> updates)
    {
        for (int i = 0; i < updates.Count; i++)
            voxGrid.set(updates[i].point, updates[i].value, updateStruct);
        lastUpdate = DateTime.Now;
    }
}
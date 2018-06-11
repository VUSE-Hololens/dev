/// MeshManager
/// Interface for Hololens spatial mapping data via HoloToolKit/SpatialMapping/SpatialMappingManager. 
/// Mark Scherer, June 2018

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Accessor class to Hololens spatial mapping data via HoloToolKit/SpatialMapping/SpatialMappingManager.
/// </summary>
[RequireComponent(typeof(HoloToolkit.Unity.SpatialMapping.SpatialMappingManager))]
public class MeshManager : Singleton<MeshManager>
{
    /// <summary>
    /// Metadata: number of vertices returned by getVertices.
    /// Note: As discussed in getVertices, are repeat vertices included in count.
    /// </summary>
    public int vertexCount { get; private set; } = 0;

    /// <summary>
    /// Metadata: number of independent meshes returned by SpatialMappingManager.
    /// </summary>
    public int meshCount { get; private set; } = 0;

    /// <summary>
    /// Constructor. ONLY to be used within Singleton, elsewhere ALWAYS use Instance().
    /// Must follow Singleton's enforced new constraint.
    /// </summary>
    public MeshManager()
    {
        // nothing to do
    }

    /// <summary>
    /// Returns list of vertices of all meshes in caches spatial mapping data via SpatialMappingManager/GetMeshes().
    /// NOTE: Will have repeats as vertex is added each time it is included in a mesh.
    /// </summary>
    public List<Vector3> getVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Mesh> meshes = 
            HoloToolkit.Unity.SpatialMapping.SpatialMappingManager.Instance.GetMeshes();
        foreach (Mesh mesh in meshes)
            vertices.AddRange(mesh.vertices);
        meshCount = meshes.Count;
        vertexCount = vertices.Count;
        return vertices;
    }
}
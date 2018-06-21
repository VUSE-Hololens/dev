/// Intersector
/// Class for calculating resultant PointValues when 2D Raster is project along a Frustum at a Mesh.
/// Content:
/// PointValue (struct)
/// ViewVector (struct)
/// Frustum (struct)
/// OcclusionCell (struct)
/// Intersector (class)

/// NOTE: Tried to make Intersector:Intersection method generic so it was ambivalent to type of img. 
/// This caused strange Unity build errors when accessing and mutating values within cells of ocGrid.
/// Currently set to byte, if img data type changes will have to manually change code.

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic spatially-aware values.
/// </summary>
public struct PointValue<T>
{
    public Vector3 Point { get; private set; }
    public T Value { get; private set; }

    public PointValue(Vector3 myPoint, T myValue)
    {
        Point = myPoint;
        Value = myValue;
    }

    /// <summary>
    /// Same as constructor, except no new memory allocation.
    /// </summary>
    public void Update(Vector3 newPoint, T newValue)
    {
        Point = newPoint;
        Value = newValue;
    }
}

/// <summary>
/// Vector for relative angle of (x,y,z) vector relative to 2D plane. Vector must originate at origin.
/// For 2D plane = XY plane, positive Z is forwards, 2D plane coordinates i,j:
/// Theta (i): angle of vector with YZ plane
/// Phi (j): angle of vector with XZ plane
/// </summary>
public struct ViewVector
{
    public double Theta { get; private set; }
    public double Phi { get; private set; }

    public ViewVector(double myTheta, double myPhi)
    {
        Theta = myTheta;
        Phi = myPhi;
    }

    /// <summary>
    /// Creates ViewVector from spatialVector.
    /// Note: spatialVector MUST originate from origin.
    /// </summary>
    public ViewVector(Vector3 spatialVector)
    {
        Theta = Intersector.RadToDeg(Intersector.AdjAtanTheta(spatialVector.x, spatialVector.z));
        Phi = Intersector.RadToDeg(Intersector.AdjAtanPhi(spatialVector));
    }
}

/// <summary>
/// Spatially-aware view field of a sensor.
/// FOV defines full angle of frustum field of view, centered at transform. 
/// </summary>
public struct Frustum
{
    public Transform Transform;
    public ViewVector FOV;

    public Frustum(Transform myTransform, ViewVector myFOV)
    {
        Transform = myTransform;
        FOV = myFOV;
    }
}

/// <summary>
/// Cell of Occlusion grid using closest-in-grid occlusion approximation strategy.
/// </summary>
public struct OcclusionCell<T>
{
    public Vector3 closest;
    public float distance;
    public T value;
    public bool nullCell;

    public static OcclusionCell<T>[,] OcGrid(Vector2 size)
    {
        OcclusionCell<T>[,] grid = new OcclusionCell<T>[(int)size.x, (int)size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
                grid[i, j].nullCell = true;
        }
        return grid;
    }
}


/// <summary>
/// Calculates resultant PointValues when 2D Raster is projected along a Frustum at a Mesh.
/// Mesh represented by list of vertices.
/// NOTE: does not currently account for occlusion.
/// </summary>
public class Intersector : HoloToolkit.Unity.Singleton<Intersector>
{
    /// <summary>
    /// Metadata: how many vertices were in passed Frustum for last call to Intersection().
    /// </summary>
    public int CheckedVertices { get; private set; }

    /// <summary>
    /// Metadata: how many vertices were in passed Frustum for last call to Intersection().
    /// </summary>
    public int VerticesInView { get; private set; }

    /// <summary>
    /// Metadata: how many vertices were in passed Frustum and non-occluded for last call to Intersection().
    /// </summary>
    public int NonOccludedVertices { get; private set; }

    // controls for occlusion grid size calculations
    public static float ObjSize = 0.1f; // m
    public static float ObjDistance = 5f; //m

    /// pre-declarations of variables in Intersection() for memory efficiency
    private int i, j, iOc, jOc, iImg, jImg;

    // new Intersection
    /// <summary>
    /// Calculates resultant PointValues when 2D Raster (img) is projected along Frustum at Mesh (vertices).
    /// Mesh represented by list of vertices.
    /// NOTE: projection's FOV should be full FOV angles, not half-angles.
    /// </summary>
    public List<PointValue<byte>> Intersection(Frustum projection, ref List<Vector3> vertices, ref byte[,] raster)
    {
        /// setup
        List<PointValue<byte>> Results = new List<PointValue<byte>>();
        VerticesInView = 0;
        Vector2 ImgSize = new Vector2();
        ImgSize.x = raster.GetLength(0);
        ImgSize.y = raster.GetLength(1);

        Vector2 OcGridSize = GridSize(projection.FOV);
        OcclusionCell<byte>[,] ocGrid = OcclusionCell<byte>.OcGrid(OcGridSize);

        /// try each vertex
        for (i = 0; i < vertices.Count; i++)
        {
            /// calculate position vector (world space)
            Vector3 Pworld = Vector(projection.Transform.position, vertices[i]);

            /// convert position vector (world space) to position vector (Frustum space)
            Vector3 Plocal = projection.Transform.InverseTransformVector(Pworld);

            /// convert position vector (Frustum space) to view vector (Frustum space)
            ViewVector Vlocal = new ViewVector(Plocal);

            /// check if view vector is within Frustum FOV
            if (Math.Abs(Vlocal.Theta) < projection.FOV.Theta / 2.0 &&
                Math.Abs(Vlocal.Phi) < projection.FOV.Phi / 2.0)
            {
                VerticesInView++;

                /// map view vector to occlusion grid
                iOc = (int)(OcGridSize.x / 2 + OcGridSize.x * (Vlocal.Theta / projection.FOV.Theta));
                jOc = (int)(OcGridSize.y / 2 + OcGridSize.y * (Vlocal.Phi / projection.FOV.Phi));

                if (ocGrid[iOc, jOc].nullCell || Pworld.magnitude < ocGrid[iOc, jOc].distance)
                {
                    /// map view vector to Img grid
                    iImg = (int)(ImgSize.x / 2 + ImgSize.x * (Vlocal.Theta / projection.FOV.Theta));
                    jImg = (int)(ImgSize.y / 2 + ImgSize.y * (Vlocal.Phi / projection.FOV.Phi));

                    ocGrid[iOc, jOc].closest = vertices[i];
                    ocGrid[iOc, jOc].distance = Pworld.magnitude;
                    ocGrid[iOc, jOc].value = raster[iImg, jImg];
                    ocGrid[iOc, jOc].nullCell = false;
                }
            }
        }

        // add vertices in occluded grid
        for (i = 0; i < OcGridSize.x; i++)
        {
            for (j = 0; j < OcGridSize.y; j++)
                if (!ocGrid[i, j].nullCell)
                    Results.Add(new PointValue<byte>(ocGrid[i, j].closest, ocGrid[i, j].value));
        }

        CheckedVertices = vertices.Count;
        NonOccludedVertices = Results.Count;
        return Results;
    }

    /// <summary>
    /// Returns vector FROM point1 TO point2.
    /// </summary>
    public Vector3 Vector(Vector3 point1, Vector3 point2)
    {
        return point2 - point1;
    }

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    public static double RadToDeg(double rad)
    {
        return rad * (180.0 / Math.PI);
    }

    public static double DegToRad(double deg)
    {
        return deg * (Math.PI / 180.0);
    }

    /// <summary>
    /// Calculates arctan (in radians) so that resultant angle is between -pi and pi.
    /// </summary>
    public static double AdjAtanTheta(double numerator, double denominator)
    {
        if (denominator < 0 && numerator < 0) /// Q3
            return -Math.PI / 2.0 - Math.Atan(denominator / numerator);
        if (numerator > 0 && denominator < 0) /// Q4
            return Math.PI / 2.0 + Math.Atan(Math.Abs(denominator) / numerator);
        return Math.Atan(numerator / denominator);
    }

    /// <summary>
    /// Calculates arctan (in radians) always in reference to plane of numerator.
    /// </summary>
    public static double AdjAtanPhi(Vector3 spatialVector)
    {
        Vector3 proj = new Vector3(spatialVector.x, 0, spatialVector.z);
        float mag = proj.magnitude;
        return Math.Atan(spatialVector.y / mag);
    }

    /// <summary>
    /// Determines the required grid size given hard-coded parameters for occlusion approximation via closest in grid.
    /// </summary>
    public static Vector2 GridSize(ViewVector FOV)
    {
        float totalViewSizeX = 2.0f * ObjDistance * (float)Math.Tan(DegToRad(FOV.Theta) / 2.0);
        float totalViewSizeY = 2.0f * ObjDistance * (float)Math.Tan(DegToRad(FOV.Phi) / 2.0);

        Vector2 dims = new Vector2();
        dims.x = (int)Math.Round(totalViewSizeX / ObjSize);
        dims.y = (int)Math.Round(totalViewSizeY / ObjSize);
        return dims;
    }
}
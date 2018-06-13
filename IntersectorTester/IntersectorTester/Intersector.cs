/// Intersector
/// Class for calculating resultant PointValues when 2D Raster is project along a Frustum at a Mesh.
/// Content:
/// PointValue (struct)
/// ViewVector (struct)
/// Frustum (struct)
/// Intersector (class)

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
        Theta = Intersector.Instance.RadToDeg(Intersector.Instance.AdjAtanTheta(spatialVector.z, spatialVector.x));
        Phi = Intersector.Instance.RadToDeg(Intersector.Instance.AdjAtanPhi(spatialVector.z, spatialVector.y));
    }
}

/// <summary>
/// Spatially-aware view field of a sensor.
/// FOV defines full angle of frustum field of view, centered at transform. 
/// </summary>
public struct Frustum
{
    public UnityEngine.Transform Transform { get; private set; }
    public ViewVector FOV { get; private set; }

    public Frustum(UnityEngine.Transform myTransform, ViewVector myFOV)
    {
        Transform = myTransform;
        FOV = myFOV;
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
    public int VerticesInView { get; private set; }

    /// <summary>
    /// Calculates resultant PointValues when 2D Raster (img) is projected along Frustum at Mesh (vertices).
    /// Mesh represented by list of vertices.
    /// NOTE 1: does not currently account for occlusion.
    /// NOTE 2: projection's FOV should be full FOV angles, not half-angles.
    /// </summary>
    public List<PointValue<T>> Intersection<T>(Frustum projection, T[,] img, List<Vector3> vertices)
    {
        int PCi = img.GetLength(0);
        int PCj = img.GetLength(1);

        List<PointValue<T>> result = new List<PointValue<T>>();

        foreach (Vector3 vertex in vertices)
        {
            /// calculate position vector (world space)
            Vector3 Pworld = Vector(projection.Transform.position, vertex);

            /// convert position vector (world space) to position vector (Frustum space)
            Vector3 Plocal = projection.Transform.InverseTransformVector(Pworld);

            /// convert position vector (Frustum space) to view vector (Frustum space)
            ViewVector Vlocal = new ViewVector(Plocal);

            /// check if view vector is within Frustum FOV
            if (Math.Abs(Vlocal.Theta) < projection.FOV.Theta / 2.0 &&
                Math.Abs(Vlocal.Phi) < projection.FOV.Phi / 2.0)
            {
                /// map view vector to position grid
                int i = (int)(PCi / 2 + PCi * (Vlocal.Theta / projection.FOV.Theta));
                int j = (int)(PCj / 2 + PCj * (Vlocal.Phi / projection.FOV.Phi));

                /// create new PointValue
                result.Add(new PointValue<T>(vertex, img[i, j]));
            }
        }

        VerticesInView = result.Count;
        return result;
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
    public double RadToDeg(double rad)
    {
        return rad * (180.0 / Math.PI);
    }

    /// <summary>
    /// Calculates arctan (in radians) so that resultant angle is between -pi and pi.
    /// </summary>
    public double AdjAtanTheta(double denominator, double numerator)
    {
        if (denominator < 0 && numerator > 0) /// Q3
            return Math.PI / 2.0 + Math.Atan(-denominator / numerator);
        if (denominator < 0 && numerator < 0) /// Q4
            return -Math.PI / 2.0 - Math.Atan(numerator / denominator);
        return Math.Atan(denominator / numerator);
    }

    /// <summary>
    /// Calculates arctan (in radians) always in reference to plane of numerator
    /// </summary>
    public double AdjAtanPhi(double denominator, double numerator)
    {
        if (denominator < 0 && numerator > 0) /// Q2
            return Math.Atan(-denominator / numerator);
        if (denominator < 0 && numerator < 0) /// Q3
            return -Math.Atan(denominator / numerator);
        return Math.Atan(denominator / numerator);
    }
}

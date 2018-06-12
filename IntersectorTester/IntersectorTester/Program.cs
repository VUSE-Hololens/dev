/// Test file for Intersector
/// Mark Scherer, June 2018 
/// 
/// NOTE: Several testing issues faced because Unity objects cannot be created outside Unity Library. 
    /// Cannot Test several key functions ouside Unity. See comments within TestIntersector().

using System;
using UnityEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("IntegratorTester... SensorIntergrator Project.");

        // Test Intersector
        TestIntersector();

        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    // Tests Intersector class
    // Currently tests:
    // Vector (T1)
        // InverseTransformVector (T2 - not implemented)
        // ViewVector alt. constructor (T3)
        // Intersection (small scale) (T2 - not implemented)
    // Yet to test:
        // Intersection (large scale)
    static void TestIntersector()
    {
        Console.WriteLine("\nTesting Intersector...");

        // Test 1: Vector from two points
        Vector3 p1 = new Vector3(0, 0, 0);
        Vector3 p2 = new Vector3(2, 2, 2);
        Console.WriteLine("Test 1: Vector from two points: {0} to {1}", pointToStr(p1), pointToStr(p2));
        Vector3 result = Intersector.Vector(p1, p2);
        Console.WriteLine("Result: {0}\n", pointToStr(result));

        /* Cannot test Unity Library outside of Unity...
        // Test 2: InverseTransformVector
        GameObject view = new GameObject();
        view.transform.Rotate(new Vector3(45, 45, 45));
        Console.WriteLine("Test 2: InverseTransformVector... view position: {0}, view euler angles: {1}",
            pointToStr(view.transform.position), pointToStr(view.transform.eulerAngles));
        Vector3[] vectors = unitVectors();
        for (int i = 0; i < 8; i++)
            Console.WriteLine("Vector{0}: WS: {1}, LS: {2}",
                i, vectors[i], view.transform.InverseTransformVector(vectors[i]));
        */

        // Test 3: View Vector alt. constructor
        Console.WriteLine("Test 3: ViewVector creation from local position vector...");
        Vector3[] vectors = unitVectors();
        for (int i = 0; i < 8; i++)
            Console.WriteLine("Vector{0} {1}: ViewVector{0}: {2}",
                i, pointToStr(vectors[i]), viewVectorToStr(new ViewVector(vectors[i])));
        Console.WriteLine();

        /* Cannot test Unity Library outside of Unity...
        // Test 4: Intersection (small scale)
        List<Vector3> vectorList = new List<Vector3>();
        for (int i = 0; i < 8; i++)
            vectorList.Add(vectors[i]);
        GameObject view2 = new GameObject();
        ViewVector viewField = new ViewVector(180, 180);
        Frustum projection = new Frustum(view2.transform, viewField);
        int[,] raster = new int[100, 100];
        Console.WriteLine("Test4: Intersection... Raster: ({0}x{1}), Frustum FOV: ({2}x{3}). Found Intersection points...",
            raster.GetLength(0), raster.GetLength(1), viewField.Theta, viewField.Phi);
        List<PointValue<int>> intersect = Intersector.Intersection<int>(projection, raster, vectorList);
        foreach (PointValue<int> pv in intersect)
            Console.WriteLine("Point {0}", pointToStr(pv.Point));
         */
    }

    // Returns point coordinates in presentable format.
    static string pointToStr(Vector3 point)
    {
        return String.Format("({0}, {1}, {2})", point.x, point.y, point.z);
    }

    static string viewVectorToStr(ViewVector vector)
    {
        return String.Format("(Theta: {0}, Phi: {1})", vector.Theta, vector.Phi);
    }

    static float randomFloat(float min, float max, System.Random rand)
    {
        double range = max - min;
        double sample = rand.NextDouble();
        double scaled = (sample * range) + min;
        return (float)scaled;
    }

    static Vector3 randomPoint(Vector3 boundsMin, Vector3 boundsMax, System.Random rand)
    {
        return new Vector3(randomFloat(boundsMin.x, boundsMax.x, rand),
            randomFloat(boundsMin.y, boundsMax.y, rand),
            randomFloat(boundsMin.z, boundsMax.z, rand));
    }

    static Vector3[] unitVectors()
    {
        Vector3[] unitVectors = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 vec = new Vector3();
            // set x
            if (i == 0 || i == 2 || i == 4 || i == 6)
                vec.x = 1;
            else
                vec.x = -1;
            // set y
            if (i == 0 || i == 1 || i == 4 || i == 5)
                vec.y = 1;
            else
                vec.y = -1;
            // set z
            if (i == 0 || i == 1 || i == 2 || i == 3)
                vec.z = 1;
            else
                vec.z = -1;
            unitVectors[i] = vec;
        }
        return unitVectors;
    }
}
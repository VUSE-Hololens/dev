/// Test file for Octree, VoxelGridManager, Singleton
/// Mark Scherer, June 2018 

using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("DevTester... SensorIntergrator Project.");

        // Test Octree
        TestOctree();

        // Test VoxelGridManager
        TestVoxelGridManager();

        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    // Test Octree data structure.
    // Tests...
        // Constructor (T1)
        // set (shallow navigation) (structUpdate: false): previous null (T2) and non-null (T4) voxels.
        // get (shallow navigation) (T3)
        // set (shallow navigation) (structUpdate: true): splitting voxel (T5)
        // get (deep navigation) (T5)
        // set (deep naviagtion) (structUpdate: true): reassigning voxel (T6)
        // get exception throwing: getting point not contained in octree (T7)
        // grow (multi-stage) (T8)
        // edge condition: point on component boundary (T9)
        // large-scale set/get (T10)
    // Note: Deep navigation: 2+ containers before Voxel; Shallow navigation: <= 1 container before Voxel. 
    static void TestOctree()
    {
        Console.WriteLine("Testing Octree...\n");
        
        // Test 1: Constructor
        Vector3 startPoint = new Vector3(0.2f, 0.2f, 0.2f);
        float minSize = 0.2f; // m
        float defaultSize = 1.0f; // m
        Octree<int> octree = new Octree<int>(startPoint, minSize, defaultSize);
        Console.WriteLine("Test 1: Construction... empty Octree created...");
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 2: Set previously-null Voxel (no structUpdate) (shallow navigation)
        int value = 100;
        octree.set(startPoint, value, false);
        Console.WriteLine("Test 2: Set null (shallow)...Previously null voxel ({0}) set with value ({1})," +
            "no struct update...", pointToStr(startPoint), value);
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 3: Get previously-set voxel value
        int returnedValue = octree.get(startPoint);
        Console.WriteLine("Test 3: Get (shallow)... getting value from voxel ({0}):", pointToStr(startPoint));
        Console.WriteLine("Returned Value: {0}", returnedValue);
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 4: Set non-null Voxel (no structUpdate)
        int newValue = 50;
        octree.set(startPoint, newValue, false);
        Console.WriteLine("Test 4: Set non-null  (shallow)... previously non-null Voxel ({0})" +
            " set with new value ({1}), no struct update...",
            pointToStr(startPoint), newValue);
        Console.WriteLine("Getting Voxel value ({0})...", pointToStr(startPoint));
        Console.WriteLine("Value: {0}", octree.get(startPoint));
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 5: Cause Voxel split (set non-null voxel, yes structUpdate)
        Vector3 point2 = new Vector3(startPoint.x, startPoint.y, startPoint.z + 0.2f);
        int value2 = 150;
        octree.set(point2, value2, true);
        Console.WriteLine("Test 5: Split... setting new point ({0}) in previous points Voxel ({1})" +
            " to {2}, yes structUpdate...", pointToStr(point2), pointToStr(startPoint), value2);
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(startPoint), octree.get(startPoint));
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point2), octree.get(point2));
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 6: Cause Voxel reassignment (set non-null voxel, structUpdate yes, Voxel too small to split)
        int value3 = 250;
        Vector3 point3 = new Vector3(startPoint.x, startPoint.y, startPoint.z - 0.1f);
        octree.set(point3, value3, true);
        Console.WriteLine("Test 6: Reassignment... setting setting new point ({0}) in previous " +
            "point's Voxel ({1}) to {2}, yes structUpdate but too small to split (< {3})...", 
            pointToStr(point3), pointToStr(startPoint), value3, octree.minSize);
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(startPoint), octree.get(startPoint));
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point2), octree.get(point2));
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point3), octree.get(point3));
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 7: Cause get outofbounds exception
        Vector3 point4 = new Vector3(startPoint.x, startPoint.y, startPoint.z + 2.0f);
        Console.WriteLine("Test 7: Get exception throwing... getting point ({0}) not contained in root bounds " +
            "({1} to {2})...", pointToStr(point4), pointToStr(octree.root.min), pointToStr(octree.root.max));
        try
        { octree.get(point4); }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("Exception properly thrown.\n");
        }

        // Test 8: Cause grow (multi-stage)
        int value4 = 350;
        Console.WriteLine("Test 8: Grow... setting point ({0}) not contained in root bounds ({1} to {2}) to {3}...", 
            pointToStr(point4), pointToStr(octree.root.min), pointToStr(octree.root.max), value4);
        octree.set(point4, value4, true);
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point2), octree.get(point2));
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point3), octree.get(point3));
        Console.WriteLine("New value ({0}) : ({1})", pointToStr(point4), octree.get(point4));
        Console.WriteLine(metadata<int>(octree) + "\n");

        // Test 9: Edge case: point on component boundary
        Octree<int> octree2 = new Octree<int>(startPoint, 0.2f, 1f);
        Vector3 boundaryPoint = new Vector3(0.5f, 0.5f, 0.5f);
        int boundaryValue = 100;
        Console.WriteLine("Test 9: Edge condition: point on component boundary...");
        Console.WriteLine("Setting {0} to {1}, root bounds {2} to {3}...", pointToStr(boundaryPoint), 
            boundaryValue, pointToStr(octree2.root.min), pointToStr(octree2.root.max));
        octree2.set(boundaryPoint, boundaryValue, false);
        Console.WriteLine("Getting {0}... value: {1}", pointToStr(boundaryPoint), octree2.get(boundaryPoint));
        Console.WriteLine(metadata<int>(octree2) + "\n");

        // Test 10: Large scale set/get
        Vector3 boundsMin = new Vector3(0, 0, 0);
        Vector3 boundsMax = new Vector3(5, 5, 5);
        defaultSize = 1.0f;
        minSize = 0.01f;
        int iterations = 100000;
        System.Random rand = new System.Random();
        startPoint = randomPoint(boundsMin, boundsMax, rand);
        Octree<int> octree3 = new Octree<int>(startPoint, minSize, defaultSize);
        Console.WriteLine("Test 10: Large scale set/get... conducting {0} iterations of random points " +
            "between {1} and {2}... ", iterations, pointToStr(boundsMin), pointToStr(boundsMax));
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            Vector3 point = randomPoint(boundsMin, boundsMax, rand);
            value = rand.Next(0, 255);
            octree3.set(point, value, true);
            returnedValue = octree3.get(point);
            if (returnedValue != value)
                Console.WriteLine("FAIL: Set/get {0}. Expected: {1}, got: {2}", pointToStr(point), value, returnedValue);
        }
        stopWatch.Stop();
        long millisecs = stopWatch.ElapsedMilliseconds;
        float perOp = 1000f * millisecs / (2.0f * iterations);
        Console.WriteLine("Avg time per op (us): {0} (minSize: {1}, defaultSize: {2})",
            perOp, octree3.minSize, octree3.defaultSize);
        Console.WriteLine("Elasped time (ms): {0}",  millisecs);
        Console.WriteLine("Final octree size: {0} ({1} to {2})", octree3.root.size(), 
            pointToStr(octree3.root.min), pointToStr(octree3.root.max));
        Console.WriteLine(metadata<int>(octree3) + "\n");
    }

    // Test VoxelGridManager
    // Tests...
        // Instance (T1)
        // About (T1)
        // Set (structUpdate: true) (T2)
        // Set (structUpdate: false) (T3)
    static void TestVoxelGridManager()
    {
        Console.WriteLine("\nTesting VoxelGridManager...\n");

        // Test 1: Constructor, About
        Console.WriteLine("Test 1: Creating VoxelGridManager...");
        VoxelGridManager manager = VoxelGridManager.Instance;
        Console.WriteLine(about(manager.about()));
        Console.WriteLine("Re-accessing VoxleGridManager Instance...");
        Console.WriteLine(about(manager.about()));
        Console.WriteLine();

        // Test 2: Set (structUpdate: true)
        Vector3 boundsMin = new Vector3(0, 0, 0);
        Vector3 boundsMax = new Vector3(5, 5, 5);
        int iterations = 1000;
        Console.WriteLine("Test 2: set... conducting {0} iterations of random points " + "between {1} and {2}... " +
            "structUpdate: {3}", iterations, pointToStr(boundsMin), pointToStr(boundsMax), 
            VoxelGridManager.Instance.updateStruct);
        System.Random rand = new System.Random();
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            Vector3 point = randomPoint(boundsMin, boundsMax, rand);
            byte value = (byte)rand.Next(0, 255);
            List<Vector3> points = new List<Vector3>();
            List<byte> values = new List<byte>();
            points.Add(point);
            values.Add(value);
            VoxelGridManager.Instance.set(points, values);
        }
        stopWatch.Stop();
        long millisecs = stopWatch.ElapsedMilliseconds;
        float perOp = 1000f * millisecs / (2.0f * iterations);
        Console.WriteLine("Avg time per op (us): {0}", perOp);
        Console.WriteLine("Elasped time (ms): {0}", millisecs);
        Console.WriteLine(about(manager.about()) + "\n");

        // Test 3: Set (structUpdate: false)
        VoxelGridManager.Instance.updateStruct = false;
        boundsMin = new Vector3(0, 0, 0);
        boundsMax = new Vector3(5, 5, 5);
        iterations = 1000;
        Console.WriteLine("Test 2: set... conducting {0} iterations of random points " + "between {1} and {2}... " +
            "structUpdate: {3}", iterations, pointToStr(boundsMin), pointToStr(boundsMax),
            VoxelGridManager.Instance.updateStruct);
        stopWatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            Vector3 point = randomPoint(boundsMin, boundsMax, rand);
            byte value = (byte)rand.Next(0, 255);
            List<Vector3> points = new List<Vector3>();
            List<byte> values = new List<byte>();
            points.Add(point);
            values.Add(value);
            VoxelGridManager.Instance.set(points, values);
        }
        stopWatch.Stop();
        millisecs = stopWatch.ElapsedMilliseconds;
        perOp = 1000f * millisecs / (2.0f * iterations);
        Console.WriteLine("Avg time per op (us): {0}", perOp);
        Console.WriteLine("Elasped time (ms): {0}", millisecs);
        Console.WriteLine(about(manager.about()) + "\n");
    }

    // Returns octree metadata in presentable format.
    static string metadata<T>(Octree<T> octree)
    {
        return String.Format("Octree metadata: Components: {0}, Voxels: {1} ({2} non-null), " +
            "Volume: {3} ({4} non-null)", octree.numComponents, octree.numVoxels, octree.numNonNullVoxels, 
            Math.Round(octree.volume, 2), Math.Round(octree.nonNullVolume, 2));
    }

    // Return VoxelGridManager's about return in presentable format
    static string about(Metadata info)
    {
        return String.Format("VoxelGridManager about: Components: {0}, Voxels: {1} ({2} non-null), " +
            "Volume: {3} ({4} non-null), Bounds: ({5} to {6}), Last Updated: {7}", 
            info.components, info.voxels, info.nonNullVoxels, 
            Math.Round(info.volume, 2), Math.Round(info.nonNullVolume, 2), 
            pointToStr(info.min), pointToStr(info.max), info.lastUpdated.ToString("hh:mm:ss.fff tt"));
    }

    // Returns point coordinates in presentable format.
    static string pointToStr(Vector3 point)
    {
        return String.Format("({0}, {1}, {2})", point.x, point.y, point.z);
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
}
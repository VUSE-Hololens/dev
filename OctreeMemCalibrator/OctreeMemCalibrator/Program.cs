/// Octree Memory Calibator
/// Used to estimate memory size of octree.
/// Mark Scherer, June 2018

/// Note: Not sure of a good C# equivalent of typedef, so for now have to manually change all type instances 
    /// when working with octree. Currently set to byte. All instances contained in Mem().

using System;
using UnityEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Octree Memory Calibrator... SensorIntergrator Project.\n");
        Mem();

        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static void Mem()
    {
        Vector3 startPoint = new Vector3();
        float minSize = 0.01f;
        float defaultSize = 1.0f;
        Vector3 boundsMin = new Vector3(-5, -5, -5);
        Vector3 boundsMax = new Vector3(5, 5, 5);
        int iterations = 50000;
        int[] checkpoints = new int[] { 10000, 20000, 30000, 40000, 50000 };
        System.Random rand = new System.Random();

        Console.WriteLine("Created Octree... bounds: {0} to {1}, minSize: {2}, defaultSize: {3}, value type: {4}",
            pointToStr(boundsMin), pointToStr(boundsMax), minSize, defaultSize, typeof(byte));

        long baseMem = GC.GetTotalMemory(false);

        Octree<byte> octree = new Octree<byte>(startPoint, minSize, defaultSize);

        for (int i = 1; i <= iterations; i++)
        {
            octree.set(randomPoint(boundsMin, boundsMax, rand), default(byte), true);

            for(int j = 0; j < checkpoints.Length; j++)
            {
                long memUse = GC.GetTotalMemory(false) - baseMem;
                if (i == checkpoints[j])
                    Console.WriteLine("Checkpoint... Points added: {0}, Mem: {1} (Octree estimate: {2}), " +
                        "Components: {3} ({4} / Component) Voxels: {5} ({6} / Voxel), " +
                        "Non-null Voxels: {7} ({8} / Non-null Voxel)", 
                        i, MemToStr(memUse), MemToStr(octree.memSize), 
                        octree.numComponents, MemToStr(memUse / octree.numComponents), 
                        octree.numVoxels, MemToStr(memUse / octree.numVoxels), 
                        octree.numNonNullVoxels, MemToStr(memUse / octree.numNonNullVoxels));
            }
        }
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

    /// <summary>
    /// Returns presentable string of memory size.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    static string MemToStr(long bytes)
    {
        if (bytes < 1000) // less than 1 kB
            return String.Format("{0} B", bytes);
        if (bytes < 1000 * 1000) // less than 1 MB
            return String.Format("{0} kB", bytes / 1000);
        if (bytes < 1000 * 1000 * 1000) // less than 1 GB
            return String.Format("{0} MB", bytes / (1000 * 1000));
        return String.Format("{0} GB", bytes / (1000 * 1000 * 1000));
    }

    // Returns point coordinates in presentable format.
    static string pointToStr(Vector3 point)
    {
        return String.Format("({0}, {1}, {2})",
            Math.Round(point.x, 2), Math.Round(point.y, 2), Math.Round(point.z, 2));
    }
}
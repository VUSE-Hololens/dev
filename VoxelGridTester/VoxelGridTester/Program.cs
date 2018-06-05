using System;
using UnityEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("DevTester... SensorIntergrator Project.");

        // Test Octree
        TestOctree();

        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }


    static void TestOctree()
    {
        // Test 1: Constructor
        Vector3 startPoint = new Vector3(0, 0, 0);
        float minSize = 0.1f; // m
        float defaultSize = 1.0f; // m
        Octree<int> octree = new Octree<int>(startPoint, minSize, defaultSize);
    }
}
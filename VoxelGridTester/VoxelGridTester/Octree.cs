/// Octree
/// Data structure for sparse, bounded 3D data.
/// Mark Scherer, June 2018

/// NOTE 1: Due to inexperience with C# and its limitations:
    /// a) Possessive, not linked tree structure. Because C# doesn't allow pointer to user-defined classes or
        /// reference reassignment, nodes 'own' subtree beneath them, not just are linked.
        /// Consequences:
            /// i) Recursive operation structure: operations must be pushed thru root and performed at node of
                /// interest instead of centrally with link to node of interest.
            /// ii): Copy-operate-swap not unlink-operate-relink. Any changes require replacing entire subtree.
    /// b) Private variables, public mutators: where C++ would use friend classes/methods, must use completely public
        /// variable or mutator. Current structure provides marginally better saftey than pure public variable.
    /// c) Central parameters recorded in each node: To avoid global variables, minSize is recorded in each Component
        /// instead of once in Octree.

/// NOTE 3: Untested (6/6/2018)


using System;
using UnityEngine;

/// <summary>
/// Used to return metadata changes from Component classes up to Octree class when conducting Octree operations.
/// <summary>
public struct metadataChange
{
    public int components, voxels, nonNullVoxels;
    public double volume, nonNullVolume;

    public metadataChange(int dComponents=0, int dVoxels=0, int dNonNullVoxels=0,
        double dVolume=0, double dNonNullVolume=0)
    {
        components = dComponents;
        voxels = dVoxels;
        nonNullVoxels = dNonNullVoxels;
        volume = dVolume;
        nonNullVolume = dNonNullVolume;
    }

    public static metadataChange operator +(metadataChange lhs, metadataChange rhs)
    {
        return new metadataChange(lhs.components + rhs.components, lhs.voxels + rhs.voxels,
            lhs.nonNullVoxels + rhs.nonNullVoxels, lhs.volume + rhs.volume, lhs.nonNullVolume + rhs.nonNullVolume);
    }
}

/// <summary>
/// Templatized data structure for sparse, bounded 3D data.
/// NOTE: child numbering scheme: X, Y, then Z order, higher values first. 0-based.
/// <summary>
public class Octree<T>
{
    /// <summary>
    /// Pointer to root Octree component.
    /// <summary>
    private OctreeContainer<T> root;

    /// <summary>
    /// Component size controls.
    /// NOTE: defaultSize only used in ctor.
    /// <summary>
    public float minSize { get; private set; }
    public float defaultSize { get; private set; }

    /// <summary>
    /// Octree metadata.
    /// <summary>
    public int numComponents { get; private set; }
    public int numVoxels { get; private set; }
    public int numNonNullVoxels { get; private set; }
    public double volume { get; private set; }
    public double nonNullVolume { get; private set; }

    /// <summary>
    /// Constructor. Creates null-value Voxel of defaultSize around point as root of Octree.
    /// <summary>
    public Octree(Vector3 point, float myMinSize, float myDefaultSize)
    {
        minSize = myMinSize;
        defaultSize = myDefaultSize;

        float buffer1 = 0.25f * defaultSize;
        float buffer2 = defaultSize - buffer1;
        Vector3 min = new Vector3(point.x - buffer1, point.y - buffer1, point.z - buffer1);
        Vector3 max = new Vector3(point.x + buffer2, point.y + buffer2, point.z + buffer2);
        root = new OctreeContainer<T>(min, max, myMinSize);

        numComponents = 9;
        numVoxels = 8;
        numNonNullVoxels = 0;
        volume = root.volume();
        nonNullVolume = 0;
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// Throws ArgumentOutOfRangeException if Octree does not contain point.
    /// <summary>
    public T get(Vector3 point)
    {
        return root.get(point);
    }

    /// <summary>
    /// Sets value at Voxel containing point.
    /// If updateStruct, re-grids contained space so no voxel contains two points.
    /// If not, replaces value in vox.
    /// If Octree does not contain point, will grow until it does.
    /// <summary>
    public void set(Vector3 point, T value, bool updateStruct)
    {
        if (!root.contains(point))
            grow(point);
        metadataChange change = root.set(point, value, updateStruct);
        updateMetadata(change);
    }

    /// <summary>
    /// Returns total bounds of Octree.
    /// <summary>
    public (Vector3 min, Vector3 max) bounds()
    {
        return (root.min, root.max);
    }

    /// <summary>
    /// Grows Octree so current root is made child of new, larger root.
    /// Grows in direction of passed point, until point is within root.
    /// <summary>
    private void grow(Vector3 point)
    {
        while (!root.contains(point))
        {
            int childNum = optimalChildNum(root, point); /// find growth direction
            var newRootBounds = root.parentBounds(childNum);
            OctreeContainer<T> newRoot = new OctreeContainer<T>(newRootBounds.Item1, newRootBounds.Item2, minSize);
            newRoot.setChild(childNum, root);
            root = newRoot;
            
            numComponents += 8;
            numVoxels += 7;
            volume = root.volume();
        }
    }

    /// <summary>
    /// ID's which child to make component to grow Octree towards point.
    /// NOTE: returns -1 if component contains point.
    /// If confused, probably easiest to chart input-outputs.
    /// <summary>
    private int optimalChildNum(OctreeComponent<T> component, Vector3 point)
    {
        if (component.contains(point))
            return -1;
        if (point.x <= component.min.x)
        {
            if (point.y <= component.min.y)
            {
                if (point.z <= component.min.z)
                    return 1; /// (<,<,<)
                else
                    return 5; /// (<,<,>)
            }
            else
            {
                if (point.z <= component.min.z)
                    return 3; /// (<,>,<)
                else
                    return 7; /// (<,>,>)
            }
        }
        else
        {
            if (point.y <= component.min.y)
            {
                if (point.z <= component.min.z)
                    return 0; /// (>,<,<)
                else
                    return 4; /// (>,<,>)
            }
            else
            {
                if (point.z <= component.min.z)
                    return 2; /// (>,>,<)
                else
                    return 6; /// (>,>,>)
            }
        }
    }

    /// <summary>
    /// updates metadata given MetadataChange from Component classes.
    /// <summary>
    private void updateMetadata(metadataChange change)
    {
        numComponents += change.components;
        numVoxels += change.voxels;
        numNonNullVoxels += change.nonNullVoxels;
        volume += change.volume;
        nonNullVolume += change.nonNullVolume;
    }
}


/// <summary>
/// Abstract class for Octree components.
/// <summary>
public abstract class OctreeComponent<T>
{
    /// <summary>
    /// Coordinate bounds for component.
    /// <summary>
    public Vector3 min { get; private set; }
    public Vector3 max { get; private set; }

    /// <summary>
    /// Minimum allowed size of a voxel.
    /// Stored in each Component to prevent need for global variable.
    /// <summary>
    public float minSize { get; private set; }

    /// <summary>
    /// Constructor.
    /// <summary>
    public OctreeComponent(Vector3 myMin, Vector3 myMax, float myMinSize)
    {
        min = myMin;
        max = myMax;
        minSize = myMinSize;
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// <summary>
    public abstract T get(Vector3 pointToGet);

    /// <summary>
    /// Accessor for Component's size.
    /// <summary>
    public float size()
    { return max.x - min.x; }

    /// <summary>
    /// Accessor for Component's volume.
    /// <summary>
    public double volume()
    { return Math.Pow(size(), 3f); }

    /// <summary>
    /// Checks if point is contained (exclusively) within Component bounds
    /// <summary>
    public bool contains(Vector3 point)
    {
        if (point.x <= min.x || point.x >= max.x)
            return false;
        if (point.y <= min.y || point.y >= max.y)
            return false;
        if (point.z <= min.z || point.z >= max.z)
            return false;
        return true;
    }

    /// <summary>
    /// Returns hypothetical parent component's bounds IF this component was the given child number.
    /// Used for creating parent in Octree grow.
    /// <summary>
    public (Vector3 min, Vector3 max) parentBounds(int childNum)
    {
        float childSize = size();
        Vector3 parentMin = new Vector3();
        Vector3 parentMax = new Vector3();
        /// find x
        if (childNum == 0 || childNum == 2 || childNum == 4 || childNum == 6)
        {
            parentMin.x = min.x;
            parentMax.x = min.x + childSize;
        }
        else
        {
            parentMin.x = min.x + childSize;
            parentMax.x = max.x;
        }
        /// find y
        if (childNum == 2 || childNum == 3 || childNum == 6 || childNum == 7)
        {
            parentMin.y = min.y;
            parentMax.y = min.y + childSize;
        }
        else
        {
            parentMin.y = min.y + childSize;
            parentMax.y = max.y;
        }
        /// find z
        if (childNum == 4 || childNum == 5 || childNum == 6 || childNum == 7)
        {
            parentMin.z = min.z;
            parentMax.z = min.z + childSize;
        }
        else
        {
            parentMin.z = min.z + childSize;
            parentMax.z = max.z;
        }
        return (parentMin, parentMax);
    }
}


/// <summary>
/// Non-leaf component of Octree, i.e. contains smaller subdivision components.
/// <summary>
public class OctreeContainer<T> : OctreeComponent<T>
{
    /// <summary>
    /// Array of pointers to children components.
    /// <summary>
    public OctreeComponent<T>[] children { get; private set; }

    /// <summary>
    /// Constructor.
    /// Sets all Container children to new Voxels.
    /// <summary>
    public OctreeContainer(Vector3 myMin, Vector3 myMax, float myMinSize)
        : base(myMin, myMax, myMinSize)
    {
        for (int i = 0; i < 8; i++)
        {
            var voxBounds = childBounds(i);
            children[i] = new Voxel<T>(voxBounds.Item1, voxBounds.Item2, myMinSize);
        }
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// If Container does not contain point, throws ArgumentOutOfRangeException.
    /// <summary>
    public override T get(Vector3 pointToGet)
    {
        if (!contains(pointToGet))
            throw new ArgumentOutOfRangeException("pointToGet", "not contained in Container.");
        int childNum = whichChild(pointToGet);
        return children[childNum].get(pointToGet);
    }

    /// <summary>
    /// Sets value at Voxel containing point.
    /// If updateStruct, re-grids contained space so no voxel contains two points.
    /// If not, replaces value in vox.
    /// If Container does not contain newPoint, throws ArgumentOutOfRangeException.
    /// Returns metadataChange object specifying changes made.
    /// <summary>
    public metadataChange set(Vector3 point, T value, bool updateStruct)
    {
        if (!contains(point))
            throw new ArgumentOutOfRangeException("point", "not contained in Container.");
        int childNum = whichChild(point);
        if (children[childNum].GetType() == typeof(OctreeContainer<T>))
            return ((OctreeContainer<T>)children[childNum]).set(point, value, updateStruct);
        else
        {
            if (!updateStruct || ((Voxel<T>)children[childNum]).point == null ||
                children[childNum].size() < 2f * minSize)
            {
                /// reassign voxel data
                return ((Voxel<T>)children[childNum]).set(point, value);
            } else
            {
                /// split, try set again
                var splitResult = ((Voxel<T>)children[childNum]).split();
                children[childNum] = splitResult.Item1;
                metadataChange change1 = splitResult.Item2;
                metadataChange change2 = ((OctreeContainer<T>)children[childNum]).set(point, value, updateStruct);
                return change1 + change2;
            }
        }
    }

    /// <summary>
    /// Mutator for a child.
    /// Used by Octree in grow, Voxel in split.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
        /// Really wish C# allowed friend classes.
    /// <summary>
    public void setChild(int childNum, OctreeComponent<T> newChild)
    {
        children[childNum] = newChild;
    }

    /// <summary>
    /// Returns hypothetical child component's bounds.
    /// Note: works if actual child is assigned to null, i.e. used in constructor.
    /// Used publicly by Voxel when splitting.
    /// <summary>
    public (Vector3, Vector3) childBounds(int childNum)
    {
        float childSize = size() / 2.0f;
        Vector3 childMin = new Vector3();
        Vector3 childMax = new Vector3();
        /// find x
        if (childNum == 0 || childNum == 2 || childNum == 4 || childNum == 6)
        {
            childMin.x = min.x;
            childMax.x = min.x + childSize;
        }
        else
        {
            childMin.x = min.x + childSize;
            childMax.x = max.x;
        }
        /// find y
        if (childNum == 2 || childNum == 3 || childNum == 6 || childNum == 7)
        {
            childMin.y = min.y;
            childMax.y = min.y + childSize;
        }
        else
        {
            childMin.y = min.y + childSize;
            childMax.y = max.y;
        }
        /// find z
        if (childNum == 4 || childNum == 5 || childNum == 6 || childNum == 7)
        {
            childMin.z = min.z;
            childMax.z = min.z + childSize;
        }
        else
        {
            childMin.z = min.z + childSize;
            childMax.z = max.z;
        }
        return (childMin, childMax);
    }

    /// <summary>
    /// Returns child number containiner point.
    /// Note: if container does not contain point, throws ArgumentOutOfRangeException.
    /// Used publicly by Voxel when splitting.
    /// <summary>
    public int whichChild(Vector3 point)
    {
        if (!contains(point))
            throw new ArgumentOutOfRangeException("point", "point not contained in Container.");
        for (int i = 0; i < 8; i++)
        {
            if (children[i].contains(point))
                return i;
        }
        return -1; // should not ever be called.
    }
}


/// <summary>
/// Leaf component of Octree, i.e. does not contain smaller subdivision components.
/// <summary>
public class Voxel<T> : OctreeComponent<T>
{
    /// <summary>
    /// Value stored in Octree.
    /// <summary>
    public T value { get; private set; }

    /// <summary>
    /// Actual coordinates of value assigned to Voxel (will be within Voxel bounds). 
    /// <summary>
    public Vector3 point { get; private set; }

    /// <summary>
    /// Constructor.
    /// <summary>
    public Voxel(Vector3 myMin, Vector3 myMax, float myMinSize, 
        Vector3 myPoint = new Vector3(), T myValue = default(T)) : base(myMin, myMax, myMinSize)
    {
        point = myPoint;
        value = myValue;
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// If Voxel does not contain point, throws ArgumentOutOfRangeException.
    /// <summary>
    public override T get(Vector3 pointToGet)
    {
        if (!contains(pointToGet))
            throw new ArgumentOutOfRangeException("pointToGet", "not contained in Container.");
        return value;
    }

    /// <summary>
    /// Mutator for Voxel data.
    /// If Voxel does not contain point, throws ArgumentOutOfRangeException.
    /// Used by OctreeContainer set.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
        /// Really wish C# allowed friend classes.
    /// <summary>
    public metadataChange set(Vector3 newPoint, T newValue)
    {
        if (!contains(newPoint))
            throw new ArgumentOutOfRangeException("newPoint", "not contained in Voxel.");
        T oldValue = value;
        point = newPoint;
        value = newValue;
        if (oldValue == null)
            return new metadataChange(dNonNullVoxels: 1, dNonNullVolume: volume());
        return new metadataChange();
    }

    /// <summary>
    /// Splits Voxel into 8 smaller Voxels, returns new Octree Container.
    /// <summary>
    public (OctreeContainer<T>, metadataChange) split()
    {
        OctreeContainer<T> replacement = new OctreeContainer<T>(min, max, minSize);

        /// add original point to new OctreeContainer
        int childNum = replacement.whichChild(point);
        var voxBounds = replacement.childBounds(childNum);
        replacement.setChild(childNum, new Voxel<T>(voxBounds.Item1, voxBounds.Item2, minSize, point, value));

        return (replacement,
            new metadataChange(dComponents:8, dVoxels:7, dNonNullVolume:-0.875 * replacement.volume()));
    }
}
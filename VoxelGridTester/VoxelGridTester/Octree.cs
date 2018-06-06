/// Octree
/// Data structure for sparse, bounded 3D data.
/// Mark Scherer, June 2018

/// NOTE 1: Due to inexperience with C# and its limitations:
    /// 1. Possessive, not linked tree structure. Because C# doesn't allow pointer to user defined classes or
        /// reference reassignment, nodes 'own' subtree beneath them, not just are linked.
        /// Consequences:
            /// a) Recursive operation structure: operations must be pushed thru root and performed at node of
                /// interest instead of centrally with link to node of interest.
            /// b): Copy-operate-swap not unlink-operate-relink. Any changes require replacing entire subtree.
    /// 2. Private variables, public mutators: where C++ would use friend classes/methods, must use completely public
        /// variable or mutator. Current structure provides marginal better saftey than pure public variable.
    /// 3. Central parameters recorded in each node: To avoid global variables, minSize, etc. is recorded in each leaf
        /// instead of once in Octree. In C++, would give nodes pointer to Octree instance they are apart of and record
        /// once centrally. Might use global variables and make Octree singleton.

/// NOTE 2: Octree metadata does not currently work.

/// NOTE 3: Untested (6/6/2018)

/// Think about: combining set and update to one, with bool control to decide if update functionality should
    /// be run. Would allow each vertex to navigate tree once, not twice. 

using System;
using UnityEngine;

/// <summary>
/// Templatized data structure for sparse, bounded 3D data.
/// NOTE: child numbering scheme: X, Y, then Z order, higher values first. 0-based.
/// <summary>
public class Octree<T>
{
    /// <summary>
    /// Pointer to root Octree component.
    /// <summary>
    private OctreeComponent<T> root;

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

    /// <summary>
    /// Constructor. Creates null-value Voxel of defaultSize around point as root of Octree.
    /// <summary>
    public Octree(Vector3 point, float myMinSize, float myDefaultSize)
    {
        minSize = myMinSize;
        defaultSize = myDefaultSize;

        float buffer = defaultSize / 2.0f;
        Vector3 min = new Vector3(point.x - buffer, point.y - buffer, point.z - buffer);
        Vector3 max = new Vector3(point.x + buffer, point.y + buffer, point.z + buffer);
        Voxel<T> root = new Voxel<T>(min, max, minSize, point);

        numComponents = 1;
        numVoxels = 1;
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
    /// If root does not contain point, Octree resized so it does.
    /// <summary>
    public void set(Vector3 point, T value)
    {
        while (!root.contains(point))
            grow(point);
        root.set(point, value);
    }

    /// <summary>
    /// Updates grid structure given new point to be included.
    /// <summary>
    public void resize(Vector3 point)
    {
        if (!root.contains(point))
        {
            /// grow tree
            grow(point);
            resize(point);
        } else
        {
            if (root.GetType() == typeof(OctreeContainer<T>))
                ((OctreeContainer<T>)root).updateContainer(point);
            if (root.GetType() == typeof(Voxel<T>))
                root = ((Voxel<T>)root).updateVoxel(point);
        }
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
    /// Grows in direction of passed point.
    /// <summary>
    private void grow(Vector3 point)
    {
        while (!root.contains(point))
        {
            /// find growth direction
            int childNum = optimalChildNum(root, point);
            var newRootBounds = root.parentBounds(childNum);
            OctreeContainer<T> newRoot = new OctreeContainer<T>(newRootBounds.Item1, newRootBounds.Item2, minSize);
            newRoot.setChild(childNum, root);
            root = newRoot;
            numComponents += 8;
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
    /// Constructor.
    /// <summary>
    public OctreeComponent(Vector3 myMin, Vector3 myMax)
    {
        min = myMin;
        max = myMax;
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// <summary>
    public abstract T get(Vector3 pointToGet);

    /// <summary>
    /// Sets value at Voxel containing point.
    /// <summary>
    public abstract void set(Vector3 newPoint, T newValue);

    /// <summary>
    /// Accessor for Component's size.
    /// <summary>
    public float size()
    { return max.x - min.x; }

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
    /// Used for creating parent when growing Octree.
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
    public OctreeContainer(Vector3 myMin, Vector3 myMax, float voxelMinSize)
        : base(myMin, myMax)
    {
        for (int i = 0; i < 8; i++)
        {
            var voxBounds = childBounds(i);
            children[i] = new Voxel<T>(voxBounds.Item1, voxBounds.Item2, voxelMinSize);
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
    /// Sets value at Voxel containing newPoint.
    /// If Container does not contain newPoint, throws ArgumentOutOfRangeException.
    /// <summary>
    public override void set(Vector3 newPoint, T newValue)
    {
        if (!contains(newPoint))
            throw new ArgumentOutOfRangeException("newPoint", "not contained in Container.");
        int childNum = whichChild(newPoint);
        children[childNum].set(newPoint, newValue);
    }

    /// <summary>
    /// Updates octree grid structure to handle newPoint
    /// If Container does not contain newPoint, throws ArgumentOutOfRangeException.
    /// Created separate updateContainer (no return) and updateVoxel (OctreeComponent return) instead of
        /// abstract update (OctreeComponent return) to avoid unecessary copy-replace at higher tree levels.
    /// <summary>
    public void updateContainer(Vector3 newPoint)
    {
        if (!contains(newPoint))
            throw new ArgumentOutOfRangeException("newPoint", "not contained in Container.");
        int childNum = whichChild(newPoint);
        if (children[childNum].GetType() == typeof(OctreeContainer<T>))
        {
            /// navigate down again
            ((OctreeContainer<T>)children[childNum]).updateContainer(newPoint);
        } else
        {
            /// reached voxel
            children[childNum] = ((Voxel<T>)children[childNum]).updateVoxel(newPoint);
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
    /// Used by Voxel when splitting.
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
    /// Used by Voxel when splitting.
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
    /// Minimum allowed size of a voxel.
    /// Stored in each voxel to prevent need for global variable.
    /// <summary>
    public float minSize { get; private set; }

    /// <summary>
    /// Constructor.
    /// <summary>
    public Voxel(Vector3 myMin, Vector3 myMax, float myMinSize, 
        Vector3 myPoint = new Vector3(), T myValue = default(T)) : base(myMin, myMax)
    {
        minSize = myMinSize;
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
    /// Sets value at Voxel containing point.
    /// If Voxel does not contain point, throws ArgumentOutOfRangeException.
    /// <summary>
    public override void set(Vector3 newPoint, T newValue)
    {
        if (!contains(newPoint))
            throw new ArgumentOutOfRangeException("newPoint", "not contained in Voxel.");
        point = newPoint;
        value = newValue;
    }

    /// <summary>
    /// Sets, resets or splits voxel to handle newPoint as appropriate.
    /// Returns updated copy of self.
    /// Created separate updateContainer (no return) and updateVoxel (OctreeComponent return) instead of
        /// abstract update (OctreeComponent return) to avoid unecessary copy-replace at higher tree levels.
    /// <summary>
    public OctreeComponent<T> updateVoxel(Vector3 newPoint)
    {
        if (point == null || size() < minSize / 2.0f || point == newPoint)
        {
            /// voxel vacant or too small to split, or direct repeat. Reset to new point.
            return new Voxel<T>(min, max, minSize, newPoint);
        } else
        {
            /// split Voxel
            return split(newPoint);
        }
    }

    /// <summary>
    /// Splits Voxel into 8 smaller Voxels, converts to Octree Container.
    /// Recursive, terminates when current point and newPoint are in difference voxels or minSize reached.
    /// <summary>
    private OctreeContainer<T> split(Vector3 newPoint)
    {
        OctreeContainer<T> replacement = new OctreeContainer<T>(min, max, minSize);

        /// add original point to new OctreeContainer
        int childNumOrigPoint = replacement.whichChild(point);
        var voxBounds = replacement.childBounds(childNumOrigPoint);
        replacement.setChild(childNumOrigPoint, new Voxel<T>(voxBounds.Item1, voxBounds.Item2, minSize, point));

        /// add newPoint in new OctreeContainer
        int childNumNewPoint = replacement.whichChild(newPoint);
        OctreeComponent<T> updatedVox = ((Voxel<T>)replacement.children[childNumNewPoint]).updateVoxel(newPoint);
        replacement.setChild(childNumNewPoint, updatedVox);

        return replacement;
    }
}
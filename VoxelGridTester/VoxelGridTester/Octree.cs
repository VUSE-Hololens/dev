/// Octree
/// Data structure for sparse, bounded 3D data.
/// Mark Scherer, June 2018

/// NOTE: Untested. Ready for testing (6/4/2018).

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
        Voxel<T> root = new Voxel<T>(min, max, point);

        numComponents = 1;
        numVoxels = 1;
    }

    /// <summary>
    /// Updates grid structure given new point to be included.
    /// <summary>
    public void resize(Vector3 point)
    {
        if (!root.contains(point))
        {
            grow(point);
            resize(point);
        }
        else
        {
            Tuple<Voxel<T>, ref OctreeContainer<T>, int> leaf = navigate(point);
            Voxel<T> vox = leaf.Item1;
            if (vox.point == null || vox.size() <= minSize)
                vox.update(point);
            else
            {
                OctreeContainer<T> parent = leaf.Item2;
                int childNum = leaf.Item3;
                OctreeContainer<T> replacement = vox.split(point);
                parent.setChild(childNum, replacement);
                numComponents += 8;
                numVoxels += 7;
            }
        }
    }

    /// <summary>
    /// Sets value at Voxel containing point.
    /// If root does not contain point, Octree resized so it does.
    /// <summary>
    public void set(Vector3 point, T value)
    {
        if (!root.contains(point))
            resize(point);
        Tuple<Voxel<T>, ref OctreeContainer<T>, int> leaf = navigate(point);
        Voxel<T> vox = leaf.Item1;
        vox.setPoint(point);
        vox.setValue(value);
    }

    /// <summary>
    /// Gets value at Voxel containing point.
    /// If root does not contain point, throws ArgumentOutOfRangeException.
    /// <summary>
    public T get(Vector3 point)
    {
        if (!root.contains(point))
            throw new ArgumentOutOfRangeException("point", "point not contained in Octree");
        Tuple<Voxel<T>, ref OctreeContainer<T>, int> leaf = navigate(point);
        Voxel<T> vox = leaf.Item1;
        return vox.value;
    }

    /// <summary>
    /// Returns total bounds of Octree.
    /// Return: Tuple of min, max.
    /// <summary>
    public Tuple<Vector3, Vector3> bounds()
    {
        return Tuple.Create<Vector3, Vector3>(root.min, root.max);
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
            Tuple<Vector3, Vector3> newRootBounds = root.parentBounds(childNum);
            OctreeContainer<T> newRoot = new OctreeContainer<T>(newRootBounds.Item1, newRootBounds.Item2);
            newRoot.setChild(childNum, root);
            root = newRoot;
            numComponents += 8;
        }
    }

    /// <summary>
    /// Returns leaf-level Voxel containing point and its direct parent OctreeContainer.
    /// NOTE: throws ArgumentOutOfRangeException if point is not contained in Octree.
    /// Parent by reference to allow manipulation.
    /// Return: Voxel (leaf), OctreeContainer (direct-parent), childNumber.
    /// <summary>
    private Tuple<Voxel<T>, ref OctreeContainer<T>, int> navigate(Vector3 point)
    {
        if (!root.contains(point))
            throw new ArgumentOutOfRangeException("point", "not currently contained in Octree");
        if (root.GetType() == typeof(Voxel<T>))
            return Tuple.Create<Voxel<T>, OctreeContainer<T>, int>(root, null, -1);
        else
            return root.navigate(point);
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
    /// Returns Tuple of min, max bounds.
    /// Used for creating parent when growing Octree.
    /// <summary>
    public Tuple<Vector3, Vector3> parentBounds(int childNum)
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
        return Tuple.Create<Vector3, Vector3>(parentMin, parentMax);
    }

    public abstract Tuple<Voxel<T>, ref OctreeContainer<T>, int> navigate(Vector3 point);
}


/// <summary>
/// Non-leaf component of Octree, i.e. contains smaller subdivision components.
/// <summary>
public class OctreeContainer<T> : OctreeComponent<T>
{
    /// <summary>
    /// Array of pointers to children components.
    /// <summary>
    private OctreeComponent<T>[] children;

    /// <summary>
    /// Constructor.
    /// Sets all Container children to new Voxels.
    /// <summary>
    public OctreeContainer(Vector3 myMin, Vector3 myMax)
        : base(myMin, myMax)
    {
        for (int i = 0; i < 8; i++)
        {
            Tuple<Vector3, Vector3> voxBounds = childBounds(i);
            children[i] = new Voxel<T>(voxBounds.Item1, voxBounds.Item2);
        }
    }

    /// <summary>
    /// Mutator for a child.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
    /// Really wish C# allowed friend classes.
    /// Used by Voxel class when splitting.
    /// <summary>
    public void setChild(int childNum, OctreeComponent<T> newChild)
    {
        children[childNum] = newChild;
    }

    /// <summary>
    /// Mutator for all children.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
    /// Really wish C# allowed friend classes.
    /// Used by Voxel class when splitting.
    /// <summary>
    public void setChildren(OctreeComponent<T>[] newChildren)
    {
        children = newChildren;
    }

    /// <summary>
    /// Navigates tree to Voxel containing point, also returning its direct parent.
    /// NOTE: throws ArgumentOutOfRangeException if point is not contained in Container.
    /// Parent by reference to allow manipulation.
    /// Return: Voxel (leaf), OctreeContainer (direct-parent), childNumber.
    /// <summary>
    public override Tuple<Voxel<T>, OctreeContainer<T>, int> navigate(Vector3 point)
    {
        if (!contains(point))
            throw new ArgumentOutOfRangeException("point", "not contained in OctreeContainer.");
        for (int i = 0; i < 8; i++)
        {
            if (children[i].contains(point))
                if (children[i].GetType() == typeof(Voxel<T>))
                    return Tuple.Create<Voxel<T>, OctreeContainer<T>>(children[i], ref this, i);
                else
                    return children[i].navigate(point);
        }
        return Tuple.Create<Voxel<T>, OctreeContainer<T>, int>(null, null, -1); // should never be called.
    }

    /// <summary>
    /// Returns specified child component's bounds.
    /// Returns Tuple of min, max bounds.
    /// <summary>
    public Tuple<Vector3, Vector3> childBounds(int childNum)
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
        return new Tuple<Vector3, Vector3>(childMin, childMax);
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
    public Voxel(Vector3 myMin, Vector3 myMax, Vector3 myPoint = new Vector3(), T myValue = default(T)) : base(myMin, myMax)
    {
        point = myPoint;
        value = myValue;
    }

    /// <summary>
    /// Mutator for point.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
    /// Really wish C# allowed friend classes.
    /// Used by Octree set function.
    /// <summary>
    public void setPoint(Vector3 newPoint)
    { point = newPoint; }

    /// <summary>
    /// Mutator for value.
    /// NOTE: Private variable/public mutator is more safe than pure public variable.
    /// Really wish C# allowed friend classes.
    /// <summary>
    /// Used by Octree set function.
    public void setValue(T newValue)
    { value = newValue; }

    /// <summary>
    /// Updates Voxel when assigned new point.
    /// <summary>
    public void update(Vector3 newPoint)
    {
        point = newPoint;
        value = default(T);
    }

    /// <summary>
    /// Splits Voxel into 8 smaller Voxels, converts to Octree Container.
    /// <summary>
    public OctreeContainer<T> split(Vector3 newPoint)
    {
        OctreeContainer<T> replacement = new OctreeContainer<T>(min, max);

        OctreeComponent<T>[] newChildren = new OctreeComponent<T>[8];
        for (int i = 0; i < 8; i++)
        {
            Tuple<Vector3, Vector3> voxBounds = replacement.childBounds(i);
            Voxel<T> newChild = new Voxel<T>(voxBounds.Item1, voxBounds.Item2);
            if (newChild.contains(point))
            {
                newChild.point = point;
                newChild.value = value;
            }
            if (newChild.contains(newPoint))
                newChild.point = newPoint;
            newChildren[i] = newChild;
        }

        replacement.setChildren(newChildren);
        return replacement;
    }

    public override Tuple<Voxel<T>, OctreeContainer<T>, int> navigate(Vector3 point)
    {
        throw new NotImplementedException();
    }
}
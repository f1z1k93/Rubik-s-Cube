using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Rotor : MonoBehaviour
{
    [SerializeField] private List<Vector3> Axes;

    private static float RotationAngle = 90f;

    private Transform RubiksCube;

    void Start()
    {
        RubiksCube = transform.parent;
    }

    public void Rotate(Quaternion rotation)
    {
        Vector3 direction;
        float angle;
        rotation.ToAngleAxis(out angle, out direction);

        var neighbors = GetNeighbors(direction);

        Assert.IsNotNull(neighbors);

        foreach (Transform cube in neighbors)
        {
            cube.parent = transform;
        }

        transform.RotateAround(transform.position, direction, angle);

        foreach (Transform cube in neighbors)
        {
            cube.parent = RubiksCube;
        }

        Assert.IsNotNull(GetNeighbors(direction));
    }

    public bool GetRotation(Transform from, Transform to, out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        foreach (var axis in Axes)
        {
            var neighbors = GetNeighbors(axis);

            if (neighbors is null)
            {
                continue;
            }

            var isFromNeighborFound = false;
            var isToNeighborFound = false;

            foreach (var neighbor in neighbors)
            {
                isFromNeighborFound = isFromNeighborFound || from == neighbor.transform;
                isToNeighborFound = isToNeighborFound || to == neighbor.transform;
            }

            if (isFromNeighborFound && isToNeighborFound)
            {
                var rotationAxis = Vector3.Cross(from.position - transform.position,
                                                 to.position - transform.position);

                rotation = Quaternion.AngleAxis(RotationAngle, rotationAxis);

                return true;
            }
        }

        return false;
    }

    private List<Transform> GetNeighbors(Vector3 axis)
    {
        var neighbors = new List<Transform>();
        var axisPlane = new Plane(axis, transform.position);
        
        foreach (Transform piece in RubiksCube)
        {
            if (piece.transform.transform != transform &&
                Mathf.Abs(axisPlane.GetDistanceToPoint(piece.transform.position)) < Vector3.kEpsilon)
            {
                neighbors.Add(piece);
            }
        }

        if (neighbors.Count != 8)
        {
            return null;
        }

        return neighbors;
    }
}

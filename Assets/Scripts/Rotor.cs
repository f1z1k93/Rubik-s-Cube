using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Rotor : MonoBehaviour
{
    [SerializeField] private List<Vector3> Axes;

    private const float RotationAngle = 90f;

    private Transform RubiksCube;

    void Start()
    {
        RubiksCube = transform.parent;
    }

    public IEnumerator Rotate(Quaternion rotation, float rotationTime = 0.2f)
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

        float RotationAnglePerFixedUpdate = (angle * Time.fixedDeltaTime) / rotationTime;
        float RotationAngleAccumulator = 0;

        yield return new WaitForFixedUpdate();

        while (RotationAngleAccumulator < angle)
        {
            RotationAnglePerFixedUpdate = Mathf.Min(angle - RotationAngleAccumulator,
                                                    RotationAnglePerFixedUpdate);

            transform.RotateAround(transform.position, direction, RotationAnglePerFixedUpdate);

            RotationAngleAccumulator += RotationAnglePerFixedUpdate;

            yield return new WaitForFixedUpdate();
        }

        foreach (Transform cube in neighbors)
        {
            cube.parent = RubiksCube;
        }

        Vector3 RoundVector3(Vector3 v) { return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z)); }

        transform.localPosition = RoundVector3(transform.localPosition);
        transform.localEulerAngles = RoundVector3(transform.localEulerAngles);

        foreach (Transform cube in neighbors)
        {
            cube.localPosition = RoundVector3(cube.localPosition);
            cube.localEulerAngles = RoundVector3(cube.localEulerAngles);
        }

        Assert.IsNotNull(GetNeighbors(direction));

        yield return null;
    }

    public bool GetRotation(Transform from, Transform to, out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        foreach (var axis in Axes)
        {
            var neighbors = GetNeighbors(GetDirectionByAxis(axis));

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

    private Vector3 GetDirectionByAxis(Vector3 axis)
    {
        if (axis == Vector3.right)
        {
            return transform.right;
        }

        if (axis == Vector3.up)
        {
            return transform.up;
        }

        if (axis == Vector3.forward)
        {
            return transform.forward;
        }

        Assert.IsTrue(false);

        return Vector3.zero;
    }
}

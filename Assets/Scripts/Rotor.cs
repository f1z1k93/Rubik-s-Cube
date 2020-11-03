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

    public IEnumerator AnimateRotation(Quaternion rotation, float rotationTime)
    {
        var animation = new RotorAnimation(this);

        animation.Prepare(rotation, rotationTime);

        yield return StartCoroutine(animation.Run());

        animation.Release();

        yield break;
    }

    public bool GetRotation(Transform from, Transform to, out Quaternion rotation)
    {
        rotation = Quaternion.identity;

        foreach (var axis in Axes)
        {
            var worldAxis = transform.TransformDirection(axis);
            var neighbors = GetNeighbors(worldAxis);

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
                var worldAxisForRotationFromTo = Vector3.Cross(from.position - transform.position,
                                                               to.position - transform.position);
                var angle = RotationAngle * Mathf.Sign(Vector3.Dot(worldAxisForRotationFromTo, worldAxis));

                rotation = Quaternion.AngleAxis(angle, axis);

                return true;
            }
        }

        return false;
    }

    public void GetRandomRotation(out Quaternion rotation)
    {
        var axis = Axes[Random.Range(0, Axes.Count)];

        rotation = Quaternion.AngleAxis(RotationAngle, axis);
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

    private class RotorAnimation
    {
        public RotorAnimation(Rotor rotor)
        {
            Rotor = rotor;
        }

        public void Prepare(Quaternion rotation, float rotationTime)
        {
            Vector3 localAxis;
            rotation.ToAngleAxis(out RotationAngle, out localAxis);

            RotationAxis = Rotor.transform.TransformDirection(localAxis);
            RotationTime = rotationTime;
            RotorNeighbors = Rotor.GetNeighbors(RotationAxis);
            RotationAnglePerSecond = RotationAngle / RotationTime;

            Assert.IsNotNull(RotorNeighbors);

            foreach (Transform neighbor in RotorNeighbors)
            {
                neighbor.parent = Rotor.transform;
            }
        }

        public IEnumerator Run()
        {
            float rotationAngleAccumulator = 0;

            while (true)
            {
                var ExpectedRotationAnglePerFrame = RotationAnglePerSecond * Time.deltaTime;

                var RotationAnglePerFrame = Mathf.Min(RotationAngle - rotationAngleAccumulator,
                                                      ExpectedRotationAnglePerFrame);

                Rotor.transform.RotateAround(Rotor.transform.position, RotationAxis, RotationAnglePerFrame);

                rotationAngleAccumulator += RotationAnglePerFrame;

                if (rotationAngleAccumulator >= RotationAngle)
                {
                    yield break;
                }

                yield return null;
            }
        }

        public void Release()
        {
            foreach (var neighbor in RotorNeighbors)
            {
                neighbor.parent = Rotor.RubiksCube;
            }

            // We should align panel after each rotation
            Vector3 RoundVector3(Vector3 v) { return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z)); }

            Rotor.transform.localPosition = RoundVector3(Rotor.transform.localPosition);
            Rotor.transform.localEulerAngles = RoundVector3(Rotor.transform.localEulerAngles);

            foreach (Transform neighbor in RotorNeighbors)
            {
                neighbor.localPosition = RoundVector3(neighbor.localPosition);
                neighbor.localEulerAngles = RoundVector3(neighbor.localEulerAngles);
            }

            Assert.IsNotNull(Rotor.GetNeighbors(RotationAxis));
        }

        private Rotor Rotor;
        private float RotationTime;
        private Vector3 RotationAxis;
        private float RotationAngle;
        private float RotationAnglePerSecond;
        private List<Transform> RotorNeighbors;
    }
}

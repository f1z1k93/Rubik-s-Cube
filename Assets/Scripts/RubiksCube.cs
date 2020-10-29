using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RubiksCube : MonoBehaviour
{
    [SerializeField] float EdgeLength; // TODO: remove it
    [SerializeField] List<Rotor> Rotors;

    private RotationInfo RotationInfo;

    public void OnStartScreenTracking(Vector2 screenPoint)
    {
        Assert.IsNull(RotationInfo);

        RotationInfo = new RotationInfo();
        RotationInfo.ScreenPointFrom = GetWorldPositionOfScreenPoint(screenPoint);
        RotationInfo.CubeTouchFrom = TryGetCubeTouch(screenPoint);
        RotationInfo.Type = RotationInfo.CubeTouchFrom is null ? RotationInfo.RotationType.CUBE :
                                                                 RotationInfo.RotationType.ROTOR;
    }

    public void OnContinueScreenTracking(Vector2 screenPoint)
    {
        Assert.IsNotNull(RotationInfo);

        RotationInfo.ScreenPointTo = GetWorldPositionOfScreenPoint(screenPoint);
        if (RotationInfo.Type == RotationInfo.RotationType.ROTOR)
        {
            RotationInfo.CubeTouchTo = TryGetCubeTouch(screenPoint);
        }

        ProcessRotationInfo(RotationInfo);

        RotationInfo.ScreenPointFrom = RotationInfo.ScreenPointTo;
    }

    public void OnStopScreenTracking(Vector2 screenPoint)
    {
        Assert.IsNotNull(RotationInfo);

        RotationInfo.ScreenPointTo = GetWorldPositionOfScreenPoint(screenPoint);
        if (RotationInfo.Type == RotationInfo.RotationType.ROTOR)
        {
            RotationInfo.CubeTouchTo = TryGetCubeTouch(screenPoint);
        }

        ProcessRotationInfo(RotationInfo);

        RotationInfo = null;
    }

    private RotationInfo.CubeTouch TryGetCubeTouch(Vector2 screenPoint)
    {
        var cameraRay = Camera.main.ScreenPointToRay(screenPoint);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            return new RotationInfo.CubeTouch {
                Piece =  hit.transform,
                Point = hit.point
            };
        }

        return null;
    }

    private void ProcessRotationInfo(RotationInfo rotationInfo)
    {
        if (RotationInfo.Type == RotationInfo.RotationType.NONE)
        {
            return;
        }

        if (RotationInfo.Type == RotationInfo.RotationType.CUBE)
        {
            RotateCube(rotationInfo.ScreenPointFrom, rotationInfo.ScreenPointTo);
            return;
        }

        if (rotationInfo.CubeTouchFrom is null || rotationInfo.CubeTouchTo is null)
        {
            return;
        }

        if (!RotateRotor(rotationInfo.CubeTouchFrom, rotationInfo.CubeTouchTo))
        {
            return;
        }

        rotationInfo.Type = RotationInfo.RotationType.NONE;
    }

    private bool RotateRotor(RotationInfo.CubeTouch from, RotationInfo.CubeTouch to)
    {
        if (from.Piece == to.Piece)
        {
            return false;
        }

        var rotorAndRotation = FindRotorAndRotation(from, to);
        
        if (rotorAndRotation is null)
        {
            return false;
        }

        var rotor = rotorAndRotation.Item1;
        var rotation = rotorAndRotation.Item2;

        StartCoroutine(rotor.Rotate(rotation));

        return true;
    }

    private void RotateCube(Vector3 screenPointPositionFrom, Vector3 screenPointPositionTo)
    {
        var rotation = Quaternion.FromToRotation(screenPointPositionFrom - transform.position,
                                                 screenPointPositionTo - transform.position);

        transform.rotation *= rotation;

        return;
    }

    private Tuple<Rotor, Quaternion> FindRotorAndRotation(RotationInfo.CubeTouch from, RotationInfo.CubeTouch to)
    {
        foreach (var rotor in Rotors)
        {
            var rotation = Quaternion.identity;

            if (rotor.GetRotation(from.Piece, to.Piece, out rotation) &&
                IsRotationFound(rotation, from.Point, to.Point)
            ) {
                
                return new Tuple<Rotor, Quaternion>(rotor, rotation);
            }
        }

        return null;
    }

    private bool IsRotationFound(Quaternion rotation, Vector3 pointFrom, Vector3 pointTo)
    {
        Vector3 dir;
        float angle;
        rotation.ToAngleAxis(out angle, out dir);

        if (Vector3.Distance(dir, transform.up) < Vector3.kEpsilon || Vector3.Distance(dir, -transform.up) < Vector3.kEpsilon) {
            return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, transform.up) ||
                     IsPointOnAxisPerpendicularCubeFaces(pointTo, transform.up));
        } else if (Vector3.Distance(dir, transform.right) < Vector3.kEpsilon || Vector3.Distance(dir, -transform.right) < Vector3.kEpsilon) {
            return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, transform.right) ||
                     IsPointOnAxisPerpendicularCubeFaces(pointTo, transform.right));
        } else if (Vector3.Distance(dir, transform.forward) < Vector3.kEpsilon || Vector3.Distance(dir, -transform.forward) < Vector3.kEpsilon) {
            return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, transform.forward) ||
                     IsPointOnAxisPerpendicularCubeFaces(pointTo, transform.forward));
        }

        Assert.IsTrue(false);

        return false;
    }

    private bool IsPointOnAxisPerpendicularCubeFaces(Vector3 point, Vector3 axis)
    {
        if (Mathf.Abs(new Plane(axis, transform.position + 0.5f * axis * EdgeLength).GetDistanceToPoint(point)) < Vector3.kEpsilon)
        {
            return true;
        }

        if (Mathf.Abs(new Plane(axis, transform.position - 0.5f * axis * EdgeLength).GetDistanceToPoint(point)) < Vector3.kEpsilon)
        {
            return true;
        }

        return false;
    }

    private Vector3 GetWorldPositionOfScreenPoint(Vector2 screenPoint)
    {
        return Camera.main.ScreenToWorldPoint(
            new Vector3(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane)
        );
    }
}

class RotationInfo
{
    public enum RotationType
    {
        NONE,
        CUBE,
        ROTOR,
    }

    public class CubeTouch
    {
        public Transform Piece;
        public Vector3 Point;
    }

    public CubeTouch CubeTouchFrom = null;
    public CubeTouch CubeTouchTo = null;

    public Vector3 ScreenPointFrom;
    public Vector3 ScreenPointTo;

    public RotationType Type;
}

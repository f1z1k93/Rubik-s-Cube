using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RubiksCube : MonoBehaviour
{
    [SerializeField] float EdgeLength; // TODO: remove it
    [SerializeField] List<Rotor> Rotors;

    private RotationInfo RotationInfo;

    private bool IsMouseButtonDown0 = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnStartScreenTracking(Input.mousePosition);
            IsMouseButtonDown0 = true;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnStopScreenTracking(Input.mousePosition);
            IsMouseButtonDown0 = false;
            return;
        }

        if (IsMouseButtonDown0)
        {
            OnContinueScreenTracking(Input.mousePosition);
            return;
        }
    }

    private void OnStartScreenTracking(Vector3 screenPosition)
    {
        Assert.IsNull(RotationInfo);

        RotationInfo = new RotationInfo();
        RotationInfo.ScreenPositionFrom = screenPosition;
        RotationInfo.CubeTouchFrom = TryGetCubeTouch(screenPosition);
        RotationInfo.Type = RotationInfo.CubeTouchFrom is null ? RotationInfo.RotationType.CUBE :
                                                                 RotationInfo.RotationType.ROTOR;
    }

    private void OnContinueScreenTracking(Vector3 screenPosition)
    {
        Assert.IsNotNull(RotationInfo);

        RotationInfo.ScreenPositionTo = screenPosition;
        if (RotationInfo.Type == RotationInfo.RotationType.ROTOR)
        {
            RotationInfo.CubeTouchTo = TryGetCubeTouch(screenPosition);
        }

        ProcessRotationInfo(RotationInfo);

        RotationInfo.ScreenPositionFrom = screenPosition;
    }

    private void OnStopScreenTracking(Vector3 screenPosition)
    {
        Assert.IsNotNull(RotationInfo);

        RotationInfo.ScreenPositionTo = screenPosition;
        if (RotationInfo.Type == RotationInfo.RotationType.ROTOR)
        {
            RotationInfo.CubeTouchTo = TryGetCubeTouch(screenPosition);
        }

        RotationInfo.ScreenPositionTo = screenPosition;

        ProcessRotationInfo(RotationInfo);

        RotationInfo = null;
    }

    private RotationInfo.CubeTouch TryGetCubeTouch(Vector3 screenPosition)
    {
        var cameraRay = Camera.main.ScreenPointToRay(screenPosition);

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
        if (rotationInfo.From is null)
        {
            var rotation = GetRotation(rotationInfo.ScreenPositionFrom, rotationInfo.ScreenPositionTo);

            transform.rotation *= rotation;

            return;
        }

        if (RotationInfo.Type == RotationInfo.RotationType.NONE)
        {
            return;
        }

        if (RotationInfo.Type == RotationInfo.RotationType.CUBE)
        {
            RotateCube(rotationInfo.ScreenPositionFrom, rotationInfo.ScreenPositionTo);
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

    private Quaternion GetRotation(Vector3 screenPositionFrom, Vector3 screenPositionTo)
    {
        var worldPositionFrom = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPositionFrom.x, screenPositionFrom.y, Camera.main.nearClipPlane)
        );

        var worldPositionTo = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPositionTo.x, screenPositionTo.y, Camera.main.nearClipPlane)
        );

        var rotation = Quaternion.FromToRotation(worldPositionFrom - transform.position,
                                                 worldPositionTo - transform.position);

        return rotation;
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

    private void RotateCube(Vector3 screenPositionFrom, Vector3 screenPositionTo)
    {
        var rotation = GetRotation(screenPositionFrom, screenPositionTo);

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

    public CubeTouch From = null;
    public CubeTouch To = null;

    public CubeTouch CubeTouchFrom = null;
    public CubeTouch CubeTouchTo = null;

    public Vector3 ScreenPositionFrom;
    public Vector3 ScreenPositionTo;

    public RotationType Type;
}

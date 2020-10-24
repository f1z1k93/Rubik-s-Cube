using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class RubiksCube : MonoBehaviour
{
    [SerializeField] float EdgeLength; // TODO: remove it
    [SerializeField] List<Rotor> Rotors;

    private RotationInfo RotationInfo;

    private bool isMouseButtonDown0 = false;

    private Vector3 MousePositon;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MousePositon = Input.mousePosition;
            StartScreenTracking(MousePositon);
            isMouseButtonDown0 = true;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopScreenTracking(Input.mousePosition);
            isMouseButtonDown0 = false;
            return;

        }

        if (isMouseButtonDown0)
        {
            if (Vector3.Distance(Input.mousePosition, MousePositon) < 1f)
            {
                return;
            }

            MousePositon = Input.mousePosition;

            ContinueScreenTracking(MousePositon);
        }
    }

    private void StartScreenTracking(Vector3 screenPosition)
    {
        if (!(RotationInfo is null))
        {
            StopScreenTracking(screenPosition);
        }

        RotationInfo = new RotationInfo();

        RotationInfo.ScreenPositionFrom = screenPosition;

        var cameraRay = Camera.main.ScreenPointToRay(screenPosition);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            RotationInfo.From = new RotationInfo.CubeTouch {
                Piece =  hit.transform,
                Point = hit.point
            };
        }
    }

    private void ContinueScreenTracking(Vector3 screenPosition)
    {
        if (RotationInfo.From is null)
        {
            return;
        }
    }

    private void StopScreenTracking(Vector3 screenPosition)
    {
        if (RotationInfo is null)
        {
            return;
        }

        RotationInfo.ScreenPositionTo = screenPosition;

        var cameraRay = Camera.main.ScreenPointToRay(screenPosition);

        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            RotationInfo.To = new RotationInfo.CubeTouch {
                Piece =  hit.transform,
                Point = hit.point
            };
        }
        
        ProcessRotationInfo(RotationInfo);

        RotationInfo = null;
    }

    private void ProcessRotationInfo(RotationInfo rotationInfo)
    {
        if (rotationInfo.From is null)
        {
            var rotation = GetRotation(rotationInfo.ScreenPositionFrom, rotationInfo.ScreenPositionTo);

            transform.rotation *= rotation;

            return;
        }

        if (rotationInfo.From != null && rotationInfo.To != null)
        {
            RotateRotor(rotationInfo.From, rotationInfo.To);
        }
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

    private void RotateRotor(RotationInfo.CubeTouch from, RotationInfo.CubeTouch to)
    {
        var rotorAndRotation = FindRotorAndRotation(from, to);
        
        if (rotorAndRotation is null)
        {
            return;
        }

        var rotor = rotorAndRotation.Item1;
        var rotation = rotorAndRotation.Item2;

        rotor.Rotate(rotation);
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
    public class CubeTouch
    {
        public Transform Piece;
        public Vector3 Point;
    }

    public CubeTouch From = null;
    public CubeTouch To = null;

    public Vector3 ScreenPositionFrom;
    public Vector3 ScreenPositionTo;
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using IEnumerator = System.Collections.IEnumerator;
using Random = UnityEngine.Random;

public class RubiksCube : MonoBehaviour
{
    [SerializeField] float EdgeLength; // TODO: remove it
    [SerializeField] List<Rotor> Rotors;
    [SerializeField] float PlaneRotationTime;

    private RotationInfo RotationInfo;
    private bool IsBusy = false;

    public void OnStartScreenTracking(Vector2 screenPoint)
    {
        Assert.IsNull(RotationInfo);

        if (IsBusy)
        {
            return;
        }

        RotationInfo = new RotationInfo();
        RotationInfo.ScreenPointFrom = GetWorldPositionOfScreenPoint(screenPoint);
        RotationInfo.CubeTouchFrom = TryGetCubeTouch(screenPoint);
        RotationInfo.Type = RotationInfo.CubeTouchFrom is null ? RotationInfo.RotationType.CUBE :
                                                                 RotationInfo.RotationType.ROTOR;
    }

    public void OnContinueScreenTracking(Vector2 screenPoint)
    {
        if (RotationInfo is null)
        {
            return;
        }

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
        if (RotationInfo is null)
        {
            return;
        }

        RotationInfo.ScreenPointTo = GetWorldPositionOfScreenPoint(screenPoint);
        if (RotationInfo.Type == RotationInfo.RotationType.ROTOR)
        {
            RotationInfo.CubeTouchTo = TryGetCubeTouch(screenPoint);
        }

        ProcessRotationInfo(RotationInfo);

        RotationInfo = null;
    }

    public void OnShuffling()
    {
        if (IsBusy)
        {
            return;
        }

        RotateRandomPanel();
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

        if (!RotatePanel(rotationInfo.CubeTouchFrom, rotationInfo.CubeTouchTo))
        {
            return;
        }

        rotationInfo.Type = RotationInfo.RotationType.NONE;
    }

    private bool RotatePanel(RotationInfo.CubeTouch from, RotationInfo.CubeTouch to)
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

        StartCoroutine(AnimatePanelRotation(rotor, rotation));

        return true;
    }

    private void RotateRandomPanel()
    {
        var rotor = Rotors[Random.Range(0, Rotors.Count)];
        var rotation = Quaternion.identity;
        rotor.GetRandomRotation(out rotation);

        StartCoroutine(AnimatePanelRotation(rotor, rotation));

        return;
    }

    private IEnumerator AnimatePanelRotation(Rotor rotor, Quaternion rotation)
    {
        Assert.IsFalse(IsBusy);
        Assert.IsTrue(PlaneRotationTime >= 0f);

        IsBusy = true;

        yield return StartCoroutine(rotor.AnimateRotation(rotation, PlaneRotationTime));

        IsBusy = false;

        yield break;
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

        if (dir == transform.up || dir == -transform.up) {
            return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, transform.up) ||
                     IsPointOnAxisPerpendicularCubeFaces(pointTo, transform.up));
        } else if (dir == transform.right || dir == -transform.right) {
            return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, transform.right) ||
                     IsPointOnAxisPerpendicularCubeFaces(pointTo, transform.right));
        } else if (dir == transform.forward || dir == -transform.forward) {
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

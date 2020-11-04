using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using IEnumerator = System.Collections.IEnumerator;
using Random = UnityEngine.Random;

public class RubiksCube : MonoBehaviour
{
    [SerializeField] float EdgeLength; // TODO: remove it
    [SerializeField] List<Rotor> Rotors;
    [SerializeField] float PlaneRotationTime;
    [SerializeField] UnityEvent IsSolvedEvent;

    public Piece[] Pieces;

    private RotationInfo RotationInfo;
    private bool IsBusy = false;
    private bool IsShuffled = false;
    private Stack<Tuple<Rotor, Quaternion>> Modifications;

    private void Start()
    {
        Modifications = new Stack<Tuple<Rotor, Quaternion>>();
        Pieces = transform.GetComponentsInChildren<Piece>();
    }

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

        IsShuffled = true;

        RotateRandomPanel();
    }

    public void OnShuffleBottonDown()
    {
        Modifications.Clear();
    }

    public void OnModificationReverting()
    {
        if (IsBusy)
        {
            return;
        }

        if (Modifications.Count == 0)
        {
            return;
        }

        var rotorAndRotation = Modifications.Pop();

        var rotor = rotorAndRotation.Item1;
        var rotation = rotorAndRotation.Item2;

        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);

        var revertedRotation = Quaternion.AngleAxis(-angle, axis);

        StartCoroutine(AnimatePanelRotation(rotor, revertedRotation));
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

        Modifications.Push(rotorAndRotation);

        var rotor = rotorAndRotation.Item1;
        var rotation = rotorAndRotation.Item2;

        StartCoroutine(AnimatePanelRotationAndCheckSolved(rotor, rotation));

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

    private IEnumerator AnimatePanelRotationAndCheckSolved(Rotor rotor, Quaternion rotation)
    {
        yield return AnimatePanelRotation(rotor, rotation);

        if (!IsShuffled)
        {
            yield break;
        }

        IsShuffled = true;

        if (IsSolved())
        {
            Modifications.Clear();
            IsSolvedEvent.Invoke();
        }
    }

    private void RotateCube(Vector3 screenPointPositionFrom, Vector3 screenPointPositionTo)
    {
        var fromToRotation = Quaternion.FromToRotation(screenPointPositionFrom - transform.position,
                                                 screenPointPositionTo - transform.position);

        transform.rotation = fromToRotation * transform.rotation;

        return;
    }

    private bool IsSolved()
    {
        foreach (var rotor in Rotors)
        {
            if (rotor.FaceColor == Piece.FaceColor.NONE)
            {
                continue;
            }

            if (!rotor.IsPanelSolved())
            {
                return false;
            }
        }

        return true;
    }

    private Tuple<Rotor, Quaternion> FindRotorAndRotation(RotationInfo.CubeTouch from, RotationInfo.CubeTouch to)
    {
        foreach (var rotor in Rotors)
        {
            var rotation = Quaternion.identity;

            if (rotor.GetRotation(from.Piece, to.Piece, out rotation) &&
                IsRotorRotationFound(rotor, rotation, from.Point, to.Point))
            {
                return new Tuple<Rotor, Quaternion>(rotor, rotation);
            }
        }

        return null;
    }

    private bool IsRotorRotationFound(Rotor rotor, Quaternion rotation, Vector3 pointFrom, Vector3 pointTo)
    {
        Vector3 localAxis;
        float angle;
        rotation.ToAngleAxis(out angle, out localAxis);

        var worldAxis = rotor.transform.TransformDirection(localAxis);

        return !(IsPointOnAxisPerpendicularCubeFaces(pointFrom, worldAxis) ||
                 IsPointOnAxisPerpendicularCubeFaces(pointTo, worldAxis));
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

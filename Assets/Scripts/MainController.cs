using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class MainController : MonoBehaviour
{
    [Serializable] public class TrackEvent : UnityEvent<Vector3> {}

    [SerializeField] private TrackEvent StartTrackEvent;
    [SerializeField] private TrackEvent ContinueTrackEvent;
    [SerializeField] private TrackEvent StopTrackEvent;

    private ScreenTracker ScreenTracker = null;

    private void Start()
    {
        ScreenTracker = new MouseScreenTracker();

        Assert.IsNotNull(ScreenTracker);
        ScreenTracker.StartScreenTrack = (Vector3 pos) => StartTrackEvent.Invoke(pos);
        ScreenTracker.ContinueScreenTrack = (Vector3 pos) => ContinueTrackEvent.Invoke(pos);
        ScreenTracker.StopScreenTrack = (Vector3 pos) => StopTrackEvent.Invoke(pos);
    }

    void Update()
    {
        ScreenTracker.UpdateTrack();
    }
}

class ScreenTracker
{
    public Action<Vector3> StartScreenTrack;
    public Action<Vector3> ContinueScreenTrack;
    public Action<Vector3> StopScreenTrack;

    public virtual void UpdateTrack()
    {
    }
}

class MouseScreenTracker : ScreenTracker
{
    private bool IsMouseButtonDown0 = false;

    public override void UpdateTrack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartScreenTrack(Input.mousePosition);
            IsMouseButtonDown0 = true;
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopScreenTrack(Input.mousePosition);
            IsMouseButtonDown0 = false;
            return;
        }

        if (IsMouseButtonDown0)
        {
            ContinueScreenTrack(Input.mousePosition);
            return;
        }
    }
}

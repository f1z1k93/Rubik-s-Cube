using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class MainController : MonoBehaviour
{
    [Serializable] public class TrackEvent : UnityEvent<Vector2> {}

    [SerializeField] private TrackEvent StartTrackEvent;
    [SerializeField] private TrackEvent ContinueTrackEvent;
    [SerializeField] private TrackEvent StopTrackEvent;

    private ScreenTracker ScreenTracker = null;
    private bool IsPause = false;

    private void Start()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                ScreenTracker = new TouchScreenTracker();
                break;
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
                ScreenTracker = new MouseScreenTracker();
                break;
            default:
                break;
        }

        Assert.IsNotNull(ScreenTracker);

        ScreenTracker.StartScreenTrack = (Vector3 pos) => StartTrackEvent.Invoke(pos);
        ScreenTracker.ContinueScreenTrack = (Vector3 pos) => ContinueTrackEvent.Invoke(pos);
        ScreenTracker.StopScreenTrack = (Vector3 pos) => StopTrackEvent.Invoke(pos);
    }

    void Update()
    {
        if (IsPause)
        {
            return;
        }

        ScreenTracker.UpdateTrack();
    }

    public void OnPause()
    {
        IsPause = true;
    }

    public void OnResume()
    {
        IsPause = false;
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
class TouchScreenTracker : ScreenTracker
{
    public override void UpdateTrack()
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);

            switch (touch.phase) {
                case TouchPhase.Began:
                    StartScreenTrack(touch.position);
                    break;
                case TouchPhase.Moved:
                    ContinueScreenTrack(touch.position);
                    break;
                case TouchPhase.Ended:
                    StopScreenTrack(touch.position);
                    break;
                default:
                    break;
            }
        }
    }
}

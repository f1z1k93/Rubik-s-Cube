using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainController : MonoBehaviour
{
    [Serializable] public class TrackEvent : UnityEvent<Vector2> {}

    [SerializeField] private TrackEvent StartTrackEvent;
    [SerializeField] private TrackEvent ContinueTrackEvent;
    [SerializeField] private TrackEvent StopTrackEvent;

    private ScreenTracker ScreenTracker = null;

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

        ScreenTracker.StartScreenTrackEvent = StartTrackEvent;
        ScreenTracker.ContinueScreenTrackEvent = ContinueTrackEvent;
        ScreenTracker.StopScreenTrackEvent = StopTrackEvent;
    }

    void Update()
    {
        ScreenTracker.UpdateTrack();
    }

    public void OnPause()
    {
        ScreenTracker.Pause();
    }
    public void OnResume()
    {
        ScreenTracker.Resume();
    }
}

class ScreenTracker
{
    public MainController.TrackEvent StartScreenTrackEvent;
    public MainController.TrackEvent ContinueScreenTrackEvent;
    public MainController.TrackEvent StopScreenTrackEvent;

    private bool IsNeedEmitTrackEvent = false;
    private bool IsPause = false;

    public virtual void UpdateTrack()
    {
    }

    public void Pause()
    {
        IsPause = true;
    }

    public void Resume()
    {
        IsPause = false;
    }

    protected void StartScreenTrack(Vector2 screenPoint, int touchId = -1)
    {
        if (IsPointOnUI(touchId) || IsPause)
        {
            IsNeedEmitTrackEvent = false;
            return;
        }

        IsNeedEmitTrackEvent = true;

        StartScreenTrackEvent.Invoke(screenPoint);
    }

    protected void ContinueScreenTrack(Vector2 screenPoint, int touchId = -1)
    {
        if (!IsNeedEmitTrackEvent)
        {
            return;
        }

        ContinueScreenTrackEvent.Invoke(screenPoint);
    }

    protected void StopScreenTrack(Vector2 screenPoint, int touchId = -1)
    {
        if (!IsNeedEmitTrackEvent)
        {
            return;
        }

        StopScreenTrackEvent.Invoke(screenPoint);
    }

    private bool IsPointOnUI(int touchId)
    {
        return EventSystem.current.IsPointerOverGameObject(touchId);
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

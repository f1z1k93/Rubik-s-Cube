using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MainController : MonoBehaviour
{
    [Serializable] public class TrackEvent : UnityEvent<Vector2> {}

    [SerializeField, CanBeNull] private TrackEvent StartTrackEvent;
    [SerializeField, CanBeNull] private TrackEvent ContinueTrackEvent;
    [SerializeField, CanBeNull] private TrackEvent StopTrackEvent;

    [CanBeNull] private ScreenTracker ScreenTracker;

    private void Start()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                ScreenTracker = new TouchScreenTracker();
                break;

            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.OSXEditor:
                ScreenTracker = new MouseScreenTracker();
                break;
                
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WebGLPlayer:
                ScreenTracker = new MouseScreenTracker();
                break;
        }

        if (ScreenTracker != null)
        {
            ScreenTracker.StartScreenTrackEvent = StartTrackEvent;
            ScreenTracker.ContinueScreenTrackEvent = ContinueTrackEvent;
            ScreenTracker.StopScreenTrackEvent = StopTrackEvent;
        }
    }

    void Update()
    {
        ScreenTracker?.UpdateTrack();
    }

    public void OnPause()
    {
        ScreenTracker?.Pause();
    }
    public void OnResume()
    {
        ScreenTracker?.Resume();
    }
}

class ScreenTracker
{
    [CanBeNull]
    public MainController.TrackEvent StartScreenTrackEvent { get; set; }
    
    [CanBeNull]
    public MainController.TrackEvent ContinueScreenTrackEvent { get; set; }
    
    [CanBeNull]
    public MainController.TrackEvent StopScreenTrackEvent { get; set; }

    private bool IsNeedEmitTrackEvent;
    private bool IsPause;

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

        StartScreenTrackEvent?.Invoke(screenPoint);
    }

    protected void ContinueScreenTrack(Vector2 screenPoint, int touchId = -1)
    {
        if (!IsNeedEmitTrackEvent)
        {
            return;
        }

        ContinueScreenTrackEvent?.Invoke(screenPoint);
    }

    protected void StopScreenTrack(Vector2 screenPoint, int touchId = -1)
    {
        if (!IsNeedEmitTrackEvent)
        {
            return;
        }

        StopScreenTrackEvent?.Invoke(screenPoint);
    }

    private bool IsPointOnUI(int touchId)
    {
        return EventSystem.current.IsPointerOverGameObject(touchId);
    }
}

class MouseScreenTracker : ScreenTracker
{
    private bool IsMouseButtonDown0;

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
            }
        }
    }
}

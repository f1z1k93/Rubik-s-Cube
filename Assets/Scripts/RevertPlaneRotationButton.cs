using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RevertPlaneRotationButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent DownEvent;

    private bool IsDown = false;
    private bool IsPause = false;

    private void Update()
    {
        if (IsDown)
        {
            DownEvent.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = false;
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

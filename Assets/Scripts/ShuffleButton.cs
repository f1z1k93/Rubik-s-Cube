using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShuffleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent ShuffleButtonDownEvent;
    [SerializeField] UnityEvent ShuffleButtonHeldDownEvent;
    [SerializeField] UnityEvent ShuffleButtonUpEvent;

    private bool IsDown = false;
    private bool IsPause = false;

    private void Update()
    {
        if (IsDown)
        {
            ShuffleButtonHeldDownEvent.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = true;
        ShuffleButtonDownEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = false;
        ShuffleButtonUpEvent.Invoke();
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

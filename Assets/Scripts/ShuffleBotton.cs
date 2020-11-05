using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShuffleBotton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent ShuffleBottonDownEvent;
    [SerializeField] UnityEvent ShuffleBottonHeldDownEvent;
    [SerializeField] UnityEvent ShuffleBottonUpEvent;

    private bool IsDown = false;
    private bool IsPause = false;

    private void Update()
    {
        if (IsDown)
        {
            ShuffleBottonHeldDownEvent.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = true;
        ShuffleBottonDownEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPause)
        {
            return;
        }

        IsDown = false;
        ShuffleBottonUpEvent.Invoke();
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

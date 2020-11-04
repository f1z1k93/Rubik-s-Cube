using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShuffleBotton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent ShuffleBottonDownEvent;
    [SerializeField] UnityEvent ShuffleBottonHeldDownEvent;
    [SerializeField] UnityEvent ShuffleBottonUpEvent;

    private bool IsDown = false;

    private void Update()
    {
        if (IsDown)
        {
            ShuffleBottonHeldDownEvent.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsDown = true;
        ShuffleBottonDownEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsDown = false;
        ShuffleBottonUpEvent.Invoke();
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShuffleBotton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent ShuffleBottonDownEvent;
    [SerializeField] UnityEvent ShuffleBottonUpEvent;

    private bool IsDown = false;

    private void Update()
    {
        if (IsDown)
        {
            ShuffleBottonDownEvent.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsDown = false;
        ShuffleBottonUpEvent.Invoke();
    }
}

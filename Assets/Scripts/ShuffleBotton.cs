using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ShuffleBotton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent ShuffleBottonDownEvent;

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
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class RevertPlaneRotationBotton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UnityEvent DownEvent;

    private bool IsDown = false;

    private void Update()
    {
        if (IsDown)
        {
            DownEvent.Invoke();
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

using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static SimpleJoystick MoveJoy;
    public static SimpleJoystick AimJoy;

    [Tooltip("Постав галочку, якщо це джойстик для стрільби (правий)")]
    public bool isAimJoystick;
    
    [Tooltip("Перетягни сюди Handle (внутрішній кружечок джойстика)")]
    public RectTransform handle;

    private float maxDist = 75f;
    public Vector2 InputVector { get; private set; }

    void Awake()
    {
        if (isAimJoystick) AimJoy = this;
        else MoveJoy = this;
    }

    void OnDestroy()
    {
        if (isAimJoystick && AimJoy == this) AimJoy = null;
        if (!isAimJoystick && MoveJoy == this) MoveJoy = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out pos);
        
        Vector2 rawInput = Vector2.ClampMagnitude(pos, maxDist);
        handle.anchoredPosition = rawInput;
        InputVector = rawInput / maxDist;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}

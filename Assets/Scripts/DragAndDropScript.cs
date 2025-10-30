using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDropScript : MonoBehaviour, IPointerDownHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGro;
    private RectTransform rectTra;

    public ObjectScript objectScr;
    public ScreenBoundriesScript screenBou;

    private Vector3 dragOffset;
    private Camera uiCamera;
    private Canvas canvas;

    void Awake()
    {
        canvasGro = GetComponent<CanvasGroup>();
        rectTra = GetComponent<RectTransform>();

        if (objectScr == null)
            objectScr = Object.FindFirstObjectByType<ObjectScript>();

        if (screenBou == null)
            screenBou = Object.FindFirstObjectByType<ScreenBoundriesScript>();

        canvas = GetComponentInParent<Canvas>();
        uiCamera = (canvas != null) ? canvas.worldCamera : Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        objectScr.effects.PlayOneShot(objectScr.audioCli[0]);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ObjectScript.drag = true;
        ObjectScript.lastDragged = eventData.pointerDrag;

        canvasGro.blocksRaycasts = false;
        canvasGro.alpha = 0.6f;

        // Move almost to front (keep UI order)
        int lastIndex = transform.parent.childCount - 1;
        int newIndex = Mathf.Max(0, lastIndex - 1);
        transform.SetSiblingIndex(newIndex);

        Vector3 pointerWorld;
        if (ScreenToWorld(eventData.position, out pointerWorld))
            dragOffset = transform.position - pointerWorld;
        else
            dragOffset = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pointerWorld;
        if (!ScreenToWorld(eventData.position, out pointerWorld))
            return;

        Vector3 target = pointerWorld + dragOffset;
        target.z = transform.position.z;

        screenBou.RecalculateBounds();
        Vector2 clamp = screenBou.GetClampedPosition(target);

        transform.position = new Vector3(clamp.x, clamp.y, target.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        objectScr.effects.PlayOneShot(objectScr.audioCli[0]);

        ObjectScript.drag = false;
        canvasGro.blocksRaycasts = true;
        canvasGro.alpha = 1f;

        if (objectScr.rightPlace)
        {
            canvasGro.blocksRaycasts = false; // lock object
            ObjectScript.lastDragged = null;
        }

        objectScr.rightPlace = false;
    }

    private bool ScreenToWorld(Vector2 screenPos, out Vector3 worldPos)
    {
        if (uiCamera == null)
        {
            worldPos = Vector3.zero;
            return false;
        }

        float z = Mathf.Abs(uiCamera.transform.position.z - transform.position.z);
        worldPos = uiCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        return true;
    }
}

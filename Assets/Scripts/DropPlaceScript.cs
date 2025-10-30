using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;

    public ObjectScript objScript;

    void Start()
    {
        if (objScript == null)
            objScript = Object.FindFirstObjectByType<ObjectScript>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        GameObject dragged = eventData.pointerDrag;
        RectTransform dragRect = dragged.GetComponent<RectTransform>();
        RectTransform placeRect = GetComponent<RectTransform>();

        // rotation difference
        placeZRot = dragRect.eulerAngles.z;
        vehicleZRot = placeRect.eulerAngles.z;
        rotDiff = Mathf.Abs(placeZRot - vehicleZRot);
        if (rotDiff > 180) rotDiff = 360 - rotDiff;

        // scale difference
        placeSiz = dragRect.localScale;
        vehicleSiz = placeRect.localScale;
        xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
        ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

        float distance = Vector2.Distance(dragRect.anchoredPosition, placeRect.anchoredPosition);
        bool overlaps = RectOverlaps(dragRect, placeRect);

        bool closeEnough = distance <= 80f;
        bool rotationOK = rotDiff <= 15f;
        bool scaleOK = xSizeDiff <= 0.15f && ySizeDiff <= 0.15f;

        if (dragged.CompareTag(tag) && closeEnough && rotationOK && scaleOK && overlaps)
        {
            objScript.rightPlace = true;

            dragRect.anchoredPosition = placeRect.anchoredPosition;
            dragRect.localRotation = placeRect.localRotation;
            dragRect.localScale = placeRect.localScale;

            dragRect.SetSiblingIndex(transform.parent.childCount - 2);

            var dragComp = dragged.GetComponent<DragAndDropScript>();
            if (dragComp != null) dragComp.enabled = false;

            var cg = dragged.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = false;

            ObjectScript.lastDragged = dragged;
            PlayCorrectSound(dragged.tag);
        }
        else
        {
            objScript.rightPlace = false;
            objScript.effects.PlayOneShot(objScript.audioCli[1]);
            ResetToStart(dragged);
        }
    }

    bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect ra = ScreenRect(a);
        Rect rb = ScreenRect(b);
        return ra.Overlaps(rb);
    }

    Rect ScreenRect(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return new Rect(corners[0].x, corners[0].y,
                        corners[2].x - corners[0].x,
                        corners[2].y - corners[0].y);
    }

    void PlayCorrectSound(string tag)
    {
        switch (tag)
        {
            case "Garbage": objScript.effects.PlayOneShot(objScript.audioCli[2]); break;
            case "Medicine": objScript.effects.PlayOneShot(objScript.audioCli[3]); break;
            case "Fire": objScript.effects.PlayOneShot(objScript.audioCli[4]); break;
            case "b2": objScript.effects.PlayOneShot(objScript.audioCli[5]); break;
            case "e61": objScript.effects.PlayOneShot(objScript.audioCli[6]); break;
            case "e46": objScript.effects.PlayOneShot(objScript.audioCli[7]); break;
            case "eskavators": objScript.effects.PlayOneShot(objScript.audioCli[8]); break;
            case "tractor1": objScript.effects.PlayOneShot(objScript.audioCli[9]); break;
            case "tractor2": objScript.effects.PlayOneShot(objScript.audioCli[10]); break;
            case "police": objScript.effects.PlayOneShot(objScript.audioCli[11]); break;
            case "cement": objScript.effects.PlayOneShot(objScript.audioCli[12]); break;
            case "bus": objScript.effects.PlayOneShot(objScript.audioCli[13]); break;
            default: break;
        }
    }

    void ResetToStart(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        }

        int i = System.Array.FindIndex(objScript.vehicles, v => v == go);
        if (i >= 0)
        {
            go.GetComponent<RectTransform>().localPosition = objScript.startCoordinates[i];
            ObjectScript.lastDragged = null;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        GameObject dragged = eventData.pointerDrag;
        RectTransform dragRect = dragged.GetComponent<RectTransform>();
        RectTransform placeRect = GetComponent<RectTransform>();

        placeZRot = dragRect.eulerAngles.z;
        vehicleZRot = placeRect.eulerAngles.z;
        rotDiff = Mathf.Abs(placeZRot - vehicleZRot);
        if (rotDiff > 180) rotDiff = 360 - rotDiff;

        placeSiz = dragRect.localScale;
        vehicleSiz = placeRect.localScale;
        xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
        ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

        float distance = Vector2.Distance(dragRect.anchoredPosition, placeRect.anchoredPosition);
        bool overlaps = RectOverlaps(dragRect, placeRect);

        bool closeEnough = distance <= 80f;
        bool rotationOK = rotDiff <= 15f;
        bool scaleOK = xSizeDiff <= 0.15f && ySizeDiff <= 0.15f;

        //  Must overlap and tag match to be valid
        if (dragged.CompareTag(tag) && closeEnough && rotationOK && scaleOK && overlaps)
        {
            Debug.Log($"Correct placement for {dragged.name}");
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
            Debug.Log($"Incorrect placement for {dragged.name}");
            objScript.rightPlace = false;
            objScript.effects.PlayOneShot(objScript.audioCli[1]);
            ResetToStart(dragged);
        }
    }

    // Helper: check if two RectTransforms overlap
    private bool RectOverlaps(RectTransform a, RectTransform b)
    {
        Rect ra = GetScreenRect(a);
        Rect rb = GetScreenRect(b);
        return ra.Overlaps(rb);
    }

    // Converts RectTransform to screen rect
    private Rect GetScreenRect(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector3 min = corners[0];
        Vector3 max = corners[2];
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }


    //  Play correct placement sound
    private void PlayCorrectSound(string tag)
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
            default: Debug.Log("Unknown tag detected (correct placement)"); break;
        }
    }

    //  Reset object to its starting position if placed wrong
    private void ResetToStart(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true; // make sure it can be dragged again
            cg.alpha = 1f;
        }

        int index = System.Array.FindIndex(objScript.vehicles, v => v == go);
        if (index >= 0 && index < objScript.startCoordinates.Length)
        {
            go.GetComponent<RectTransform>().localPosition = objScript.startCoordinates[index];
            ObjectScript.lastDragged = null;
            Debug.Log($" {go.name} reset to start position.");
        }
        else
        {
            Debug.LogWarning($" Reset failed: {go.name} not found in objScript.vehicles.");
        }
    }
}

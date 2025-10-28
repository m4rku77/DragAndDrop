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

        if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            if (eventData.pointerDrag.CompareTag(tag))
            {
                // Calculate rotation & size difference
                placeZRot = eventData.pointerDrag.GetComponent<RectTransform>().eulerAngles.z;
                vehicleZRot = GetComponent<RectTransform>().eulerAngles.z;
                rotDiff = Mathf.Abs(placeZRot - vehicleZRot);

                placeSiz = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                vehicleSiz = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
                ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);

                Debug.Log($"Rotation diff: {rotDiff}, X diff: {xSizeDiff}, Y diff: {ySizeDiff}");

                //  Easy mode placement (forgiving)
                if ((rotDiff <= 15 || (rotDiff >= 345 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.15 && ySizeDiff <= 0.15))
                {
                    Debug.Log(" Correct placement");
                    objScript.rightPlace = true;

                    RectTransform dragRect = eventData.pointerDrag.GetComponent<RectTransform>();
                    RectTransform placeRect = GetComponent<RectTransform>();

                    dragRect.anchoredPosition = placeRect.anchoredPosition;
                    dragRect.localRotation = placeRect.localRotation;
                    dragRect.localScale = placeRect.localScale;

                    //  Disable dragging for placed car (so WinManager counts it as done)
                    var dragComp = eventData.pointerDrag.GetComponent<DragAndDropScript>();
                    if (dragComp != null)
                        dragComp.enabled = false;

                    //  Play correct sound
                    PlayCorrectSound(eventData.pointerDrag.tag);
                }
                else
                {
                    //  Close but not correct
                    objScript.rightPlace = false;
                    objScript.effects.PlayOneShot(objScript.audioCli[1]);
                    ResetToStart(eventData.pointerDrag.tag);
                }
            }
            else
            {
                //  Completely wrong tag
                objScript.rightPlace = false;
                objScript.effects.PlayOneShot(objScript.audioCli[1]);
                ResetToStart(eventData.pointerDrag.tag);
            }
        }
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

    //  Reset to starting position if wrong
    private void ResetToStart(string tag)
    {
        int index = System.Array.FindIndex(objScript.vehicles, v => v.CompareTag(tag));
        if (index >= 0 && index < objScript.startCoordinates.Length)
        {
            objScript.vehicles[index].GetComponent<RectTransform>().localPosition =
                objScript.startCoordinates[index];
        }
        else
        {
            Debug.LogWarning($" Reset failed — unknown or missing tag: {tag}");
        }
    }
}

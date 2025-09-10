using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceScript : MonoBehaviour, IDropHandler
{
    private float placeZRot, vehicleZRot, rotDiff;
    private Vector3 placeSiz, vehicleSiz;
    private float xSizeDiff, ySizeDiff;
    public ObjectScript objScript;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnDrop(PointerEventData eventData)
    {
        if((eventData.pointerDrag != null) && 
            Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            if(eventData.pointerDrag.tag.Equals(tag))
            {
               placeZRot = 
                    eventData.pointerDrag.GetComponent<RectTransform>().transform.eulerAngles.z;
                
                vehicleZRot = 
                    GetComponent<RectTransform>().transform.eulerAngles.z;

                rotDiff = Mathf.Abs(placeZRot - vehicleZRot);
                Debug.Log("Rotation difference: " + rotDiff);

                placeSiz = eventData.pointerDrag.GetComponent<RectTransform>().localScale;
                vehicleSiz = GetComponent<RectTransform>().localScale;
                xSizeDiff = Mathf.Abs(placeSiz.x - vehicleSiz.x);
                ySizeDiff = Mathf.Abs(placeSiz.y - vehicleSiz.y);
                Debug.Log("X size difference: " + xSizeDiff);
                Debug.Log("Y size difference: " + ySizeDiff);

                if((rotDiff <= 5 || (rotDiff >= 355 && rotDiff <= 360)) &&
                    (xSizeDiff <= 0.05 && ySizeDiff <= 0.05)) {
                    Debug.Log("Correct place");
                    //.......
                }

            } else
            {
                objScript.rightPlace = false;
                objScript.effects.PlayOneShot(objScript.audioCli[1]);

                switch(eventData.pointerDrag.tag)
                {
                    case "Garbage":
                        objScript.vehicles[0].GetComponent<RectTransform>().localPosition = 
                            objScript.startCoordinates[0];
                        break;

                    case "Medicine":
                        objScript.vehicles[1].GetComponent<RectTransform>().localPosition =
                           objScript.startCoordinates[1];
                        break;

                    case "Fire":
                        objScript.vehicles[2].GetComponent<RectTransform>().localPosition =
                           objScript.startCoordinates[2];
                        break;

                    default:
                        Debug.Log("Unknown tag detected");
                        break;
                }
            }
        }
    }
}

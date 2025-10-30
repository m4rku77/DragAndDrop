using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    public float maxZoom = 530f, minZoom = 150f;
    public float pinchZoomSpeed = 0.9f;
    public float touchPanSpeed = 1f;

    public ScreenBoundriesScript screenBoundries;
    public Camera cam;

    float startZoom;
    Vector2 lastTouchPos;
    int panFingerId = -1;
    bool isTouchPanning = false;

    float lastTapTime = 0f;
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;

    void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (screenBoundries == null) screenBoundries = FindFirstObjectByType<ScreenBoundriesScript>();
    }

    void Start()
    {
        startZoom = cam.orthographicSize;
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (TransformationScript.isTransforming)
            return;

        HandleSingleTouch();
        HandlePinchZoom();

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void HandleSingleTouch()
    {
        if (Input.touchCount != 1)
            return;

        Touch t = Input.GetTouch(0);

        if (IsTouchingUI(t.position))
            return;

        if (t.phase == TouchPhase.Began)
        {
            float dt = Time.time - lastTapTime;
            if (dt <= doubleTapMaxDelay && Vector2.Distance(t.position, lastTouchPos) <= doubleTapMaxDistance)
            {
                StartCoroutine(ResetZoomSmooth());
                lastTapTime = 0f;
            }
            else
            {
                lastTapTime = Time.time;
            }

            lastTouchPos = t.position;
            panFingerId = t.fingerId;
            isTouchPanning = true;
        }
        else if (t.phase == TouchPhase.Moved && isTouchPanning && t.fingerId == panFingerId)
        {
            Vector2 delta = t.position - lastTouchPos;
            transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            lastTouchPos = t.position;
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            isTouchPanning = false;
            panFingerId = -1;
        }
    }

    bool IsTouchingUI(Vector2 touchPos)
    {
        PointerEventData data = new PointerEventData(EventSystem.current) { position = touchPos };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                return true;
        }
        return false;
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount != 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float prevDist = ((t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition)).magnitude;
        float currDist = (t0.position - t1.position).magnitude;
        cam.orthographicSize -= (currDist - prevDist) * pinchZoomSpeed;
    }

    Vector3 ScreenDeltaToWorldDelta(Vector2 delta)
    {
        float worldPerPixel = (cam.orthographicSize * 2f) / Screen.height;
        return new Vector3(delta.x * worldPerPixel, delta.y * worldPerPixel, 0f);
    }

    IEnumerator ResetZoomSmooth()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialZoom = cam.orthographicSize;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(initialZoom, startZoom, elapsed / duration);

            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);

            yield return null;
        }

        cam.orthographicSize = startZoom;
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }
}

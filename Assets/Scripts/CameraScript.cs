using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float minZoom = 150f;
    private float maxZoom;
    public float pinchZoomSpeed = 0.9f;
    public float mouseZoomSpeed = 150f;

    [Header("Pan Settings")]
    public float touchPanSpeed = 1f;
    public float mouseFollowSpeed = 1f;

    [Header("References")]
    public Camera cam;
    public ScreenBoundriesScript screenBoundries;

    private Vector2 lastTouchPos;
    private int panFingerId = -1;
    private bool isTouchPanning = false;

    private float lastTapTime = 0f;
    public float doubleTapMaxDelay = 0.4f;
    public float doubleTapMaxDistance = 100f;

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (screenBoundries == null)
            screenBoundries = FindFirstObjectByType<ScreenBoundriesScript>();
    }

    void Start()
    {
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void Update()
    {
        if (TransformationScript.isTransforming)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleDesktopInput();
#else
        HandleTouchInput();
#endif

        if (Input.touchCount == 2)
            HandlePinch();

        UpdateMaxZoom();
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    #region Desktop Input
    void HandleDesktopInput()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return; // Ignore input over UI

        DesktopFollowCursor();

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
            cam.orthographicSize -= scroll * mouseZoomSpeed;
    }

    void DesktopFollowCursor()
    {
        Vector3 mouse = Input.mousePosition;

        if (mouse.x < 0 || mouse.x > Screen.width || mouse.y < 0 || mouse.y > Screen.height)
            return;

        Vector3 screenPoint = new Vector3(mouse.x, mouse.y, cam.nearClipPlane);
        Vector3 targetWorld = cam.ScreenToWorldPoint(screenPoint);
        Vector3 desired = new Vector3(targetWorld.x, targetWorld.y, transform.position.z);

        transform.position = Vector3.Lerp(transform.position, desired, mouseFollowSpeed * Time.deltaTime);
    }
    #endregion

    #region Touch Input
    void HandleTouchInput()
    {
        if (Input.touchCount != 1)
            return;

        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began && IsTouchingUIButton(t.position))
            return; // Ignore touches that start on UI

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
        else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && isTouchPanning && t.fingerId == panFingerId)
        {
            if (!IsTouchingUIButton(t.position))
            {
                Vector2 delta = t.position - lastTouchPos;
                transform.Translate(ScreenDeltaToWorldDelta(delta) * touchPanSpeed, Space.World);
            }
            lastTouchPos = t.position;
        }
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
        {
            isTouchPanning = false;
            panFingerId = -1;
        }
    }
    #endregion

    bool IsTouchingUIButton(Vector2 touchPos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = touchPos
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                return true;
        }

        return false;
    }

    void HandlePinch()
    {
        if (Input.touchCount < 2)
            return;

        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
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
        float targetZoom = maxZoom;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cam.orthographicSize = Mathf.Lerp(initialZoom, targetZoom, elapsed / duration);
            screenBoundries.RecalculateBounds();
            transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
            yield return null;
        }

        cam.orthographicSize = targetZoom;
        screenBoundries.RecalculateBounds();
        transform.position = screenBoundries.GetClampedCameraPosition(transform.position);
    }

    void UpdateMaxZoom()
    {
        if (screenBoundries == null || cam == null)
            return;

        Rect wb = screenBoundries.worldBounds;
        float maxZoomHeight = wb.height / 2f;
        float maxZoomWidth = (wb.width / 2f) / cam.aspect;
        maxZoom = Mathf.Min(maxZoomHeight, maxZoomWidth);
    }
}

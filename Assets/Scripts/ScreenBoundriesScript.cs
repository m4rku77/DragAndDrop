using UnityEngine;

public class ScreenBoundriesScript : MonoBehaviour
{
    [HideInInspector] public Vector3 screenPoint, offset;
    [HideInInspector] public float minX, maxX, minY, maxY;

    [Header("World Limits (Edit for your level size)")]
    public Rect worldBounds = new Rect(-960, -540, 1920, 1080);

    [Range(0f, 0.5f)]
    public float padding = 0.02f;

    public Camera targetCamera;

    public float minCamX { get; private set; }
    public float maxCamX { get; private set; }
    public float minCamY { get; private set; }
    public float maxCamY { get; private set; }

    float lastOrthoSize;
    float lastAspect;
    Vector3 lastCamPos;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        RecalculateBounds();
    }

    void Update()
    {
        if (targetCamera == null)
            return;

        bool changed = false;

        if (!Mathf.Approximately(targetCamera.orthographicSize, lastOrthoSize))
            changed = true;

        if (!Mathf.Approximately(targetCamera.aspect, lastAspect))
            changed = true;

        if (targetCamera.transform.position != lastCamPos)
            changed = true;

        if (changed)
            RecalculateBounds();
    }

    public void RecalculateBounds()
    {
        if (targetCamera == null)
            return;

        float wbMinX = worldBounds.xMin;
        float wbMaxX = worldBounds.xMax;
        float wbMinY = worldBounds.yMin;
        float wbMaxY = worldBounds.yMax;

        float halfH = targetCamera.orthographicSize;
        float halfW = halfH * targetCamera.aspect;

        // Horizontal camera clamp
        if (halfW * 2f >= (wbMaxX - wbMinX))
        {
            minCamX = maxCamX = (wbMinX + wbMaxX) * 0.5f;
        }
        else
        {
            minCamX = wbMinX + halfW;
            maxCamX = wbMaxX - halfW;
        }

        // Vertical camera clamp
        if (halfH * 2f >= (wbMaxY - wbMinY))
        {
            minCamY = maxCamY = (wbMinY + wbMaxY) * 0.5f;
        }
        else
        {
            minCamY = wbMinY + halfH;
            maxCamY = wbMaxY - halfH;
        }

        lastOrthoSize = targetCamera.orthographicSize;
        lastAspect = targetCamera.aspect;
        lastCamPos = targetCamera.transform.position;
    }

    // Clamp draggable object inside world
    public Vector2 GetClampedPosition(Vector3 curPosition)
    {
        float shrinkW = worldBounds.width * padding;
        float shrinkH = worldBounds.height * padding;

        float wbMinX = worldBounds.xMin + shrinkW;
        float wbMaxX = worldBounds.xMax - shrinkW;
        float wbMinY = worldBounds.yMin + shrinkH;
        float wbMaxY = worldBounds.yMax - shrinkH;

        float x = Mathf.Clamp(curPosition.x, wbMinX, wbMaxX);
        float y = Mathf.Clamp(curPosition.y, wbMinY, wbMaxY);

        return new Vector2(x, y);
    }

    // Clamp camera inside world
    public Vector3 GetClampedCameraPosition(Vector3 desiredCenter)
    {
        float x = Mathf.Clamp(desiredCenter.x, minCamX, maxCamX);
        float y = Mathf.Clamp(desiredCenter.y, minCamY, maxCamY);
        return new Vector3(x, y, desiredCenter.z);
    }
}

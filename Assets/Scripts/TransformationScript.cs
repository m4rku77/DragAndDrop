using UnityEngine;

public class TransformationScript : MonoBehaviour
{
    [Header("Adjust Speed")]
    public float rotateSpeed = 45f;     // degrees per second
    public float scaleSpeed = 0.3f;     // scale units per second
    public float minScale = 0.3f;
    public float maxScale = 0.9f;

    void Update()
    {
        if (ObjectScript.lastDragged == null) return;

        var rect = ObjectScript.lastDragged.GetComponent<RectTransform>();

        // Rotation
        if (Input.GetKey(KeyCode.Z))
            rect.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.X))
            rect.Rotate(0, 0, -rotateSpeed * Time.deltaTime);

        //  Scaling
        Vector3 scale = rect.localScale;

        if (Input.GetKey(KeyCode.UpArrow))
            scale.y = Mathf.Clamp(scale.y + scaleSpeed * Time.deltaTime, minScale, maxScale);

        if (Input.GetKey(KeyCode.DownArrow))
            scale.y = Mathf.Clamp(scale.y - scaleSpeed * Time.deltaTime, minScale, maxScale);

        if (Input.GetKey(KeyCode.RightArrow))
            scale.x = Mathf.Clamp(scale.x + scaleSpeed * Time.deltaTime, minScale, maxScale);

        if (Input.GetKey(KeyCode.LeftArrow))
            scale.x = Mathf.Clamp(scale.x - scaleSpeed * Time.deltaTime, minScale, maxScale);

        rect.localScale = scale;
    }
}

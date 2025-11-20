using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class HanoiBlock : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 mouseOffset;
    private Camera cam;

    [Header("Settings")]
    public float stickSnapTolerance = 0.5f;
    public float fallSpeed = 50f; // fast fall speed
    public LayerMask stickLayer;  // layer for sticks (trigger only)
    public LayerMask blockLayer;  // layer for blocks including floor

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0; // manual fall
    }

    void OnMouseDown()
    {
        if (!IsTopBlock()) return;

        isDragging = true;
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseOffset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = new Vector3(mousePos.x + mouseOffset.x, mousePos.y + mouseOffset.y, transform.position.z);
        rb.MovePosition(targetPos);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        // Snap X to nearest stick if within tolerance
        Collider2D stick = Physics2D.OverlapCircle(transform.position, stickSnapTolerance, stickLayer);
        if (stick)
        {
            Vector3 pos = transform.position;
            pos.x = stick.transform.position.x;
            transform.position = pos;
        }

        // Start fast fall immediately
        rb.gravityScale = 0; // manual control
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    bool IsTopBlock()
    {
        BoxCollider2D myBox = GetComponent<BoxCollider2D>();
        Collider2D[] allBlocks = Physics2D.OverlapBoxAll(transform.position, new Vector2(0.1f, 100f), 0f, blockLayer);

        foreach (Collider2D other in allBlocks)
        {
            if (other.gameObject == gameObject) continue;

            // Check if the other block is on the same stick (x close enough)
            if (Mathf.Abs(other.transform.position.x - transform.position.x) < 0.5f)
            {
                // If the other block is above this one, we are NOT the top
                if (other.bounds.min.y > myBox.bounds.max.y)
                {
                    return false;
                }
            }
        }

        return true; // no block above on this stick
    }

}

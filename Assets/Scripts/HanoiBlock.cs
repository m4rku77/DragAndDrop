using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class HanoiBlock : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 mouseOffset;
    private Camera cam;

    private Vector3 lastValidPosition; // position before dragging

    [Header("Settings")]
    public float stickSnapTolerance = 10f;
    public float fallSpeed = 90f; // fast fall speed
    public LayerMask stickLayer;  // layer for sticks (trigger only)
    public LayerMask blockLayer;  // layer for blocks including floor

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        cam = Camera.main;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;
    }

    void OnMouseDown()
    {
        if (!IsTopBlock()) return;

        lastValidPosition = transform.position;

        isDragging = true;
        rb.linearVelocity = Vector2.zero;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseOffset = transform.position - mousePos;
    }

    public float dragSmooth = 100f; // higher = more responsive

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z); // distance to camera
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        worldPos += mouseOffset;
        worldPos.z = transform.position.z;

        // **Directly set Transform.position for instant response**
        transform.position = worldPos;
    }




    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        BoxCollider2D myBox = GetComponent<BoxCollider2D>();
        Vector2 bottomPos = new Vector2(transform.position.x, transform.position.y - myBox.bounds.extents.y);

        // Check all sticks overlapping the block bottom
        Collider2D stick = Physics2D.OverlapCircle(bottomPos, stickSnapTolerance, stickLayer);

        if (stick != null)
        {
            // Snap X immediately to stick center for proper fall
            Vector3 pos = transform.position;
            pos.x = stick.transform.position.x;
            transform.position = pos;

            // Start falling
            StartCoroutine(FallAnimation());
        }
        else
        {
            // Not touching stick -> revert
            transform.position = lastValidPosition;
        }
    }




    bool IsTopBlock()
    {
        BoxCollider2D myBox = GetComponent<BoxCollider2D>();
        float stickTolerance = 0.3f;
        float checkHeight = 0.1f;

        Vector2 boxSize = new Vector2(stickTolerance, checkHeight);
        Vector2 boxCenter = new Vector2(transform.position.x, myBox.bounds.max.y + checkHeight / 2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, blockLayer);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject != gameObject)
                return false;
        }

        return true;
    }

    private IEnumerator FallAnimation()
    {
        rb.linearVelocity = Vector2.zero;

        float liftAmount = 0.3f;
        float liftTime = 0.1f;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * liftAmount;

        // Lift slightly
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / liftTime;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        BoxCollider2D myBox = GetComponent<BoxCollider2D>();
        float halfHeight = myBox.bounds.extents.y;

        while (true)
        {
            // Move down manually
            transform.position += Vector3.down * fallSpeed * Time.unscaledDeltaTime;

            Vector2 bottomPos = new Vector2(transform.position.x, transform.position.y - halfHeight - 0.01f);

            // Check for any stick below with the block's width
            Vector2 boxSize = new Vector2(myBox.bounds.size.x, 0.1f);
            Collider2D stickBelow = Physics2D.OverlapBox(bottomPos, boxSize, 0f, stickLayer);

            if (stickBelow != null)
            {
                // Snap X to stick center for a proper landing
                transform.position = new Vector3(stickBelow.transform.position.x, transform.position.y, transform.position.z);
            }

            // Check for blocks or floor below
            RaycastHit2D hit = Physics2D.BoxCast(bottomPos, boxSize, 0f, Vector2.down, 0.05f, blockLayer);
            if (hit.collider != null)
            {
                // Tower of Hanoi rule
                if (TryGetBlockSize(tag, out int currentSize) &&
                    TryGetBlockSize(hit.collider.tag, out int belowSize))
                {
                    if (currentSize < belowSize)
                    {
                        // Invalid move -> revert
                        transform.position = lastValidPosition;
                        break;
                    }
                }

                float top = hit.collider.bounds.max.y;
                transform.position = new Vector3(transform.position.x, top + halfHeight, transform.position.z);
                break;
            }

            yield return null;
        }
    }

    // Helper method to safely parse block tag
    private bool TryGetBlockSize(string tag, out int size)
        {
            size = -1;
            if (tag.EndsWith("block"))
            {
                string numberPart = tag.Replace("block", "");
                return int.TryParse(numberPart, out size);
            }
            return false;
        }

    }
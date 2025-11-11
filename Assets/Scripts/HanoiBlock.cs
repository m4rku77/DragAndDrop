using UnityEngine;
using System.Collections;
using System.Linq;

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
    public float slideSpeed = 5f; // speed of sliding to stick
    public LayerMask stickLayer;  // layer for sticks (trigger only)
    public LayerMask blockLayer;  // layer for blocks including floor

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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
        rb.gravityScale = 1;

        // Check for nearby stick
        Collider2D stick = Physics2D.OverlapCircle(transform.position, stickSnapTolerance, stickLayer);
        if (stick)
        {
            Vector3 targetPos = stick.transform.position;

            // Get top block on the stick to stack above it
            targetPos.y = GetLowestAvailableY(stick.transform);


            // Slide smoothly to target position
            StartCoroutine(SlideToStick(targetPos, slideSpeed));
        }
    }

    IEnumerator SlideToStick(Vector3 targetPosition, float speed)
    {
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        rb.gravityScale = 1;
    }

    bool IsTopBlock()
    {
        // Raycast slightly up to see if any block is above
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.1f, Vector2.up, 0.2f, blockLayer);
        if (hit.collider == null) return true;
        return !hit.collider.isTrigger;
    }

    GameObject GetTopBlockOnStick(Transform stick)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(stick.position, 0.5f, blockLayer);
        GameObject top = null;
        float highestY = float.MinValue;

        foreach (var c in nearby)
        {
            if (c.transform.position.y > highestY && c.gameObject != gameObject)
            {
                highestY = c.transform.position.y;
                top = c.gameObject;
            }
        }

        return top;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Snap to floor if touched
        if (collision.gameObject.CompareTag("floor"))
        {
            Vector3 pos = transform.position;
            float floorTop = collision.collider.bounds.max.y;
            float blockHeight = GetComponent<BoxCollider2D>().bounds.size.y;
            pos.y = floorTop + blockHeight / 2f;
            transform.position = pos;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1;
        }
    }

    float GetLowestAvailableY(Transform stick)
    {
        float blockHeight = GetComponent<BoxCollider2D>().bounds.size.y;

        // Find all blocks in the blockLayer
        Collider2D[] allBlocks = GameObject.FindObjectsOfType<HanoiBlock>()
            .Select(b => b.GetComponent<Collider2D>())
            .Where(c => ((1 << c.gameObject.layer) & blockLayer) != 0 && c.gameObject != gameObject)
            .ToArray();

        float highestY = stick.position.y; // start at stick bottom

        foreach (var c in allBlocks)
        {
            // check if block is “on this stick” (close enough in X)
            if (Mathf.Abs(c.transform.position.x - stick.position.x) < 0.5f)
            {
                float cTop = c.bounds.max.y;
                if (cTop > highestY)
                    highestY = cTop;
            }
        }

        return highestY + blockHeight / 2f;
    }

}

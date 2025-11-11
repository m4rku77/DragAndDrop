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
    public LayerMask stickLayer;
    public LayerMask blockLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // prevent tunneling
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

        // Check for stick snap
        Collider2D stick = Physics2D.OverlapCircle(transform.position, stickSnapTolerance, stickLayer);
        if (stick)
        {
            TrySnapToStick(stick.transform);
        }
    }

    void TrySnapToStick(Transform stick)
    {
        GameObject topBlock = GetTopBlockOnStick(stick);

        if (topBlock != null)
        {
            int mySize = GetBlockSize();
            int topSize = GetBlockSize(topBlock);

            if (mySize < topSize)
            {
                SnapAboveBlock(topBlock);
            }
        }
        else
        {
            // Snap to base of stick
            Vector3 pos = stick.position;
            pos.y = stick.position.y + 0.5f;
            transform.position = pos;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1;
        }
    }

    GameObject GetTopBlockOnStick(Transform stick)
    {
        Collider2D[] nearby = Physics2D.OverlapCircleAll(stick.position, 0.5f, blockLayer);
        GameObject top = null;
        float highestY = float.MinValue;

        foreach (var c in nearby)
        {
            if (c.transform.position.y > highestY)
            {
                highestY = c.transform.position.y;
                top = c.gameObject;
            }
        }

        return top;
    }

    void SnapAboveBlock(GameObject lowerBlock)
    {
        Vector3 pos = lowerBlock.transform.position;
        float height = lowerBlock.GetComponent<BoxCollider2D>().bounds.size.y;
        pos.y += height;
        transform.position = pos;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
    }

    bool IsTopBlock()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.2f, Vector2.up, 0.5f, blockLayer);
        return hit.collider == null;
    }

    int GetBlockSize()
    {
        string num = System.Text.RegularExpressions.Regex.Replace(gameObject.tag, "[^0-9]", "");
        if (int.TryParse(num, out int size))
            return size;
        return 0;
    }

    int GetBlockSize(GameObject obj)
    {
        string num = System.Text.RegularExpressions.Regex.Replace(obj.tag, "[^0-9]", "");
        if (int.TryParse(num, out int size))
            return size;
        return 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug logs for testing
        Debug.Log(gameObject.name + " collided with: " + collision.gameObject.name);

        // Floor collision
        if (collision.gameObject.CompareTag("floor"))
        {
            Debug.Log(gameObject.name + " hit the floor!");

            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;

            // Snap on top of floor
            Vector3 pos = transform.position;
            float floorTop = collision.collider.bounds.max.y;
            float blockHeight = GetComponent<BoxCollider2D>().bounds.size.y;
            pos.y = floorTop + blockHeight / 2f;
            transform.position = pos;
        }
    }
}

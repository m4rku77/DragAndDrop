using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BlockSnap : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Blocks and Sticks")]
    public List<Transform> blocks;      // Drag your block GameObjects here
    public List<Transform> sticks;      // Drag your stick GameObjects here
    public Transform floor;             // Drag your floor GameObject here

    private Rigidbody2D draggingRb;
    private Transform draggingBlock;
    private Vector3 offset;

    private void Awake()
    {
        // Ensure blocks have Rigidbody2D and BoxCollider2D
        foreach (var block in blocks)
        {
            if (block.GetComponent<Rigidbody2D>() == null)
            {
                var rb = block.gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f; // normal gravity
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            if (block.GetComponent<BoxCollider2D>() == null)
                block.gameObject.AddComponent<BoxCollider2D>();
        }

        // Ensure sticks have BoxCollider2D (trigger optional)
        foreach (var stick in sticks)
        {
            if (stick.GetComponent<BoxCollider2D>() == null)
                stick.gameObject.AddComponent<BoxCollider2D>();
        }

        // Floor should have BoxCollider2D and Rigidbody2D (set to static)
        if (floor != null)
        {
            if (floor.GetComponent<BoxCollider2D>() == null)
                floor.gameObject.AddComponent<BoxCollider2D>();
            if (floor.GetComponent<Rigidbody2D>() == null)
            {
                var rb = floor.gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
            }
        }
    }

    // Call these from UI Event Triggers or implement custom raycast for 2D
    public void OnPointerDown(PointerEventData eventData)
    {
        // Raycast to find the block clicked
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null && blocks.Contains(hit.collider.transform))
        {
            draggingBlock = hit.collider.transform;
            draggingRb = draggingBlock.GetComponent<Rigidbody2D>();

            // Offset for smooth dragging
            offset = draggingBlock.position - new Vector3(worldPos.x, worldPos.y, draggingBlock.position.z);

            // Disable gravity while dragging
            draggingRb.gravityScale = 0f;
            draggingRb.linearVelocity = Vector2.zero;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingBlock == null) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        draggingBlock.position = new Vector3(worldPos.x, worldPos.y, draggingBlock.position.z) + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingBlock == null) return;

        // Re-enable gravity
        draggingRb.gravityScale = 1f;

        // Snap to stick if close enough
        Transform closestStick = null;
        float closestDist = Mathf.Infinity;

        foreach (var stick in sticks)
        {
            float dist = Vector2.Distance(draggingBlock.position, stick.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestStick = stick;
            }
        }

        if (closestStick != null)
        {
            // Find the top of existing blocks on this stick
            float topY = closestStick.position.y;
            foreach (var block in blocks)
            {
                if (block == draggingBlock) continue;
                if (Mathf.Abs(block.position.x - closestStick.position.x) < 0.1f)
                {
                    float blockTop = block.position.y + block.GetComponent<BoxCollider2D>().bounds.size.y / 2f;
                    if (blockTop > topY)
                        topY = blockTop;
                }
            }

            // Snap block on top
            float blockHeight = draggingBlock.GetComponent<BoxCollider2D>().bounds.size.y;
            draggingBlock.position = new Vector3(closestStick.position.x, topY + blockHeight / 2f, draggingBlock.position.z);
            draggingRb.linearVelocity = Vector2.zero;
        }

        draggingBlock = null;
        draggingRb = null;
    }
}

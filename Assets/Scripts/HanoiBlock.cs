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
    public float fallSpeed = 90f;
    public LayerMask stickLayer;
    public LayerMask blockLayer;
    public float dragSmooth = 100f;

    [Header("Audio")]
    public AudioClip fallSound;
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        cam = Camera.main;

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 0;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    void OnMouseDown()
    {
        // Play click sound
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);

        if (!IsTopBlock()) return;

        lastValidPosition = transform.position;

        isDragging = true;
        rb.linearVelocity = Vector2.zero;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseOffset = transform.position - mousePos;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        worldPos += mouseOffset;
        worldPos.z = transform.position.z;

        transform.position = worldPos;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        BoxCollider2D myBox = GetComponent<BoxCollider2D>();

        // Wider / more forgiving detection at the bottom
        float extraWidth = 0.2f;
        Vector2 checkSize = new Vector2(myBox.bounds.size.x + extraWidth, 0.3f);
        Vector2 checkCenter = new Vector2(
            myBox.bounds.center.x,
            myBox.bounds.min.y + checkSize.y * 0.5f
        );

        // Check any stick overlapping near the bottom of the block
        Collider2D stick = Physics2D.OverlapBox(checkCenter, checkSize, 0f, stickLayer);

        if (stick != null)
        {
            Vector3 pos = transform.position;
            pos.x = stick.transform.position.x;
            transform.position = pos;

            StartCoroutine(FallAnimation());
        }
        else
        {
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
        // Play fall sound
        if (fallSound != null)
            audioSource.PlayOneShot(fallSound);

        rb.linearVelocity = Vector2.zero;

        float liftAmount = 0.3f;
        float liftTime = 0.1f;

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * liftAmount;

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
            transform.position += Vector3.down * fallSpeed * Time.unscaledDeltaTime;

            Vector2 bottomPos = new Vector2(transform.position.x, transform.position.y - halfHeight - 0.01f);
            Vector2 boxSize = new Vector2(myBox.bounds.size.x, 0.1f);

            // Align with stick below if any
            Collider2D stickBelow = Physics2D.OverlapBox(bottomPos, boxSize, 0f, stickLayer);
            if (stickBelow != null)
            {
                transform.position = new Vector3(
                    stickBelow.transform.position.x,
                    transform.position.y,
                    transform.position.z
                );
            }

            // Check for blocks or floor below
            RaycastHit2D hit = Physics2D.BoxCast(bottomPos, boxSize, 0f, Vector2.down, 0.05f, blockLayer);
            if (hit.collider != null)
            {
                // Tower of Hanoi rule:
                // You CANNOT place a bigger block on a smaller block.
                if (TryGetBlockSize(tag, out int currentSize) &&
                    TryGetBlockSize(hit.collider.tag, out int belowSize))
                {
                    Debug.Log($"Trying to place {tag} (size {currentSize}) on {hit.collider.tag} (size {belowSize})");

                    if (currentSize > belowSize)
                    {
                        // Invalid move -> revert
                        Debug.Log("Invalid move: bigger on smaller. Reverting.");
                        transform.position = lastValidPosition;
                        break;
                    }
                }

                float top = hit.collider.bounds.max.y;
                transform.position = new Vector3(
                    transform.position.x,
                    top + halfHeight,
                    transform.position.z
                );

                // Valid move finished here – count it
                if (MoveCounter.Instance != null)
                    MoveCounter.Instance.AddMove();

                // Check win condition after a successful placement
                if (HanoiWinManager.Instance != null)
                    HanoiWinManager.Instance.CheckWin();

                break;
            }

            yield return null;
        }
    }

    // 0block is biggest, 1block is second biggest, etc
    private bool TryGetBlockSize(string tag, out int size)
    {
        size = -1;

        if (!tag.EndsWith("block"))
            return false;

        string num = tag.Replace("block", "");
        if (!int.TryParse(num, out int index))
            return false;

        // Bigger physical block => bigger "size" number
        // 0block -> 100, 1block -> 99, 2block -> 98, ...
        size = 100 - index;

        return true;
    }
}

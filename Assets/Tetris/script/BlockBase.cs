using System.Collections;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    Rigidbody2D rb;
    int frame = 60;
    int nowfram = 0;
    float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0f, 0f, 90f);
            if (!IsRotationSafe())
            {
                transform.Rotate(0f, 0f, -90f);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            while (canmove(Vector3.down))
            {
                transform.position += new Vector3(0, -0.5f, 0);
            }
            float snappedY = Mathf.Round(transform.position.y * 2f) / 2f;
            transform.position = new Vector3(transform.position.x, snappedY, 0);
            OnHardDropSettle();
            return;
        }

        HandleInputMovement(KeyCode.RightArrow, Vector3.right, 0.5f);
        HandleInputMovement(KeyCode.LeftArrow, Vector3.left, -0.5f);
        HandleInputMovement(KeyCode.DownArrow, Vector3.down, -0.5f);
    }

    void HandleInputMovement(KeyCode key, Vector3 dir, float moveAmount)
    {
        if (Input.GetKeyDown(key))
        {
            if (canmove(dir)) transform.position += new Vector3(moveAmount, dir.y == 0 ? 0 : moveAmount, 0);
        }

        if (Input.GetKey(key))
        {
            timer += Time.deltaTime;
            if (timer >= 0.2f)
            {
                if (canmove(dir))
                {
                    if (dir.y == 0) transform.position += new Vector3(moveAmount, 0, 0);
                    else transform.position += new Vector3(0, moveAmount, 0);
                }
                timer = 0;
            }
        }
        else if (Input.GetKeyUp(key))
        {
            timer = 0;
        }
    }

    private void FixedUpdate()
    {
        nowfram++;
        if (nowfram >= frame)
        {
            fall();
            nowfram = 0;
        }
    }

    public void fall()
    {
        if (canmove(Vector3.down))
        {
            transform.position += new Vector3(0, -0.5f, 0);
        }
        else
        {
            this.enabled = false;
        }
    }

    void OnHardDropSettle()
    {
        this.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 rayStart = (Vector2)child.position + (Vector2)direction * 0.26f;
            RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, 0.1f);

            if (hit.collider != null)
            {
                if (hit.transform != transform && hit.transform.parent != transform)
                {
                    if (hit.collider.CompareTag("Floor") || hit.collider.CompareTag("Block"))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Collider2D hit = Physics2D.OverlapPoint(child.position);
            if (hit != null)
            {
                if (hit.transform != transform && hit.transform.parent != transform)
                {
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block"))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Floor") || collision.CompareTag("Block"))
        {
            this.enabled = false;
        }
    }
}
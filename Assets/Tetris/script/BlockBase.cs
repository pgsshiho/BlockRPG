using System.Collections;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    Rigidbody2D rb;
    int frame = 60;
    int nowfram = 0;
    float timer = 0f;
    Conebase cb;
    bool gameover = false ;
    public GameObject ghost;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
    }
    void LateUpdate() // 이동이 다 끝난 후 고스트 위치 계산
    {
        if (ghost != null)
        {
            UpdateGhostPosition();
        }
    }
    bool canmoveGhost(Vector3 direction)
    {
        foreach (Transform child in ghost.transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            targetPos.x = Mathf.Round(targetPos.x * 2f) / 2f;
            targetPos.y = Mathf.Round(targetPos.y * 2f) / 2f;

            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
            foreach (var hit in hits)
            {
                // 실제 블록(나)과 고스트 자신을 제외한 바닥/블록 감지
                if (hit.transform.parent != transform && hit.transform != transform &&
                    hit.transform.parent != ghost.transform && hit.transform != ghost.transform)
                {
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
                }
            }
        }
        return true;
    }
    void UpdateGhostPosition()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        // 가짜 하드드랍: 갈 수 있는 끝까지 내림
        while (canmoveGhost(Vector3.down))
        {
            ghost.transform.position += new Vector3(0, -0.5f, 0);
        }

        // 그리드 스냅
        ghost.transform.position = new Vector3(
            Mathf.Round(ghost.transform.position.x * 2f) / 2f,
            Mathf.Round(ghost.transform.position.y * 2f) / 2f,
            0
        );
    }
    void Update()
    {
        // 1. 회전 로직 (월킥 포함)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0f, 0f, 90f);
            if (!IsRotationSafe())
            {
                transform.position += new Vector3(1.0f, 0, 0);
                if (!IsRotationSafe())
                {
                    transform.position += new Vector3(-2.0f, 0, 0);
                    if (!IsRotationSafe())
                    {
                        transform.position += new Vector3(1.0f, 0, 0);
                        transform.Rotate(0f, 0f, -90f);
                    }
                }
            }
        }

        // 2. 하드 드랍
        if (Input.GetKeyDown(KeyCode.Space))
        {
            while (canmove(Vector3.down))
            {
                transform.position += new Vector3(0, -0.5f, 0);
            }
            // 소수점 오차 보정 (그리드 스냅)
            transform.position = new Vector3(Mathf.Round(transform.position.x * 2f) / 2f, Mathf.Round(transform.position.y * 2f) / 2f, 0);
            OnHardDropSettle();
            return;
        }

        // 3. 이동 로직
        HandleInputMovement(KeyCode.RightArrow, Vector3.right, 0.5f);
        HandleInputMovement(KeyCode.LeftArrow, Vector3.left, -0.5f);
        HandleInputMovement(KeyCode.DownArrow, Vector3.down, -0.5f);
    }

    void HandleInputMovement(KeyCode key, Vector3 dir, float moveAmount)
    {
        if (Input.GetKeyDown(key))
        {
            if (canmove(dir)) transform.position += (dir.y == 0) ? new Vector3(moveAmount, 0, 0) : new Vector3(0, moveAmount, 0);
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
            OnHardDropSettle();
        }
    }

    void OnHardDropSettle()
    {
        if (ghost != null) Destroy(ghost);
        this.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        if(transform.position.y >= 9.2f)
        {
            gameover = true;
            Debug.Log("게임오버");
        }
        cb.Clone();
    }

    // [중요] 레이캐스트 대신 OverlapPoint를 사용하여 겹침 현상 해결
    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            // 다음 이동할 좌표 계산
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            
            // 미세한 부동 소수점 오차 방지
            targetPos.x = Mathf.Round(targetPos.x * 2f) / 2f;
            targetPos.y = Mathf.Round(targetPos.y * 2f) / 2f;

            // 해당 지점에 있는 모든 콜라이더 검사
            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
            foreach (var hit in hits)
            {
                // 내 몸 조각이 아닌 다른 것이라면
                if (hit.transform.parent != transform && hit.transform != transform)
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

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(Mathf.Round(child.position.x * 2f) / 2f, Mathf.Round(child.position.y * 2f) / 2f);
            Collider2D[] hits = Physics2D.OverlapPointAll(pos);
            foreach (var hit in hits)
            {
                if (hit.transform.parent != transform && hit.transform != transform)
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
}
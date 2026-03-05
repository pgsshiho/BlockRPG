using System.Collections;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    Rigidbody2D rb;
    int frame = 60;
    int nowfram = 0;
    float timer = 0f;
    float lockDelayTimer = 0f;

    Conebase cb;
    GameManager gm;
    blockclear bc;

    public GameObject ghost;
    public bool isground = false;
    public int trying = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
        bc = FindAnyObjectByType<blockclear>();
        gm = FindAnyObjectByType<GameManager>();
        SnapToGrid();
    }

    void LateUpdate()
    {
        if (ghost != null && !gm.isON) UpdateGhostPosition();
    }

    void Update()
    {
        if (gm.isON) return;

        if (isground) lockDelayTimer += Time.deltaTime;

        // 1. 회전 로직 (안전성 강화)
        if (Input.GetKeyDown(KeyCode.UpArrow)) RotateBlock();

        // 2. 하드 드랍
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();

        // 3. 이동 처리
        HandleInputMovement(KeyCode.RightArrow, Vector3.right);
        HandleInputMovement(KeyCode.LeftArrow, Vector3.left);
        HandleInputMovement(KeyCode.DownArrow, Vector3.down);

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.DownArrow))
            timer = 0;
    }

    void RotateBlock()
    {
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // 1. 일단 회전
        if (!gameObject.name.Contains("ㅁ")) { 
        transform.Rotate(0, 0, 90);
        SnapToGrid();

        // 2. 만약 회전 직후가 안전하다면 바로 성공 처리
        if (IsRotationSafe())
        {
            Sound.instance.swing.Play();
            return;
        }

        // 3. 충돌했다면 월킥(Wall Kick) 시도 (T-스핀의 핵심)
        // 순서: 좌, 우, 하, 상, 대각선 순으로 체크 범위를 넓힘
        Vector3[] kickOffsets = {
        new Vector3(-0.5f, 0, 0),  // 좌
        new Vector3(0.5f, 0, 0),   // 우
        new Vector3(0, -0.5f, 0),  // 하 (T-스핀 시 아래로 찍어 누르기)
        new Vector3(-0.5f, -0.5f, 0),
        new Vector3(0.5f, -0.5f, 0),
        new Vector3(0, 0.5f, 0),   // 상
        new Vector3(-1.0f, 0, 0),  // 좌2
        new Vector3(1.0f, 0, 0)    // 우2
    };

        bool fixedPos = false;
        foreach (Vector3 offset in kickOffsets)
        {
            transform.position = originalPos + offset;
            SnapToGrid();

            if (IsRotationSafe())
            {
                fixedPos = true;
                break;
            }
        }

        // 4. 결과 처리
        if (!fixedPos)
        {
            // 어디로 밀어도 겹치면 회전 취소
            transform.position = originalPos;
            transform.rotation = originalRot;
        }
        else
        {
            if (isground) lockDelayTimer = 0;
            trying++;
            Sound.instance.swing.Play();

            // [추가] T-스핀 판정 로그 (선택 사항)
            if (gameObject.name.Contains("T") || gameObject.name.Contains("t"))
            {
                Debug.Log("T-Spin Potential Rotation Success!");
            }
        }
        }
    }

    void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x * 2f) / 2f,
            Mathf.Round(transform.position.y * 2f) / 2f, 0);
    }

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(
                Mathf.Round(child.position.x * 2f) / 2f,
                Mathf.Round(child.position.y * 2f) / 2f);

            Collider2D[] hits = Physics2D.OverlapPointAll(pos);
            foreach (var hit in hits)
            {
                if (hit.transform.parent != transform && hit.transform != transform)
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
            }
        }
        return true;
    }

    void HandleInputMovement(KeyCode key, Vector3 dir)
    {
        if (Input.GetKeyDown(key))
        {
            if (canmove(dir)) { transform.position += dir * 0.5f; SnapToGrid(); }
        }
        if (Input.GetKey(key))
        {
            timer += Time.deltaTime;
            if (timer >= 0.15f)
            {
                if (canmove(dir)) { transform.position += dir * 0.5f; SnapToGrid(); }
                timer = 0.12f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (gm.isON) return;
        UpdateLevelSpeed();
        nowfram++;
        if (nowfram >= frame) { Fall(); nowfram = 0; }
    }

    public void Fall()
    {
        if (canmove(Vector3.down)) { transform.position += new Vector3(0, -0.5f, 0); isground = false; lockDelayTimer = 0; }
        else { isground = true; if (lockDelayTimer >= 0.5f || trying >= 15) OnHardDropSettle(); }
    }

    void OnHardDropSettle()
    {
        if (ghost != null) Destroy(ghost);
        SnapToGrid();
        foreach (Transform child in transform)
        {
            Vector2Int index = blockclear.PosToIndex(child.position);
            if (index.x >= 0 && index.x < blockclear.width && index.y >= 0 && index.y < blockclear.height)
            {
                blockclear.grid[index.x, index.y] = child;
                child.tag = "Block";
                Debug.Log($"저장 위치: [X:{index.x}, Y:{index.y}]");
            }
        }
        bc.DeleteFullLines();
        Sound.instance.drop.Play();
        this.enabled = false;
        cb.Clone();
    }

    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            targetPos.x = Mathf.Round(targetPos.x * 2f) / 2f;
            targetPos.y = Mathf.Round(targetPos.y * 2f) / 2f;
            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
            foreach (var hit in hits)
                if (hit.transform.parent != transform && hit.transform != transform)
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
        }
        return true;
    }

    void UpdateGhostPosition()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        while (canmoveGhost(Vector3.down)) ghost.transform.position += new Vector3(0, -0.5f, 0);
        ghost.transform.position = new Vector3(Mathf.Round(ghost.transform.position.x * 2f) / 2f, Mathf.Round(ghost.transform.position.y * 2f) / 2f, 0);
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
                if (hit.transform.parent != transform && hit.transform != transform && hit.transform.parent != ghost.transform)
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
        }
        return true;
    }

    void HardDrop()
    {
        while (canmove(Vector3.down)) transform.position += new Vector3(0, -0.5f, 0);
        SnapToGrid(); OnHardDropSettle();
    }

    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;
        int[] frames = { 60, 54, 48, 42, 36, 32, 28, 24, 20, 18, 16, 14, 12, 10, 8, 7, 6, 5, 4 };
        frame = 3;
        for (int i = 0; i < 19; i++) { if (s < (i + 1) * 2000) { frame = frames[i]; break; } }
        frame = Mathf.Max(1, frame + (3 - d) * 5);
    }
}
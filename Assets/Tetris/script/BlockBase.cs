using System.Collections;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    Rigidbody2D rb;
    int frame = 60;
    int nowfram = 0;
    float timer = 0f;
    float lockDelayTimer = 0f;

    int dropDistance = 1;

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

        if (Input.GetKeyDown(KeyCode.UpArrow)) RotateBlock();
        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();

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

        if (!gameObject.name.Contains("ㅁ"))
        {
            transform.Rotate(0, 0, -90);
            SnapToGrid();

            if (IsRotationSafe())
            {
                Sound.instance.swing.Play();
                return;
            }

            Vector3[] kickOffsets = {
                new Vector3(-0.5f, 0, 0),
                new Vector3(0.5f, 0, 0),
                new Vector3(0, -0.5f, 0),
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0, 0.5f, 0),
                new Vector3(-1.0f, 0, 0),
                new Vector3(1.0f, 0, 0)
            };

            bool fixedPos = false;
            foreach (Vector3 offset in kickOffsets)
            {
                transform.position = originalPos + offset;
                SnapToGrid();

                if (IsRotationSafe())
                {
                    if (gameObject.name.Contains("Z"))
                    {
                        Debug.Log("z-spin");
                    }
                    else if (gameObject.name.Contains("ㅗ")){
                        Debug.Log("T-spin");
                    }
                    fixedPos = true;
                    break;
                }
            }

            if (!fixedPos)
            {
                transform.position = originalPos;
                transform.rotation = originalRot;
            }
            else
            {
                if (isground) lockDelayTimer = 0;
                trying++;
                Sound.instance.swing.Play();

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

    // 🔥 1. 회전 시 맵 밖으로 뚫고 나가는지 체크하는 로직 추가!
    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = new Vector2(
                Mathf.Round(child.position.x * 2f) / 2f,
                Mathf.Round(child.position.y * 2f) / 2f);

            // [핵심 추가] 그리드 좌표를 가져와서 맵을 벗어났으면 즉시 회전 취소!
            Vector2Int index = blockclear.PosToIndex(pos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;

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
        for (int i = 0; i < dropDistance; i++)
        {
            if (canmove(Vector3.down))
            {
                transform.position += new Vector3(0, -0.5f, 0);
                isground = false;
                lockDelayTimer = 0;
            }
            else
            {
                isground = true;
                break;
            }
        }

        if (isground)
        {
            if (lockDelayTimer >= 0.5f || trying >= 15)
                OnHardDropSettle();
        }
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

    // 🔥 2. 이동 시에도 맵 밖으로 뚫고 나가는지 체크!
    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            targetPos.x = Mathf.Round(targetPos.x * 2f) / 2f;
            targetPos.y = Mathf.Round(targetPos.y * 2f) / 2f;

            // [핵심 추가] 맵 바깥 범위 차단
            Vector2Int index = blockclear.PosToIndex(targetPos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;

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

        // 🔥 3. 무한 루프 억제기 장착! (안전장치 50번 제한)
        int loopSafe = 0;
        while (canmoveGhost(Vector3.down) && loopSafe < 50)
        {
            ghost.transform.position += new Vector3(0, -0.5f, 0);
            loopSafe++;
        }

        ghost.transform.position = new Vector3(Mathf.Round(ghost.transform.position.x * 2f) / 2f, Mathf.Round(ghost.transform.position.y * 2f) / 2f, 0);
    }

    bool canmoveGhost(Vector3 direction)
    {
        foreach (Transform child in ghost.transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            targetPos.x = Mathf.Round(targetPos.x * 2f) / 2f;
            targetPos.y = Mathf.Round(targetPos.y * 2f) / 2f;

            // [핵심 추가] 고스트 블록도 맵 밖으로 못 나가게 막음
            Vector2Int index = blockclear.PosToIndex(targetPos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;

            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
            foreach (var hit in hits)
                if (hit.transform.parent != transform && hit.transform != transform && hit.transform.parent != ghost.transform)
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
        }
        return true;
    }

    void HardDrop()
    {
        // 🔥 여기도 무한 루프 억제기 장착!
        int loopSafe = 0;
        while (canmove(Vector3.down) && loopSafe < 50)
        {
            transform.position += new Vector3(0, -0.5f, 0);
            loopSafe++;
        }
        SnapToGrid(); OnHardDropSettle();
    }

    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;
        int[] frames = { 60, 54, 48, 42, 36, 32, 28, 24, 20, 18, 16, 14, 12, 10, 8, 7, 6, 5, 4 };

        int baseFrame = 60;
        for (int i = 0; i < 19; i++)
        {
            if (s < (i + 1) * 2000) { baseFrame = frames[i]; break; }
        }

        int rawFrame = baseFrame + (3 - d) * 7;

        if (rawFrame >= 1)
        {
            frame = rawFrame;
            dropDistance = 1;
        }
        else
        {
            frame = 1;
            dropDistance = 1 + Mathf.CeilToInt(Mathf.Abs(rawFrame) / 5f);
        }
    }
}
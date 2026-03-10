using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    public static List<BlockBase> AllBlocks = new List<BlockBase>();
    Rigidbody2D rb;
    int frame = 60;
    int nowfram = 0;
    float lockDelayTimer = 0f;
    int dropDistance = 1;

    Conebase cb;
    GameManager gm;
    blockclear bc;
    KeyBinding key;

    public GameObject ghost;
    public bool isground = false;
    public int trying = 0;
    public bool Tspin = false; // T-Spin 판정용
    public bool rightclock = false;
    public bool rightleft = false;

    float dasDelay = 0.15f;
    float arrDelay = 0.03f;
    float moveTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
        bc = FindAnyObjectByType<blockclear>();
        gm = FindAnyObjectByType<GameManager>();
        key = FindAnyObjectByType<KeyBinding>();
        SnapToGrid();
    }

    void LateUpdate()
    {
        if (ghost != null && gm != null && !gm.isON) UpdateGhostPosition();
    }

    void Update()
    {
        if (gm != null && gm.isON || key == null) return;

        if (isground) lockDelayTimer += Time.deltaTime;

        if (Input.GetKeyDown(key.rotate)) { rightclock = false; RotateBlock(); }
        if (Input.GetKeyDown(key.zRotate)) { rightclock = true; RotateBlock(); }
        if (Input.GetKeyDown(key.aRotate)) { rightleft = true; RotateBlock(); }
        if (Input.GetKeyDown(key.hardDrop)) HardDrop();

        HandleInputMovement(key.right, Vector3.right);
        HandleInputMovement(key.left, Vector3.left);
        HandleInputMovement(key.down, Vector3.down);

        if (Input.GetKeyUp(key.right) || Input.GetKeyUp(key.left) || Input.GetKeyUp(key.down))
            moveTimer = 0;
    }

    void RotateBlock()
    {
        if (gameObject.name.Contains("ㅁ")) return;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // 1. 회전 수행
        if (rightleft) { transform.Rotate(0, 0, 180); rightleft = false; }
        else if (!rightclock) { transform.Rotate(0, 0, -90); }
        else { transform.Rotate(0, 0, 90); }

        // [핵심] I자 블록 피벗 보정: 회전 중심이 칸 경계에 있도록 0.25f씩 미세 조정
        // 에디터에서 프리팹 구조에 따라 이 수치는 조정이 필요할 수 있습니다.
        if (gameObject.name.Contains("I"))
        {
            // I자는 회전 시마다 중심축이 0.5단위로 어긋나므로 이를 보정
            // (보통 0.25f씩 당기거나 밀면 격자에 딱 맞게 떨어집니다)
        }

        bool success = false;

        // 2. 즉시 체크
        if (IsRotationSafe())
        {
            success = true;
        }
        else
        {
            // 3. Wall Kick (끼워넣기) 시도
            Vector3[] kickOffsets;
            if (gameObject.name.Contains("I"))
            {
                // I자 전용 강력 보정 데이터
                kickOffsets = new Vector3[] {
                    new Vector3(0.5f, 0, 0),  new Vector3(-0.5f, 0, 0),
                    new Vector3(1f, 0, 0),    new Vector3(-1f, 0, 0),
                    new Vector3(0, 1f, 0),    new Vector3(0, -1f, 0),
                    new Vector3(2f, 0, 0),    new Vector3(-2f, 0, 0),
                    new Vector3(0, 2f, 0),    new Vector3(1f, 1f, 0)
                };
            }
            else
            {
                kickOffsets = new Vector3[] {
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0),
                    new Vector3(0, -0.5f, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                    new Vector3(0, 1.0f, 0)
                };
            }

            foreach (Vector3 offset in kickOffsets)
            {
                transform.position = originalPos + offset;
                if (IsRotationSafe())
                {
                    success = true;
                    break;
                }
            }
        }

        if (success)
        {
            SnapToGrid();
            if (isground) lockDelayTimer = 0;
            trying++;
            if (Sound.instance != null) Sound.instance.swing.Play();
        }
        else
        {
            transform.position = originalPos;
            transform.rotation = originalRot;
        }
    }

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = child.position;
            Vector2Int index = blockclear.PosToIndex(pos);

            // 맵 경계 체크
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;

            // [중요] OverlapBox의 크기를 0.4f로 설정하여 인접 블록과의 미세 충돌 무시
            Collider2D hit = Physics2D.OverlapBox(pos, new Vector2(0.4f, 0.4f), 0);
            if (hit != null)
            {
                if (hit.transform.parent != transform && hit.transform != transform)
                {
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
                }
            }
        }
        return true;
    }

    void HandleInputMovement(KeyCode keycode, Vector3 dir)
    {
        if (Input.GetKeyDown(keycode))
        {
            MoveOnce(dir);
            moveTimer = 0;
        }

        if (Input.GetKey(keycode))
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > dasDelay)
            {
                if (moveTimer > dasDelay + arrDelay)
                {
                    MoveOnce(dir);
                    moveTimer = dasDelay;
                }
            }
        }
    }

    void MoveOnce(Vector3 dir)
    {
        if (canmove(dir))
        {
            transform.position += dir * 0.5f;
            SnapToGrid();
            if (isground) lockDelayTimer = 0;
        }
    }

    void HardDrop()
    {
        int loopSafe = 0;
        while (canmove(Vector3.down) && loopSafe < 80)
        {
            transform.position += new Vector3(0, -0.5f, 0);
            loopSafe++;
        }
        SnapToGrid();
        OnHardDropSettle();
    }

    void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x * 2f) / 2f,
            Mathf.Round(transform.position.y * 2f) / 2f, 0);
    }

    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            // 이동 시에도 충돌 판정 범위를 블록(0.5f)보다 약간 작게 설정
            Collider2D hit = Physics2D.OverlapBox(targetPos, new Vector2(0.45f, 0.45f), 0);

            if (hit != null && hit.transform.parent != transform && hit.transform != transform)
            {
                if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
            }

            Vector2Int index = blockclear.PosToIndex(targetPos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;
        }
        return true;
    }

    void UpdateGhostPosition()
    {
        if (ghost == null) return;
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        int loopSafe = 0;
        while (canmoveGhost(Vector3.down) && loopSafe < 80)
        {
            ghost.transform.position += new Vector3(0, -0.5f, 0);
            loopSafe++;
        }
    }

    bool canmoveGhost(Vector3 direction)
    {
        foreach (Transform child in ghost.transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            Vector2Int index = blockclear.PosToIndex(targetPos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;

            Collider2D hit = Physics2D.OverlapPoint(targetPos);
            if (hit != null && hit.transform.parent != transform && hit.transform != transform && hit.transform.parent != ghost.transform)
            {
                if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
            }
        }
        return true;
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
            }
        }
        if (bc != null) bc.DeleteFullLines();
        if (Sound.instance != null) Sound.instance.drop.Play();
        this.enabled = false;
        if (cb != null) cb.Clone();
    }

    private void FixedUpdate()
    {
        if (gm != null && gm.isON) return;
        UpdateLevelSpeed();
        nowfram++;
        if (nowfram >= frame) { Fall(); nowfram = 0; }
    }

    public void Fall()
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
        }

        if (isground && (lockDelayTimer >= 0.5f || trying >= 15))
            OnHardDropSettle();
    }

    void UpdateLevelSpeed() { /* 기존 로직 유지 */ }
    private void OnEnable() { AllBlocks.Add(this); }
    private void OnDisable() { AllBlocks.Remove(this); }
}
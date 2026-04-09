using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    public static List<BlockBase> AllBlocks = new List<BlockBase>();
    public bool skipSpawnNext = false;
    Rigidbody2D rb;
    int frame = 60, nowfram = 0, dropDistance = 1;
    float moveTimer = 0f, lockDelayTimer = 0f;

    // 스크립트 참조
    Conebase cb;
    GameManager gm;
    blockclear bc;
    KeyBinding key;

    public GameObject ghost;
    // Tspin: T-Spin 여부, rightclock: 시계방향 여부, rightleft: 180도 회전 여부, Irotate: I블록 회전 상태
    public bool isground = false, IsTwist = false, rightclock = false, rightleft = false, Irotate = false;
    public int trying = 0; // 조작 횟수 카운트 (무한 회전 방지)
    public GameObject BaseDesigh, ChangeDesigh;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
        bc = FindAnyObjectByType<blockclear>();
        gm = FindAnyObjectByType<GameManager>();
        key = FindAnyObjectByType<KeyBinding>();

        SnapToGrid();
        if (ghost != null) UpdateGhostPosition();
    }

    void Update()
    {
        if (gm.isON) return;

        // 속도 및 조작 설정 (Stat 스크립트 기반)
        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        float moveMod = Mathf.Clamp(Mathf.Pow(2, speedStat / 5f), 0.2f, 1.5f);
        float currentDas = 0.15f * moveMod;
        float currentArr = 0.03f * moveMod;

        if (isground) lockDelayTimer += Time.deltaTime;

        // 회전 입력 처리
        if (Input.GetKeyDown(key.rotate)) { rightclock = false; RotateBlock(); }
        if (Input.GetKeyDown(key.zRotate)) { rightclock = true; RotateBlock(); }
        if (Input.GetKeyDown(key.aRotate)) { rightleft = true; RotateBlock(); }
        if (Input.GetKeyDown(key.hardDrop)) HardDrop();

        // 좌우하 이동 처리
        HandleInputMovement(key.right, Vector3.right, currentDas, currentArr);
        HandleInputMovement(key.left, Vector3.left, currentDas, currentArr);
        HandleInputMovement(key.down, Vector3.down, currentDas, currentArr);

        // 입력 떼었을 때 타이머 초기화
        if (Input.GetKeyUp(key.right) || Input.GetKeyUp(key.left) || Input.GetKeyUp(key.down))
        {
            moveTimer = 0;
        }

        // 아무 키 입력 시 고스트 위치 업데이트 (최적화)
        if (Input.anyKey && ghost != null && !gm.isON)
        {
            UpdateGhostPosition();
        }
    }

    void RotateBlock()
    {
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // O블록(ㅁ)은 회전하지 않음
        if (!gameObject.name.Contains("ㅁ"))
        {
            // 1. 회전 적용
            if (rightleft)
            {
                transform.Rotate(0, 0, 180);
                rightleft = false;
            }
            else if (!rightclock) // 반시계
            {
                if (gameObject.name.Contains("I"))
                {
                    transform.Rotate(0, 0, Irotate ? -90 : 90);
                    Irotate = !Irotate;
                }
                else transform.Rotate(0, 0, -90);
            }
            else // 시계
            {
                if (gameObject.name.Contains("I"))
                {
                    transform.Rotate(0, 0, Irotate ? 90 : -90);
                    Irotate = !Irotate;
                }
                else transform.Rotate(0, 0, 90);
            }

            SnapToGrid();
            bool success = IsRotationSafe();

            // 2. 충돌 시 0.5 단위 킥백(Kickback) 시도
            if (!success)
            {
                Vector3[] kickOffsets = gameObject.name.Contains("I") ?
                    new Vector3[] {
                        new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0, 0.5f, 0),
                        new Vector3(0, -0.5f, 0), new Vector3(-1f, 0, 0), new Vector3(1f, 0, 0)
                    } :
                    new Vector3[] {
                        new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0, -0.5f, 0),
                        new Vector3(0, 0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0)
                    };

                foreach (Vector3 offset in kickOffsets)
                {
                    transform.position = originalPos + offset;
                    SnapToGrid();
                    if (IsRotationSafe())
                    {
                        success = true;
                        break;
                    }
                }
            }

            if (success)
            {
                if (isground) lockDelayTimer = 0;
                trying++; // 조작 횟수 누적
                Sound.instance.swing.Play();
                CheckTwistStatus();
            }
            else
            {
                // 모든 시도 실패 시 원래대로 복구
                transform.position = originalPos;
                transform.rotation = originalRot;
            }
        }
    }

    void CheckTwistStatus()
    {
        if (gameObject.name.Contains("ㅗ") || gameObject.name.Contains("T"))
        {
            Vector2[] corners = {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f)
            };

            int count = 0;
            foreach (Vector2 offset in corners)
            {
                Vector2 check = (Vector2)transform.position + offset;
                if (IsPosOccupied(check)) count++;
            }
            IsTwist = (count >= 3); // 4개 구석 중 3개 이상 막히면 T-Spin
        }
    }

    bool IsPosOccupied(Vector2 pos)
    {
        Vector2Int idx = blockclear.PosToIndex(pos);
        if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return true;

        Collider2D hit = Physics2D.OverlapCircle(pos, 0.15f);
        return (hit != null && hit.transform.parent != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")));
    }

    void SnapToGrid()
    {
        transform.position = new Vector3(
            Mathf.Round(transform.position.x * 2f) / 2f,
            Mathf.Round(transform.position.y * 2f) / 2f,
            0
        );
    }

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = child.position;
            Vector2Int idx = blockclear.PosToIndex(pos);

            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;

            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.2f);
            foreach (var hit in hits)
            {
                if (hit.transform.parent != transform && hit.transform != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                {
                    return false;
                }
            }
        }
        return true;
    }

    void HandleInputMovement(KeyCode key, Vector3 dir, float das, float arr)
    {
        Vector3 finalDir = (gm != null && gm.IsMirrored) ? (dir == Vector3.right ? Vector3.left : (dir == Vector3.left ? Vector3.right : dir)) : dir;

        if (Input.GetKeyDown(key))
        {
            MoveOnce(finalDir);
            moveTimer = 0;
        }

        if (Input.GetKey(key))
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > das)
            {
                if (moveTimer > das + arr)
                {
                    MoveOnce(finalDir);
                    moveTimer = das;
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

    private void FixedUpdate()
    {
        if (gm.isON) return;

        UpdateLevelSpeed();
        nowfram++;

        if (nowfram >= frame)
        {
            Fall();
            nowfram = 0;
        }
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
            int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
            float lockMod = Mathf.Pow(2, speedStat / 3f);
            // 0.3초 혹은 일정 조작 횟수(15회) 초과 시 고정
            if (lockDelayTimer >= 0.35f * lockMod || trying >= 15)
            {
                OnHardDropSettle();
            }
        }
    }

    void OnHardDropSettle()
    {

        if (skipSpawnNext) return;
        if (ghost != null) Destroy(ghost);
        SnapToGrid();

        foreach (Transform child in transform)
        {
            Vector2Int idx = blockclear.PosToIndex(child.position);

            if (idx.x >= 0 && idx.x < blockclear.width && idx.y >= 0 && idx.y < blockclear.height)
            {
                blockclear.grid[idx.x, idx.y] = child;
                child.tag = "Block";
            }
        }

        bc.DeleteFullLines();
        Sound.instance.drop.Play();
        this.enabled = false;
        cb.Clone(); // 다음 블록 생성
    }

    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            Vector2Int idx = blockclear.PosToIndex(targetPos);

            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;
            if (idx.y < blockclear.height && blockclear.grid[idx.x, idx.y] != null) return false;

            Collider2D hit = Physics2D.OverlapBox(targetPos, new Vector2(0.45f, 0.45f), 0);
            if (hit != null && hit.transform.parent != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
            {
                return false;
            }
        }
        return true;
    }

    void UpdateGhostPosition()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        int loop = 0;
        while (canmoveGhost(Vector3.down) && loop < 100) // 맵 높이에 따라 루프 횟수 조절
        {
            ghost.transform.position += new Vector3(0, -0.5f, 0);
            loop++;
        }

        ghost.transform.position = new Vector3(
            Mathf.Round(ghost.transform.position.x * 2f) / 2f,
            Mathf.Round(ghost.transform.position.y * 2f) / 2f,
            0
        );
    }

    bool canmoveGhost(Vector3 direction)
    {
        foreach (Transform child in ghost.transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            Vector2Int idx = blockclear.PosToIndex(targetPos);

            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;

            Collider2D[] hits = Physics2D.OverlapCircleAll(targetPos, 0.2f);
            foreach (var hit in hits)
            {
                if (hit.transform.parent != transform && hit.transform != transform &&
                    hit.transform.parent != ghost.transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                {
                    return false;
                }
            }
        }
        return true;
    }

    void HardDrop()
    {
        int loop = 0;
        while (canmove(Vector3.down) && loop < 100)
        {
            transform.position += new Vector3(0, -0.5f, 0);
            loop++;
        }
        SnapToGrid();
        if (!canmove(Vector3.down))
        {
            OnHardDropSettle();
        }
    }

    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;
        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;

        int[] frames = { 60, 50, 42, 35, 30, 25, 20, 16, 13, 10, 8, 7, 6, 5, 4, 3, 2, 2, 1 };

        float levelIndex = s / 1000f;
        float baseFrame;

        if (levelIndex < frames.Length) baseFrame = frames[Mathf.FloorToInt(levelIndex)];
        else baseFrame = 1f / (levelIndex - frames.Length + 1);

        baseFrame -= (d - 3) * 5;
        float finalFrame = baseFrame * Mathf.Pow(2, speedStat / 3f);

        if (finalFrame >= 1f)
        {
            frame = Mathf.RoundToInt(finalFrame);
            dropDistance = 1;
        }
        else
        {
            frame = 1;
            dropDistance = Mathf.CeilToInt(1f / Mathf.Max(0.0001f, finalFrame));
        }
    }

    private void OnEnable() { AllBlocks.Add(this); }
    private void OnDisable() { AllBlocks.Remove(this); }

    public void change()
    {
        if (BaseDesigh != null) BaseDesigh.SetActive(false);
        if (ChangeDesigh != null) ChangeDesigh.SetActive(true);

        if (ghost != null)
        {
            Transform gBase = ghost.transform.Find(BaseDesigh.name);
            Transform gChange = ghost.transform.Find(ChangeDesigh.name);
            if (gBase != null) gBase.gameObject.SetActive(false);
            if (gChange != null) gChange.gameObject.SetActive(true);
        }
    }

    public void dechange()
    {
        if (BaseDesigh != null) BaseDesigh.SetActive(true);
        if (ChangeDesigh != null) ChangeDesigh.SetActive(false);

        if (ghost != null)
        {
            Transform gBase = ghost.transform.Find(BaseDesigh.name);
            Transform gChange = ghost.transform.Find(ChangeDesigh.name);
            if (gBase != null) gBase.gameObject.SetActive(true);
            if (gChange != null) gChange.gameObject.SetActive(false);
        }
    }
}
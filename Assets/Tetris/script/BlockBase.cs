using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    public static List<BlockBase> AllBlocks = new List<BlockBase>();
    
    [Header("Settings")]
    public bool skipSpawnNext = false;
    public GameObject ghost;
    public GameObject BaseDesigh;
    public GameObject ChangeDesigh;

    [Header("Status")]
    public bool isground = false;
    public bool IsTwist = false; // T-Spin 판정 결과
    public int trying = 0; // 조작 횟수 (무한 회전 방지)

    // 내부 변수
    private Rigidbody2D rb;
    private int frame = 60;
    private int nowfram = 0;
    private int dropDistance = 1;
    private float moveTimer = 0f;
    private float lockDelayTimer = 0f;
    private bool lastActionWasRotate = false;
    private bool lastMirrorState;

    // 스크립트 참조
    private Conebase cb;
    private GameManager gm;
    private blockclear bc;
    private KeyBinding key;

    // SRS(Super Rotation System) 범용 킥 데이터 (0.5 단위 그리드 기준)
    // 인덱스: 0(기본), 1(좌), 2(우), 3(하), 4(상), 5(좌하단)
    private readonly Vector3[] kickOffsets = {
        new Vector3(0, 0, 0),
        new Vector3(-0.5f, 0, 0),
        new Vector3(0.5f, 0, 0),
        new Vector3(0, -0.5f, 0),
        new Vector3(0, 0.5f, 0),
        new Vector3(-0.5f, -0.5f, 0)
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
        bc = FindAnyObjectByType<blockclear>();
        gm = FindAnyObjectByType<GameManager>();
        key = FindAnyObjectByType<KeyBinding>();

        SnapToGrid();
        if (ghost != null) UpdateGhostPosition();
        if (gm != null) lastMirrorState = gm.IsMirrored;
    }

    void Update()
    {
        if (gm != null && gm.isON) return;

        // 속도 및 조작 설정 (Stat 기반)
        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        float moveMod = Mathf.Clamp(Mathf.Pow(2, speedStat / 5f), 0.2f, 1.5f);
        float currentDas = 0.15f * moveMod;
        float currentArr = 0.03f * moveMod;

        if (isground) lockDelayTimer += Time.deltaTime;

        // 회전 입력 처리
        if (Input.GetKeyDown(key.rotate)) RotateBlock(-90f); // 시계
        if (Input.GetKeyDown(key.zRotate)) RotateBlock(90f);  // 반시계
        if (Input.GetKeyDown(key.aRotate)) RotateBlock(180f); // 180도
        
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

        // 미러 모드 변경 체크
        CheckMirrorChange();

        // 고스트 업데이트
        if (Input.anyKey && ghost != null)
        {
            UpdateGhostPosition();
        }
    }

    void CheckMirrorChange()
    {
        if (gm != null && gm.IsMirrored != lastMirrorState)
        {
            SnapToGrid();
            UpdateGhostPosition();
            lastMirrorState = gm.IsMirrored;
        }
    }

    void RotateBlock(float angle)
    {
        // O형 블록(ㅁ)은 회전 제외
        if (gameObject.name.Contains("ㅁ") || gameObject.name.Contains("O")) return;

        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        // 1. 회전 적용
        transform.Rotate(0, 0, angle);
        
        bool success = false;

        // 2. SRS 킥 시도
        foreach (Vector3 offset in kickOffsets)
        {
            float mirrorFactor = (gm != null && gm.IsMirrored) ? -1f : 1f;
            Vector3 testOffset = new Vector3(offset.x * mirrorFactor, offset.y, 0);

            transform.position = originalPos + testOffset;
            if (IsRotationSafe())
            {
                success = true;
                break;
            }
        }

        if (success)
        {
            SnapToGrid();
            lastActionWasRotate = true;
            if (isground) lockDelayTimer = 0;
            trying++;
            if (Sound.instance != null) Sound.instance.swing.Play();
            CheckTwistStatus();
        }
        else
        {
            // 모든 킥 실패 시 복구
            transform.position = originalPos;
            transform.rotation = originalRot;
        }
    }

    void CheckTwistStatus()
    {
        // T-Spin 판정 (T자 블록 계열 확인)
        if (gameObject.name.Contains("ㅗ") || gameObject.name.Contains("T"))
        {
            Vector2[] corners = {
                new Vector2(-0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-0.5f, -0.5f), new Vector2(0.5f, -0.5f)
            };

            int count = 0;
            foreach (Vector2 offset in corners)
            {
                // 월드 좌표 기준 귀퉁이 체크
                Vector2 checkPos = (Vector2)transform.position + (Vector2)(transform.rotation * offset);
                if (IsPosOccupied(checkPos)) count++;
            }
            // 3개 이상의 구석이 막혀있고 마지막 행동이 회전일 때 T-Spin
            IsTwist = (count >= 3 && lastActionWasRotate);
        }
    }

    bool IsPosOccupied(Vector2 pos)
    {
        Vector2Int idx = blockclear.PosToIndex(pos);
        if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return true;

        Collider2D hit = Physics2D.OverlapBox(pos, new Vector2(0.4f, 0.4f), 0);
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

    void HandleInputMovement(KeyCode k, Vector3 dir, float das, float arr)
    {
        Vector3 finalDir = (gm != null && gm.IsMirrored) ? (dir == Vector3.right ? Vector3.left : (dir == Vector3.left ? Vector3.right : dir)) : dir;

        if (Input.GetKeyDown(k))
        {
            MoveOnce(finalDir);
            moveTimer = 0;
        }

        if (Input.GetKey(k))
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
            lastActionWasRotate = false; // 이동 시 T-Spin 조건 취소
            if (isground) lockDelayTimer = 0;
        }
    }

    private void FixedUpdate()
    {
        if (gm != null && gm.isON) return;

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
                lastActionWasRotate = false;
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
        if (Sound.instance != null) Sound.instance.drop.Play();
        this.enabled = false;
        cb.Clone();
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
        if (ghost == null) return;
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        int loop = 0;
        while (canmoveGhost(Vector3.down) && loop < 40)
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
        while (canmove(Vector3.down) && loop < 40)
        {
            transform.position += new Vector3(0, -0.5f, 0);
            loop++;
        }
        SnapToGrid();
        OnHardDropSettle();
    }

    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;
        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;

        int[] framesArr = { 60, 50, 42, 35, 30, 25, 20, 16, 13, 10, 8, 7, 6, 5, 4, 3, 2, 2, 1 };
        float levelIndex = s / 1000f;
        float baseFrame;

        if (levelIndex < framesArr.Length) baseFrame = framesArr[Mathf.FloorToInt(levelIndex)];
        else baseFrame = 1f / (levelIndex - framesArr.Length + 1);

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
        UpdateGhostDesign(false);
    }

    public void dechange()
    {
        if (BaseDesigh != null) BaseDesigh.SetActive(true);
        if (ChangeDesigh != null) ChangeDesigh.SetActive(false);
        UpdateGhostDesign(true);
    }

    void UpdateGhostDesign(bool showBase)
    {
        if (ghost != null)
        {
            Transform gBase = ghost.transform.Find(BaseDesigh?.name ?? "");
            Transform gChange = ghost.transform.Find(ChangeDesigh?.name ?? "");
            if (gBase != null) gBase.gameObject.SetActive(showBase);
            if (gChange != null) gChange.gameObject.SetActive(!showBase);
        }
    }
}
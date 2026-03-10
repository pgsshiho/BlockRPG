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
    public bool Tspin = false;
    public bool rightclock = false;
    public bool rightleft = false;

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

        // 위쪽 화살표: 반시계 방향 (또는 설정에 따라 시계)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            rightclock = false;
            RotateBlock();
        }
        // Z키: 시계 방향 (괄호 추가 완료)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            rightclock = true;
            RotateBlock();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            rightleft = true;
            RotateBlock();
        }
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
            // 1. 회전 실행
            if (rightleft) { transform.Rotate(0, 0, 180); rightleft = false; }
            else if (!rightclock) { transform.Rotate(0, 0, -90); }
            else { transform.Rotate(0, 0, 90); }

            SnapToGrid();

            bool success = false;

            // 2. 즉시 성공 여부 확인
            if (IsRotationSafe())
            {
                success = true;
            }
            else
            {
                // 3. 실패 시 KickOffsets 설정
                Vector3[] kickOffsets;

                if (gameObject.name.Contains("I"))
                {
                    // I자용: 더 넓은 범위와 과감한 보정
                    kickOffsets = new Vector3[] {
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0),
                    new Vector3(0, 0.5f, 0),  new Vector3(0, -0.5f, 0),
                    new Vector3(-1.0f, 0, 0), new Vector3(1.0f, 0, 0),
                    new Vector3(0, 1.0f, 0),  new Vector3(-2.0f, 0, 0),
                    new Vector3(2.0f, 0, 0)
                };
                }
                else
                {
                    // 일반 블록(L, J, T, S, Z)용: 미세 보정 중심
                    kickOffsets = new Vector3[] {
                    new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0),
                    new Vector3(0, -0.5f, 0), new Vector3(0, 0.5f, 0),
                    new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                    new Vector3(-1.0f, 0, 0), new Vector3(1.0f, 0, 0),
                    new Vector3(0, 1.0f, 0) // 바닥 탈출용
                };
                }

                // 4. 보정값 순차적 적용
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

            // 5. 최종 처리
            if (success)
            {
                if (isground) lockDelayTimer = 0;
                trying++;
                Sound.instance.swing.Play();

                // (이후 T-Spin 판정 로직 계속...)
            }
            else
            {
                // 모든 보정 실패 시 되돌리기
                transform.position = originalPos;
                transform.rotation = originalRot;
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

    float dasDelay = 0.15f; // 처음 눌렀을 때 대기 시간
    float arrDelay = 0.03f; // 연사 속도
    float moveTimer = 0f;

    void HandleInputMovement(KeyCode key, Vector3 dir)
    {
        Vector3 finalDir = (gm != null && gm.IsMirrored) ?
            (dir == Vector3.right ? Vector3.left : (dir == Vector3.left ? Vector3.right : dir)) : dir;

        if (Input.GetKeyDown(key))
        {
            MoveOnce(finalDir);
            moveTimer = 0; // 타이머 초기화
        }

        if (Input.GetKey(key))
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > dasDelay)
            {
                // DAS 시간이 지나면 ARR 간격으로 계속 이동
                if (moveTimer > dasDelay + arrDelay)
                {
                    MoveOnce(finalDir);
                    moveTimer = dasDelay; // ARR 간격 유지를 위해 값 조정
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
            // 땅에 닿아있을 때 좌우로 움직이면 고정 시간을 초기화해주는 배려 (Infinity Lock)
            if (isground) lockDelayTimer = 0;
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

            // OverlapPoint 대신 OverlapBox 사용
            // 크기를 0.45f로 설정하여 옆 블록과의 미세한 접촉으로 인한 오류 방지
            Collider2D hit = Physics2D.OverlapBox(targetPos, new Vector2(0.45f, 0.45f), 0);

            if (hit != null)
            {
                // 내 자식이 아니고, 바닥이나 다른 블록이면 이동 불가
                if (hit.transform.parent != transform && hit.transform != transform)
                {
                    if (hit.CompareTag("Floor") || hit.CompareTag("Block")) return false;
                }
            }

            // 기존의 맵 밖 체크 로직 유지
            Vector2Int index = blockclear.PosToIndex(targetPos);
            if (index.x < 0 || index.x >= blockclear.width || index.y < 0) return false;
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

        int[] frames = { 60, 50, 42, 35, 30, 25, 20, 16, 13, 10, 8, 7, 6, 5, 4, 3, 2, 2, 1 };

        int scoreInterval = 1000;
        int level = s / scoreInterval; // 현재 레벨 계산

        // 1. 배열 범위를 벗어나지 않도록 clamp 처리
        int index = Mathf.Min(level, frames.Length - 1);
        int baseFrame = frames[index];

        // 2. 난이도 보정
        int rawFrame = baseFrame - (d - 3) * 5;

        if (rawFrame >= 1)
        {
            frame = rawFrame;
            dropDistance = 1;
        }
        else
        {
            frame = 1;
            // 3. dropDistance의 최대치를 제한 (예: 20칸)하여 무한 루프 렉 방지
            int calculatedDistance = 1 + Mathf.CeilToInt(Mathf.Abs(rawFrame) / 3f);
            dropDistance = Mathf.Min(calculatedDistance, 20);
        }
    }
}
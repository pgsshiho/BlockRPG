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
    blockclear bc;
    float timers = 0f;
    public bool isground = false;
    public int trying = 0;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cb = FindAnyObjectByType<Conebase>();
        bc = FindAnyObjectByType<blockclear>();
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
        if(isground == true)
        {
            timers++;
        }
        // 1. 회전 로직 (월킥 보강)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Rotate(0f, 0f, 90f);
            Sound.instance.swing.Play();
            // 회전 후 겹친다면 여러 방향으로 튕겨나가며 빈자리 찾기
            if (!IsRotationSafe())
            {
                // 월킥 후보지들: 왼쪽으로 0.5, 오른쪽으로 0.5, 왼쪽으로 1.0, 오른쪽으로 1.0, 위로 0.5
                Vector3[] kickOffsets = {
                    new Vector3(-0.5f, 0, 0),
                    new Vector3(0.5f, 0, 0),
                    new Vector3(-1.0f, 0, 0),
                    new Vector3(1.0f, 0, 0),
                    new Vector3(0, 0.5f, 0)
                };

                bool fixedRotation = false;
                foreach (Vector3 offset in kickOffsets)
                {
                    transform.position += offset;
                    if (IsRotationSafe())
                    {
                        fixedRotation = true;
                        break;
                    }
                    transform.position -= offset; // 안 맞으면 원복 후 다음 시도
                }

                if (!fixedRotation)
                {
                    // 모든 방향 실패 시 회전 취소
                    transform.Rotate(0f, 0f, -90f);
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
            timers = 0; // 사운드 추가
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
            timer = 0;

            if (canmove(dir))
            {
                transform.position += (dir.y == 0) ? new Vector3(moveAmount, 0, 0) : new Vector3(0, moveAmount, 0);
                if (isground) timers = 0; // 바닥에서 옆으로 이동 시 유예 초기화
            }
            else if (key == KeyCode.DownArrow) // [추가] 아래 키 눌렀는데 못 움직이면(바닥이면)
            {
                OnHardDropSettle(); // 즉시 설치
                return;
            }
        }

        if (Input.GetKey(key))
        {
            timer += Time.deltaTime;
            float initialDelay = 0.15f;
            float repeatInterval = 0.03f;

            if (timer >= initialDelay)
            {
                if (canmove(dir))
                {
                    if (dir.y == 0) transform.position += new Vector3(moveAmount, 0, 0);
                    else transform.position += new Vector3(0, moveAmount, 0);
                    if (isground) timers = 0;
                }
                else if (key == KeyCode.DownArrow) // [추가] 꾹 누르는 중에도 바닥에 닿으면
                {
                    OnHardDropSettle(); // 즉시 설치
                    return;
                }
                timer = initialDelay - repeatInterval;
            }
        }
        // ... KeyUp 로직 ...
    }

    private void FixedUpdate()
    {
        UpdateLevelSpeed(); // 매 고정 프레임마다 속도 체크
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
            isground = false; // 아래로 한 칸이라도 내려가면 지면 상태 해제
            timers = 0;       // 타이머 초기화
        }
        else
        {
            isground = true;
            // 30프레임(약 0.5초) 지났거나 15번 비볐으면 고정
            if (timers >= 20 || trying >= 15) OnHardDropSettle();
        }
    }

    // BlockBase.cs 내부 수정
    void OnHardDropSettle()
    {
        if (ghost != null) Destroy(ghost);

        foreach (Transform child in transform)
        {
            // blockclear 클래스의 함수를 사용
            Vector2Int index = blockclear.PosToIndex(child.position);

            if (index.x >= 0 && index.x < blockclear.width && index.y >= 0 && index.y < blockclear.height)
            {
                blockclear.grid[index.x, index.y] = child;
                child.tag = "Block";
            }
        }

        // blockclear 컴포넌트를 찾아서 호출
        FindAnyObjectByType<blockclear>().DeleteFullLines();
        Sound.instance.drop.Play();
        this.enabled = false;
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.isKinematic = true; }
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
    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;

        if (s < 1000) frame = 60;
        else if (s < 2000) frame = 54;
        else if (s < 3500) frame = 48;
        else if (s < 5000) frame = 42;
        else if (s < 7000) frame = 36;
        else if (s < 9000) frame = 32;
        else if (s < 11000) frame = 28;
        else if (s < 13500) frame = 24;
        else if (s < 16000) frame = 20;
        else if (s < 19000) frame = 18;
        else if (s < 22000) frame = 16;
        else if (s < 25000) frame = 14;
        else if (s < 29000) frame = 12;
        else if (s < 33000) frame = 10;
        else if (s < 38000) frame = 8;
        else if (s < 43000) frame = 7;
        else if (s < 49000) frame = 6;
        else if (s < 55000) frame = 5;
        else if (s < 62000) frame = 4;
        else frame = 3; // 최고 난이도
    }
}
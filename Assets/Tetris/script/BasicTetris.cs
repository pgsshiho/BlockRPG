using System.Collections;
using UnityEngine;

public class BasicTetris : MonoBehaviour
{
    public int level = 1;
    private int waitFrameCount = 0;
    public bool isbottom = false;
    public bool isdone = false;

    Rigidbody2D rb;
    createblock cb;

    // --- 좌우 이동 관련 변수 ---
    float horizontalTimer = 0;   // 누르고 있는 시간 측정
    float moveDelay = 0.2f;      // 처음 꾹 눌렀을 때 대기 시간 (DAS)
    float moveInterval = 0.05f;  // 연속 이동 속도 (ARR)

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        cb = FindObjectOfType<createblock>();
    }

    // 입력 감지는 Update에서 해야 반응속도가 좋습니다.
    void Update()
    {
        if (isbottom) return;

        // 1. 좌우 이동 처리 (꾹 누르기 포함)
        HandleHorizontalInput();

        // 2. 하드 드롭 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
    }

    void FixedUpdate()
    {
        if (isbottom) return;

        waitFrameCount++;
        if (waitFrameCount >= GetFallSpeed(level))
        {
            Fall();
            waitFrameCount = 0;
        }
    }

    // 좌우 이동 로직
    void HandleHorizontalInput()
    {
        // 오른쪽 또는 왼쪽 키를 누르고 있는가?
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            int direction = Input.GetKey(KeyCode.RightArrow) ? 1 : -1;

            // 처음 딱 눌렀을 때 (한 칸 즉시 이동)
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveSide(direction);
                horizontalTimer = 0; // 타이머 초기화
            }

            horizontalTimer += Time.deltaTime;

            // 꾹 누르기 딜레이가 지났다면 연속 이동
            if (horizontalTimer >= moveDelay)
            {
                MoveSide(direction);
                // 연속 이동 간격만큼 타이머를 깎아서 리듬을 맞춤
                horizontalTimer -= moveInterval;
            }
        }
        else
        {
            // 키를 떼면 타이머 초기화
            horizontalTimer = 0;
        }
    }

    void MoveSide(int dir)
    {
        // 절대 좌표(1, -1)가 아니라 '현재 위치에서 이동'
        transform.position += new Vector3(dir, 0, 0);

        // TODO: 여기서 벽이나 다른 블록에 겹쳤는지 체크 후, 겹쳤다면 다시 원상복구 시키는 로직이 필요함
    }

    public int GetFallSpeed(int lv)
    {
        int f = 60 - (lv * 3);
        return Mathf.Max(1, f);
    }

    void Fall()
    {
        // 낙하 단위는 보통 1.0f가 격자에 딱 맞습니다.
        transform.position += new Vector3(0, -1f, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isbottom || isdone) return;

        // 위치 보정 (정수 좌표로 딱 붙이기)
        transform.position = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            0
        );

        isbottom = true;
        isdone = true;

        if (cb != null) cb.spawn();
        this.enabled = false; // 스크립트 끄기
    }
    public void HardDrop()
    {
        float minDistance = 19f; 

        foreach (Transform child in transform)
        {
            RaycastHit2D hit = Physics2D.Raycast(child.position, Vector2.down, 19f, LayerMask.GetMask("Block", "Floor"));

            if (hit.collider != null)
            {
                float distance = child.position.y - hit.point.y;
                if (distance < minDistance) minDistance = distance;
            }
        }
        transform.position += new Vector3(0, -Mathf.Floor(minDistance - 0.5f), 0);
        isbottom = true;
    }
}
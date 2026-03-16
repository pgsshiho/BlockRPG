using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockBase : MonoBehaviour
{
    public static List<BlockBase> AllBlocks = new List<BlockBase>();

    Rigidbody2D rb;
    int frame = 60, nowfram = 0, dropDistance = 1;
    float moveTimer = 0f, lockDelayTimer = 0f;

    Conebase cb;
    GameManager gm;
    blockclear bc;
    KeyBinding key;

    public GameObject ghost;
    public bool isground = false, Tspin = false, rightclock = false, rightleft = false, Irotate = false;
    public int trying = 0;
    public GameObject BaseDesigh, ChangeDesigh;

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
        if (ghost != null && !gm.isON)
        {
            UpdateGhostPosition();
        }
    }

    void Update()
    {
        if (gm.isON) return;

        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        float moveMod = Mathf.Clamp(Mathf.Pow(2, speedStat / 5f), 0.2f, 1.5f);
        float currentDas = 0.15f * moveMod;
        float currentArr = 0.03f * moveMod;

        if (isground)
        {
            lockDelayTimer += Time.deltaTime;
        }

        if (Input.GetKeyDown(key.rotate)) { rightclock = false; RotateBlock(); }
        if (Input.GetKeyDown(key.zRotate)) { rightclock = true; RotateBlock(); }
        if (Input.GetKeyDown(key.aRotate)) { rightleft = true; RotateBlock(); }
        if (Input.GetKeyDown(key.hardDrop)) HardDrop();

        HandleInputMovement(key.right, Vector3.right, currentDas, currentArr);
        HandleInputMovement(key.left, Vector3.left, currentDas, currentArr);
        HandleInputMovement(key.down, Vector3.down, currentDas, currentArr);

        if (Input.GetKeyUp(key.right) || Input.GetKeyUp(key.left) || Input.GetKeyUp(key.down))
        {
            moveTimer = 0;
        }
    }

    void RotateBlock()
    {
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        if (!gameObject.name.Contains("ㅁ"))
        {
            if (rightleft)
            {
                transform.Rotate(0, 0, 180);
                rightleft = false;
            }
            else if (!rightclock)
            {
                if (gameObject.name.Contains("I"))
                {
                    transform.Rotate(0, 0, Irotate ? -90 : 90);
                    Irotate = !Irotate;
                }
                else
                {
                    transform.Rotate(0, 0, -90);
                }
            }
            else
            {
                if (gameObject.name.Contains("I"))
                {
                    transform.Rotate(0, 0, Irotate ? 90 : -90);
                    Irotate = !Irotate;
                }
                else
                {
                    transform.Rotate(0, 0, 90);
                }
            }

            SnapToGrid();
            bool success = IsRotationSafe();

            if (!success)
            {
                Vector3[] kickOffsets = gameObject.name.Contains("I") ?
                    new Vector3[] {
                        new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0, 0.5f, 0),
                        new Vector3(0, -0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
                        new Vector3(-1f, 0, 0), new Vector3(0, 1f, 0), new Vector3(1f, 0, 0)
                    } :
                    new Vector3[] {
                        new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0, -0.5f, 0),
                        new Vector3(0, 0.5f, 0), new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0),
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
                trying++;
                Sound.instance.swing.Play();
                CheckTSpin();
            }
            else
            {
                transform.position = originalPos;
                transform.rotation = originalRot;
            }
        }
    }

    void CheckTSpin()
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
                Vector2Int idx = blockclear.PosToIndex(check);

                if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0)
                {
                    count++;
                }
                else
                {
                    Collider2D hit = Physics2D.OverlapPoint(check);
                    if (hit != null && hit.transform.parent != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                    {
                        count++;
                    }
                }
            }
            Tspin = (count >= 3);
        }
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
            Vector2 pos = new Vector2(Mathf.Round(child.position.x * 2f) / 2f, Mathf.Round(child.position.y * 2f) / 2f);
            Vector2Int idx = blockclear.PosToIndex(pos);

            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;

            Collider2D[] hits = Physics2D.OverlapPointAll(pos);
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
            if (moveTimer > das && moveTimer > das + arr)
            {
                MoveOnce(finalDir);
                moveTimer = das;
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
            if (lockDelayTimer >= 0.3f * lockMod || trying >= 15)
            {
                OnHardDropSettle();
            }
        }
    }

    void OnHardDropSettle()
    {
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
        cb.Clone();
    }

    bool canmove(Vector3 direction)
    {
        foreach (Transform child in transform)
        {
            Vector2 targetPos = (Vector2)child.position + (Vector2)direction * 0.5f;
            Collider2D hit = Physics2D.OverlapBox(targetPos, new Vector2(0.45f, 0.45f), 0);

            if (hit != null && hit.transform.parent != transform && hit.transform != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
            {
                return false;
            }

            Vector2Int idx = blockclear.PosToIndex(targetPos);
            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;
        }
        return true;
    }

    void UpdateGhostPosition()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;

        int loop = 0;
        while (canmoveGhost(Vector3.down) && loop < 50)
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

            Collider2D[] hits = Physics2D.OverlapPointAll(targetPos);
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
        while (canmove(Vector3.down) && loop < 50)
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

        int[] frames = { 60, 50, 42, 35, 30, 25, 20, 16, 13, 10, 8, 7, 6, 5, 4, 3, 2, 2, 1 };

        float levelIndex = s / 1000f;
        float baseFrame;

        if (levelIndex < frames.Length)
        {
            baseFrame = frames[Mathf.FloorToInt(levelIndex)];
        }
        else
        {
            baseFrame = 1f / (levelIndex - frames.Length + 1);
        }

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
        BaseDesigh.SetActive(false);
        ChangeDesigh.SetActive(true);
    }

    public void dechange()
    {
        BaseDesigh.SetActive(true);
        ChangeDesigh.SetActive(false);
    }
}
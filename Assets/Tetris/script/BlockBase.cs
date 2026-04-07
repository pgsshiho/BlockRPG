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
    public bool isground = false, IsTwist = false, isClockwise = false, isFlip = false, Irotate = false;
    public int moveAttempts = 0;
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

        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        float moveMod = Mathf.Clamp(Mathf.Pow(2, speedStat / 5.2f), 0.25f, 1.6f);
        float currentDas = 0.16f * moveMod;
        float currentArr = 0.035f * moveMod;

        if (isground) lockDelayTimer += Time.deltaTime;

        if (Input.GetKeyDown(key.rotate)) { isClockwise = false; RotatePiece(); }
        if (Input.GetKeyDown(key.zRotate)) { isClockwise = true; RotatePiece(); }
        if (Input.GetKeyDown(key.aRotate)) { isFlip = true; RotatePiece(); }
        if (Input.GetKeyDown(key.hardDrop)) HardDrop();

        HandleInputMovement(key.right, Vector3.right, currentDas, currentArr);
        HandleInputMovement(key.left, Vector3.left, currentDas, currentArr);
        HandleInputMovement(key.down, Vector3.down, currentDas, currentArr);

        if (Input.GetKeyUp(key.right) || Input.GetKeyUp(key.left) || Input.GetKeyUp(key.down)) moveTimer = 0;
        if (Input.anyKey && ghost != null && !gm.isON) UpdateGhostPosition();
    }

    void RotatePiece()
    {
        Vector3 originalPos = transform.position;
        Quaternion originalRot = transform.rotation;

        if (!gameObject.name.Contains("ㅁ"))
        {
            if (isFlip) { transform.Rotate(0, 0, 180); isFlip = false; }
            else if (!isClockwise)
            {
                if (gameObject.name.Contains("I")) { transform.Rotate(0, 0, Irotate ? -90 : 90); Irotate = !Irotate; }
                else transform.Rotate(0, 0, -90);
            }
            else
            {
                if (gameObject.name.Contains("I")) { transform.Rotate(0, 0, Irotate ? 90 : -90); Irotate = !Irotate; }
                else transform.Rotate(0, 0, 90);
            }

            SnapToGrid();
            bool success = IsRotationSafe();

            if (!success)
            {
                Vector3[] customOffsets = gameObject.name.Contains("I") ?
                    new Vector3[] { new Vector3(-0.51f, 0, 0), new Vector3(0.51f, 0, 0), new Vector3(0, 0.51f, 0) } :
                    new Vector3[] { new Vector3(-0.51f, 0, 0), new Vector3(0.51f, 0, 0), new Vector3(0, -0.51f, 0) };

                foreach (Vector3 offset in customOffsets)
                {
                    transform.position = originalPos + offset;
                    SnapToGrid();
                    if (IsRotationSafe()) { success = true; break; }
                }
            }

            if (success)
            {
                if (isground) lockDelayTimer = 0;
                moveAttempts++;
                Sound.instance.swing.Play();
                CheckBonusMove(); // T-Spin 판정 로직 호출
            }
            else
            {
                transform.position = originalPos;
                transform.rotation = originalRot;
            }
        }
    }

    // [핵심 변경] T-Spin 판정: 머리 위가 막혀 있는지 체크
    void CheckBonusMove()
    {
        if (gameObject.name.Contains("ㅗ") || gameObject.name.Contains("T"))
        {
            Vector2 checkDirection = Vector2.up;
            float rotZ = transform.eulerAngles.z;

            if (Mathf.Abs(rotZ - 90) < 10) checkDirection = Vector2.left;
            else if (Mathf.Abs(rotZ - 180) < 10) checkDirection = Vector2.down;
            else if (Mathf.Abs(rotZ - 270) < 10) checkDirection = Vector2.right;

            Vector2 checkPos = (Vector2)transform.position + (checkDirection * 0.51f);
            if (IsPosOccupied(checkPos)) IsTwist = true;
        }
    }

    bool IsPosOccupied(Vector2 pos)
    {
        Vector2Int idx = blockclear.PosToIndex(pos);
        if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return true;
        Collider2D hit = Physics2D.OverlapPoint(pos);
        return (hit != null && hit.transform.parent != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")));
    }

    void SnapToGrid()
    {
        float step = 0.5f;
        transform.position = new Vector3(Mathf.Round(transform.position.x / step) * step, Mathf.Round(transform.position.y / step) * step, 0);
    }

    bool IsRotationSafe()
    {
        foreach (Transform child in transform)
        {
            Vector2 pos = child.position;
            Vector2Int idx = blockclear.PosToIndex(pos);
            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;
            Collider2D[] hits = Physics2D.OverlapPointAll(pos);
            foreach (var hit in hits)
            {
                if (hit.transform.parent != transform && hit.transform != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                    return false;
            }
        }
        return true;
    }

    void HandleInputMovement(KeyCode key, Vector3 dir, float das, float arr)
    {
        Vector3 finalDir = (gm != null && gm.IsMirrored) ? (dir == Vector3.right ? Vector3.left : (dir == Vector3.left ? Vector3.right : dir)) : dir;
        if (Input.GetKeyDown(key)) { MoveOnce(finalDir); moveTimer = 0; }
        if (Input.GetKey(key))
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > das)
            {
                if (moveTimer > das + arr) { MoveOnce(finalDir); moveTimer = das; }
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
            else { isground = true; break; }
        }

        if (isground)
        {
            int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
            float lockMod = Mathf.Pow(2.1f, speedStat / 3.5f);
            if (lockDelayTimer >= 0.35f * lockMod || moveAttempts >= 18) OnPieceSettle();
        }
    }

    void OnPieceSettle()
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
            Vector2Int idx = blockclear.PosToIndex(targetPos);
            if (idx.x < 0 || idx.x >= blockclear.width || idx.y < 0) return false;
            if (idx.y < blockclear.height && blockclear.grid[idx.x, idx.y] != null) return false;
            Collider2D hit = Physics2D.OverlapBox(targetPos, new Vector2(0.38f, 0.38f), 0);
            if (hit != null && hit.transform.parent != transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                return false;
        }
        return true;
    }

    void UpdateGhostPosition()
    {
        ghost.transform.position = transform.position;
        ghost.transform.rotation = transform.rotation;
        int loop = 0;
        while (canmoveGhost(Vector3.down) && loop < 60)
        {
            ghost.transform.position += new Vector3(0, -0.5f, 0);
            loop++;
        }
        float step = 0.5f;
        ghost.transform.position = new Vector3(Mathf.Round(ghost.transform.position.x / step) * step, Mathf.Round(ghost.transform.position.y / step) * step, 0);
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
                if (hit.transform.parent != transform && hit.transform != transform && hit.transform.parent != ghost.transform && (hit.CompareTag("Floor") || hit.CompareTag("Block")))
                    return false;
            }
        }
        return true;
    }

    void HardDrop()
    {
        int loop = 0;
        while (canmove(Vector3.down) && loop < 100) { transform.position += new Vector3(0, -0.5f, 0); loop++; }
        SnapToGrid();
        if (!canmove(Vector3.down)) OnPieceSettle();
    }

    void UpdateLevelSpeed()
    {
        int s = blockclear.ScoreForSpeed;
        int speedStat = (Stat.instance != null) ? Stat.instance.spd : 0;
        int d = (Stat.instance != null) ? Stat.instance.difficult : 3;
        int[] frames = { 60, 48, 38, 32, 28, 22, 18, 14, 11, 9, 7, 5, 4, 3, 2, 1 };
        float levelIndex = s / 1200f;
        float baseFrame = (levelIndex < frames.Length) ? frames[Mathf.FloorToInt(levelIndex)] : 1f;
        baseFrame -= (d - 3) * 4.5f;
        float finalFrame = baseFrame * Mathf.Pow(2.1f, speedStat / 3.2f);
        if (finalFrame >= 1f) { frame = Mathf.RoundToInt(finalFrame); dropDistance = 1; }
        else { frame = 1; dropDistance = Mathf.CeilToInt(1f / Mathf.Max(0.0001f, finalFrame)); }
    }

    private void OnEnable() { AllBlocks.Add(this); }
    private void OnDisable() { AllBlocks.Remove(this); }
}
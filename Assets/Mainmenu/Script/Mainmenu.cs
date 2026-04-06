using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    private Stat st;
    public GameObject panel;
    public TextMeshProUGUI dif;
    public GameObject select;
    public TextMeshProUGUI statusText;
    public GameObject keypanel;
    public GameObject ResetWarning;
    public GameObject custumpanel;
    public GameObject Statpanel;
    private bool isWaitingForKey = false;
    int nowpage = 0;

    [Header("MainPage / Bestiary")]
    public GameObject[] Page;        // 도감 각 페이지 오브젝트
    public GameObject[] PageHide;    // 도감 각 페이지를 가리고 있는 '잠금/물음표' 오브젝트
    public FindEnemy enemyData;      // ScriptableObject 할당용

    [Header("Key Display Texts")]
    public TextMeshProUGUI rotateTxt;
    public TextMeshProUGUI rightTxt;
    public TextMeshProUGUI leftTxt;
    public TextMeshProUGUI downTxt;
    public TextMeshProUGUI hardDropTxt;
    public TextMeshProUGUI holdTxt;
    public TextMeshProUGUI hold2Txt;
    public TextMeshProUGUI zRotateTxt;
    public TextMeshProUGUI aRotateTxt;
    public TextMeshProUGUI openstatTxt;

    void Start()
    {
        st = Stat.instance;
        if (st == null) st = FindAnyObjectByType<Stat>();

        UpdateKeyUI();
        UpdateEnemyDiscovery(); // 게임 시작 시 도감 해제 상태 반영
    }

    void Update()
    {
        if (st != null && dif != null)
            dif.text = st.difficult.ToString();
    }

    // --- [도감 해제 로직] ---
    public void UpdateEnemyDiscovery()
    {
        // 세이렌 제거로 인해 총 11개로 체크 범위를 변경함
        if (enemyData == null || PageHide == null || PageHide.Length < 11)
        {
            Debug.LogWarning("FindEnemy 데이터나 PageHide 배열 설정이 부족합니다. (필요 개수: 11)");
            return;
        }

        PageHide[0].SetActive(!enemyData.slime);
        PageHide[1].SetActive(!enemyData.goblin);
        PageHide[2].SetActive(!enemyData.ouger);
        PageHide[3].SetActive(!enemyData.golem);
        PageHide[4].SetActive(!enemyData.chraken);
        PageHide[5].SetActive(!enemyData.ghost);
        PageHide[6].SetActive(!enemyData.dragon);
        PageHide[7].SetActive(!enemyData.crown);
        PageHide[8].SetActive(!enemyData.shaman);
        PageHide[9].SetActive(!enemyData.knight_night);
        PageHide[10].SetActive(!enemyData.boss);
    }

    // --- [페이지 제어] ---
    public void OpenPage()
    {
        UpdateEnemyDiscovery();

        if (nowpage < Page.Length)
        {
            Page[nowpage].SetActive(true);
            nowpage++;
        }
    }

    public void beforePage()
    {
        if (nowpage > 1)
        {
            Page[nowpage - 1].SetActive(false);
            nowpage--;
            Page[nowpage - 1].SetActive(true);
        }
    }

    public void ClosePage()
    {
        nowpage = 0;
        foreach (GameObject page in Page)
        {
            page.SetActive(false);
        }
    }

    // --- [기존 로직] ---
    public void qu() => Application.Quit();

    public void infinitestart()
    {
        Time.timeScale = 1f;
        if (Stat.instance != null) Stat.instance.hp = Stat.instance.maxhp;
        SceneChanger.BG("Tetris");
    }

    public void difup() { if (st != null && st.difficult < 10) st.difficult++; }
    public void difdown() { if (st != null && st.difficult > 1) st.difficult--; }

    public void setting()
    {
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform t = canvas.transform.Find("SoundUI");
            if (t != null)
            {
                panel = t.gameObject;
                panel.SetActive(true);
            }
        }
    }

    public void outsetting() { if (panel != null) panel.SetActive(false); }
    public void openmode() => select.SetActive(true);
    public void closmode() => select.SetActive(false);

    // --- [키 바인딩 로직] ---
    public void UpdateKeyUI()
    {
        if (KeyBinding.instance == null) return;
        if (rotateTxt) rotateTxt.text = KeyBinding.instance.rotate.ToString();
        if (rightTxt) rightTxt.text = KeyBinding.instance.right.ToString();
        if (leftTxt) leftTxt.text = KeyBinding.instance.left.ToString();
        if (downTxt) downTxt.text = KeyBinding.instance.down.ToString();
        if (hardDropTxt) hardDropTxt.text = KeyBinding.instance.hardDrop.ToString();
        if (holdTxt) holdTxt.text = KeyBinding.instance.hold.ToString();
        if (hold2Txt) hold2Txt.text = KeyBinding.instance.hold2.ToString();
        if (zRotateTxt) zRotateTxt.text = KeyBinding.instance.zRotate.ToString();
        if (aRotateTxt) aRotateTxt.text = KeyBinding.instance.aRotate.ToString();
        if (openstatTxt) openstatTxt.text = KeyBinding.instance.openstat.ToString();
    }

    public void ChangeKey(int keyIndex) { if (!isWaitingForKey) StartCoroutine(WaitForKeyPress(keyIndex)); }

    IEnumerator WaitForKeyPress(int keyIndex)
    {
        isWaitingForKey = true;
        if (statusText != null) statusText.text = "Press Any Key...";
        bool keyDetected = false;
        while (!keyDetected)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode) && kcode != KeyCode.None && !kcode.ToString().Contains("Mouse"))
                    {
                        ApplyKey(keyIndex, kcode);
                        keyDetected = true;
                        break;
                    }
                }
            }
            yield return null;
        }
        if (statusText != null) statusText.text = "Saved!";
        yield return new WaitForSeconds(0.5f);
        if (statusText != null) statusText.text = "";
        isWaitingForKey = false;
    }

    void ApplyKey(int index, KeyCode newKey)
    {
        KeyBinding kb = KeyBinding.instance;
        if (kb == null) return;
        switch (index)
        {
            case 0: kb.rotate = newKey; break;
            case 1: kb.right = newKey; break;
            case 2: kb.left = newKey; break;
            case 3: kb.down = newKey; break;
            case 4: kb.hardDrop = newKey; break;
            case 5: kb.hold = newKey; break;
            case 6: kb.hold2 = newKey; break;
            case 7: kb.zRotate = newKey; break;
            case 8: kb.aRotate = newKey; break;
            case 10: kb.openstat = newKey; break;
        }
        UpdateKeyUI();
        kb.SaveKeys();
    }

    public void openkey() { keypanel.SetActive(true); UpdateKeyUI(); }
    public void closekey() { keypanel.SetActive(false); UpdateKeyUI(); }
    public void StoryStart() => SceneChanger.BG("StoryTetris");
    public void DungeonStart() => SceneChanger.BG("Dungeon");
    public void ResetLevelCheck() => ResetWarning.SetActive(true);

    public void ResetLevel()
    {
        PlayerPrefs.DeleteAll();
        if (Stat.instance != null)
        {
            Stat.instance.difficult = 3;
            Stat.instance.it = 5;
            Stat.instance.atk = 0;
            Stat.instance.spd = 0;
            Stat.instance.maxstatpoint = 0;
            Stat.instance.hp = 100;
            Stat.instance.maxhp = 100;
            Stat.instance.level = 1;
            Stat.instance.ex = 0;
            Stat.instance.SaveData();
            Stat.instance.RefreshUI();
        }

        // 도감 데이터 리셋 (세이렌 제외)
        if (enemyData != null)
        {
            enemyData.slime = enemyData.goblin = enemyData.ouger =
            enemyData.golem = enemyData.chraken = enemyData.ghost = enemyData.dragon =
            enemyData.crown = enemyData.shaman = enemyData.knight_night = enemyData.boss = false;
        }

        UpdateEnemyDiscovery();
        ResetWarning.SetActive(false);
    }

    public void CancelReset() => ResetWarning.SetActive(false);
    public void OpenCustum() => custumpanel.SetActive(true);
    public void CloseCustum() => custumpanel.SetActive(false);
    public void StartCUstom() => SceneChanger.BG("custom");
    public void OpenStat() => Statpanel.SetActive(true);
    public void CloseStat() => Statpanel.SetActive(false);
}
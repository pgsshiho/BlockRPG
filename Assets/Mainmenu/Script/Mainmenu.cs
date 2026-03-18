using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    private Stat st; // Stat.instance를 캐싱
    public GameObject panel;
    public TextMeshProUGUI dif;
    public GameObject select;
    public TextMeshProUGUI statusText;
    public GameObject keypanel;
    public GameObject ResetWarning;
    public GameObject custumpanel;
    private bool isWaitingForKey = false;

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
        // 싱글톤 인스턴스를 우선 참조
        st = Stat.instance;
        if (st == null) st = FindAnyObjectByType<Stat>();

        UpdateKeyUI();
    }

    void Update()
    {
        // 난이도 텍스트 업데이트 (st가 null이 아닐 때만)
        if (st != null && dif != null)
            dif.text = st.difficult.ToString();
    }

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

    public void qu() => Application.Quit();

    public void infinitestart()
    {
        Time.timeScale = 1f;
        if (Stat.instance != null) Stat.instance.hp = Stat.instance.maxhp;
        SceneManager.LoadScene("Tetris");
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
            else Debug.LogError("SoundUI를 찾을 수 없습니다.");
        }
    }

    public void outsetting() { if (panel != null) panel.SetActive(false); }
    public void openmode() => select.SetActive(true);
    public void closmode() => select.SetActive(false);

    public void ChangeKey(int keyIndex)
    {
        if (isWaitingForKey) return;
        StartCoroutine(WaitForKeyPress(keyIndex));
    }

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
    public void StoryStart() => SceneManager.LoadScene("StoryTetris");

    public void ResetLevelCheck() => ResetWarning.SetActive(true);

    public void ResetLevel()
    {
        // 1. 저장된 모든 데이터 삭제
        PlayerPrefs.DeleteAll();

        // 2. 메모리 상의 Stat 초기화
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

            // 3. 초기화된 상태를 다시 저장 (덮어쓰기)
            Stat.instance.SaveData();
            // 4. UI 갱신 (StatUI 등이 있다면 호출됨)
            Stat.instance.RefreshUI();
        }

        ResetWarning.SetActive(false);
        Debug.Log("모든 데이터가 초기화 및 재저장되었습니다.");
    }

    public void CancelReset() => ResetWarning.SetActive(false);
    public void OpenCustum() => custumpanel.SetActive(true);
    public void CloseCustum() => custumpanel.SetActive(false);
    public void StartCUstom() => SceneManager.LoadScene("custom");
}
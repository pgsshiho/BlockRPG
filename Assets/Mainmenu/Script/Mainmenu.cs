using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    Stat st;
    public GameObject panel;
    public TextMeshProUGUI dif;
    public GameObject select;
    public TextMeshProUGUI statusText;
    public GameObject keypanel;
    private bool isWaitingForKey = false;

    [Header("Key Display Texts (연결 필수)")]
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
        st = FindAnyObjectByType<Stat>();
    }

    void Update()
    {
        // 난이도 텍스트 업데이트
        if (st != null) dif.text = st.difficult.ToString();
    }

    // 현재 KeyBinding 인스턴스의 값을 텍스트에 동기화
    void UpdateKeyUI()
    {
        if (KeyBinding.instance == null) return;

        // null 체크를 통해 연결되지 않은 텍스트가 있어도 에러가 나지 않게 방지
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

    public void difup()
    {
        if (st != null && st.difficult < 10) st.difficult++;
    }

    public void difdown()
    {
        if (st != null && st.difficult > 1) st.difficult--;
    }

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
                    // 마우스 클릭은 제외하고 키보드 입력만 받음
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
        yield return new WaitForSeconds(0.5f); // 잠깐 보여주고
        if (statusText != null) statusText.text = ""; // 텍스트 지우기
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
        // [최적화] 키가 바뀌었을 때만 UI를 한 번 업데이트합니다.
        UpdateKeyUI();

        // [저장] 실제 빌드에서도 유지되게 하려면 여기서 저장 로직을 추가합니다.
        kb.SaveKeys();
    }
    public void openkey() { 
        keypanel.SetActive(true);
        UpdateKeyUI();
    }
    public void closekey() { keypanel.SetActive(false);
        UpdateKeyUI();
    }
}
using UnityEngine;

public class KeyBinding : MonoBehaviour
{
    public static KeyBinding instance;
    public KeyData defaultKeys;

    [Header("Current Keys")]
    public KeyCode rotate;
    public KeyCode right, left, down, hardDrop, hold, hold2, zRotate, aRotate, openstat;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKeys();
        }
        else { Destroy(gameObject); }
    }

    // --- 추가된 초기화 함수 ---
    public void ResetKeys()
    {
        if (defaultKeys == null)
        {
            Debug.LogError("KeyBinding: DefaultKeys가 연결되지 않아 초기화할 수 없습니다!");
            return;
        }

        // 1. 현재 변수들을 기본값으로 덮어쓰기
        rotate = defaultKeys.rotate;
        right = defaultKeys.right;
        left = defaultKeys.left;
        down = defaultKeys.down;
        hardDrop = defaultKeys.hardDrop;
        hold = defaultKeys.hold;
        hold2 = defaultKeys.hold2;
        zRotate = defaultKeys.zRotate;
        aRotate = defaultKeys.aRotate;
        openstat = defaultKeys.openstat;

        // 2. 변경된 기본값을 저장소(PlayerPrefs)에 즉시 반영
        SaveKeys();

        Debug.Log("키 설정을 초기값으로 리셋했습니다.");
    }
    // -----------------------

    public void SaveKeys()
    {
        PlayerPrefs.SetInt("Key_Rotate", (int)rotate);
        PlayerPrefs.SetInt("Key_Right", (int)right);
        PlayerPrefs.SetInt("Key_Left", (int)left);
        PlayerPrefs.SetInt("Key_Down", (int)down);
        PlayerPrefs.SetInt("Key_HardDrop", (int)hardDrop);
        PlayerPrefs.SetInt("Key_Hold", (int)hold);
        PlayerPrefs.SetInt("Key_Hold2", (int)hold2);
        PlayerPrefs.SetInt("Key_ZRotate", (int)zRotate);
        PlayerPrefs.SetInt("Key_ARotate", (int)aRotate);
        PlayerPrefs.SetInt("Key_OpenStat", (int)openstat);
        PlayerPrefs.Save();
    }

    public void LoadKeys()
    {
        if (defaultKeys == null)
        {
            Debug.LogError("KeyBinding: DefaultKeys(ScriptableObject)가 인스펙터에 연결되지 않았습니다!");
            return;
        }

        rotate = (KeyCode)PlayerPrefs.GetInt("Key_Rotate", (int)defaultKeys.rotate);
        right = (KeyCode)PlayerPrefs.GetInt("Key_Right", (int)defaultKeys.right);
        left = (KeyCode)PlayerPrefs.GetInt("Key_Left", (int)defaultKeys.left);
        down = (KeyCode)PlayerPrefs.GetInt("Key_Down", (int)defaultKeys.down);
        hardDrop = (KeyCode)PlayerPrefs.GetInt("Key_HardDrop", (int)defaultKeys.hardDrop);
        hold = (KeyCode)PlayerPrefs.GetInt("Key_Hold", (int)defaultKeys.hold);
        hold2 = (KeyCode)PlayerPrefs.GetInt("Key_Hold2", (int)defaultKeys.hold2);
        zRotate = (KeyCode)PlayerPrefs.GetInt("Key_ZRotate", (int)defaultKeys.zRotate);
        aRotate = (KeyCode)PlayerPrefs.GetInt("Key_ARotate", (int)defaultKeys.aRotate);
        openstat = (KeyCode)PlayerPrefs.GetInt("Key_OpenStat", (int)defaultKeys.openstat);

        Debug.Log("키 설정을 로드했습니다.");
    }
}
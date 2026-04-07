using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject panel;
    public GameObject statpanel;
    public bool isON = false;
    SoundScript ss;
    private KeyBinding key;
    private EnemySpawn es;
    public GameObject blackpanel;
    public GameObject hiddenhold;
    public bool IsMirrored { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // 중복 생성 방지

        Time.timeScale = 1;
        isON = false;
        IsMirrored = false;

        if (panel != null) panel.SetActive(false);
        if (statpanel != null) statpanel.SetActive(false);
    }

    private void Start()
    {
        key = KeyBinding.instance;
    }

    void Update()
    {
        // 1. ESC로 일시정지
        forstorytriggrt tuto = FindAnyObjectByType<forstorytriggrt>();
        if (tuto != null && tuto.istuto) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isON) PauseGame();
            else ResumeGame();
        }

        // 2. 스탯창 열기/닫기
        if (key != null && Input.GetKeyDown(key.openstat))
        {
            if (!isON) openstat();
            else if (statpanel.activeSelf) closestat();
        }
    }

    // GameManager.cs 내부
    public void PauseGame()
    {
        SoundScript[] allSoundScripts = FindObjectsByType<SoundScript>(FindObjectsSortMode.None);

        foreach (SoundScript script in allSoundScripts)
        {
            script.RefreshSliderOnOpen(); 
        }
        Time.timeScale = 0;
        isON = true;
        if (panel != null) panel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        isON = false;
        if (panel != null) panel.SetActive(false);
        if (statpanel != null) statpanel.SetActive(false);
    }

    public void openstat()
    {
        Time.timeScale = 0;
        isON = true;
        if (statpanel != null) statpanel.SetActive(true);
    }

    public void closestat()
    {
        Time.timeScale = 1;
        isON = false;
        if (statpanel != null) statpanel.SetActive(false);
    }

    public void backmenu()
    {
        Time.timeScale = 1;
        es = FindAnyObjectByType<EnemySpawn>();
        if (es != null)
        {
            es.i = 0; 
            es.isSpawning = false;
        }
        blockclear.ScoreForSpeed = 0;
        blockclear.currentScore = 0;

        SceneChanger.BG("Mainmenu");
    }

    public void SetMirrorMode(bool isMirrored)
    {
        this.IsMirrored = isMirrored;
        if (Camera.main != null)
        {
            Camera.main.ResetProjectionMatrix();
            if (isMirrored)
            {
                // 투영 행렬 반전
                Matrix4x4 mat = Camera.main.projectionMatrix;
                mat *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
                Camera.main.projectionMatrix = mat;
            }
        }
    }
    void OnPreRender()
    {
        if (IsMirrored) GL.invertCulling = true;
    }

    void OnPostRender()
    {
        GL.invertCulling = false;
    }
    public void dechange()
    {
        blackpanel.SetActive(false);
    }
    public void change()
    {
        blackpanel.SetActive(true);
    }
    public void openhold()
    {
        hiddenhold.SetActive(false);
    }
    public void closehold()
    {
        hiddenhold.SetActive(true);
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject panel;
    public bool isON = false;
    KeyBinding key;
    public GameObject statpanel;
    EnemySpawn es;
    // 현재 화면과 조작이 반전된 상태인지 외부에서 확인할 수 있는 변수
    public bool IsMirrored { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        Time.timeScale = 1;
        isON = false;
        IsMirrored = false; // 초기화

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    private void Start()
    {
        key = KeyBinding.instance;
    }
    void Update()
    {
        // DialogueManager.Instance 대신 현재 게임 상태(isON 등)를 확인하거나
        // EventChannel을 통해 대화 시작/종료 상태를 GameManager가 알고 있게 설계하는 것이 좋습니다.
        if (isON) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isON) PauseGame();
            else ResumeGame();
        }
        if (Input.GetKeyDown(key.openstat))
        {
            if (!isON) openstat();
            else closestat();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        panel.SetActive(true);
        isON = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        panel.SetActive(false);
        isON = false;
    }

    public void backmenu()
    {
        // 씬 이동 전 시간 스케일 복구는 필수!
        Time.timeScale = 1;
        es =FindAnyObjectByType<EnemySpawn>();
        es.i = 0;
        SceneManager.LoadScene("Mainmenu");
    }

    // 화면 반전 실행 함수
    public void SetMirrorMode(bool isMirrored)
    {
        this.IsMirrored = isMirrored;

        if (Camera.main != null)
        {
            // 1. 카메라 행렬 초기화 (반드시 호출해야 함)
            Camera.main.ResetProjectionMatrix();

            if (isMirrored)
            {
                // 2. 현재 투영 행렬을 가져와서 X축을 -1배 시킴
                Matrix4x4 mat = Camera.main.projectionMatrix;
                mat *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
                Camera.main.projectionMatrix = mat;
            }
        }
    }
    public void openstat()
    {
        Time.timeScale = 0;
        statpanel.SetActive(true);
        isON = true;
    }
    public void closestat()
    {
        Time.timeScale = 1;
        statpanel.SetActive(false);
        isON = false;
    }
}
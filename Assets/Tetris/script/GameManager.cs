using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject panel;
    public bool isON = false;
    KeyBinding key;
    public GameObject statpanel;
    // 현재 화면과 조작이 반전된 상태인지 외부에서 확인할 수 있는 변수
    public bool IsMirrored { get; private set; }

    void Awake()
    {
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
        // 일시정지 로직
        if (Input.GetKeyDown(key.menu))
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
        SceneManager.LoadScene("Mainmenu");
    }

    // 화면 반전 실행 함수
    public void SetMirrorMode(bool isMirrored)
    {
        IsMirrored = isMirrored;

        // 카메라의 스케일을 반전시켜 거울 효과 연출
        if (Camera.main != null)
        {
            Vector3 scale = Camera.main.transform.localScale;
            scale.x = isMirrored ? -1f : 1f;
            Camera.main.transform.localScale = scale;
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
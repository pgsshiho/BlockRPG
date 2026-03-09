using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    Stat st;
    public GameObject panel;
    public TextMeshProUGUI dif;
    void Start()
    {
        st = FindAnyObjectByType<Stat>();
    }
   void Update()
    {
        dif.text = st.difficult.ToString();
    }
    public void qu()
    {
        Application.Quit();
    }
    public void start()
    {
        st.hp = st.maxhp;
        SceneManager.LoadScene("Tetris");
    }
public void difup()
{
    if (st.difficult < 10)
    {
        st.difficult++;
    }
}
public void difdown()
{
    if (st.difficult > 1)
    {
        st.difficult--;
    }
}
    public void setting()
    {
        // 1. 먼저 씬에서 "Canvas"라는 이름을 가진 오브젝트를 찾음
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas != null)
        {
            // 2. Canvas의 자식 중 "SoundUI"라는 이름을 가진 녀석을 찾음 (꺼져있어도 찾음)
            Transform t = canvas.transform.Find("SoundUI");
            if (t != null)
            {
                panel = t.gameObject;
                panel.SetActive(true);
            }
            else Debug.LogError("Canvas 아래에 SoundUI가 없습니다!");
        }
        else Debug.LogError("씬에 Canvas라는 이름의 오브젝트가 없습니다!");
    }

    public void outsetting()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}

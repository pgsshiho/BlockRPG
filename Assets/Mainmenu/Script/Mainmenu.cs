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
        Time.timeScale = 1f; 
        if (Stat.instance != null)
        {
            Stat.instance.hp = Stat.instance.maxhp;
        }
        SceneManager.LoadScene("Tetris");
    }
    public void difup()
{
        st = FindAnyObjectByType<Stat>();
        if (st.difficult < 10)
    {
        st.difficult++;
    }
}
public void difdown()
{
    st = FindAnyObjectByType<Stat>();
    if (st.difficult > 1)
    {
        st.difficult--;
    }
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

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
        panel.SetActive(true);
    }
    public void outsetting()
    {
        panel.SetActive(false);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject panel;
    public bool isON = false;
    void Start()
    {
        
    }
    void Awake()
    {
        // 씬이 시작될 때 시간이 멈춰있을 가능성을 방지
        Time.timeScale = 1;
        isON = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isON == false)
        {
            Time.timeScale = 0;
            panel.SetActive(true);
            isON = true;
        } 
        else if(Input.GetKeyDown(KeyCode.Escape) && isON == true)
        {
            Time.timeScale = 1;
            panel.SetActive(false);
            isON = false;
        }
    }
    public void backmenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }
    public void continues()
    {
        panel.SetActive(false);
        Time.timeScale = 1;
        isON = false;
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject panel;
    public bool isON = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isON == true)
        {
            Time.timeScale = 0;
            panel.SetActive(true);
            isON = false;
        } 
        else if(Input.GetKeyDown(KeyCode.Escape) && isON == false)
        {
            Time.timeScale = 1;
            panel.SetActive(false);
            isON = true;
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
        isON = true;
    }
}

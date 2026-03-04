using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    void Start()
    {
        
    }
   void Update()
    {
       
    }
    public void qu()
    {
        Application.Quit();
    }
    public void start()
    {
        SceneManager.LoadScene("Tetris");
    }
}

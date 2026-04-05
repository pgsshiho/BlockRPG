using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameclear : MonoBehaviour
{
    public TextMeshProUGUI gameover;
    public static int score;
    void Start()
    {
        GAmeover();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void GAmeover()
    {
        gameover.text = score + "";
    }
    public void backmenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }
}

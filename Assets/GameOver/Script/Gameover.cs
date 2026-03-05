using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameover : MonoBehaviour
{
    public TextMeshProUGUI gameover;
    public static string killerName = "알 수 없는 적";
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
        gameover.text = killerName + "에게 사망하였습니다";
    }
    public void backmenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }
}

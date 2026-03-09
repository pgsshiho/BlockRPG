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
        // killerName에서 "(Clone)" 문자열을 찾아서 빈 문자열("")로 바꿉니다.
        string cleanName = killerName.Replace("_0(Clone)", "").Trim();

        gameover.text = cleanName + "에게 사망하였습니다";
    }
    public void backmenu()
    {
        SceneManager.LoadScene("Mainmenu");
    }
}

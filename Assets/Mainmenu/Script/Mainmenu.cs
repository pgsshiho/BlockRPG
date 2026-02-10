using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    void Start()
    {
        
    }
   void Update()
    {
        if (Input.anyKeyDown) {
            SceneManager.LoadScene("worldmap");
            Debug.Log("눌림");
        }
    }
}

using UnityEngine;

public class KeyBinding : MonoBehaviour
{
    public KeyCode rotate = KeyCode.UpArrow;
    public KeyCode right = KeyCode.RightArrow;
    public KeyCode left = KeyCode.LeftArrow;
    public KeyCode down = KeyCode.DownArrow;
    public KeyCode hardDrop = KeyCode.Space;
    public KeyCode zRotate = KeyCode.Z;
    public KeyCode aRotate = KeyCode.A;
    public KeyCode menu = KeyCode.Escape;
    public KeyCode openstat = KeyCode.K;
    public static KeyBinding instance;
    void Start()
    {

    }

    void Update()
    {

    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("중복된 Stat 객체를 삭제합니다.");
            Destroy(gameObject);
            return;
        }
    }
}

using UnityEngine;

public class DataHolder : MonoBehaviour
{
    public static DataHolder instance;
    public int[] monsterCounts = new int[11];

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
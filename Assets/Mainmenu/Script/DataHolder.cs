using UnityEngine;

public class DataHolder : MonoBehaviour
{
    public static DataHolder instance;
    public int[] monsterCounts = new int[12]; // 반드시 12개 확인

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=green>DataHolder 생성 및 유지 설정 완료</color>");
        }
        else if (instance != this)
        {
            // 중요: 이미 인스턴스가 있다면 새로 생긴(데이터가 비어있는) 오브젝트를 즉시 파괴
            Debug.Log("<color=yellow>중복된 DataHolder 파괴 (기존 데이터 유지)</color>");
            Destroy(gameObject);
        }
    }
}
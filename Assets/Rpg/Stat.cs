using UnityEngine;

public class Stat : MonoBehaviour
{
    public static Stat instance;
    public int difficult = 3;

    public int it = 0, atk = 0, def = 0;
    public int maxstatpoint = 0;
    public int currentmaxstatpoint = 0;

    public int hp = 100;
    public int maxhp = 100;
    public int level = 1;
    public float ex = 0;

    private void Awake()
    {
        if (instance == null)
        {
            // 1. 내가 처음 생성된 Stat이라면 인스턴스로 등록
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 2. 이미 instance가 있다면, 새로 생긴(씬 전환으로 생성된) 나는 삭제
            Debug.Log("중복된 Stat 객체를 삭제합니다.");
            Destroy(gameObject);
            return; // 중요: 파괴된 객체가 이후 로직을 타지 않게 즉시 종료
        }
    }

    void Update()
    {
    }

    // 경험치가 쌓일 때마다 호출하거나 Update에서 체크
    public void LevelCheck()
    {
        float requiredEx = level * 30f;
        if (ex >= requiredEx)
        {
            ex -= requiredEx; // 남은 경험치 이월
            level++;
            maxstatpoint += 5; // 레벨업 시 포인트 보너스 (예시)
            Debug.Log("Level Up! Current Level: " + level);
        }
    }

    public void upit() { it++; }
    public void upatk() { atk++; }
    public void updef() { def++; }

    // 외부에서 경험치 획득 시 호출
    public void GainExperience(float amount)
    {
        ex += amount;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void upit() { it++; }
    public void upatk() { atk++; }
    public void updef() { def++; }

    // 1. 데미지 함수 (연산자 수정 완료)
    public void damage(int Damage, string name)
    {
        hp -= Damage;
        if (hp <= 0)
        {
            Gameover.killerName = name;
            SceneManager.LoadScene("Gameover");
        }
    }

    // 2. 경험치 획득 함수 (하나로 합침)
    public void GainExperience(float amount)
    {
        ex += amount;
        LevelCheck(); // 경험치 먹을 때마다 레벨업 체크
    }

    // 3. 레벨업 로직
    public void LevelCheck()
    {
        float requiredEx = level * 30f;

        // 경험치가 충분하면 계속 레벨업 할 수 있도록 while 사용 (선택사항)
        while (ex >= requiredEx)
        {
            ex -= requiredEx;
            level++;
            maxstatpoint += 5; // 레벨업 보너스 포인트
            Debug.Log("Level Up! Current Level: " + level);

            // 다음 레벨에 필요한 경험치 재계산
            requiredEx = level * 30f;
        }
    }
}
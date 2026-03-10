using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    public GameObject hpbar;

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
    public void Update()
    {
    }
    public void upit() { it++; }
    public void upatk() { atk++; }
    public void updef() { def++; }

    // 1. 데미지 함수 (연산자 수정 완료)
    public void damage(int Damage, string name)
    {
        hp -= Damage * difficult;
        hpcal();
        if (hp <= 0)
        {
            Gameover.killerName = name;
            SceneManager.LoadScene("Gameover");
        }
    }
    private void Start()
    {
        hpcal();
        Debug.Log($"현재 체력{hp}");
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
    // Stat.cs 에 추가 및 수정
    public void ResetStatus()
    {
        hp = maxhp;
        // UI는 씬이 로드된 후에 찾아야 하므로 여기서는 값만 초기화합니다.
    }

    public void hpcal()
    {
        // 씬 전환 직후에는 Unity가 오브젝트를 바로 못 찾을 수 있으므로 
        // 매번 새로 찾는 로직을 강화합니다.
        GameObject found = GameObject.Find("HPBar");
        if (found != null)
        {
            hpbar = found;
            float hpRatio = (float)hp / maxhp;
            hpbar.GetComponent<Image>().fillAmount = hpRatio;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Tetris")
        {
            hpcal(); // 씬이 로드되면 UI 바를 새로 찾아 연결
        }
    }
}
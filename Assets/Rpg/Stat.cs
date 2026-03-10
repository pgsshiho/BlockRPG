using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
    public static Stat instance;
    public int difficult = 3;

    public int it = 0, atk = 0, spd = 0;
    public int maxstatpoint = 0;
    public int hp = 100;
    public int maxhp = 100;
    public int level = 1;
    public float ex = 0;
    public GameObject hpbar;
    StatUI su;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // 1. 기존에 이미 존재하던 데이터(메인화면 등에서 온 것)를 현재 인스턴스로 복사
            this.it = instance.it;
            this.atk = instance.atk;
            this.spd = instance.spd;
            this.maxstatpoint = instance.maxstatpoint;
            this.hp = instance.hp;
            this.maxhp = instance.maxhp;
            this.level = instance.level;
            this.ex = instance.ex;

            // 2. 구버전(UI가 없을 수도 있는 오브젝트)을 삭제
            Destroy(instance.gameObject);

            // 3. 현재 UI가 붙어있는 '나'를 새로운 대표(instance)로 설정
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public virtual void upit()
    {
        if (maxstatpoint > 0 && it < 5)
        {
            it++;
            maxstatpoint--; // 포인트 소모 추가
        }
    }

    public virtual void upatk()
    {
        if (maxstatpoint > 0)
        {
            atk++;
            maxstatpoint--;
        }
    }

    public virtual void upmaxhp()
    {
        if (maxstatpoint > 0)
        {
            maxhp += 10;
            maxstatpoint--;
        }
    }

    public virtual void upspd()
    {
        if (maxstatpoint > 0 && spd < 10)
        {
            spd++;
            maxstatpoint--;
        }
    }
    public virtual void downit() { it--; maxstatpoint++; }
    public virtual void downatk() { atk--; maxstatpoint++; }
    public virtual void downmaxhp() { maxhp = Mathf.Max(maxhp - 10, 10); maxstatpoint++; } // 체력 최소값 보호
    public virtual void downspd() { spd = Mathf.Max(spd - 1, -10); maxstatpoint++; }

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
    }

    public void GainExperience(float amount)
    {
        ex += amount;
        LevelCheck();
    }

    public void LevelCheck()
    {
        float requiredEx = level * 30f;
        while (ex >= requiredEx)
        {
            ex -= requiredEx;
            level++;
            maxstatpoint += 1;
            requiredEx = level * 30f;
        }
    }

    public void ResetStatus()
    {
        hp = maxhp;
        hpcal();
    }

    public void hpcal()
    {
        GameObject found = GameObject.Find("HPBar");
        if (found != null)
        {
            hpbar = found;
            float hpRatio = (float)hp / maxhp;
            hpbar.GetComponent<Image>().fillAmount = hpRatio;
        }
    }

    public void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    public void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Tetris") hpcal();
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stat : MonoBehaviour
{
    public static Stat instance;
    public int difficult = 3;

    public int it = 5, atk = 0, spd = 0;
    public int maxstatpoint = 0;
    public int hp = 100;
    public int maxhp = 100;
    public int level = 1;
    public float ex = 0;
    public GameObject hpbar;
    public GameObject expbar;
    StatUI su;
    Enemybase eb;
    EnemySpawn es;
    float requiredEx;
    protected virtual void Awake() // virtual로 선언하여 자식에서 확장 가능하게 함
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // [중요] 기존의 모든 데이터를 현재(새로운) 인스턴스로 복사
            this.it = instance.it;
            this.atk = instance.atk;
            this.spd = instance.spd;
            this.maxstatpoint = instance.maxstatpoint;
            this.hp = instance.hp;
            this.maxhp = instance.maxhp;
            this.level = instance.level;
            this.ex = instance.ex;
            this.difficult = instance.difficult;

            Destroy(instance.gameObject);
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
        if (maxstatpoint > 0 && atk < 100)
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
            eb = FindAnyObjectByType<Enemybase>();
            es = FindAnyObjectByType<EnemySpawn>();
            es.i = 0;
            Destroy(eb.gameObject);
            SceneManager.LoadScene("Gameover");
        }
    }

    private void Start()
    {
        requiredEx = level * 30f;
        expcal();
        hpcal();
    }

    public void GainExperience(float amount)
    {
        ex += amount;
        expcal();
        LevelCheck();
    }

    public void LevelCheck()
    {
        requiredEx = level * 30f;
        while (ex >= requiredEx)
        {
            ex -= requiredEx;
            level++;
            maxstatpoint += 1;
            requiredEx = level * 30f;
            expcal();
            su.levelup();
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
    public void expcal()
    {
        GameObject founde = GameObject.Find("EXBar");
        if (founde != null)
        {
            expbar = founde;
            float exRatio = (float)ex / requiredEx;
            expbar.GetComponent<Image>().fillAmount = exRatio;
        }
    }
    public void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    public void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Tetris") {
            hpcal();
            expcal();
                }
    }
}
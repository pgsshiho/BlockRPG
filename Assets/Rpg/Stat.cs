using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Stat : MonoBehaviour, TakeDamage
{
    public static Stat instance;

    [Header("Settings")]
    public int difficult = 3;

    [Header("Status Values")]
    public int it = 5;
    public int atk = 0;
    public int spd = 0;
    public int maxstatpoint = 0;
    public int hp = 100;
    public int maxhp = 100;
    public int level = 1;
    public float ex = 0;

    private float requiredEx;
    private Image hpBarImage;
    private Image expBarImage;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 기존 인스턴스가 있다면 새 객체를 파괴하여 데이터 유지
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateRequiredEx();
        RefreshUI();
    }

    // 스탯 상승 로직
    public void upit() { if (maxstatpoint > 0 && it < 5) { it++; maxstatpoint--; } }
    public void upatk() { if (maxstatpoint > 0 && atk < 20) { atk++; maxstatpoint--; } }
    public void upmaxhp() { if (maxstatpoint > 0) { maxhp += 10; maxstatpoint--; hp = maxhp; hpcal(); } }
    public void upspd() { if (maxstatpoint > 0 && spd < 10) { spd++; maxstatpoint--; } }

    // 스탯 감소 로직
    public void downit() { if (it > 0) { it--; maxstatpoint++; } }
    public void downatk() { if (atk > 0) { atk--; maxstatpoint++; } }
    public void downmaxhp() { if (maxhp > 10) { maxhp -= 10; hp = Mathf.Min(hp, maxhp); maxstatpoint++; hpcal(); } }
    public void downspd() { if (spd > -10) { spd--; maxstatpoint++; } }

    public void damage(int Damage, string killerName)
    {
        hp -= Damage * difficult;
        hpcal();
        if (hp <= 0)
        {
            Gameover.killerName = killerName;
            Enemybase eb = FindFirstObjectByType<Enemybase>();
            if (eb != null) Destroy(eb.gameObject);
            SceneManager.LoadScene("Gameover");
        }
    }

    public void GainExperience(float amount)
    {
        ex += amount;
        LevelCheck();
        expcal();
    }
    public void TakeDamage(int amount, string attackerName)
    {
        damage(amount, attackerName);
    }
    private void LevelCheck()
    {
        UpdateRequiredEx();
        while (ex >= requiredEx)
        {
            ex -= requiredEx;
            level++;
            maxstatpoint += 1;
            UpdateRequiredEx();

            // UI에 레벨업 알림 보내기
            StatUI su = FindFirstObjectByType<StatUI>();
            if (su != null) su.ShowLevelUp();
        }
    }

    private void UpdateRequiredEx() => requiredEx = level * 30f;

    public void hpcal()
    {
        if (hpBarImage == null) FindUIBars();
        if (hpBarImage != null) hpBarImage.fillAmount = (float)hp / maxhp;
    }

    public void expcal()
    {
        if (expBarImage == null) FindUIBars();
        if (expBarImage != null) expBarImage.fillAmount = ex / requiredEx;
    }

    private void FindUIBars()
    {
        GameObject hObj = GameObject.Find("HPBar");
        if (hObj != null) hpBarImage = hObj.GetComponent<Image>();

        GameObject eObj = GameObject.Find("EXBar");
        if (eObj != null) expBarImage = eObj.GetComponent<Image>();
    }

    public void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    public void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        FindUIBars();
        hpcal();
        expcal();
    }
}
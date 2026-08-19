using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Stat : MonoBehaviour, TakeDamage
{
    public static Stat instance;

    [Header("Settings")]
    public int difficult = 2;

    [Header("Status Values")]
    public int it = 0;
    public int atk = 0;
    public int spd = 0;
    public int maxstatpoint = 0;
    public float hp = 100;
    public int maxhp = 100;
    public int level = 1;
    public float ex = 0;

    [Header("UI References")]
    public Image hpBarImage;
    public Image expBarImage;

    public Action OnStatChanged;
    public Action OnLevelUp;
    private float requiredEx;
    reincarnationSkill skill;
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();

            // 핵심: 클래스 선택 이벤트 구독
            // Role.OnRoleChosen static 액션을 사용하여 씬이 바뀌어도 연결 유지
            Role.OnRoleChosen += HandleRoleChanged;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        skill = FindAnyObjectByType<reincarnationSkill>();
        UpdateRequiredEx();
        RefreshUI();
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("Saved_Level", level);
        PlayerPrefs.SetFloat("Saved_Ex", ex);
        PlayerPrefs.SetInt("Saved_MaxStatPoint", maxstatpoint);
        PlayerPrefs.SetInt("Saved_ATK", atk);
        PlayerPrefs.SetInt("Saved_SPD", spd);
        PlayerPrefs.SetInt("Saved_IT", it);
        PlayerPrefs.SetInt("Saved_MaxHP", maxhp);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        level = PlayerPrefs.GetInt("Saved_Level", 1);
        ex = PlayerPrefs.GetFloat("Saved_Ex", 0);
        maxstatpoint = PlayerPrefs.GetInt("Saved_MaxStatPoint", 0);
        atk = PlayerPrefs.GetInt("Saved_ATK", 0);
        spd = PlayerPrefs.GetInt("Saved_SPD", 0);
        it = PlayerPrefs.GetInt("Saved_IT", 0);
        maxhp = PlayerPrefs.GetInt("Saved_MaxHP", 100);
        hp = maxhp;
        UpdateRequiredEx();
    }
    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 구독 해제
        Role.OnRoleChosen -= HandleRoleChanged;
    }
    public void upit() { if (maxstatpoint > 0 && it < 5) { it++; maxstatpoint--; SaveAndNotify(); } }
    public void upatk() { if (maxstatpoint > 0 && atk < 20) { atk++; maxstatpoint--; SaveAndNotify(); } }
    public void upmaxhp() { if (maxstatpoint > 0) { maxhp += 10; maxstatpoint--; hpcal(); SaveAndNotify(); } }
    public void upspd() { if (maxstatpoint > 0 && spd < 10) { spd++; maxstatpoint--; SaveAndNotify(); } }

    public void downit() { if (it > baseIt) { it--; maxstatpoint++; SaveAndNotify(); } }
    public void downatk() { if (atk > baseAtk) { atk--; maxstatpoint++; SaveAndNotify(); } }
    public void downmaxhp() { if (maxhp > baseMaxHp) { maxhp -= 10; hp = Mathf.Min(hp, maxhp); maxstatpoint++; hpcal(); SaveAndNotify(); } }
    public void downspd() { if (spd > baseSpd) { spd--; maxstatpoint++; SaveAndNotify(); } }
    [Header("Base Role Stats (Cannot be reduced)")]
    public int baseIt = 0;
    public int baseAtk = 0;
    public int baseSpd = 0;
    public int baseMaxHp = 100;
    private void SaveAndNotify()
    {
        SaveData();
        SafeNotify();
    }
    private void SafeNotify()
    {
        try { OnStatChanged?.Invoke(); }
        catch (Exception) { OnStatChanged = null; } // 죽은 객체 청소
    }

    public void damage(int Damage, string killerName)
    {
        if (!skill.isshiled)
        {
            hp -= Damage * difficult * 0.7f;
            hpcal();
            diecheck(Damage, killerName);
        }
        else
        {
            return;
        }
    }

    public void GainExperience(float amount)
    {   
        float difficultyMultiplier = difficult / 2f;

        // 최종 경험치 계산
        float finalEx = amount * difficultyMultiplier;

        ex += finalEx;
        LevelCheck();
        expcal();
        SaveData();
    }
    public void TakeDamage(int amount, string attackerName) => damage(amount, attackerName);

    private void LevelCheck()
    {
        UpdateRequiredEx();
        bool isLeveledUp = false;
        while (ex >= requiredEx)
        {
            ex -= requiredEx;
            level++;    
            maxstatpoint += 1;
            UpdateRequiredEx();
            isLeveledUp = true;
        }
        if (isLeveledUp)
        {
            OnLevelUp?.Invoke();
            SaveAndNotify();
        }
    }

    private void UpdateRequiredEx() => requiredEx = level * 30f;

    public void hpcal()
    {
        if (hpBarImage != null)
            hpBarImage.fillAmount = hp / maxhp;
    }
    public void expcal()
    {
        if (expBarImage != null)
            expBarImage.fillAmount = ex / requiredEx;
    }

    private void FindUIBars()
    {
        GameObject hObj = GameObject.Find("HPBar");
        if (hObj != null) hpBarImage = hObj.GetComponent<Image>();
        GameObject eObj = GameObject.Find("EXBar");
        if (eObj != null) expBarImage = eObj.GetComponent<Image>();
    }

    public void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    public void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { RefreshUI(); skill = FindAnyObjectByType<reincarnationSkill>(); }

    public void RefreshUI()
    {
        FindUIBars();

        if (hpBarImage != null)
            hpBarImage.fillAmount = hp / maxhp;

        if (expBarImage != null)
            expBarImage.fillAmount = ex / requiredEx;

        SafeNotify();
    }
    public void diecheck(int Damage, string killerName)
    {
        if (hp <= 0)
        {
            Gameover.killerName = killerName;
            SceneChanger.BG("Gameover");
        }
    }
    private void HandleRoleChanged(RoleSkills chosenRole)
    {
        // 1. 새로운 직업 적용 전, 현재 투자된 포인트들을 제외하고 기초값만 초기화
        // (투자한 포인트는 유지하고 직업 보너스 수치만 뺌)
        ResetCurrentRoleBonus();

        // 2. 선택된 직업에 따라 기초 최소치(base) 및 현재치(it, atk..) 증가
        switch (chosenRole)
        {
            case RoleSkills.Warrior:
                baseMaxHp = 120; // 기본 100 + 직업 20
                maxhp += 20;
                break;
            case RoleSkills.Rogue:
                baseSpd = 2;
                spd += 2;
                break;
            case RoleSkills.Archer:
                baseAtk = 2;
                atk += 2;
                break;
            case RoleSkills.Wizard:
                baseIt = 2;
                it += 2;
                break;
        }

        hp = Mathf.Max(hp, maxhp);
        SaveAndNotify();
    }

    private void ResetCurrentRoleBonus()
    {
        // 이전 직업이 무엇이었든 기초 최소치를 기본값으로 되돌림
        // 현재 스탯에서 이전 직업이 부여했던 base 수치를 차감
        it -= baseIt;
        atk -= baseAtk;
        spd -= baseSpd;
        maxhp -= (baseMaxHp - 100); // 100은 순수 기본 체력

        // 기초값 초기화
        baseIt = 0;
        baseAtk = 0;
        baseSpd = 0;
        baseMaxHp = 100;

        hp = Mathf.Min(hp, maxhp);
    }
    public void FullHeal()
    {
        hp = maxhp;
        hpcal();
    }
    public void ResetStat()
    {
        difficult = 3;

        it = 0;
        atk = 0;
        spd = 0;

        maxstatpoint = 0;

        hp = 100;
        maxhp = 100;

        level = 1;
        ex = 0;

        SaveData();
        RefreshUI();
    }
}
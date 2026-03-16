using System;
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

    [Header("UI References")]
    private Image hpBarImage;
    private Image expBarImage;

    public Action OnStatChanged;
    public Action OnLevelUp;
    private float requiredEx;

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData(); // 게임 시작 시 데이터 로드
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateRequiredEx();
        RefreshUI();
    }

    // --- [ 데이터 저장 및 로드 시스템 ] ---
    public void SaveData()
    {
        PlayerPrefs.SetInt("Saved_Level", level);
        PlayerPrefs.SetFloat("Saved_Ex", ex);
        PlayerPrefs.SetInt("Saved_MaxStatPoint", maxstatpoint);
        PlayerPrefs.SetInt("Saved_ATK", atk);
        PlayerPrefs.SetInt("Saved_SPD", spd);
        PlayerPrefs.SetInt("Saved_IT", it);
        PlayerPrefs.SetInt("Saved_MaxHP", maxhp);
        PlayerPrefs.Save(); // 물리적인 저장소에 즉시 기록
        Debug.Log("데이터가 저장되었습니다.");
    }

    public void LoadData()
    {
        // 저장된 데이터가 있을 때만 불러오고, 없으면 기본값(뒤의 숫자)을 사용
        level = PlayerPrefs.GetInt("Saved_Level", 1);
        ex = PlayerPrefs.GetFloat("Saved_Ex", 0);
        maxstatpoint = PlayerPrefs.GetInt("Saved_MaxStatPoint", 0);
        atk = PlayerPrefs.GetInt("Saved_ATK", 0);
        spd = PlayerPrefs.GetInt("Saved_SPD", 0);
        it = PlayerPrefs.GetInt("Saved_IT", 5);
        maxhp = PlayerPrefs.GetInt("Saved_MaxHP", 100);

        hp = maxhp; // 시작 시 체력 풀로 채움
        UpdateRequiredEx();
    }

    // 환생 등을 위한 데이터 초기화 메서드
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("모든 데이터가 초기화되었습니다.");
    }

    // --- [ 스탯 조작 로직 ] ---
    // 공통적으로 스탯이 변할 때마다 SaveData()를 호출하여 자동 저장합니다.

    public void upit() { if (maxstatpoint > 0 && it < 5) { it++; maxstatpoint--; SaveAndNotify(); } }
    public void upatk() { if (maxstatpoint > 0 && atk < 20) { atk++; maxstatpoint--; SaveAndNotify(); } }
    public void upmaxhp() { if (maxstatpoint > 0) { maxhp += 10; maxstatpoint--; hp = maxhp; hpcal(); SaveAndNotify(); } }
    public void upspd() { if (maxstatpoint > 0 && spd < 10) { spd++; maxstatpoint--; SaveAndNotify(); } }

    public void downit() { if (it > 0) { it--; maxstatpoint++; SaveAndNotify(); } }
    public void downatk() { if (atk > 0) { atk--; maxstatpoint++; SaveAndNotify(); } }
    public void downmaxhp() { if (maxhp > 10) { maxhp -= 10; hp = Mathf.Min(hp, maxhp); maxstatpoint++; hpcal(); SaveAndNotify(); } }
    public void downspd() { if (spd > -10) { spd--; maxstatpoint++; SaveAndNotify(); } }

    private void SaveAndNotify()
    {
        OnStatChanged?.Invoke();
        SaveData();
    }

    // --- [ 전투 및 레벨 로직 ] ---
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
        SaveData(); // 경험치 획득 시에도 저장 (진행도 보존)
    }

    public void TakeDamage(int amount, string attackerName)
    {
        damage(amount, attackerName);
    }

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
            OnStatChanged?.Invoke();
            OnLevelUp?.Invoke();
            // 레벨업 직후 상태 저장
            SaveData();
        }
    }

    private void UpdateRequiredEx() => requiredEx = level * 30f;

    // --- [ UI 업데이트 로직 ] ---
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
        OnStatChanged?.Invoke(); // 씬 이동 후 스탯 텍스트 갱신용
    }
}
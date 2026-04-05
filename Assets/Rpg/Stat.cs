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
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
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
        hp -= Damage * difficult;
        hpcal();
        diecheck(Damage, killerName);
    }

    public void GainExperience(float amount)
    {
        ex += amount;
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

    public void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    public void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RefreshUI();

    public void RefreshUI()
    {
        FindUIBars();
        hpcal();
        expcal();
        SafeNotify();
    }
    public void diecheck(int Damage, string killerName)
    {
        if (hp <= 0)
        {
            Gameover.killerName = killerName;
            SceneManager.LoadScene("Gameover");
        }
    }
}
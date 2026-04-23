using UnityEngine;
using System;

public enum SkillType
{
    Shield,
    Lightning,
    SelfHeal
}

public class Reincarnation : MonoBehaviour
{
    public static Reincarnation instance;

    [Header("Reincarnation Data")]
    public int reincarnationCount = 0;
    public int reincarnationPoints = 0; // 스킬 업그레이드용 별도 포인트

    [Header("Skill Levels")]
    public int shieldLevel = 0;
    public int lightningLevel = 0;
    public int selfHealLevel = 0;

    public Action OnSkillChanged; // UI 갱신용 이벤트

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadReincarnationData();
        }
        else Destroy(gameObject);
    }

    // 환생 실행 시 호출 (외부 환생 코드에서 호출)
    public void PerformReincarnation()
    {
        reincarnationCount++;
        reincarnationPoints += 1;

        SaveReincarnationData();
        OnSkillChanged?.Invoke();
    }

    // 스킬 레벨업 통합 관리
    public void UpgradeSkill(SkillType type)
    {
        if (reincarnationPoints <= 0)
        {
            return;
        }

        switch (type)
        {
            case SkillType.Shield:
                shieldLevel++;
                break;
            case SkillType.Lightning:
                lightningLevel++;
                break;
            case SkillType.SelfHeal:
                selfHealLevel++;
                break;
        }

        reincarnationPoints--; // 포인트 차감
        SaveReincarnationData();
        OnSkillChanged?.Invoke();
    }

    // 데이터 저장
    public void SaveReincarnationData()
    {
        PlayerPrefs.SetInt("Reinc_Count", reincarnationCount);
        PlayerPrefs.SetInt("Reinc_Points", reincarnationPoints);
        PlayerPrefs.SetInt("Skill_Shield", shieldLevel);
        PlayerPrefs.SetInt("Skill_Lightning", lightningLevel);
        PlayerPrefs.SetInt("Skill_SelfHeal", selfHealLevel);
        PlayerPrefs.Save();
    }

    // 데이터 로드
    public void LoadReincarnationData()
    {
        reincarnationCount = PlayerPrefs.GetInt("Reinc_Count", 0);
        reincarnationPoints = PlayerPrefs.GetInt("Reinc_Points", 0);
        shieldLevel = PlayerPrefs.GetInt("Skill_Shield", 0);
        lightningLevel = PlayerPrefs.GetInt("Skill_Lightning", 0);
        selfHealLevel = PlayerPrefs.GetInt("Skill_SelfHeal", 0);
    }
}
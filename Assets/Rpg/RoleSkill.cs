using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleSkill : Role
{
    [System.Serializable]
    public struct SkillUI
    {
        public RoleSkills role;
        public GameObject skillObject; // 해당 직업의 스킬 UI 부모 오브젝트
        public Image cooldownImage;    // 쿨타임을 표시할 Image (Filled 방식)
        public float cooldownTime;     // 스킬별 쿨타임 설정
    }

    [Header("Skill UI Settings")]
    public List<SkillUI> skillUIList;

    private Dictionary<RoleSkills, SkillUI> skillLookup = new Dictionary<RoleSkills, SkillUI>();
    private Dictionary<RoleSkills, float> lastUsedTime = new Dictionary<RoleSkills, float>();
    private KeyBinding key;

    public static bool isWarrior = false;
    public static bool isRogue = false;
    public static bool isArcher = false;
    public static bool isWizard = false;
    public static int avoid = 2;

    void Start()
    {
        key = FindAnyObjectByType<KeyBinding>();

        // 빠른 조회를 위해 딕셔너리에 담기
        foreach (var ui in skillUIList)
        {
            skillLookup[ui.role] = ui;
            lastUsedTime[ui.role] = -100f;
        }

        RefreshSkillUI();
    }

    void Update()
    {
        UpdateCooldownDisplay();

        if (Input.GetKeyDown(key.Skill))
        {
            if (CanUseSkill(roleSkill))
            {
                ExecuteSkill();
                lastUsedTime[roleSkill] = Time.time;
            }
        }
    }
    public void RefreshSkillUI()
    {
        foreach (var ui in skillUIList)
        {
            if (ui.skillObject != null)
                ui.skillObject.SetActive(ui.role == roleSkill);
        }
    }

    bool CanUseSkill(RoleSkills role)
    {
        if (!skillLookup.ContainsKey(role)) return false;
        return Time.time >= lastUsedTime[role] + skillLookup[role].cooldownTime;
    }

    void UpdateCooldownDisplay()
    {
        foreach (var ui in skillUIList)
        {
            if (ui.cooldownImage == null) continue;

            float remaining = (lastUsedTime[ui.role] + ui.cooldownTime) - Time.time;
            if (remaining > 0)
            {
                ui.cooldownImage.fillAmount = remaining / ui.cooldownTime;
            }
            else
            {
                ui.cooldownImage.fillAmount = 0;
            }
        }
    }

    void ExecuteSkill()
    {
        switch (roleSkill)
        {
            case RoleSkills.Warrior:
                StopAllCoroutines();
                StartCoroutine(WarriorBuffCoroutine());
                break;
            case RoleSkills.Rogue:
                isRogue = true; avoid = 2;
                break;
            case RoleSkills.Archer:
                isArcher = true;
                break;
            case RoleSkills.Wizard:
                isWizard = true;
                Enemybase[] enemies = FindObjectsByType<Enemybase>(FindObjectsSortMode.None);
                foreach (var e in enemies) if (!e.IsDead) e.ApplyStun(3.0f);
                break;
        }
    }

    IEnumerator WarriorBuffCoroutine()
    {
        isWarrior = true;
        yield return new WaitForSeconds(6.0f);
        isWarrior = false;
    }
}
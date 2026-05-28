using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleSkill : Role
{
    [Header("오브젝트 연결")]
    // 스프라이트가 교체될 메인 오브젝트의 SpriteRenderer
    public SpriteRenderer skillIconRenderer;
    // 쿨타임 시 켜질 가림막 오브젝트 (Skillenable)
    public GameObject skillEnableObj;

    [Header("직업별 스프라이트")]
    public Sprite warriorIcon;
    public Sprite rogueIcon;
    public Sprite archerIcon;
    public Sprite wizardIcon;

    [Header("직업별 쿨타임 (초)")]
    public float warriorCooldown = 10f;
    public float rogueCooldown = 8f;
    public float archerCooldown = 5f;
    public float wizardCooldown = 15f;

    private Dictionary<RoleSkills, float> cooldownLookup = new Dictionary<RoleSkills, float>();
    private Dictionary<RoleSkills, float> lastUsedTime = new Dictionary<RoleSkills, float>();
    private KeyBinding key;

    public static bool isWarrior, isRogue, isArcher, isWizard;
    public static int avoid = 2;

    void Awake()
    {
        // 씬 시작 시 데이터 로드
        roleSkill = (RoleSkills)PlayerPrefs.GetInt("SelectedRole", 0);
    }

    void OnEnable() { Role.OnRoleChosen += HandleRoleChange; }
    void OnDisable() { Role.OnRoleChosen -= HandleRoleChange; }

    void Start()
    {
        key = FindAnyObjectByType<KeyBinding>();

        // 쿨타임 값 세팅
        cooldownLookup[RoleSkills.Warrior] = warriorCooldown;
        cooldownLookup[RoleSkills.Rogue] = rogueCooldown;
        cooldownLookup[RoleSkills.Archer] = archerCooldown;
        cooldownLookup[RoleSkills.Wizard] = wizardCooldown;
        cooldownLookup[RoleSkills.None] = 0f;

        // 시작 즉시 사용 가능하도록 초기화
        foreach (RoleSkills role in System.Enum.GetValues(typeof(RoleSkills)))
            lastUsedTime[role] = -100f;

        RefreshSkillUI();
    }

    void Update()
    {
        // 가림막 오브젝트 On/Off 제어
        UpdateCooldownState();

        // 스킬 실행 체크
        if (Input.GetKeyDown(key.Skill) && CanUseSkill())
        {
            ExecuteSkill();
            lastUsedTime[roleSkill] = Time.time;
        }
    }

    public void RefreshSkillUI()
    {
        if (skillIconRenderer == null) return;

        // SpriteRenderer의 sprite를 직접 교체
        switch (roleSkill)
        {
            case RoleSkills.Warrior: skillIconRenderer.sprite = warriorIcon; break;
            case RoleSkills.Rogue: skillIconRenderer.sprite = rogueIcon; break;
            case RoleSkills.Archer: skillIconRenderer.sprite = archerIcon; break;
            case RoleSkills.Wizard: skillIconRenderer.sprite = wizardIcon; break;
            case RoleSkills.None: skillIconRenderer.sprite = null; break;
        }
    }

    private void HandleRoleChange(RoleSkills newRole)
    {
        roleSkill = newRole;
        RefreshSkillUI();
    }

    bool CanUseSkill()
    {
        return Time.time >= lastUsedTime[roleSkill] + cooldownLookup[roleSkill];
    }

    void UpdateCooldownState()
    {
        if (skillEnableObj == null) return;

        // 쿨타임 중이면 켜고, 아니면 끔
        bool isCoolingDown = !CanUseSkill();
        if (skillEnableObj.activeSelf != isCoolingDown)
        {
            skillEnableObj.SetActive(isCoolingDown);
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
            case RoleSkills.Rogue: isRogue = true; avoid = 2; break;
            case RoleSkills.Archer: isArcher = true; break;
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
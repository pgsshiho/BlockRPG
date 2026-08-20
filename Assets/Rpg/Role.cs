using System;
using UnityEngine;

public enum RoleSkills
{
    Warrior,
    Rogue,
    Archer,
    Wizard,
    None
}

public class Role : MonoBehaviour
{
    public RoleSkills roleSkill = RoleSkills.None;
    public GameObject Choose; // 직업 선택 창 UI

    // 핵심: static 액션을 선언하여 어디서든 구독 가능하게 함
    public static Action<RoleSkills> OnRoleChosen;

    public void Start()
    {
        // 1. 저장된 직업 로드 (기본값: None)
        roleSkill = (RoleSkills)PlayerPrefs.GetInt("SelectedRole", (int)RoleSkills.None);

        // 2. 직업 유무에 따른 UI 활성화 처리
        if (Choose != null)
        {
            // 직업이 None이면 선택창을 켜고(true), 직업이 이미 있으면 선택창을 끕니다(false)
            Choose.SetActive(roleSkill == RoleSkills.None);
        }

        // 3. 직업 변경 이벤트 전달
        OnRoleChosen?.Invoke(roleSkill);
    }

    public void chooseRole(int index)
    {
        roleSkill = (RoleSkills)index;

        // 씬이 바뀌어도 기억할 수 있도록 저장
        PlayerPrefs.SetInt("SelectedRole", index);
        PlayerPrefs.Save();

        OnRoleChosen?.Invoke(roleSkill);

        if (Choose != null) Choose.SetActive(false);
    }

    // 직업을 None으로 초기화하는 전용 메서드
    public void ResetRole()
    {
        roleSkill = RoleSkills.None;
        PlayerPrefs.SetInt("SelectedRole", (int)RoleSkills.None);
        PlayerPrefs.Save();

        OnRoleChosen?.Invoke(roleSkill);

        if (Choose != null) Choose.SetActive(true); // None이 되었으므로 선택창 재활성화
    }
}
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
    public GameObject Choose;
    
    // 핵심: static 액션을 선언하여 어디서든 구독 가능하게 함
    public static Action<RoleSkills> OnRoleChosen;
    public void Start()
    {
        roleSkill = (RoleSkills)PlayerPrefs.GetInt("SelectedRole", 0);
        OnRoleChosen?.Invoke(roleSkill);
    }
    public void chooseRole(int index)
    {
        roleSkill = (RoleSkills)index;

        // 씬이 바뀌어도 기억할 수 있도록 저장 (0: Warrior, 1: Rogue 등)
        PlayerPrefs.SetInt("SelectedRole", index);

        OnRoleChosen?.Invoke(roleSkill);

        if (Choose != null) Choose.SetActive(false);
    }
}

using System;
using UnityEngine;
public enum RoleSkills
{
    Warrior,
    Rogue,
    Archer,
    Wizard
}

public class Role : MonoBehaviour
{
    public RoleSkills roleSkill;
    public GameObject Choose;

    // 핵심: static 액션을 선언하여 어디서든 구독 가능하게 함
    public static Action<RoleSkills> OnRoleChosen;

    public void chooseRole(int index)
    {
        roleSkill = (RoleSkills)index;

        // 1. 이벤트를 발생시킴 (구독 중인 Stat이 이를 수신함)
        OnRoleChosen?.Invoke(roleSkill);

        // 2. 선택창 비활성화
        if (Choose != null) Choose.SetActive(false);
    }
}

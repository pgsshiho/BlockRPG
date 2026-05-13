using UnityEngine;
using TMPro;

public class SkillUI : MonoBehaviour
{
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI shieldLvText;
    public TextMeshProUGUI lightningLvText;
    public TextMeshProUGUI healLvText;
    public GameObject UpgradePanel;
    private void Start()
    {
        if (Reincarnation.instance != null)
        {
            Reincarnation.instance.OnSkillChanged += UpdateSkillDisplay;
        }
        UpdateSkillDisplay();
    }

    void UpdateSkillDisplay()
    {
        var rc = Reincarnation.instance;
        pointText.text = $": {rc.reincarnationPoints}";
        shieldLvText.text = $"Lv.{rc.shieldLevel}";
        lightningLvText.text = $"Lv.{rc.lightningLevel}";
        healLvText.text = $"Lv.{rc.selfHealLevel}";
    }

    // 버튼에 연결할 함수들
    public void Btn_UpgradeShield() => Reincarnation.instance.UpgradeSkill(SkillType.Shield);
    public void Btn_UpgradeLightning() => Reincarnation.instance.UpgradeSkill(SkillType.Lightning);
    public void Btn_UpgradeHeal() => Reincarnation.instance.UpgradeSkill(SkillType.SelfHeal);
    public void CloseUpgrade() => UpgradePanel.SetActive(false);
}
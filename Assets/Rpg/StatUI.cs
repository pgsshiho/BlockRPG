using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class StatUI : Stat
{

    [Header("UI Text Components")]
    public TextMeshProUGUI itText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI maxhpText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI statp;
    [Header("OverStat Popup")]
    public GameObject overstat;
    public TextMeshProUGUI over;
    [Header("LevelUp Group")]
    public GameObject leveluppanel;
    public TextMeshProUGUI levelupText;

    private void Start()
    {
        UpdateUI();
        if (overstat != null) overstat.SetActive(false);
    }

    private void Update()
    {
        UpdateUI();
    }

    public override void upit()
    {
        if (maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        if (it >= 5) { overs("더 이상 올릴 수 없습니다!"); return; }
        base.upit();
    }

    public override void upatk()
    {
        if (maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        if (spd >= 100) { overs("더 이상 올릴 수 없습니다!"); return; }
        base.upatk();
    }

    public override void upspd()
    {
        if (maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        if (spd >= 10) { overs("더 이상 올릴 수 없습니다!"); return; }
        base.upspd();
    }

    public override void upmaxhp()
    {
        if (maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        base.upmaxhp(); hpcal();
    }

    public override void downit()
    {
        if (it <= 0) { overs("0보다 작아질 수 없습니다"); return; }
        base.downit();
    }

    public override void downatk()
    { // 누락된 공격력 감소 추가
        if (atk <= 0) { overs("0보다 작아질 수 없습니다"); return; }
        base.downatk();
    }

    public override void downmaxhp()
    {
        if (maxhp <= 10) { overs("10보다 작아질 수 없습니다"); return; }
        base.downmaxhp(); hpcal();
    }

    public override void downspd()
    {
        if (spd <= -10) { overs("-10보다 작아질 수 없습니다"); return; }
        base.downspd();
    }

    private void UpdateUI()
    {
        if (itText != null) itText.text = it.ToString();
        if (atkText != null) atkText.text = atk.ToString();
        if (maxhpText != null) maxhpText.text = maxhp.ToString();
        if (spdText != null) spdText.text = spd.ToString();
        if (statp != null) statp.text = "Points: " + maxstatpoint.ToString();
    }

    public void overs(string message)
    {
        if (overstat != null)
        {
            CancelInvoke("Hidelevelup"); // 기존 예약된 끄기 취소 (중첩 방지)
            overstat.SetActive(true);
            over.text = message;
            Invoke("Hidelevelup", 1f);
        }
    }
    public void levelup()
    {
        CancelInvoke("HideOverStat");
        overstat.SetActive(true);
        over.text = "LevelUp!";
        Invoke("Hidelevelup", 1f);
    }
    private void HideOverStat()
    {
        overstat.SetActive(false);
    }
}
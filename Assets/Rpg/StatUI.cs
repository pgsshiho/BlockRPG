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

    private void Start()
    {
        // 만약 메인에서 넘어온 인스턴스가 있다면 그 데이터를 내 것으로 가져옴
        if (Stat.instance != null && Stat.instance != this)
        {
            this.it = Stat.instance.it;
            this.atk = Stat.instance.atk;
            this.spd = Stat.instance.spd;
            this.maxstatpoint = Stat.instance.maxstatpoint;
            // ... 데이터 복사 ...

            // 기존 빈 껍데기 Stat은 지우고 내가 대장이 됨
            Destroy(Stat.instance.gameObject);
            Stat.instance = this;
            DontDestroyOnLoad(gameObject);
        }

        UpdateUI();
        if (overstat != null) overstat.SetActive(false);
    }

    // --- UP 계열 ---
    public override void upit()
    {
        if (base.maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        if (base.it >= 5) { overs("더 이상 올릴 수 없습니다!"); return; }
        base.upit(); UpdateUI();
    }

    public override void upatk()
    {
        if (base.maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        base.upatk(); UpdateUI();
    }

    public override void upspd()
    {
        if (base.maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        if (base.spd >= 10) { overs("더 이상 올릴 수 없습니다!"); return; }
        base.upspd(); UpdateUI();
    }

    public override void upmaxhp()
    {
        if (base.maxstatpoint <= 0) { overs("포인트가 부족합니다!"); return; }
        base.upmaxhp(); UpdateUI(); hpcal();
    }

    // --- DOWN 계열 (조건문 수정 및 포인트 복구) ---
    public override void downit()
    {
        if (base.it <= 0) { overs("0보다 작아질 수 없습니다"); return; } // <= 로 수정
        base.downit();
        base.maxstatpoint++; // 포인트 반환
        UpdateUI();
    }

    public override void downmaxhp()
    {
        if (base.maxhp <= 10) { overs("10보다 작아질 수 없습니다"); return; } // <= 로 수정
        base.downmaxhp();
        base.maxstatpoint++; 
        UpdateUI();
        hpcal();
    }

    public override void downspd()
    {
        if (base.spd <= -10) { overs("-10보다 작아질 수 없습니다"); return; } // <= 로 수정
        base.downspd();
        base.maxstatpoint++; // 포인트 반환
        UpdateUI();
    }

    private void UpdateUI()
    {
        itText.text = base.it.ToString();
        atkText.text = base.atk.ToString();
        maxhpText.text = base.maxhp.ToString();
        spdText.text = base.spd.ToString();
        if (statp != null) statp.text = "Points: " + base.maxstatpoint.ToString();
    }

    public void overs(string message)
    {
        if (overstat != null)
        {
            CancelInvoke("HideOverStat"); // 기존 예약된 끄기 취소 (중첩 방지)
            overstat.SetActive(true);
            over.text = message;
            Invoke("HideOverStat", 2f);
        }
    }

    private void HideOverStat()
    {
        overstat.SetActive(false);
    }
}
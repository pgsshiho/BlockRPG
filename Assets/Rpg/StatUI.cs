using TMPro;
using UnityEngine;

public class StatUI : MonoBehaviour
{
    [Header("Status Text (TMP)")]
    public TextMeshProUGUI itText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI maxhpText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI statpText;

    [Header("Popups")]
    public GameObject overstatPopup;
    public TextMeshProUGUI popupText;

    private void Start()
    {
        if (overstatPopup != null) overstatPopup.SetActive(false);
        if (Stat.instance != null)
        {
            Stat.instance.OnStatChanged += UpdateDisplay;
            Stat.instance.OnLevelUp += ShowLevelUp;
        }

        // 초기 화면 세팅
        UpdateDisplay();
    }
    private void OnDestroy()
    {
        // 오브젝트가 사라질 때 구독 해제 (메모리 누수 방지)
        if (Stat.instance != null)
        {
            Stat.instance.OnStatChanged -= UpdateDisplay;
            Stat.instance.OnLevelUp -= ShowLevelUp;
        }
    }

    private void UpdateDisplay()
    {
        if (Stat.instance == null) return;

        if (itText != null) itText.text = Stat.instance.it.ToString();
        if (atkText != null) atkText.text = Stat.instance.atk.ToString();
        if (maxhpText != null) maxhpText.text = Stat.instance.maxhp.ToString();
        if (spdText != null) spdText.text = Stat.instance.spd.ToString();
        if (statpText != null) statpText.text = "Points: " + Stat.instance.maxstatpoint.ToString();
    }

    // --- 버튼 연동 함수 (Inspector에서 Button OnClick에 연결) ---
    public void Btn_UpIt()
    {
        if (Stat.instance.maxstatpoint <= 0) { ShowMessage("포인트 부족!"); return; }
        if (Stat.instance.it >= 5) { ShowMessage("최대치 도달!"); return; }
        Stat.instance.upit();
    }
    public void Btn_UpAtk()
    {
        if (Stat.instance.maxstatpoint <= 0) { ShowMessage("포인트 부족!"); return; }
        if (Stat.instance.atk >= 20) { ShowMessage("최대치 도달!"); return; }
        Stat.instance.upatk();
    }
    public void Btn_UpSpd()
    {
        if (Stat.instance.maxstatpoint <= 0) { ShowMessage("포인트 부족!"); return; }
        if (Stat.instance.spd >= 10) { ShowMessage("최대치 도달!"); return; }
        Stat.instance.upspd();
    }
    public void Btn_UpMaxHp()
    {
        if (Stat.instance.maxstatpoint <= 0) { ShowMessage("포인트 부족!"); return; }
        Stat.instance.upmaxhp();
    }

    public void Btn_DownIt() => Stat.instance.downit();
    public void Btn_DownAtk() => Stat.instance.downatk();
    public void Btn_DownSpd() => Stat.instance.downspd();
    public void Btn_DownMaxHp() => Stat.instance.downmaxhp();

    // --- 팝업 로직 ---
    public void ShowMessage(string message)
    {
        if (overstatPopup == null) return;
        CancelInvoke("HidePopup");
        overstatPopup.SetActive(true);
        popupText.text = message;
        Invoke("HidePopup", 1f);
    }

    public void ShowLevelUp()
    {
        ShowMessage("Level Up!");
    }

    private void HidePopup()
    {
        if (overstatPopup != null) overstatPopup.SetActive(false);
    }
}
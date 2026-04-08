using TMPro;
using UnityEngine;
using DG.Tweening; // DOTween 추가
using UnityEngine.UI;
using UnityEngine.Localization.Settings; // Localization 추가

public class StatUI : MonoBehaviour
{
    [Header("Status Text (TMP)")]
    public TextMeshProUGUI itText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI maxhpText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI statpText;

    [Header("Popups (Animation)")]
    public Image overstatPanel; // Image로 변경 (페이드 애니메이션용)
    public TextMeshProUGUI popupText;
    [SerializeField] private string tableName = "MyTable"; // 테이블 이름

    private Sequence popupSequence;

    private void Start()
    {
        if (overstatPanel != null)
        {
            overstatPanel.gameObject.SetActive(false);
            InitSequence();
        }

        if (Stat.instance != null)
        {
            Stat.instance.OnStatChanged += UpdateDisplay;
            Stat.instance.OnLevelUp += () => ShowMessage("MSG_LEVEL_UP");
        }
        UpdateDisplay();
    }

    private void InitSequence()
    {
        // Notion 스타일의 시퀀스 구조 적용
        popupSequence = DOTween.Sequence()
            .Append(overstatPanel.transform.DOScale(1.2f, 0.15f).From(Vector3.one * 0.5f))
            .Append(overstatPanel.transform.DOScale(1f, 0.15f))
            .AppendInterval(1f)
            .Append(overstatPanel.DOFade(0, 0.5f))
            .Join(popupText.DOFade(0, 0.5f))
            .OnComplete(() => {
                overstatPanel.gameObject.SetActive(false);
                overstatPanel.color = Color.white; // 알파 초기화
                popupText.color = Color.white;
            })
            .SetAutoKill(false).Pause();
    }

    private void UpdateDisplay()
    {
        if (Stat.instance == null) return;
        itText.text = Stat.instance.it.ToString();
        atkText.text = Stat.instance.atk.ToString();
        maxhpText.text = Stat.instance.maxhp.ToString();
        spdText.text = Stat.instance.spd.ToString();

        // 레이블 번역 적용
        string label = GetText("UI_POINTS_LABEL");
        statpText.text = $"{label} {Stat.instance.maxstatpoint}";
    }

    // 버튼 로직 (Key 전달 방식으로 통일)
    public void Btn_UpIt() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.it >= 5) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upit(); }
    public void Btn_UpAtk() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.atk >= 20) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upatk(); }
    public void Btn_UpSpd() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.spd >= 10) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upspd(); }
    public void Btn_UpMaxHp() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } Stat.instance.upmaxhp(); }

    public void Btn_DownIt() => Stat.instance.downit();
    public void Btn_DownAtk() => Stat.instance.downatk();
    public void Btn_DownSpd() => Stat.instance.downspd();
    public void Btn_DownMaxHp() => Stat.instance.downmaxhp();

    // 메시지 출력 (Key를 받아 번역 후 시퀀스 재생)
    public void ShowMessage(string key)
    {
        if (overstatPanel == null) return;
        popupText.text = GetText(key);
        overstatPanel.gameObject.SetActive(true);
        popupSequence.Restart();
    }

    private string GetText(string key)
    {
        try { return LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key); }
        catch { return key; }
    }

    private void OnDestroy()
    {
        if (Stat.instance != null)
        {
            Stat.instance.OnStatChanged -= UpdateDisplay;
        }
    }
}
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
    public TextMeshProUGUI LevelText;
    public int maxhpmin = 10;
    public int atkmin = 0;
    public int spdmin = 0;
    public int itmin = 0;
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
            Stat.instance.OnLevelUp += HandleUpgrade;
        }
        UpdateDisplay();
    }
    public void HandleUpgrade()
    {
        UpdateDisplay();
        ShowMessage("MSG_LEVEL_UP");
    }
    private void InitSequence()
    {
        // 의도한 색상 변수 설정 (알파값 145/255 = 약 0.568)
        Color targetColor = new Color(0, 0, 0, 145f / 255f);

        popupSequence = DOTween.Sequence()
            .Append(overstatPanel.transform.DOScale(1.2f, 0.15f).From(Vector3.one * 0.5f))
            .Append(overstatPanel.transform.DOScale(1f, 0.15f))
            .AppendInterval(1f)
            .Append(overstatPanel.DOFade(0, 0.5f))
            .Join(popupText.DOFade(0, 0.5f))
            .OnComplete(() => {
                overstatPanel.gameObject.SetActive(false);

                // 여기서 강제로 '의도한 색'으로 되돌림
                overstatPanel.color = targetColor;
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
        LevelText.text = "Level: " + Stat.instance.level.ToString();
        // 레이블 번역 적용
        string label = GetText("UI_POINTS_LABEL");
        statpText.text = $"{label} {Stat.instance.maxstatpoint}";
    }

    // 버튼 로직 (Key 전달 방식으로 통일)
    public void Btn_UpIt() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.it >= 5) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upit(); }
    public void Btn_UpAtk() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.atk >= 20) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upatk(); }
    public void Btn_UpSpd() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } if (Stat.instance.spd >= 10) { ShowMessage("MSG_MAX_REACHED"); return; } Stat.instance.upspd(); }
    public void Btn_UpMaxHp() { if (Stat.instance.maxstatpoint <= 0) { ShowMessage("MSG_NO_POINTS"); return; } Stat.instance.upmaxhp(); }

    // StatUI.cs 내부 버튼 로직 수정
    public void Btn_DownIt()
    {
        if (Stat.instance.it <= Stat.instance.baseIt)
        {
            ShowMessage("MSG_MIN_REACHED"); // "직업 기초 스탯 이하로는 줄일 수 없습니다"
            return;
        }
        Stat.instance.downit();
    }

    public void Btn_DownAtk()
    {
        if (Stat.instance.atk <= Stat.instance.baseAtk)
        {
            ShowMessage("MSG_MIN_REACHED");
            return;
        }
        Stat.instance.downatk();
    }

    public void Btn_DownSpd()
    {
        if (Stat.instance.spd <= Stat.instance.baseSpd)
        {
            ShowMessage("MSG_MIN_REACHED");
            return;
        }
        Stat.instance.downspd();
    }

    public void Btn_DownMaxHp()
    {
        if (Stat.instance.maxhp <= Stat.instance.baseMaxHp)
        {
            ShowMessage("MSG_MIN_REACHED");
            return;
        }
        Stat.instance.downmaxhp();
    }

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
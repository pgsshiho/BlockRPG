using UnityEngine;
using UnityEngine.UI;

public class SoundScript : MonoBehaviour
{
    public enum SoundType { BGM, SFX }
    public SoundType type;
    private Slider slider;
    private bool isUpdating = false;

    void Awake() => slider = GetComponent<Slider>();

    void Start()
    {
        // 처음 시작할 때 슬라이더 위치 초기화
        RefreshSliderOnOpen();

        slider.onValueChanged.AddListener(OnSliderValueChanged);

        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged += SyncSlider;
    }

    private void OnSliderValueChanged(float val)
    {
        if (isUpdating) return;

        if (type == SoundType.BGM) Sound.instance.SetLevelBGM(val);
        else Sound.instance.SetLevelSFX(val);
    }

    public void SyncSlider(float bgm, float sfx)
    {
        isUpdating = true;
        slider.value = (type == SoundType.BGM) ? bgm : sfx;
        isUpdating = false;
    }

    // ★ GameManager에서 호출하는 핵심 함수입니다. 이 부분이 없으면 에러가 납니다!
    public void RefreshSliderOnOpen()
    {
        isUpdating = true;
        float savedVal = (type == SoundType.BGM) ?
            PlayerPrefs.GetFloat("BGM_Value", 0.75f) :
            PlayerPrefs.GetFloat("SFX_Value", 0.75f);
        slider.value = savedVal;
        isUpdating = false;
    }

    void OnDestroy()
    {
        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged -= SyncSlider;
    }

    void OnEnable()
    {
        if (slider != null) RefreshSliderOnOpen();
    }
}
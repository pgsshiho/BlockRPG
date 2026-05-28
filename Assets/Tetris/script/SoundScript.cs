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
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }

        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged += SyncSlider;

        RefreshSliderOnOpen();

        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float val)
    {
        if (isUpdating || Sound.instance == null) return;

        if (type == SoundType.BGM) Sound.instance.SetLevelBGM(val);
        else Sound.instance.SetLevelSFX(val);
    }

    public void SyncSlider(float bgm, float sfx)
    {
        if (slider == null) return;

        isUpdating = true;
        slider.value = (type == SoundType.BGM) ? bgm : sfx;
        isUpdating = false;
    }

    public void RefreshSliderOnOpen()
    {
        if (slider == null) return;

        isUpdating = true;

        // 현재 로컬에 저장되어 있는 가장 확실한 유저의 세팅 값을 가져옴
        float currentVal = (type == SoundType.BGM) ?
            PlayerPrefs.GetFloat("BGM_Value", 0.75f) :
            PlayerPrefs.GetFloat("SFX_Value", 0.75f);

        slider.value = currentVal;
        isUpdating = false;
    }

    void OnDestroy()
    {
        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged -= SyncSlider;
    }

    void OnEnable()
    {
        RefreshSliderOnOpen();
    }
}
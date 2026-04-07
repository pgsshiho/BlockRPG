using UnityEngine;
using UnityEngine.UI;

public class SoundScript : MonoBehaviour
{
    public enum SoundType { BGM, SFX }
    public SoundType type;
    private Slider slider;

    void Awake() => slider = GetComponent<Slider>();

    void Start()
    {
        RefreshSliderOnOpen();

        slider.onValueChanged.AddListener(val => {
            if (type == SoundType.BGM) Sound.instance.SetLevelBGM(val);
            else Sound.instance.SetLevelSFX(val);
        });

        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged += SyncSlider;
    }

    public void SyncSlider(float bgm, float sfx)
    {
        // 내 타입에 맞는 값만 받아와서 적용 (상대방 값은 무시)
        slider.value = (type == SoundType.BGM) ? bgm : sfx;
    }

    public void RefreshSliderOnOpen()
    {
        float savedVal = (type == SoundType.BGM) ?
            PlayerPrefs.GetFloat("BGM_Value", 0.75f) :
            PlayerPrefs.GetFloat("SFX_Value", 0.75f);
        slider.value = savedVal;
    }

    void OnDestroy()
    {
        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged -= SyncSlider;
    }
}
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
        // 1. 현재 저장된 값으로 내 슬라이더 초기화
        float savedVal = (type == SoundType.BGM) ?
            PlayerPrefs.GetFloat("BGM_Value", 0.75f) :
            PlayerPrefs.GetFloat("SFX_Value", 0.75f);
        slider.value = savedVal;

        // 2. 슬라이더 조작 시 매니저의 함수 호출
        slider.onValueChanged.AddListener(val => {
            if (type == SoundType.BGM) Sound.instance.SetLevelBGM(val);
            else Sound.instance.SetLevelSFX(val);
        });

        // 3. 매니저의 방송 채널 구독
        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged += SyncSlider;
    }

    void SyncSlider(float bgm, float sfx)
    {
        slider.value = (type == SoundType.BGM) ? bgm : sfx;
    }

    void OnDestroy()
    {
        if (Sound.instance != null)
            Sound.instance.OnVolumeDataChanged -= SyncSlider;
    }
}

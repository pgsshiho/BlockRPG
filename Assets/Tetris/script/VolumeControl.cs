    using UnityEngine;
    using UnityEngine.UI;

    public class VolumeControl : MonoBehaviour
    {
        public Slider bgmSlider;
        public Slider sfxSlider;

        void Start()
        {
            // 1. 기존에 저장된 값을 슬라이더에 로드
            bgmSlider.value = PlayerPrefs.GetFloat("BGM_Value", 0.75f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFX_Value", 0.75f);

            // 2. 슬라이더 이벤트 연결
            bgmSlider.onValueChanged.AddListener(val => Sound.instance.SetLevelBGM(val));
            sfxSlider.onValueChanged.AddListener(val => Sound.instance.SetLevelSFX(val));
        }
    }
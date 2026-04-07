using UnityEngine;
using UnityEngine.Audio;
using System;

public class Sound : MonoBehaviour
{
    public static Sound instance;

    [Header("Audio Sources")]
    public AudioSource swing;
    public AudioSource drop;
    public AudioSource tspinSuccess;
    public AudioSource blockclear;
    public AudioSource slimehit;
    public AudioSource Goblin;
    public AudioSource Golem;
    public AudioSource Ghost;
    public AudioSource boss;
    public AudioSource night_knight;
    public AudioSource ouger;
    public AudioSource Prism_Dragon;
    public AudioSource Shamen;
    public AudioSource Kraken;
    public AudioSource Jester;
    public AudioSource combo;
    public AudioSource siren;

    [Header("Settings UI")]
    public AudioMixer mixer;

    // 볼륨 데이터가 변할 때 모든 슬라이더에게 알리는 이벤트
    public event Action<float, float> OnVolumeDataChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitVolume()
    {
        // 초기 로딩 시 Mixer에 값 적용
        SetLevelBGM(PlayerPrefs.GetFloat("BGM_Value", 0.75f), false);
        SetLevelSFX(PlayerPrefs.GetFloat("SFX_Value", 0.75f), false);
    }

    // save 매개변수를 추가해 초기화 시 중복 저장을 방지합니다.
    public void SetLevelBGM(float value, bool save = true)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("BGM", volume);

        if (save) PlayerPrefs.SetFloat("BGM_Value", value);

        float currentSFX = PlayerPrefs.GetFloat("SFX_Value", 0.75f);
        OnVolumeDataChanged?.Invoke(value, currentSFX);
    }

    public void SetLevelSFX(float value, bool save = true)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("SFX", volume);

        if (save) PlayerPrefs.SetFloat("SFX_Value", value);

        float currentBGM = PlayerPrefs.GetFloat("BGM_Value", 0.75f);
        OnVolumeDataChanged?.Invoke(currentBGM, value);
    }

    public void PlaySE(AudioSource source)
    {
        if (source != null) source.Play();
    }
}
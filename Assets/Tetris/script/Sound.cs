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

    // Action<BGM값, SFX값>
    public Action<float, float> OnVolumeDataChanged;

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
        SetLevelBGM(PlayerPrefs.GetFloat("BGM_Value", 0.75f));
        SetLevelSFX(PlayerPrefs.GetFloat("SFX_Value", 0.75f));
    }

    public void SetLevelBGM(float value)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("BGM", volume);
        PlayerPrefs.SetFloat("BGM_Value", value);

        // 현재 저장된 SFX 값을 가져와서 함께 쏴줍니다 (SFX 슬라이더는 가만히 있게 함)
        float currentSFX = PlayerPrefs.GetFloat("SFX_Value", 0.75f);
        OnVolumeDataChanged?.Invoke(value, currentSFX);
    }

    public void SetLevelSFX(float value)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX_Value", value);

        // 현재 저장된 BGM 값을 가져와서 함께 쏴줍니다 (BGM 슬라이더는 가만히 있게 함)
        float currentBGM = PlayerPrefs.GetFloat("BGM_Value", 0.75f);
        OnVolumeDataChanged?.Invoke(currentBGM, value);
    }

    public void PlaySE(AudioSource source)
    {
        if (source != null) source.Play();
    }
}
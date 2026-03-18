using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System; // 씬 전환 감지를 위해 추가

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
    public GameObject currentSceneUI;
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
    public void PlaySE(AudioSource source)
    {
        if (source != null) source.Play();
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
        OnVolumeDataChanged?.Invoke(value, PlayerPrefs.GetFloat("BGM_Value", 0.75f));
    }

    public void SetLevelSFX(float value)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX_Value", value);
        OnVolumeDataChanged?.Invoke(value, PlayerPrefs.GetFloat("SFX_Value", 0.75f));
    }
}
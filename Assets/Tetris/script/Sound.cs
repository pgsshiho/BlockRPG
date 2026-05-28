using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections;

public class Sound : MonoBehaviour
{
    public static Sound instance;

    [Header("Audio Sources")]
    public AudioSource swing;
    public AudioSource potion;
    public AudioSource drop;
    public AudioSource blockclear;
    [Header("Enemy Attack")]
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
    public AudioSource siren;
    [Header("Enemy Dead")]
    public AudioSource slimehit_dead;
    public AudioSource Goblin_dead;
    public AudioSource Golem_dead;
    public AudioSource Ghost_dead;
    public AudioSource boss_dead;
    public AudioSource night_knight_dead;
    public AudioSource ouger_dead;
    public AudioSource Prism_Dragon_dead;
    public AudioSource Shamen_dead;
    public AudioSource Kraken_dead;
    public AudioSource Jester_dead;
    public AudioSource siren_dead;

    [Header("Settings UI")]
    public AudioMixer mixer;

    private float bgmVolume = 0.75f;
    private float sfxVolume = 0.75f;

    public event Action<float, float> OnVolumeDataChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // ★ 핵심: Awake에서 즉시 세팅하지 않고, 코루틴을 시작합니다.
            StartCoroutine(InitVolumeDelayed());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ★ 유니티 오디오 믹서 로딩 타이밍을 맞추기 위한 지연 초기화 함수
    private IEnumerator InitVolumeDelayed()
    {
        // 1. 저장된 데이터를 로드합니다.
        bgmVolume = PlayerPrefs.GetFloat("BGM_Value", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFX_Value", 0.75f);

        // 2. 유니티 오디오 엔진과 믹서가 완전히 준비될 때까지 '한 프레임' 대기합니다.
        yield return null;

        // 3. 이제 안정적인 타이밍에 오디오 믹서에 값을 강제로 꽂아 넣습니다.
        float bgmdB = Mathf.Log10(Mathf.Max(bgmVolume, 0.0001f)) * 20;
        mixer.SetFloat("BGM", bgmdB);

        float sfxdB = Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20;
        mixer.SetFloat("SFX", sfxdB);

        // 4. 슬라이더들에게도 현재 로드된 최종 값을 전달해 동기화합니다.
        OnVolumeDataChanged?.Invoke(bgmVolume, sfxVolume);
    }

    public void SetLevelBGM(float value, bool save = true)
    {
        bgmVolume = Mathf.Clamp01(value);

        float volume = Mathf.Log10(Mathf.Max(bgmVolume, 0.0001f)) * 20;
        mixer.SetFloat("BGM", volume);

        if (save) PlayerPrefs.SetFloat("BGM_Value", bgmVolume);

        OnVolumeDataChanged?.Invoke(bgmVolume, sfxVolume);
    }

    public void SetLevelSFX(float value, bool save = true)
    {
        sfxVolume = Mathf.Clamp01(value);

        float volume = Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20;
        mixer.SetFloat("SFX", volume);

        if (save) PlayerPrefs.SetFloat("SFX_Value", sfxVolume);

        OnVolumeDataChanged?.Invoke(bgmVolume, sfxVolume);
    }

    public void PlaySE(AudioSource source)
    {
        if (source != null) source.Play();
    }
    public void SaveCurrentVolume()
    {
        // 1. 현재 실시간 변수에 담긴 값을 PlayerPrefs에 꽂아 넣습니다.
        PlayerPrefs.SetFloat("BGM_Value", bgmVolume);
        PlayerPrefs.SetFloat("SFX_Value", sfxVolume);

        // 2. 디스크에 물리적으로 즉시 쓰기 작업을 수행합니다. (데이터 유실 방지)
        PlayerPrefs.Save();

        Debug.Log($"[SoundManager] 볼륨 저장 완료 - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }
}
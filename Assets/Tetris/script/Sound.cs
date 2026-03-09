using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환 감지를 위해 추가

public class Sound : MonoBehaviour
{
    public static Sound instance;

    [Header("Audio Sources")]
    public AudioSource swing;
    public AudioSource drop;
    public AudioSource tspinSuccess;

    [Header("Settings UI")]
    public AudioMixer mixer;
    public GameObject currentSceneUI;
    public Slider bgmSlider;
    public Slider sfxSlider;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬이 바뀔 때마다 실행될 함수 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 씬이 넘어갈 때마다 자동으로 호출됨
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 새 씬에서 "SoundUI"라는 이름의 패널을 찾음
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            Transform t = canvas.transform.Find("SoundUI");
            if (t != null)
            {
                currentSceneUI = t.gameObject;

                // 2. 그 패널 안에 있는 슬라이더들을 새로 연결
                bgmSlider = currentSceneUI.transform.Find("BGM")?.GetComponent<Slider>();
                sfxSlider = currentSceneUI.transform.Find("SFX")?.GetComponent<Slider>();

                // 3. 슬라이더 이벤트 재연결 및 값 동기화
                RebindSliders();
            }
        }
    }

    private void RebindSliders()
    {
        float bgm = PlayerPrefs.GetFloat("BGM_Value", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFX_Value", 0.75f);

        if (bgmSlider != null)
        {
            bgmSlider.value = bgm;
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(SetLevelBGM);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfx;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetLevelSFX);
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
    }

    public void SetLevelSFX(float value)
    {
        float volume = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mixer.SetFloat("SFX", volume);
        PlayerPrefs.SetFloat("SFX_Value", value);
    }
}
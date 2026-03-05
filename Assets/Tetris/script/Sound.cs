using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
{
    public static Sound instance;
    public AudioSource swing;
    public AudioSource drop;
    public AudioMixer mixer;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    public void SetLevelBGM(float value)
    {
        mixer.SetFloat("BGM", Mathf.Log10(value) * 20);
    }
    public void SetLevelSFX(float value)
    {
        mixer.SetFloat("SFX", Mathf.Log10(value) * 20);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

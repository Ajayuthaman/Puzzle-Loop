using UnityEngine;

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip clickClip;
    public AudioClip winClip;
    public AudioClip bgMusic;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        // Singleton pattern (keeps first instance)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // apply initial volumes
        if (musicSource != null) musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void OnValidate()
    {
        // keep inspector changes applied in editor
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    // Generic SFX play (one shot, allows overlap)
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * volumeScale));
    }

    // Convenience helpers
    public void PlayClick() => PlaySFX(clickClip);
    public void PlayWin() => PlaySFX(winClip);
    public void PlayMusic() => PlaySFX(bgMusic);

    // Play/Stop music
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource == null) return;
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    // Volume controls
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void MuteAll(bool mute)
    {
        if (musicSource != null) musicSource.mute = mute;
        if (sfxSource != null) sfxSource.mute = mute;
    }
}

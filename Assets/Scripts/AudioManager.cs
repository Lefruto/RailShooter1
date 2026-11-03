using UnityEngine;

/// <summary>
/// Lightweight global audio manager providing procedural sound effects
/// with zero external asset dependencies. Works in Editor and standalone builds.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip laserClip;
    private AudioClip hitClip;
    private AudioClip explosionClip;
    private AudioClip powerupClip;
    private AudioClip gameOverClip;
    private AudioClip winClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        GenerateClips();
    }

    private void GenerateClips()
    {
        laserClip = CreateLaserClip();
        hitClip = CreateHitClip();
        explosionClip = CreateExplosionClip();
        powerupClip = CreatePowerupClip();
        gameOverClip = CreateGameOverClip();
        winClip = CreateWinClip();
    }

    public void PlayLaser(float volume = 0.5f)
    {
        if (laserClip != null && sfxSource != null)
            sfxSource.PlayOneShot(laserClip, volume);
    }

    public void PlayHit(float volume = 0.6f)
    {
        if (hitClip != null && sfxSource != null)
            sfxSource.PlayOneShot(hitClip, volume);
    }

    public void PlayExplosion(float volume = 0.8f)
    {
        if (explosionClip != null && sfxSource != null)
            sfxSource.PlayOneShot(explosionClip, volume);
    }

    public void PlayPowerup(float volume = 0.7f)
    {
        if (powerupClip != null && sfxSource != null)
            sfxSource.PlayOneShot(powerupClip, volume);
    }

    public void PlayGameOver(float volume = 0.8f)
    {
        if (gameOverClip != null && sfxSource != null)
            sfxSource.PlayOneShot(gameOverClip, volume);
    }

    public void PlayWin(float volume = 0.8f)
    {
        if (winClip != null && sfxSource != null)
            sfxSource.PlayOneShot(winClip, volume);
    }

    // Procedural Synth: Laser frequency sweep from 900Hz to 200Hz
    private AudioClip CreateLaserClip()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(950f, 220f, progress);
            float envelope = 1f - progress;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("SFX_Laser", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Procedural Synth: Sharp metallic/spark hit sound
    private AudioClip CreateHitClip()
    {
        int sampleRate = 44100;
        float duration = 0.08f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float noise = (Random.value * 2f - 1f) * 0.5f;
            float tone = Mathf.Sin(2f * Mathf.PI * 600f * t) * 0.5f;
            float envelope = Mathf.Exp(-progress * 15f);
            samples[i] = (tone + noise) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("SFX_Hit", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Procedural Synth: Heavy rumble and explosion burst
    private AudioClip CreateExplosionClip()
    {
        int sampleRate = 44100;
        float duration = 0.65f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float noise = (Random.value * 2f - 1f);
            float lowRumble = Mathf.Sin(2f * Mathf.PI * 65f * t) * 0.6f;
            float envelope = Mathf.Pow(1f - progress, 2f);
            samples[i] = (noise * 0.7f + lowRumble) * envelope * 0.6f;
        }

        AudioClip clip = AudioClip.Create("SFX_Explosion", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Procedural Synth: Upward rising chime powerup
    private AudioClip CreatePowerupClip()
    {
        int sampleRate = 44100;
        float duration = 0.28f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(350f, 950f, progress);
            float envelope = 1f - Mathf.Abs(progress * 2f - 1f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("SFX_Powerup", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Procedural Synth: Descending defeat sequence
    private AudioClip CreateGameOverClip()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = Mathf.Lerp(440f, 110f, progress);
            float envelope = 1f - progress;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("SFX_GameOver", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // Procedural Synth: Bright triumphant fanfare
    private AudioClip CreateWinClip()
    {
        int sampleRate = 44100;
        float duration = 1.0f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float progress = t / duration;
            float freq = (progress < 0.33f) ? 523.25f : (progress < 0.66f) ? 659.25f : 783.99f;
            float envelope = 1f - (progress % 0.33f) / 0.33f;
            samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.4f;
        }

        AudioClip clip = AudioClip.Create("SFX_Win", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

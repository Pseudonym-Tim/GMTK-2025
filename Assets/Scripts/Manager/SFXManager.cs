using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles everything related to sound effects...
/// </summary>
public class SFXManager : Singleton<SFXManager>
{
    [SerializeField] private List<SFXInfo> soundDatabase;
    [SerializeField] private AudioMixerGroup masterMixerGroup;

    private readonly List<AudioSource> active3DSources = new List<AudioSource>();
    private bool hasBeenSetup = false;

    private Coroutine sfxFadeCoroutine;

    private void Awake()
    {
        if(!hasBeenSetup)
        {
            Setup();
        }
    }

    public void Setup()
    {
        if(hasBeenSetup) { return; }
        hasBeenSetup = true;

        // Setup sound information...
        foreach(SFXInfo soundInfo in soundDatabase)
        {
            SetupSoundInfo(soundInfo);
        }

        float savedMasterVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        UpdateMasterVolume(savedMasterVolume);
    }

    public float GetCurrentVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void Play2DSound(string soundName)
    {
        SFXInfo soundToPlayInfo = GetSoundInfo(soundName);
        if(soundToPlayInfo == null) { Debug.LogWarning($"Couldn't play sound: [{soundName}]! It doesn't exist!"); return; }
        SetRandomPitch(soundToPlayInfo.soundAudioSource, soundToPlayInfo);

        if(soundToPlayInfo.loopAudio && soundToPlayInfo.soundAudioSource.isPlaying)
        {
            return;
        }

        soundToPlayInfo.soundAudioSource.Play();
    }

    public AudioSource Play3DSound(string soundName, Transform parent = null, Vector2 position = default)
    {
        SFXInfo soundToPlayInfo = GetSoundInfo(soundName);

        if(soundToPlayInfo == null)
        {
            Debug.LogWarning($"SFXManager: Sound \"{soundName}\" not found!");
            return null;
        }

        GameObject newAudioObject = new GameObject($"[{soundToPlayInfo.soundName}] Audio Source 3D");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();

        if(parent != null)
        {
            newAudioObject.transform.SetParent(parent, false);
        }
        else
        {
            newAudioSource.transform.position = position;
        }

        newAudioSource.clip = soundToPlayInfo.audioClip;
        newAudioSource.outputAudioMixerGroup = masterMixerGroup;
        newAudioSource.volume = soundToPlayInfo.volume;

        if(soundToPlayInfo.useRandomPitch)
        {
            float rand = Random.Range(soundToPlayInfo.pitchRandomMin, soundToPlayInfo.pitchRandomMax);
            newAudioSource.pitch = rand;
        }
        else
        {
            newAudioSource.pitch = soundToPlayInfo.pitch;
        }

        newAudioSource.spatialBlend = 1f;

        if(soundToPlayInfo.spatial3DSoundInfo != null)
        {
            newAudioSource.rolloffMode = soundToPlayInfo.spatial3DSoundInfo.rolloffMode;
            newAudioSource.minDistance = soundToPlayInfo.spatial3DSoundInfo.minAudibleDistance;
            newAudioSource.maxDistance = soundToPlayInfo.spatial3DSoundInfo.maxAudibleDistance;
        }

        newAudioSource.loop = soundToPlayInfo.loopAudio;
        newAudioSource.Play();

        active3DSources.Add(newAudioSource);

        if(!soundToPlayInfo.loopAudio)
        {
            float actualDuration = soundToPlayInfo.audioClip.length / newAudioSource.pitch;
            Destroy(newAudioObject, actualDuration + 0.05f);
        }

        return newAudioSource;
    }

    public void Stop3DSound(AudioSource audioSource, bool destroyAudioSource = false)
    {
        if(audioSource == null) { return; }

        if(active3DSources.Contains(audioSource))
        {
            active3DSources.Remove(audioSource);
        }

        audioSource.Stop();

        if(destroyAudioSource)
        {
            Destroy(audioSource.gameObject);
        }
    }

    private void SetRandomPitch(AudioSource audioSource, SFXInfo soundInfo)
    {
        float randPitch = Random.Range(soundInfo.pitchRandomMin, soundInfo.pitchRandomMax);
        audioSource.pitch = soundInfo.useRandomPitch ? randPitch : 1;
    }

    public void Stop2DSound(string soundName)
    {
        SFXInfo soundToStop = GetSoundInfo(soundName);
        if(soundToStop == null) { Debug.LogWarning($"Couldn't stop sound: [{soundName}]"); return; }
        soundToStop.soundAudioSource.Stop();
    }

    public void StopAllSounds()
    {
        // Stop all 2D audio sources...
        foreach(SFXInfo soundInfo in soundDatabase)
        {
            if(soundInfo.soundAudioSource != null)
            {
                soundInfo.soundAudioSource.Stop();
            }
        }

        // Stop/Destroy all 3D audio sources...
        for(int i = active3DSources.Count - 1; i >= 0; i--)
        {
            AudioSource audioSource = active3DSources[i];

            if(audioSource != null)
            {
                audioSource.Stop();
                Destroy(audioSource.gameObject);
            }

            active3DSources.RemoveAt(i);
        }
    }

    public bool IsPlayingSound(string soundName)
    {
        SFXInfo soundInfo = GetSoundInfo(soundName);
        if(soundInfo.soundAudioSource.isPlaying) { return true; }
        return false;
    }


    public Coroutine FadeAudio(bool fadeIn, float duration)
    {
        if(sfxFadeCoroutine != null)
        {
            StopCoroutine(sfxFadeCoroutine);
        }

        float baseVol = GetCurrentVolume();
        float startVol = fadeIn ? 0f : baseVol;
        float endVol = fadeIn ? baseVol : 0f;

        sfxFadeCoroutine = StartCoroutine(FadeMasterVolumeCoroutine(startVol, endVol, duration));
        return sfxFadeCoroutine;
    }

    private IEnumerator FadeMasterVolumeCoroutine(float startVol, float endVol, float duration)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;
            UpdateMasterVolume(Mathf.Lerp(startVol, endVol, t), false);
            elapsed += Time.deltaTime;
            yield return null;
        }

        UpdateMasterVolume(endVol, false);
    }

    public void UpdateMasterVolume(float linearVolume, bool saveValue = true)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        float dB = (linearVolume <= 0.0001f) ? -80f : Mathf.Log10(linearVolume) * 20f;

        masterMixerGroup.audioMixer.SetFloat("SFXVolume", dB);

        if(saveValue)
        {
            PlayerPrefs.SetFloat("SFXVolume", linearVolume);
        }
    }

    public void SetupSoundInfo(SFXInfo soundInfo)
    {
        // We add the audio components to separate objects so we don't lag the editor...
        GameObject newAudioObject = new GameObject($"[{soundInfo.soundName}] Audio Source");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
        newAudioSource.transform.parent = transform;

        // Setup audio source settings...
        soundInfo.soundAudioSource = newAudioSource;
        soundInfo.soundAudioSource.clip = soundInfo.audioClip;
        soundInfo.soundAudioSource.outputAudioMixerGroup = masterMixerGroup;
        soundInfo.soundAudioSource.volume = soundInfo.volume;
        soundInfo.soundAudioSource.pitch = soundInfo.pitch;
        soundInfo.soundAudioSource.loop = soundInfo.loopAudio;
    }

    public SFXInfo GetSoundInfo(string soundName)
    {
        return soundDatabase.FirstOrDefault(soundInfo => soundInfo.soundName == soundName);
    }
}

[System.Serializable]
public class SFXInfo
{
    public string soundName = "NewSound";
    public AudioClip audioClip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Range(0.1f, 3f)] public float pitchRandomMin = 1f;
    [Range(0.1f, 3f)] public float pitchRandomMax = 1f;
    public bool useRandomPitch = false;
    public bool loopAudio = false;
    public Spatial3DSoundInfo spatial3DSoundInfo;
    public AudioSource soundAudioSource;

    [System.Serializable]
    public class Spatial3DSoundInfo
    {
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        public float minAudibleDistance = 1;
        public float maxAudibleDistance = 5;
    }
}

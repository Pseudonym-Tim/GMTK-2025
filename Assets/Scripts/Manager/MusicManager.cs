using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles everything related to in-game music...
/// </summary>
public class MusicManager : Singleton<MusicManager>
{
    private const float DEFAULT_CROSSFADE_TIME = 1f;

    [SerializeField] private List<MusicTrackInfo> musicDatabase;
    [SerializeField] private AudioMixerGroup masterMixerGroup;

    private AudioSource currentMusicSource;
    private MusicTrackInfo previousTrack;
    private bool hasBeenSetup = false;

    private Coroutine crossFadeCoroutine;
    private Coroutine musicFadeCoroutine;

    public void Awake()
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

        foreach(MusicTrackInfo trackInfo in musicDatabase)
        {
            SetupMusicTrack(trackInfo);
        }

        float savedMasterVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        UpdateMasterVolume(savedMasterVolume);
    }

    public void Pause()
    {
        currentMusicSource?.Pause();
    }

    public void Resume()
    {
        currentMusicSource?.Play();
    }

    public void PlayTrack(string trackID, bool crossFade = false, float crossFadeDuration = DEFAULT_CROSSFADE_TIME)
    {
        MusicTrackInfo trackToPlay = GetMusicTrackInfo(trackID);

        if(trackToPlay == null)
        {
            Debug.LogWarning($"Couldn't play music: [{trackID}] not found in database.");
            return;
        }

        // If we have something playing and crossFade requested, do it...
        if(currentMusicSource != null && crossFade)
        {
            CrossFadeToTrack(trackToPlay, crossFadeDuration);
        }
        else
        {
            StopMusic();
            previousTrack = trackToPlay;
            currentMusicSource = trackToPlay.musicAudioSource;
            currentMusicSource.volume = trackToPlay.volume;
            currentMusicSource.PlayDelayed(0.05f);
        }
    }

    public void CrossFadeToTrack(MusicTrackInfo newTrack, float duration)
    {
        AudioSource fromSource = currentMusicSource;
        AudioSource toSource = newTrack.musicAudioSource;

        if(toSource == fromSource) { return; }

        if(crossFadeCoroutine != null)
        {
            StopCoroutine(crossFadeCoroutine);
        }

        float fromStartVol = fromSource != null ? fromSource.volume : 0f;
        float toTargetVol = newTrack.volume;

        crossFadeCoroutine = StartCoroutine(CrossFadeCoroutine(fromSource, toSource, fromStartVol, toTargetVol, duration));

        previousTrack = newTrack;
        currentMusicSource = toSource;
    }

    private IEnumerator CrossFadeCoroutine(AudioSource fromSource, AudioSource toSource, float fromStartVol, float toTargetVol, float duration)
    {
        toSource.volume = 0f;
        toSource.PlayDelayed(0.05f);

        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;

            if(fromSource != null)
            {
                fromSource.volume = Mathf.Lerp(fromStartVol, 0f, t);
            }

            toSource.volume = Mathf.Lerp(0f, toTargetVol, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // At the end, fully stop the old and set the new to its intended volume...
        if(fromSource != null)
        {
            fromSource.Stop();
            fromSource.volume = fromStartVol;
        }

        toSource.volume = toTargetVol;
    }

    public Coroutine FadeAudio(bool fadeIn, float duration)
    {
        if(musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        float baseVol = GetCurrentVolume();
        float startVol = fadeIn ? 0f : baseVol;
        float endVol = fadeIn ? baseVol : 0f;

        musicFadeCoroutine = StartCoroutine(FadeMasterVolumeCoroutine(startVol, endVol, duration));
        return musicFadeCoroutine;
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

        masterMixerGroup.audioMixer.SetFloat("MusicVolume", dB);

        if(saveValue)
        {
            PlayerPrefs.SetFloat("MusicVolume", linearVolume);
        }
    }

    public void PlayRandom()
    {
        if(musicDatabase.Count == 0)
        {
            Debug.LogWarning("No music tracks available to play.");
            return;
        }

        MusicTrackInfo randomTrack;

        if(musicDatabase.Count == 1)
        {
            randomTrack = musicDatabase[0];
        }
        else
        {
            do
            {
                randomTrack = musicDatabase[Random.Range(0, musicDatabase.Count)];
            } 
            while(randomTrack == previousTrack);
        }

        StopMusic();
        previousTrack = randomTrack;
        currentMusicSource = randomTrack.musicAudioSource;
        currentMusicSource.PlayDelayed(0.05f);
    }

    public void StopMusic()
    {
        if(currentMusicSource != null)
        {
            currentMusicSource.Stop();
            currentMusicSource.time = 0f;
        }
    }

    private void SetupMusicTrack(MusicTrackInfo musicTrack)
    {
        if(musicTrack.audioClip == null)
        {
            Debug.LogWarning($"MusicTrackInfo '{musicTrack.trackName}' has no AudioClip assigned.");
            return;
        }

        GameObject newAudioObject = new GameObject($"[{musicTrack.trackName}] Audio Source");
        AudioSource newAudioSource = newAudioObject.AddComponent<AudioSource>();
        newAudioSource.transform.parent = transform;

        musicTrack.musicAudioSource = newAudioSource;
        musicTrack.musicAudioSource.clip = musicTrack.audioClip;
        musicTrack.musicAudioSource.outputAudioMixerGroup = masterMixerGroup;
        musicTrack.musicAudioSource.volume = musicTrack.volume;
        musicTrack.musicAudioSource.loop = true;
        musicTrack.musicAudioSource.playOnAwake = false;
    }

    private MusicTrackInfo GetMusicTrackInfo(string trackID) => musicDatabase.Find(trackInfo => trackInfo.trackID == trackID);

    public bool IsPlayingMusic() => currentMusicSource != null && currentMusicSource.isPlaying;

    public float GetCurrentVolume() => PlayerPrefs.GetFloat("MusicVolume", 1f);
}

[System.Serializable]
public class MusicTrackInfo
{
    public string trackName = "NewMusicTrack";
    public string trackID = "newMusicTrack";
    public AudioClip audioClip;
    public AudioSource musicAudioSource;
    [Range(0f, 1f)] public float volume = 1f;
}

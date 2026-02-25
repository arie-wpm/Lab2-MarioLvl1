using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour {

    public static AudioManager instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public Sound[] sounds;
    public AudioClip introClip;
    public AudioClip loopClip;


    private Dictionary<string, Sound> _soundLookup;
    private AudioSource _sfxSource;
    private AudioSource _introSource;
    private AudioSource _bgmSource;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _introSource = gameObject.AddComponent<AudioSource>();

        _soundLookup = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds) {
            _soundLookup.Add(sound.name, sound);
        }
    }

    public void Play(string name) {
        if (!_soundLookup.ContainsKey(name)) {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        
        var sound = _soundLookup[name];
        _sfxSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlayBGM() {
        double startTime = AudioSettings.dspTime + 0.1;
        _introSource.clip = introClip;
        _introSource.PlayScheduled(startTime);
        _bgmSource.clip = loopClip;
        _bgmSource.loop = true;
        _bgmSource.PlayScheduled(startTime + introClip.length);
    }

    public void PauseBGM() => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();
    public void StopBGM() => _bgmSource.Stop();
    public void SpeedUpBGM() => _bgmSource.pitch = 1.25f;
}

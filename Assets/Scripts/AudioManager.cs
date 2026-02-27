using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance { get; private set; }

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

    private Coroutine _bgmCoroutine;

    private bool _isInPlayMode = false;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _introSource = gameObject.AddComponent<AudioSource>();

        _soundLookup = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds) {
            _soundLookup.Add(sound.name, sound);
        }

        // prelaod audio
        introClip.LoadAudioData();
        loopClip.LoadAudioData();
    }

    void Start() {
        // Prewarm BGMs
        _introSource.clip = introClip;
        _introSource.Play();
        _introSource.Stop();
        _introSource.volume = 0f;
        _bgmSource.clip = loopClip;
        _bgmSource.loop = true;
        _bgmSource.Play();
        _bgmSource.Stop();
        _bgmSource.volume = 0f;
    }

    void Update()
    {
        StateManager.GameState currentState = StateManager.CurrentGameState();
        
        switch (currentState) {
            case StateManager.GameState.NULL: _isInPlayMode = false; break;
            case StateManager.GameState.Play: _isInPlayMode = true; break;
            case StateManager.GameState.StartScreen: _isInPlayMode = false; break;
            case StateManager.GameState.PauseScreen: _isInPlayMode = false; break;
            case StateManager.GameState.Dead: _isInPlayMode = false; break;
            case StateManager.GameState.Won: _isInPlayMode = true; break;
        }
    }

    public void Play(string name) {
        if (!_isInPlayMode) return;
        if (!_soundLookup.ContainsKey(name)) {
            Debug.LogWarning("Sound " + name + " not found!");
            return;
        }
        
        var sound = _soundLookup[name];
        _sfxSource.PlayOneShot(sound.clip, sound.volume);
    }

    public void PlayBGM() {
        if (_bgmCoroutine != null) return;
        _bgmCoroutine = StartCoroutine(StartPlayingBGM());
    }

    IEnumerator StartPlayingBGM() {
        _introSource.volume = 1f;
        _introSource.Play();
        yield return new WaitForSeconds(_introSource.clip.length);
        _introSource.Stop();
        _introSource.volume = 0f;
        _bgmSource.volume = 1f;
        _bgmSource.Play();
    }

    public void PauseBGM() {
        _bgmSource.Pause();
        _introSource.Pause();
    }

    public void ResumeBGM() {
        _bgmSource.UnPause();
        _introSource.UnPause();
    }

    public void StopBGM() {
        _bgmSource.Stop();
        _introSource.Stop();
        _bgmCoroutine = null;

    }
    public void SpeedUpBGM() {
        _bgmSource.pitch = 1.25f;
        _introSource.pitch = 1.25f;
    }
    public bool IsBGMPlaying() => _isInPlayMode;
}

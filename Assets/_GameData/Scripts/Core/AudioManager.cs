using System;
using _GameData.Scripts.ScriptableObjects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _GameData.Scripts.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [field: SerializeField] public AudioLibraryData AudioLibraryData { get; private set; }
        [SerializeField] private AudioSource backgroundAudioSource;
        [SerializeField] private AudioSource oneShotAudioSource;

        public float CurrentVolume { get; private set; }

        public Action OnAudioManagerLoaded;

        private void Awake()
        {
            InitSingleton();
        }

        private void Start()
        {
            LoadVolumeData();
            InitBackgroundMusic();
        }

        private void InitSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        private void LoadVolumeData()
        {
            var volumeData = SaveManager.GetValue(SaveKeys.MusicVolume, 1f);
            UpdateVolume(volumeData);
            OnAudioManagerLoaded?.Invoke();
        }

        private void InitBackgroundMusic()
        {
            backgroundAudioSource.Play();
        }

        public void UpdateVolume(float volumeData)
        {
            CurrentVolume = volumeData;
            backgroundAudioSource.volume = CurrentVolume;
            oneShotAudioSource.volume = CurrentVolume;
        }

        public void PlaySfx(AudioData audioData)
        {
            if (audioData.useRandomPitch) oneShotAudioSource.pitch = Random.Range(audioData.minPitch, audioData.maxPitch);
            oneShotAudioSource.PlayOneShot(audioData.audioClip, audioData.volume);
        }

        private void OnDestroy()
        {
            SaveManager.SaveValue(SaveKeys.MusicVolume, CurrentVolume);
        }
    }
}
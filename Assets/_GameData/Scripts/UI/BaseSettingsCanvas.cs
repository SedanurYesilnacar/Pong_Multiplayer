using _GameData.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace _GameData.Scripts.UI
{
    public abstract class BaseSettingsCanvas : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button backButton;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Start()
        {
            UpdateVolumeSlider();
        }

        protected virtual void SubscribeEvents()
        {
            backButton.onClick.AddListener(BackClickHandler);
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChangedHandler);
            AudioManager.Instance.OnAudioManagerLoaded += OnAudioManagerLoadedHandler;
        }

        protected virtual void UnsubscribeEvents()
        {
            backButton.onClick.RemoveAllListeners();
            volumeSlider.onValueChanged.RemoveAllListeners();
            AudioManager.Instance.OnAudioManagerLoaded -= OnAudioManagerLoadedHandler;
        }

        private void UpdateVolumeSlider()
        {
            volumeSlider.value = AudioManager.Instance.CurrentVolume;
        }

        protected abstract void BackClickHandler();

        private void OnVolumeSliderValueChangedHandler(float value)
        {
            AudioManager.Instance.UpdateVolume(value);
        }

        private void OnAudioManagerLoadedHandler()
        {
            UpdateVolumeSlider();
        }
    }
}
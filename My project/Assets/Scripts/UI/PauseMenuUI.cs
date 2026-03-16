using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ColorDrive
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle vibrationToggle;

        void Start()
        {
            gameObject.SetActive(false);
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnGameStateChanged;

            // Load saved preferences
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            if (vibrationToggle != null)
                vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Paused);
        }

        public void OnResumePressed() => GameManager.Instance?.ResumeGame();

        public void OnRestartPressed()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.RestartLevel();
        }

        public void OnHomePressed()
        {
            gameObject.SetActive(false);
            GameManager.Instance?.GoToMainMenu();
        }

        public void OnMusicVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            AudioListener.volume = value;
        }

        public void OnSFXVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        public void OnVibrationToggleChanged(bool enabled)
        {
            PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
        }
    }
}

using UnityEngine;
using UnityEngine.UI; // Обов'язково для роботи з Toggle

public class SettingsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundToggle;

    private void OnEnable()
    {
        // Коли вікно відкривається, ми запитуємо актуальні дані у менеджера
        if (AudioManager.Instance != null)
        {
            musicToggle.isOn = AudioManager.Instance.isMusicOn;
            soundToggle.isOn = AudioManager.Instance.isSoundOn;
        }
    }

    // Ці методи ми підключимо до подій On Value Changed у самих Toggle
    public void OnMusicToggleChanged(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(value);
        }
    }

    public void OnSoundToggleChanged(bool value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSounds(value);
        }
    }
}
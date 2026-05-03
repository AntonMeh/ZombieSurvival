using UnityEngine;
using UnityEngine.UI; 

public class SettingsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundToggle;

    private void OnEnable()
    {

        if (AudioManager.Instance != null)
        {
            musicToggle.isOn = AudioManager.Instance.isMusicOn;
            soundToggle.isOn = AudioManager.Instance.isSoundOn;
        }
    }

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
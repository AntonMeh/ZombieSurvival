using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Tooltip("Музика, яка має грати на цьому рівні/сцені")]
    public AudioClip levelMusic;

    void Start()
    {
        if (AudioManager.Instance != null && levelMusic != null)
        {
            AudioManager.Instance.PlayMusic(levelMusic);

            AudioManager.Instance.ApplySettings();
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("MusicPlayer: AudioManager відсутній! Переконайся, що він є на сцені MainMenu.");
        }
    }
}

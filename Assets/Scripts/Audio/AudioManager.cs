using UnityEngine;
using UnityEngine.Audio; // Якщо використовуєш AudioMixer

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Settings State")]
    public bool isMusicOn = true;
    public bool isSoundOn = true;

    void Awake()
    {
        // Реалізація Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Об'єкт не видаляється при зміні сцени
            LoadSettings(); // Завантажуємо налаштування при старті
        }
        else
        {
            Destroy(gameObject); // Видаляємо дублікати, якщо повернулися в меню
        }
    }

    // Метод для перемикання музики
    public void ToggleMusic(bool value)
    {
        isMusicOn = value;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0); // Зберігаємо (1 - true, 0 - false)
        ApplySettings();
    }

    // Метод для перемикання звуків
    public void ToggleSounds(bool value)
    {
        isSoundOn = value;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        ApplySettings();
    }

    private void LoadSettings()
    {
        // Завантажуємо, за замовчуванням ставимо 1 (true)
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        ApplySettings();
    }

    public void ApplySettings()
    {
        // Тут логіка вимкнення звуку. 
        // Наприклад, через AudioListener (вимикає все) або конкретні AudioSource
        AudioListener.pause = !isSoundOn; 
        
        // Якщо є фонова музика на цьому об'єкті:
        // GetComponent<AudioSource>().mute = !isMusicOn;
        
        Debug.Log($"Settings applied: Music={isMusicOn}, Sound={isSoundOn}");
    }
}
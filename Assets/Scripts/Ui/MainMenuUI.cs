using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Скрипт для сцени MainMenu.
/// Кнопки: Play, Quit.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Назва ігрової сцени для завантаження")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        // Переконуємось що timeScale = 1 (міг залишитись 0 після Game Over)
        Time.timeScale = 1f;
        Cursor.visible = true;
    }

    /// <summary>
    /// Кнопка "Грати" — завантажує ігрову сцену.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Кнопка "Вийти" — закриває гру.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit Game");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}

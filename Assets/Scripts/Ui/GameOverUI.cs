using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Для перезавантаження сцен

public class GameOverUI : MonoBehaviour
{
    public GameObject endPanel;
    public TMP_Text summaryText;

    public void ShowEndScreen()
    {
        // Зупиняємо час у грі
        Time.timeScale = 0f;

        // Зберігаємо рекорд
        ScoreManager.Instance.SaveIfNewRecord();

        // Формуємо текст статистики
        string timeStr = TimeManager.Instance.timerText.text; // Беремо вже готовий час
        int score = ScoreManager.Instance.currentScore;
        int best = ScoreManager.Instance.GetHighScore();
        int coins = Object.FindFirstObjectByType<PlayerInventory>().coinsCount;

        summaryText.text =
                        $"BEST SCORE: {best}"+
                        $"SCORE: {score}\n" +
                        $"TIME: {timeStr}\n" +
                        $"COINS: {coins}\n";

        endPanel.SetActive(true);
        // Показуємо курсор, якщо він був прихований
        Cursor.visible = true;
    }

    // Методи для кнопок
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Заміни на назву своєї сцени меню
    }
}
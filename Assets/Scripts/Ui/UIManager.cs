using TMPro; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TMP_Text coinText;
    public GameObject endPanel;
    public GameObject gamePanel;
    public TMP_Text summaryText;
    public TMP_Text gameOverText;
    public GameObject settingsPanel;
    public GameObject pausePanel;

    void Awake()
    {
        Instance = this;
        coinText.text = "Coins: 0";
    }

    public void UpdateCoinDisplay(int amount)
    {
        coinText.text = "Coins: " + amount;
    }
    public void ShowEndScreen()
    {
        ShowEndPanel("GAME OVER");
    }

    public void ShowVictoryScreen()
    {
        ShowEndPanel("VICTORY!");
    }

    private void ShowEndPanel(string title)
    {
        // Блокуємо паузу — гра закінчена
        if (PauseManager.Instance != null)
            PauseManager.Instance.SetGameOver();

        gamePanel.SetActive(false);
        endPanel.SetActive(true);
        Time.timeScale = 0f;

        ScoreManager.Instance.SaveIfNewRecord();
        gameOverText.text = title;

        string timeStr = TimeManager.Instance.timerText.text;
        int score = ScoreManager.Instance.currentScore;
        int best = ScoreManager.Instance.GetHighScore();
        int coins = PlayerController.Instance.Inventory.coinsCount;

        // Зберігаємо зароблені монети до загального балансу
        if (CoinManager.Instance != null && coins > 0)
        {
            CoinManager.Instance.AddCoins(coins);
        }

        int totalCoins = CoinManager.Instance != null ? CoinManager.Instance.TotalCoins : coins;

        // Додаємо інфо про хвилі, якщо WaveManager існує
        string waveInfo = "";
        if (WaveManager.Instance != null)
            waveInfo = $"WAVES: {WaveManager.Instance.GetCurrentWaveNumber()}/{WaveManager.Instance.GetTotalWaves()}\n";

        summaryText.text =
                        $"BEST SCORE: {best}\n" +
                        $"SCORE: {score}\n" +
                        $"TIME: {timeStr}\n" +
                        waveInfo +
                        $"COINS: +{coins}\n" +
                        $"TOTAL COINS: {totalCoins}\n";

        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        if (PauseManager.Instance != null)
            PauseManager.Instance.ResumeGame();
    }
    
    public void ShowSettings()
    {
        settingsPanel.SetActive(true);
    }
    public void HideSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
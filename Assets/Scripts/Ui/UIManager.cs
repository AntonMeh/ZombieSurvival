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

    [Header("End Screen Stars")]
    public UnityEngine.UI.Image[] endScreenStars;
    public Color starActiveColor = Color.yellow;
    public Color starInactiveColor = new Color(0.3f, 0.3f, 0.3f, 1f);

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
        ShowEndPanel("GAME OVER", 0); // Програш = 0 зірок
    }

    public void ShowVictoryScreen()
    {
        // Зберігаємо прогрес рівня при перемозі
        if (SaveManager.Instance != null)
        {
            int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            SaveManager.Instance.CompleteLevel(currentLevel);

            // Розрахунок зірок (на основі часу або хвиль)
            int stars = CalculateStars();
            SaveManager.Instance.SaveStars(currentLevel, stars);

            // Зберігаємо найкращий час
            if (TimeManager.Instance != null)
            {
                float time = TimeManager.Instance.GetFinalTime();
                SaveManager.Instance.SaveBestTime(currentLevel, time);
            }

            ShowEndPanel("VICTORY!", stars);
        }
        else
        {
            ShowEndPanel("VICTORY!", CalculateStars());
        }
    }

    private void ShowEndPanel(string title, int starsEarned)
    {
        // Блокуємо паузу — гра закінчена
        if (PauseManager.Instance != null)
            PauseManager.Instance.SetGameOver();

        gamePanel.SetActive(false);
        endPanel.SetActive(true);
        Time.timeScale = 0f;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SaveIfNewRecord();

        if (gameOverText != null)
            gameOverText.text = title;

        string timeStr = TimeManager.Instance.timerText.text;
        int score = ScoreManager.Instance.currentScore;
        int best = ScoreManager.Instance.GetHighScore();
        int coins = 0;
        if (PlayerController.Instance != null && PlayerController.Instance.Inventory != null)
        {
            coins = PlayerController.Instance.Inventory.coinsCount;
        }

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

        // Оновлюємо зірки на екрані перемоги
        if (endScreenStars != null && endScreenStars.Length > 0)
        {
            for (int i = 0; i < endScreenStars.Length; i++)
            {
                if (endScreenStars[i] != null)
                {
                    endScreenStars[i].color = (i < starsEarned) ? starActiveColor : starInactiveColor;
                }
            }
        }

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

    /// <summary>
    /// Розраховує зірки за рівень.
    /// 3 зірки — всі хвилі пройдені.
    /// 2 зірки — більше половини хвиль.
    /// 1 зірка — хоча б одна хвиля.
    /// </summary>
    int CalculateStars()
    {
        if (WaveManager.Instance == null) return 1;

        int current = WaveManager.Instance.GetCurrentWaveNumber();
        int total = WaveManager.Instance.GetTotalWaves();

        if (current >= total) return 3;             
        if (current >= total / 2) return 2;         
        return 1;                                    
    }
}
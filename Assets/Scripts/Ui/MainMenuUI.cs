using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject levelSelectionPanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;

    [Header("Coin Display")]
    public TMP_Text coinBalanceText; 

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;

        ShowMainPanel();
    }

    public void ShowMainPanel()
    {
        CloseAllPanels();
        mainPanel.SetActive(true);
        UpdateCoinDisplay();
    }

    public void ShowLevelSelection()
    {
        CloseAllPanels();
        levelSelectionPanel.SetActive(true);
    }

    public void ShowShop()
    {
        CloseAllPanels();
        shopPanel.SetActive(true);
        UpdateCoinDisplay();
    }

    public void ShowSettings()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        if (shopPanel) shopPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
    }
    public void LoadLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void UpdateCoinDisplay()
    {
        if (coinBalanceText != null && CoinManager.Instance != null)
        {
            coinBalanceText.text = "Coins: " + CoinManager.Instance.TotalCoins;
        }
    }

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

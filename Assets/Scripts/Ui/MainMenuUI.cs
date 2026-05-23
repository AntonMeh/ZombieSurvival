using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject levelSelectionPanel;
    public GameObject shopPanel;
    public GameObject settingsPanel;

    [Header("Multiplayer Panels")]
    public GameObject modeSelectionPanel;
    public GameObject mpAuthPanel;
    public GameObject lobbyRoomPanel;

    [Header("Multiplayer Elements")]
    public TMP_InputField joinCodeInputField;
    public TMP_Text roomCodeDisplayText;
    public Button startGameButton;
    public Button readyButton;

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
        if (mainPanel) mainPanel.SetActive(true);
        UpdateCoinDisplay();
    }

    public void ShowModeSelection()
    {
        CloseAllPanels();
        if (modeSelectionPanel) modeSelectionPanel.SetActive(true);
    }

    public void ShowLevelSelection()
    {
        CloseAllPanels();
        if (levelSelectionPanel) levelSelectionPanel.SetActive(true);
    }

    public void ShowMultiplayer()
    {
        CloseAllPanels();
        if (mpAuthPanel) mpAuthPanel.SetActive(true);
    }

    public void CreateNetworkRoom()
    {
        CloseAllPanels();
        if (lobbyRoomPanel != null)
        {
            lobbyRoomPanel.SetActive(true);
        }
        if (roomCodeDisplayText != null)
        {
            roomCodeDisplayText.text = "ROOM CODE:";
        }
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
        }
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(false);
        }
    }

    public void JoinNetworkRoom()
    {
        if (joinCodeInputField != null && !string.IsNullOrEmpty(joinCodeInputField.text))
        {
            string code = joinCodeInputField.text.ToUpper();
            CloseAllPanels();
            if (lobbyRoomPanel != null)
            {
                lobbyRoomPanel.SetActive(true);
            }
            if (roomCodeDisplayText != null)
            {
                roomCodeDisplayText.text = code;
            }
            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(false);
            }
            if (readyButton != null)
            {
                readyButton.gameObject.SetActive(true);
            }
        }
    }

    public void BackToMainMenu()
    {
        ShowMainPanel();
    }

    public void BackToModeSelection()
    {
        CloseAllPanels();
        if (modeSelectionPanel) modeSelectionPanel.SetActive(true);
    }

    public void BackToMpAuth()
    {
        CloseAllPanels();
        if (mpAuthPanel) mpAuthPanel.SetActive(true);
    }

    public void ShowShop()
    {
        CloseAllPanels();
        if (shopPanel) shopPanel.SetActive(true);
        UpdateCoinDisplay();
    }

    public void ShowSettings()
    {
        CloseAllPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (mainPanel) mainPanel.SetActive(false);
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        if (shopPanel) shopPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (modeSelectionPanel) modeSelectionPanel.SetActive(false);
        if (mpAuthPanel) mpAuthPanel.SetActive(false);
        if (lobbyRoomPanel) lobbyRoomPanel.SetActive(false);
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

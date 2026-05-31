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
	public LobbyRoomUI lobbyRoomUI;

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

	public async void CreateNetworkRoom()
	{
		if (roomCodeDisplayText != null)
		{
			roomCodeDisplayText.text = "GENERATING CODE...";
		}

		string code = await RelayManager.Instance.CreateRelayRoom(4);

		if (!string.IsNullOrEmpty(code))
		{
			CloseAllPanels();
			if (lobbyRoomUI != null)
			{
				lobbyRoomUI.Show();
			}
			else if (lobbyRoomPanel != null)
			{
				LobbyRoomUI comp = lobbyRoomPanel.GetComponent<LobbyRoomUI>();
				if (comp != null)
				{
					comp.Show();
				}
			}
			if (roomCodeDisplayText != null)
			{
				roomCodeDisplayText.text = $"ROOM CODE: <color=#FFA500>{code}</color>";
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
		else
		{
			if (roomCodeDisplayText != null)
			{
				roomCodeDisplayText.text = "<color=red>FAILED</color>";
			}
		}
	}

	public async void JoinNetworkRoom()
	{
		if (joinCodeInputField != null && !string.IsNullOrEmpty(joinCodeInputField.text))
		{
			string code = joinCodeInputField.text.Trim().ToUpper();

			bool isSuccess = await RelayManager.Instance.JoinRelayRoom(code);

			if (isSuccess)
			{
				CloseAllPanels();
				if (lobbyRoomUI != null)
				{
					lobbyRoomUI.Show();
				}
				else if (lobbyRoomPanel != null)
				{
					LobbyRoomUI comp = lobbyRoomPanel.GetComponent<LobbyRoomUI>();
					if (comp != null)
					{
						comp.Show();
					}
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
			else
			{
				if (roomCodeDisplayText != null)
				{
					roomCodeDisplayText.text = "<color=red>JOIN FAILED</color>";
				}
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
		if (lobbyRoomUI != null)
		{
			lobbyRoomUI.Hide();
		}
		else if (lobbyRoomPanel != null)
		{
			LobbyRoomUI comp = lobbyRoomPanel.GetComponent<LobbyRoomUI>();
			if (comp != null)
			{
				comp.Hide();
			}
		}
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

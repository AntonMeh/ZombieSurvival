using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTile : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text levelNumberText;
    public Image[] starImages;       
    public GameObject lockIcon;      
    public Button playButton;

    [Header("Star Colors")]
    public Color starActive = Color.yellow;
    public Color starInactive = new Color(0.3f, 0.3f, 0.3f, 1f);

    private int levelNumber;
    private string sceneName;

    public void Setup(int level, string scene)
    {
        levelNumber = level;
        sceneName = scene;

        levelNumberText.text = level.ToString();

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayClicked);

        UpdateState();
    }

    public void UpdateState()
    {
        if (SaveManager.Instance == null) return;

        bool unlocked = SaveManager.Instance.IsLevelUnlocked(levelNumber);
        int stars = SaveManager.Instance.GetStars(levelNumber);

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        playButton.interactable = unlocked;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
                starImages[i].color = (i < stars) ? starActive : starInactive;
        }
    }

	private void OnPlayClicked()
	{
		PlayerPrefs.SetInt("CurrentLevel", levelNumber);
		PlayerPrefs.Save();

		if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
		{
			LobbyRoomUI lobbyRoomUI = FindFirstObjectByType<LobbyRoomUI>();
			if (lobbyRoomUI != null)
			{
				lobbyRoomUI.SaveSelectedCharactersToRelayManager();
			}
			NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}
		else
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}

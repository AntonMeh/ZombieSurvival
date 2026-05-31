using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    public GameObject pausePanel;

    public bool IsPaused { get; private set; }

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {

        if (pausePanel != null)
            pausePanel.SetActive(false);

        IsPaused = false;
    }

    void Update()
    {

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isGameOver)
                TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;
        
		bool isMultiplayer = Unity.Netcode.NetworkManager.Singleton != null && (Unity.Netcode.NetworkManager.Singleton.IsServer || Unity.Netcode.NetworkManager.Singleton.IsClient);
		if (!isMultiplayer)
		{
			Time.timeScale = 0f;
		}

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Cursor.visible = true;
    }

    public void SetGameOver()
    {
        isGameOver = true;

        if (IsPaused)
        {
            IsPaused = false;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }
    }
}

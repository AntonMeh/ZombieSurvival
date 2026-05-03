using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (levelNumber <= 1) return true;
        return PlayerPrefs.GetInt("LevelUnlocked_" + levelNumber, 0) == 1;
    }

    public void UnlockLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("LevelUnlocked_" + levelNumber, 1);
        PlayerPrefs.Save();
    }

    public bool IsLevelCompleted(int levelNumber)
    {
        return PlayerPrefs.GetInt("LevelCompleted_" + levelNumber, 0) == 1;
    }

    public void CompleteLevel(int levelNumber)
    {
        PlayerPrefs.SetInt("LevelCompleted_" + levelNumber, 1);
        UnlockLevel(levelNumber + 1);
    }

    public int GetStars(int levelNumber)
    {
        return PlayerPrefs.GetInt("LevelStars_" + levelNumber, 0);
    }

    public void SaveStars(int levelNumber, int stars)
    {
        int current = GetStars(levelNumber);
        if (stars > current)
        {
            PlayerPrefs.SetInt("LevelStars_" + levelNumber, stars);
            PlayerPrefs.Save();
        }
    }

    public float GetBestTime(int levelNumber)
    {
        return PlayerPrefs.GetFloat("LevelBestTime_" + levelNumber, 0f);
    }

    public void SaveBestTime(int levelNumber, float time)
    {
        float current = GetBestTime(levelNumber);
        if (current == 0f || time < current)
        {
            PlayerPrefs.SetFloat("LevelBestTime_" + levelNumber, time);
            PlayerPrefs.Save();
        }
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("SaveManager: Весь прогрес скинуто!");
    }
}

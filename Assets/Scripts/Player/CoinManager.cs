using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private const string COINS_KEY = "TotalCoins";

    public int TotalCoins { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
    }

    private void LoadCoins()
    {
        TotalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        TotalCoins += amount;
        SaveCoins();
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || TotalCoins < amount)
            return false;

        TotalCoins -= amount;
        SaveCoins();
        return true;
    }

    public bool CanAfford(int amount)
    {
        return TotalCoins >= amount;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, TotalCoins);
        PlayerPrefs.Save();
    }

    public void ResetCoins()
    {
        TotalCoins = 0;
        SaveCoins();
    }
}

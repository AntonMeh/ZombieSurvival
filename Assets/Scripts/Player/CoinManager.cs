using UnityEngine;

/// <summary>
/// Глобальний менеджер монет гравця.
/// Зберігає загальну кількість монет у PlayerPrefs між сесіями.
/// Використовується для магазину зброї та нарахування монет після гри.
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    private const string COINS_KEY = "TotalCoins";

    /// <summary>
    /// Загальний баланс монет гравця (збережений між сесіями).
    /// </summary>
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

    /// <summary>
    /// Завантажує збережені монети з PlayerPrefs.
    /// </summary>
    private void LoadCoins()
    {
        TotalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    /// <summary>
    /// Додає монети до загального балансу та зберігає.
    /// Використовується після завершення гри для нарахування зароблених монет.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;

        TotalCoins += amount;
        SaveCoins();
    }

    /// <summary>
    /// Витрачає монети (для магазину). Повертає true якщо достатньо монет.
    /// </summary>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || TotalCoins < amount)
            return false;

        TotalCoins -= amount;
        SaveCoins();
        return true;
    }

    /// <summary>
    /// Перевіряє чи достатньо монет для покупки.
    /// </summary>
    public bool CanAfford(int amount)
    {
        return TotalCoins >= amount;
    }

    /// <summary>
    /// Зберігає поточний баланс у PlayerPrefs.
    /// </summary>
    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, TotalCoins);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Скидає всі монети (для дебагу або нової гри з нуля).
    /// </summary>
    public void ResetCoins()
    {
        TotalCoins = 0;
        SaveCoins();
    }
}

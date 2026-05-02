using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Менеджер магазину зброї для Головного Меню.
/// </summary>
public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public WeaponData[] availableWeapons;
    
    [Header("UI Elements")]
    public TMP_Text totalCoinsText;
    public TMP_Text weaponNameText;
    public TMP_Text weaponStatsText;
    public TMP_Text buyButtonText;
    public Button buyEquipButton;

    private int currentIndex = 0;

    void Start()
    {
        // Базова зброя (перша в списку) завжди розблокована
        if (availableWeapons.Length > 0)
        {
            PlayerPrefs.SetInt("WeaponUnlocked_" + availableWeapons[0].weaponName, 1);
            
            // Якщо активна зброя ще не вибрана — ставимо базову
            if (!PlayerPrefs.HasKey("ActiveWeapon"))
            {
                PlayerPrefs.SetString("ActiveWeapon", availableWeapons[0].weaponName);
            }
        }

        UpdateShopUI();
    }

    void OnEnable()
    {
        UpdateShopUI();
    }

    /// <summary>
    /// Гортання списку зброї вправо
    /// </summary>
    public void NextWeapon()
    {
        currentIndex = (currentIndex + 1) % availableWeapons.Length;
        UpdateShopUI();
    }

    /// <summary>
    /// Гортання списку зброї вліво
    /// </summary>
    public void PreviousWeapon()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = availableWeapons.Length - 1;
        UpdateShopUI();
    }

    /// <summary>
    /// Оновлює весь UI магазину для поточної зброї
    /// </summary>
    public void UpdateShopUI()
    {
        if (availableWeapons.Length == 0) return;

        if (CoinManager.Instance != null)
            totalCoinsText.text = "COINS: " + CoinManager.Instance.TotalCoins;

        WeaponData currentWeapon = availableWeapons[currentIndex];
        weaponNameText.text = currentWeapon.weaponName;
        
        weaponStatsText.text = 
            $"Damage: {currentWeapon.damage}\n" +
            $"Fire Rate: {currentWeapon.fireRate}s";

        bool isUnlocked = PlayerPrefs.GetInt("WeaponUnlocked_" + currentWeapon.weaponName, 0) == 1;
        string activeWeaponName = PlayerPrefs.GetString("ActiveWeapon", "");

        if (activeWeaponName == currentWeapon.weaponName)
        {
            buyButtonText.text = "EQUIPPED";
            buyEquipButton.interactable = false;
        }
        else if (isUnlocked)
        {
            buyButtonText.text = "EQUIP";
            buyEquipButton.interactable = true;
        }
        else
        {
            buyButtonText.text = "BUY: " + currentWeapon.price;
            // Кнопка активна тільки якщо є гроші
            buyEquipButton.interactable = CoinManager.Instance != null && CoinManager.Instance.CanAfford(currentWeapon.price);
        }
    }

    /// <summary>
    /// Викликається кнопкою BUY/EQUIP
    /// </summary>
    public void OnBuyEquipButtonClicked()
    {
        WeaponData currentWeapon = availableWeapons[currentIndex];
        bool isUnlocked = PlayerPrefs.GetInt("WeaponUnlocked_" + currentWeapon.weaponName, 0) == 1;

        if (isUnlocked)
        {
            // Equip
            PlayerPrefs.SetString("ActiveWeapon", currentWeapon.weaponName);
            PlayerPrefs.Save();
        }
        else
        {
            // Buy
            if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(currentWeapon.price))
            {
                PlayerPrefs.SetInt("WeaponUnlocked_" + currentWeapon.weaponName, 1);
                PlayerPrefs.SetString("ActiveWeapon", currentWeapon.weaponName);
                PlayerPrefs.Save();
            }
        }

        UpdateShopUI();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponTile : MonoBehaviour
{
    [Header("UI Elements")]
    public Image weaponImage;
    public TMP_Text weaponNameText;
    public TMP_Text statsText;
    public TMP_Text priceText;
    public TMP_Text buttonText;
    public Button actionButton;

    private WeaponData weaponData;
    private ShopManager shopManager;

    public void Setup(WeaponData data, ShopManager manager)
    {
        weaponData = data;
        shopManager = manager;

        weaponNameText.text = data.weaponName;

        if (weaponImage != null && data.weaponSprite != null)
            weaponImage.sprite = data.weaponSprite;

        statsText.text = $"DMG: {data.damage}    RATE: {data.fireRate}s";

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnButtonClicked);

        UpdateState();
    }

    public void UpdateState()
    {
        if (weaponData == null) return;

        bool isUnlocked = PlayerPrefs.GetInt("WeaponUnlocked_" + weaponData.weaponName, 0) == 1;
        string activeWeapon = PlayerPrefs.GetString("ActiveWeapon", "");

        if (activeWeapon == weaponData.weaponName)
        {
            buttonText.text = "EQUIPPED";
            actionButton.interactable = false;
            priceText.text = "";
        }
        else if (isUnlocked)
        {
            buttonText.text = "EQUIP";
            actionButton.interactable = true;
            priceText.text = "";
        }
        else
        {
            buttonText.text = "BUY";
            actionButton.interactable = CoinManager.Instance != null 
                                     && CoinManager.Instance.CanAfford(weaponData.price);
            priceText.text = weaponData.price.ToString() + "coins";
        }
    }

    void OnButtonClicked()
    {
        if (shopManager != null)
            shopManager.OnTileBuyEquip(weaponData);
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public WeaponData[] availableWeapons;

    [Header("UI")]
    public Transform tilesContainer;  
    public GameObject weaponTilePrefab; 
    public TMP_Text totalCoinsText;

    private List<WeaponTile> spawnedTiles = new List<WeaponTile>();

    void Start()
    {

        if (availableWeapons.Length > 0)
        {
            PlayerPrefs.SetInt("WeaponUnlocked_" + availableWeapons[0].weaponName, 1);

            if (!PlayerPrefs.HasKey("ActiveWeapon"))
            {
                PlayerPrefs.SetString("ActiveWeapon", availableWeapons[0].weaponName);
                PlayerPrefs.Save();
            }
        }

        GenerateTiles();
    }

    void OnEnable()
    {
        UpdateAllTiles();
    }

    void GenerateTiles()
    {

        foreach (var tile in spawnedTiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        spawnedTiles.Clear();

        foreach (var weapon in availableWeapons)
        {
            GameObject tileObj = Instantiate(weaponTilePrefab, tilesContainer);
            WeaponTile tile = tileObj.GetComponent<WeaponTile>();
            tile.Setup(weapon, this);
            spawnedTiles.Add(tile);
        }

        UpdateCoinsDisplay();
    }

    public void OnTileBuyEquip(WeaponData weapon)
    {
        bool isUnlocked = PlayerPrefs.GetInt("WeaponUnlocked_" + weapon.weaponName, 0) == 1;

        if (isUnlocked)
        {

            PlayerPrefs.SetString("ActiveWeapon", weapon.weaponName);
            PlayerPrefs.Save();
        }
        else
        {

            if (CoinManager.Instance != null && CoinManager.Instance.SpendCoins(weapon.price))
            {
                PlayerPrefs.SetInt("WeaponUnlocked_" + weapon.weaponName, 1);
                PlayerPrefs.SetString("ActiveWeapon", weapon.weaponName);
                PlayerPrefs.Save();
            }
        }

        UpdateAllTiles();
    }

    void UpdateAllTiles()
    {
        UpdateCoinsDisplay();

        foreach (var tile in spawnedTiles)
        {
            if (tile != null) tile.UpdateState();
        }
    }

    void UpdateCoinsDisplay()
    {
        if (totalCoinsText != null && CoinManager.Instance != null)
            totalCoinsText.text = "COINS: " + CoinManager.Instance.TotalCoins;
    }
}

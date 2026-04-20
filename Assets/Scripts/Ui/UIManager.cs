using UnityEngine;
using TMPro; 

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public TMP_Text coinText;

    void Awake()
    {
        Instance = this;
        coinText.text = "Coins: 0";
    }

    public void UpdateCoinDisplay(int amount)
    {
        coinText.text = "Coins: " + amount;
    }
}
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public int coinsCount = 0;

    public void AddCoins(int amount)
    {
        coinsCount += amount;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinDisplay(coinsCount);
        }
    }
}
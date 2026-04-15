using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public AudioClip collectSound; 

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                SoundManager.Instance.PlaySound(collectSound, 0.7f);
            }

            PlayerInventory inventory = other.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                inventory.AddCoins(value);
            }

            Destroy(gameObject);
        }
    }
}
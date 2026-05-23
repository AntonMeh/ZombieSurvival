using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIPlayerEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI readyStatusText;
    [SerializeField] private GameObject hostCrownIcon;
    [SerializeField] private Image playerIconImage; // Посилання на наш колишній білий квадрат

    // Метод для оновлення візуалу, тепер включає масив спрайтів персонажів та вибраний ID
    public void UpdateEntry(string playerName, bool isReady, bool isHost, int characterId, Sprite[] characterSprites)
    {
        playerNameText.text = playerName;
        
        if (readyStatusText != null)
        {
            readyStatusText.text = isReady ? "<color=#00FF00>READY</color>" : "<color=#FF3333>NOT READY</color>";
        }

        if (hostCrownIcon != null)
        {
            hostCrownIcon.SetActive(isHost);
        }

        // Перевіряємо, чи індекс коректний і чи призначено картинку
        if (playerIconImage != null && characterSprites != null && characterId >= 0 && characterId < characterSprites.Length)
        {
            playerIconImage.sprite = characterSprites[characterId];
        }
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerEntry : MonoBehaviour
{
    #region Inspector Fields

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _readyStatusText;
    [SerializeField] private GameObject _hostCrownIcon;
    [SerializeField] private Image _playerIconImage;

    #endregion

    #region Public Methods

    public void UpdateEntry(string playerName, bool isReady, bool isHost, int characterId, Sprite[] characterSprites)
    {
        if (_playerNameText != null)
        {
            _playerNameText.text = playerName;
        }

        if (_readyStatusText != null)
        {
            _readyStatusText.text = isReady ? "<color=#00FF00>READY</color>" : "<color=#FF3333>NOT READY</color>";
        }

        if (_hostCrownIcon != null)
        {
            _hostCrownIcon.SetActive(isHost);
        }

        if (_playerIconImage != null && characterSprites != null && characterId >= 0 && characterId < characterSprites.Length)
        {
            _playerIconImage.sprite = characterSprites[characterId];
        }
    }

    #endregion
}
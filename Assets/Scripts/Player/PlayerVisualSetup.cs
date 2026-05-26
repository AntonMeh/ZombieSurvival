using Unity.Netcode;
using UnityEngine;

public class PlayerVisualSetup : NetworkBehaviour
{
    #region Inspector Fields

    [Header("Visual Components")]
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    [Header("Character Assets")]
    [SerializeField] private RuntimeAnimatorController[] _characterAnimators;

    #endregion

    #region Private Fields

    // Синхронізована змінна: сервер записує, всі клієнти читають
    private NetworkVariable<int> _characterId = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    #endregion

    #region Network Lifecycle

    public override void OnNetworkSpawn()
    {
        // Підписуємося на зміну ID (щоб нові гравці бачили правильні скіни старих гравців)
        _characterId.OnValueChanged += OnCharacterIdChanged;

        // Застосовуємо візуал одразу при спавні
        ApplyVisuals(_characterId.Value);
    }

    public override void OnNetworkDespawn()
    {
        _characterId.OnValueChanged -= OnCharacterIdChanged;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Встановлює ID персонажа. Може бути викликано ТІЛЬКИ на сервері під час спавну.
    /// </summary>
    public void SetCharacterIdServer(int id)
    {
        if (!IsServer) return;
        _characterId.Value = id;
    }

    #endregion

    #region Private Methods

    private void OnCharacterIdChanged(int previousValue, int newValue)
    {
        ApplyVisuals(newValue);
    }

    private void ApplyVisuals(int id)
    {
        if (_characterAnimators == null || id < 0 || id >= _characterAnimators.Length)
        {
            Debug.LogWarning("[PlayerVisualSetup] Invalid character ID or animators array is empty.");
            return;
        }

        // Підміняємо контролер анімацій (всі стани Idle, Run залишаться, але з іншими спрайтами)
        if (_playerAnimator != null)
        {
            _playerAnimator.runtimeAnimatorController = _characterAnimators[id];
        }
    }

    #endregion
}
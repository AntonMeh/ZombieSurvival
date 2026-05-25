using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#region Data Structures

public struct LobbyPlayerData : INetworkSerializable, System.IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public bool IsReady;
    public int SelectedCharacterIndex;

    public LobbyPlayerData(ulong clientId, bool isReady, int selectedCharacterIndex)
    {
        ClientId = clientId;
        IsReady = isReady;
        SelectedCharacterIndex = selectedCharacterIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref SelectedCharacterIndex);
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId &&
               IsReady == other.IsReady &&
               SelectedCharacterIndex == other.SelectedCharacterIndex;
    }
}

#endregion

public class LobbyRoomUI : NetworkBehaviour
{
    #region Inspector Fields

    [Header("Room Header Elements")]
    [SerializeField] private TMP_Text _roomCodeText;
    [SerializeField] private TMP_Text _playerCountText;

    [Header("Player List Container")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerRowPrefab;

    [Header("Action Buttons")]
    [SerializeField] private Button _leaveButton;
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _startGameButton;

    [Header("Character Selection")]
    [SerializeField] private List<Sprite> _characterPortraits;
    [SerializeField] private Image _localCharacterPreview;
    [SerializeField] private Button _prevCharacterButton;
    [SerializeField] private Button _nextCharacterButton;

    #endregion

    #region Private Fields

    private int _currentCharacterIndex = 0;
    private Dictionary<ulong, LobbyPlayerData> _lobbyPlayersData = new Dictionary<ulong, LobbyPlayerData>();

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (_readyButton != null) _readyButton.onClick.AddListener(OnReadyButtonClicked);
        if (_leaveButton != null) _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        if (_startGameButton != null) _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        if (_prevCharacterButton != null) _prevCharacterButton.onClick.AddListener(OnPrevCharacterClicked);
        if (_nextCharacterButton != null) _nextCharacterButton.onClick.AddListener(OnNextCharacterClicked);

        UpdateLocalCharacterPreview();
    }

    #endregion

    #region Network Lifecycle

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            ulong hostId = NetworkManager.Singleton.LocalClientId;
            if (!_lobbyPlayersData.ContainsKey(hostId))
            {
                _lobbyPlayersData.Add(hostId, new LobbyPlayerData(hostId, false, _currentCharacterIndex));
            }
            
            UpdateLobbyDisplay();
        }
        else
        {
            RequestLobbyUpdateServerRpc();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    #endregion

    #region Connection Logic

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (!_lobbyPlayersData.ContainsKey(clientId))
        {
            _lobbyPlayersData.Add(clientId, new LobbyPlayerData(clientId, false, 0));
        }
        UpdateLobbyDisplay();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        _lobbyPlayersData.Remove(clientId);
        UpdateLobbyDisplay();
    }

    #endregion

    #region Lobby UI Updates

    public void UpdateLobbyDisplay()
    {
        if (!IsServer) return;

        ClearPlayerList();

        List<ulong> clientIds = new List<ulong>();
        List<bool> readyStates = new List<bool>();
        List<int> characterIndices = new List<int>();

        bool allClientsReady = true;
        int connectedClientCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;
            bool isReady = false;
            int characterIndex = 0;

            if (_lobbyPlayersData.TryGetValue(clientId, out LobbyPlayerData data))
            {
                isReady = data.IsReady;
                characterIndex = data.SelectedCharacterIndex;
            }

            clientIds.Add(clientId);
            readyStates.Add(isReady);
            characterIndices.Add(characterIndex);

            CreatePlayerRow(clientId, isReady, characterIndex);

            if (clientId != NetworkManager.ServerClientId && !isReady)
            {
                allClientsReady = false;
            }
        }

        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(true);
            _startGameButton.interactable = allClientsReady;
        }

        if (_readyButton != null)
        {
            _readyButton.gameObject.SetActive(false);
        }

        UpdatePlayerCountText(connectedClientCount);
        UpdateLobbyDisplayClientRpc(clientIds.ToArray(), readyStates.ToArray(), characterIndices.ToArray(), allClientsReady);
    }

    private void ClearPlayerList()
    {
        if (_playerListContainer == null) return;

        foreach (Transform child in _playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreatePlayerRow(ulong clientId, bool isReady, int characterIndex)
    {
        if (_playerRowPrefab == null || _playerListContainer == null) return;

        GameObject rowGo = Instantiate(_playerRowPrefab, _playerListContainer);
        
        if (rowGo.TryGetComponent(out LobbyPlayerRow row))
        {
            bool isHost = (clientId == NetworkManager.ServerClientId);
            string roleLabel = isHost ? " (Host)" : " (Client)";
            
            if (row.NicknameText != null)
            {
                row.NicknameText.text = "Player " + clientId + roleLabel;
            }

            if (row.HostCrownIcon != null)
            {
                row.HostCrownIcon.gameObject.SetActive(isHost);
            }

            if (row.PlayerPortrait != null && _characterPortraits != null && _characterPortraits.Count > characterIndex)
            {
                row.PlayerPortrait.sprite = _characterPortraits[characterIndex];
            }

            if (row.StatusText != null)
            {
                if (isHost)
                {
                    row.StatusText.text = "HOST";
                    row.StatusText.color = Color.cyan;
                }
                else
                {
                    row.StatusText.text = isReady ? "READY" : "NOT READY";
                    row.StatusText.color = isReady ? Color.green : new Color(1f, 0.5f, 0f);
                }
            }
        }
    }

    private void UpdatePlayerCountText(int count)
    {
        if (_playerCountText != null)
        {
            _playerCountText.text = $"PLAYERS ({count} / 4)";
        }
    }

    private void UpdateLocalCharacterPreview()
    {
        if (_localCharacterPreview != null && _characterPortraits != null && _characterPortraits.Count > 0)
        {
            _localCharacterPreview.sprite = _characterPortraits[_currentCharacterIndex];
        }
    }

    #endregion

    #region Network RPCs

    [ClientRpc]
    private void UpdateLobbyDisplayClientRpc(ulong[] clientIds, bool[] readyStates, int[] characterIndices, bool allClientsReady)
    {
        if (IsServer) return; 

        ClearPlayerList();

        for (int i = 0; i < clientIds.Length; i++)
        {
            CreatePlayerRow(clientIds[i], readyStates[i], characterIndices[i]);
        }

        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(false);
        }

        if (_readyButton != null)
        {
            _readyButton.gameObject.SetActive(true);
        }

        UpdatePlayerCountText(clientIds.Length);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleReadyServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (_lobbyPlayersData.ContainsKey(clientId))
        {
            var data = _lobbyPlayersData[clientId];
            data.IsReady = !data.IsReady;
            _lobbyPlayersData[clientId] = data;
        }
        UpdateLobbyDisplay();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateSelectedCharacterServerRpc(int characterIndex, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (_lobbyPlayersData.ContainsKey(clientId))
        {
            var data = _lobbyPlayersData[clientId];
            data.SelectedCharacterIndex = characterIndex;
            _lobbyPlayersData[clientId] = data;
        }
        UpdateLobbyDisplay();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestLobbyUpdateServerRpc()
    {
        UpdateLobbyDisplay();
    }

    #endregion

    #region UI Event Handlers

    private void OnReadyButtonClicked()
    {
        ToggleReadyServerRpc();
    }

    private void OnPrevCharacterClicked()
    {
        if (_characterPortraits == null || _characterPortraits.Count == 0) return;
        
        _currentCharacterIndex--;
        if (_currentCharacterIndex < 0)
        {
            _currentCharacterIndex = _characterPortraits.Count - 1;
        }

        UpdateLocalCharacterPreview();
        UpdateSelectedCharacterServerRpc(_currentCharacterIndex);
    }

    private void OnNextCharacterClicked()
    {
        if (_characterPortraits == null || _characterPortraits.Count == 0) return;
        
        _currentCharacterIndex++;
        if (_currentCharacterIndex >= _characterPortraits.Count)
        {
            _currentCharacterIndex = 0;
        }

        UpdateLocalCharacterPreview();
        UpdateSelectedCharacterServerRpc(_currentCharacterIndex);
    }

    private void OnLeaveButtonClicked()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        MainMenuUI mainMenu = FindFirstObjectByType<MainMenuUI>();
        if (mainMenu != null)
        {
            mainMenu.BackToModeSelection();
        }
    }

    private void OnStartGameButtonClicked()
    {
        if (IsServer)
        {
            NetworkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    #endregion
}
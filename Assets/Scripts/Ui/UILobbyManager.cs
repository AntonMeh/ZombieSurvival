using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyManager : NetworkBehaviour
{
    #region Nested Types
    
    public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
    {
        public ulong ClientId;
        public FixedString32Bytes PlayerName;
        public bool IsReady;
        public int CharacterId;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsReady);
            serializer.SerializeValue(ref CharacterId);
        }

        public bool Equals(PlayerLobbyData other)
        {
            return ClientId == other.ClientId && 
                   PlayerName.Equals(other.PlayerName) && 
                   IsReady == other.IsReady &&
                   CharacterId == other.CharacterId;
        }
    }
    
    #endregion

    #region Inspector Fields

    [Header("Panel Flow References")]
    [SerializeField] private GameObject _authPanel;
    [SerializeField] private GameObject _lobbyRoomPanel;
    [SerializeField] private TMP_InputField _roomCodeInputField;
    [SerializeField] private TextMeshProUGUI _roomCodeText;

    [Header("UI Containers")]
    [SerializeField] private Transform _playerListContainer;
    [SerializeField] private GameObject _playerEntryPrefab;
    [SerializeField] private TextMeshProUGUI _playerCountText;

    [Header("Character Sprites")]
    [SerializeField] private Sprite[] _characterIcons;

    [Header("Lobby Buttons")]
    [SerializeField] private Button _readyButton;
    [SerializeField] private Button _startGameButton;

    #endregion

    #region Private Fields
    
    private NetworkList<PlayerLobbyData> _roomPlayers;
    private const int MAX_PLAYERS = 4;
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        _roomPlayers = new NetworkList<PlayerLobbyData>();
        
        if (_readyButton != null) 
            _readyButton.onClick.AddListener(OnReadyButtonClicked);
            
        if (_startGameButton != null) 
            _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
    }
    
    #endregion

    #region Network Lifecycle
    
    public override void OnNetworkSpawn()
    {
        _roomPlayers.OnListChanged += OnRoomPlayersChanged;

        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(IsServer);
            UpdateStartButtonState();
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            AddPlayerToList(NetworkManager.Singleton.LocalClientId);
        }

        RefreshPlayerListUI();
    }

    public override void OnNetworkDespawn()
    {
        _roomPlayers.OnListChanged -= OnRoomPlayersChanged;
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    
    #endregion

    #region Player List Management
    
    private void OnClientConnected(ulong clientId)
    {
        if (IsServer) AddPlayerToList(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer) RemovePlayerFromList(clientId);
    }

    private void AddPlayerToList(ulong clientId)
    {
        int assignedCharacterId = GetFirstAvailableCharacterId();
        string defaultName = $"Survivor {clientId + 1}";
        
        _roomPlayers.Add(new PlayerLobbyData
        {
            ClientId = clientId,
            PlayerName = defaultName,
            IsReady = false,
            CharacterId = assignedCharacterId
        });
    }

    private void RemovePlayerFromList(ulong clientId)
    {
        for (int i = 0; i < _roomPlayers.Count; i++)
        {
            if (_roomPlayers[i].ClientId == clientId)
            {
                _roomPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private int GetFirstAvailableCharacterId()
    {
        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            bool isTaken = false;
            foreach (var p in _roomPlayers)
            {
                if (p.CharacterId == i)
                {
                    isTaken = true;
                    break;
                }
            }
            
            if (!isTaken) return i;
        }
        return 0; // Резервний варіант, якщо всі слоти зайняті
    }
    
    #endregion

    #region UI Update Logic
    
    private void OnRoomPlayersChanged(NetworkListEvent<PlayerLobbyData> changeEvent)
    {
        RefreshPlayerListUI();
        if (IsServer) UpdateStartButtonState();
    }

    private void RefreshPlayerListUI()
    {
        foreach (Transform child in _playerListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var playerData in _roomPlayers)
        {
            GameObject entryGo = Instantiate(_playerEntryPrefab, _playerListContainer);
            if (entryGo.TryGetComponent(out UIPlayerEntry entryScript))
            {
                bool isHost = playerData.ClientId == NetworkManager.ServerClientId;
                entryScript.UpdateEntry(
                    playerData.PlayerName.ToString(), 
                    playerData.IsReady, 
                    isHost, 
                    playerData.CharacterId, 
                    _characterIcons
                );
            }
        }

        if (_playerCountText != null)
        {
            _playerCountText.text = $"Players: {_roomPlayers.Count} / {MAX_PLAYERS}";
        }
    }

    private void UpdateStartButtonState()
    {
        if (!IsServer || _startGameButton == null) return;

        bool allReady = true;
        foreach (var player in _roomPlayers)
        {
            if (player.ClientId != NetworkManager.ServerClientId && !player.IsReady)
            {
                allReady = false;
                break;
            }
        }

        _startGameButton.interactable = allReady;
    }
    
    #endregion

    #region Network RPCs
    
    private void OnReadyButtonClicked()
    {
        ToggleReadyStatusServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleReadyStatusServerRpc(ulong clientId)
    {
        for (int i = 0; i < _roomPlayers.Count; i++)
        {
            if (_roomPlayers[i].ClientId == clientId)
            {
                var data = _roomPlayers[i];
                data.IsReady = !data.IsReady; 
                _roomPlayers[i] = data; 
                break;
            }
        }
    }
    
    private void OnStartGameButtonClicked()
    {
        if (!IsServer) return;
        
        _startGameButton.interactable = false;
        NetworkManager.SceneManager.LoadScene("Level2", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    #endregion

    #region Relay Connection Flow
    
    public async void StartHostWithRelay()
    {
        if (_roomCodeText != null) _roomCodeText.text = "GENERATING CODE...";
        
        string code = await RelayManager.Instance.CreateRelayRoom(MAX_PLAYERS);

        if (!string.IsNullOrEmpty(code))
        {
            if (_roomCodeText != null) _roomCodeText.text = $"ROOM CODE: <color=#FFA500>{code}</color>";
            if (_authPanel != null) _authPanel.SetActive(false);
            if (_lobbyRoomPanel != null) _lobbyRoomPanel.SetActive(true);
        }
        else
        {
            if (_roomCodeText != null) _roomCodeText.text = "<color=red>FAILED</color>";
        }
    }

    public async void StartClientWithRelay()
    {
        if (_roomCodeInputField == null || string.IsNullOrEmpty(_roomCodeInputField.text)) return;
        
        string cleanCode = _roomCodeInputField.text.Trim().ToUpper();
        if (_roomCodeText != null) _roomCodeText.text = $"JOINING: <color=#FFA500>{cleanCode}</color>";

        bool isSuccess = await RelayManager.Instance.JoinRelayRoom(cleanCode);

        if (isSuccess)
        {
            if (_authPanel != null) _authPanel.SetActive(false);
            if (_lobbyRoomPanel != null) _lobbyRoomPanel.SetActive(true);
        }
        else
        {
            if (_roomCodeText != null) _roomCodeText.text = "<color=red>JOIN FAILED</color>";
        }
    }
    
    #endregion
}
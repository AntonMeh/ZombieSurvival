using System.Collections.Generic;
using TMPro;
using Unity.Collections;
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

    [Header("Character Portraits")]
    [SerializeField] private List<Sprite> _characterPortraits;

	[Header("Multiplayer Flow Controls")]
	[SerializeField] private GameObject _mainPanelContent;
	[SerializeField] private GameObject _levelSelectionPanel;

    #endregion

    #region Private Fields

	private NetworkList<LobbyPlayerData> _lobbyPlayers;
	private NetworkVariable<FixedString32Bytes> _lobbyRoomCode = new NetworkVariable<FixedString32Bytes>(
		"",
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Server
	);

    #endregion

    #region Unity Lifecycle

	private void Awake()
	{
		_lobbyPlayers = new NetworkList<LobbyPlayerData>();
	}

    private void Start()
    {
        if (_readyButton != null)
		{
			_readyButton.onClick.AddListener(OnReadyButtonClicked);
			_readyButton.interactable = false;
		}
        if (_leaveButton != null) _leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        if (_startGameButton != null)
		{
			_startGameButton.onClick.AddListener(OnStartGameButtonClicked);
			_startGameButton.interactable = false;
		}
    }

    #endregion

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
		Debug.Log($"[LobbyRoomUI] OnNetworkSpawn called. IsServer={IsServer}, IsClient={IsClient}, LocalClientId={NetworkManager.Singleton.LocalClientId}");

		_lobbyRoomCode.OnValueChanged += OnRoomCodeChanged;
		_lobbyPlayers.OnListChanged += OnLobbyPlayersChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

			_lobbyPlayers.Clear();

            ulong hostId = NetworkManager.Singleton.LocalClientId;
			_lobbyPlayers.Add(new LobbyPlayerData(hostId, false, 0));
            
			if (RelayManager.Instance != null && !string.IsNullOrEmpty(RelayManager.Instance.JoinCode))
			{
				_lobbyRoomCode.Value = RelayManager.Instance.JoinCode;
			}
        }
        else
        {
			UpdateRoomCodeText(_lobbyRoomCode.Value.ToString());
			if (_readyButton != null)
			{
				_readyButton.interactable = true;
			}
        }

		UpdateLobbyUI();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
		Debug.Log("[LobbyRoomUI] OnNetworkDespawn called.");

		_lobbyRoomCode.OnValueChanged -= OnRoomCodeChanged;
		_lobbyPlayers.OnListChanged -= OnLobbyPlayersChanged;

        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    #region Connection Logic

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
		Debug.Log($"[LobbyRoomUI] Server detected client connection: {clientId}");

		bool exists = false;
		for (int i = 0; i < _lobbyPlayers.Count; i++)
		{
			if (_lobbyPlayers[i].ClientId == clientId)
			{
				exists = true;
				break;
			}
		}

		if (!exists)
		{
			int assignedChar = GetFirstAvailableCharacterIndex();
			_lobbyPlayers.Add(new LobbyPlayerData(clientId, false, assignedChar));
		}
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
		Debug.Log($"[LobbyRoomUI] Server detected client disconnection: {clientId}");

		for (int i = 0; i < _lobbyPlayers.Count; i++)
		{
			if (_lobbyPlayers[i].ClientId == clientId)
			{
				_lobbyPlayers.RemoveAt(i);
				break;
			}
		}
    }

	private int GetFirstAvailableCharacterIndex()
	{
		if (_characterPortraits == null || _characterPortraits.Count == 0)
		{
			return 0;
		}

		for (int i = 0; i < _characterPortraits.Count; i++)
		{
			bool taken = false;
			for (int j = 0; j < _lobbyPlayers.Count; j++)
			{
				if (_lobbyPlayers[j].SelectedCharacterIndex == i)
				{
					taken = true;
					break;
				}
			}
			if (!taken)
			{
				return i;
			}
		}
		return 0;
	}

    #endregion

	#region Public Methods

	public void Show()
	{
		if (_mainPanelContent != null)
		{
			_mainPanelContent.SetActive(true);
		}
	}

	public void Hide()
	{
		if (_mainPanelContent != null)
		{
			_mainPanelContent.SetActive(false);
		}
	}

	public void SaveSelectedCharactersToRelayManager()
	{
		if (RelayManager.Instance != null)
		{
			RelayManager.Instance.SelectedCharacters.Clear();
			for (int i = 0; i < _lobbyPlayers.Count; i++)
			{
				RelayManager.Instance.SelectedCharacters[_lobbyPlayers[i].ClientId] = _lobbyPlayers[i].SelectedCharacterIndex;
			}
		}
	}

	#endregion

    #region Lobby UI Updates

	private void OnLobbyPlayersChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
	{
		Debug.Log($"[LobbyRoomUI] OnLobbyPlayersChanged: EventType={changeEvent.Type}, ClientId={changeEvent.Value.ClientId}, Index={changeEvent.Index}");
		UpdateLobbyUI();
	}

    private void UpdateLobbyUI()
    {
		Debug.Log($"[LobbyRoomUI] UpdateLobbyUI called. Current players count in NetworkList: {_lobbyPlayers.Count}");
        ClearPlayerList();

        bool allClientsReady = true;
        int connectedClientCount = _lobbyPlayers.Count;

		for (int i = 0; i < _lobbyPlayers.Count; i++)
		{
			var player = _lobbyPlayers[i];
			CreatePlayerRow(player.ClientId, player.IsReady, player.SelectedCharacterIndex);

			if (player.ClientId != NetworkManager.ServerClientId && !player.IsReady)
			{
				allClientsReady = false;
			}
		}

        if (IsServer)
		{
			if (_startGameButton != null)
			{
				_startGameButton.gameObject.SetActive(true);
				_startGameButton.interactable = allClientsReady;
			}

			if (_readyButton != null)
			{
				_readyButton.gameObject.SetActive(false);
			}
		}
		else
		{
			if (_startGameButton != null)
			{
				_startGameButton.gameObject.SetActive(false);
			}

			if (_readyButton != null)
			{
				_readyButton.gameObject.SetActive(true);
				_readyButton.interactable = true;
			}
		}

        UpdatePlayerCountText(connectedClientCount);
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

		if (rowGo.TryGetComponent(out UIPlayerEntry entry))
		{
			bool isHost = (clientId == NetworkManager.ServerClientId);
			string playerName = "Player " + clientId + (isHost ? " (Host)" : " (Client)");
			entry.UpdateEntry(
				playerName,
				isReady,
				isHost,
				characterIndex,
				_characterPortraits != null ? _characterPortraits.ToArray() : null
			);
		}
    }

    private void UpdatePlayerCountText(int count)
    {
        if (_playerCountText != null)
        {
			int maxPlayers = (_characterPortraits != null && _characterPortraits.Count > 0) ? _characterPortraits.Count : 4;
            _playerCountText.text = $"PLAYERS {count} / {maxPlayers}";
        }
    }

	private void OnRoomCodeChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
	{
		UpdateRoomCodeText(newValue.ToString());
	}

	private void UpdateRoomCodeText(string code)
	{
		if (_roomCodeText != null)
		{
			_roomCodeText.text = $"ROOM CODE: <color=#FFA500>{code}</color>";
		}
	}

    #endregion

    #region Network RPCs

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleReadyServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
		for (int i = 0; i < _lobbyPlayers.Count; i++)
		{
			if (_lobbyPlayers[i].ClientId == clientId)
			{
				var data = _lobbyPlayers[i];
				data.IsReady = !data.IsReady;
				_lobbyPlayers[i] = data;
				break;
			}
		}
    }

    #endregion

    #region UI Event Handlers

    private void OnReadyButtonClicked()
    {
        if (IsSpawned)
		{
			ToggleReadyServerRpc();
		}
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
        if (IsSpawned && IsServer)
        {
			SaveSelectedCharactersToRelayManager();
			if (_levelSelectionPanel != null)
			{
				_levelSelectionPanel.SetActive(true);
			}
			else
			{
				NetworkManager.SceneManager.LoadScene("Level2", UnityEngine.SceneManagement.LoadSceneMode.Single);
			}
        }
    }

    #endregion
}
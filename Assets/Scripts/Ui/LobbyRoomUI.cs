using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections.Generic;

public class LobbyRoomUI : NetworkBehaviour
{
    [Header("Room Header Elements")]
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text playerCountText;

    [Header("Player List Container")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerRowPrefab;

    [Header("Action Buttons")]
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    [Header("Character Selection")]
    [SerializeField] private List<Sprite> characterPortraits;
    [SerializeField] private Image localCharacterPreview;
    [SerializeField] private Button prevCharacterButton;
    [SerializeField] private Button nextCharacterButton;

    private int currentCharacterIndex = 0;

    // Server-side state tracking
    private Dictionary<ulong, LobbyPlayerData> lobbyPlayersData = new Dictionary<ulong, LobbyPlayerData>();

    private void Start()
    {
        // Add local button listeners
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyButtonClicked);
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        }
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        }
        if (prevCharacterButton != null)
        {
            prevCharacterButton.onClick.AddListener(OnPrevCharacterClicked);
        }
        if (nextCharacterButton != null)
        {
            nextCharacterButton.onClick.AddListener(OnNextCharacterClicked);
        }

        UpdateLocalCharacterPreview();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Register the host player
            ulong hostId = NetworkManager.Singleton.LocalClientId;
            if (!lobbyPlayersData.ContainsKey(hostId))
            {
                lobbyPlayersData.Add(hostId, new LobbyPlayerData(hostId, false, currentCharacterIndex));
            }
            
            UpdateLobbyDisplay();
        }
        else
        {
            // Clients request initial synchronization
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

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (!lobbyPlayersData.ContainsKey(clientId))
        {
            lobbyPlayersData.Add(clientId, new LobbyPlayerData(clientId, false, 0));
        }
        UpdateLobbyDisplay();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        lobbyPlayersData.Remove(clientId);
        UpdateLobbyDisplay();
    }

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

            if (lobbyPlayersData.TryGetValue(clientId, out LobbyPlayerData data))
            {
                isReady = data.IsReady;
                characterIndex = data.SelectedCharacterIndex;
            }

            clientIds.Add(clientId);
            readyStates.Add(isReady);
            characterIndices.Add(characterIndex);

            // Instantiate row prefab locally for host
            CreatePlayerRow(clientId, isReady, characterIndex);

            // Host checks if all other clients are ready
            if (clientId != NetworkManager.ServerClientId)
            {
                if (!isReady)
                {
                    allClientsReady = false;
                }
            }
        }

        // Host button setup
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = allClientsReady;
        }
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(false);
        }

        UpdatePlayerCountText(connectedClientCount);

        // Sync with all clients
        UpdateLobbyDisplayClientRpc(clientIds.ToArray(), readyStates.ToArray(), characterIndices.ToArray(), allClientsReady);
    }

    [ClientRpc]
    private void UpdateLobbyDisplayClientRpc(ulong[] clientIds, bool[] readyStates, int[] characterIndices, bool allClientsReady)
    {
        if (IsServer) return; // Server already rebuilt its local UI

        ClearPlayerList();

        for (int i = 0; i < clientIds.Length; i++)
        {
            CreatePlayerRow(clientIds[i], readyStates[i], characterIndices[i]);
        }

        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(false);
        }
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(true);
        }

        UpdatePlayerCountText(clientIds.Length);
    }

    private void ClearPlayerList()
    {
        if (playerListContainer == null) return;

        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreatePlayerRow(ulong clientId, bool isReady, int characterIndex)
    {
        if (playerRowPrefab == null || playerListContainer == null) return;

        GameObject rowGo = Instantiate(playerRowPrefab, playerListContainer);
        LobbyPlayerRow row = rowGo.GetComponent<LobbyPlayerRow>();
        if (row != null)
        {
            bool isHost = (clientId == NetworkManager.ServerClientId);
            string roleLabel = isHost ? " (Host)" : " (Client)";
            row.nicknameText.text = "Player " + clientId + roleLabel;

            if (row.hostCrownIcon != null)
            {
                row.hostCrownIcon.gameObject.SetActive(isHost);
            }

            if (row.playerPortrait != null && characterPortraits != null && characterPortraits.Count > characterIndex)
            {
                row.playerPortrait.sprite = characterPortraits[characterIndex];
            }

            if (row.statusText != null)
            {
                if (isHost)
                {
                    row.statusText.text = "HOST";
                    row.statusText.color = Color.cyan;
                }
                else
                {
                    row.statusText.text = isReady ? "READY" : "NOT READY";
                    row.statusText.color = isReady ? Color.green : new Color(1f, 0.5f, 0f);
                }
            }
        }
    }

    private void UpdatePlayerCountText(int count)
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"PLAYERS ({count} / 4)";
        }
    }

    private void OnReadyButtonClicked()
    {
        ToggleReadyServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleReadyServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (lobbyPlayersData.ContainsKey(clientId))
        {
            var data = lobbyPlayersData[clientId];
            data.IsReady = !data.IsReady;
            lobbyPlayersData[clientId] = data;
        }
        UpdateLobbyDisplay();
    }

    private void OnPrevCharacterClicked()
    {
        if (characterPortraits == null || characterPortraits.Count == 0) return;
        currentCharacterIndex--;
        if (currentCharacterIndex < 0)
        {
            currentCharacterIndex = characterPortraits.Count - 1;
        }

        UpdateLocalCharacterPreview();
        UpdateSelectedCharacterServerRpc(currentCharacterIndex);
    }

    private void OnNextCharacterClicked()
    {
        if (characterPortraits == null || characterPortraits.Count == 0) return;
        currentCharacterIndex++;
        if (currentCharacterIndex >= characterPortraits.Count)
        {
            currentCharacterIndex = 0;
        }

        UpdateLocalCharacterPreview();
        UpdateSelectedCharacterServerRpc(currentCharacterIndex);
    }

    private void UpdateLocalCharacterPreview()
    {
        if (localCharacterPreview != null && characterPortraits != null && characterPortraits.Count > 0)
        {
            localCharacterPreview.sprite = characterPortraits[currentCharacterIndex];
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void UpdateSelectedCharacterServerRpc(int characterIndex, RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (lobbyPlayersData.ContainsKey(clientId))
        {
            var data = lobbyPlayersData[clientId];
            data.SelectedCharacterIndex = characterIndex;
            lobbyPlayersData[clientId] = data;
        }
        UpdateLobbyDisplay();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestLobbyUpdateServerRpc()
    {
        UpdateLobbyDisplay();
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
}

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

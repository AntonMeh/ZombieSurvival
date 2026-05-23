using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyManager : NetworkBehaviour
{
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

    [Header("UI Containers")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private TextMeshProUGUI playerCountText;

    [Header("Character Sprites")]
    [SerializeField] private Sprite[] characterIcons;

    [Header("Lobby Buttons")]
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    private NetworkList<PlayerLobbyData> roomPlayers;

    private void Awake()
    {
        roomPlayers = new NetworkList<PlayerLobbyData>();
        
        if (readyButton != null) readyButton.onClick.AddListener(OnReadyButtonClicked);
        if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameButtonClicked);
    }

    public override void OnNetworkSpawn()
    {
        roomPlayers.OnListChanged += OnRoomPlayersChanged;

        // Кнопка старту доступна ТІЛЬКИ хосту
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(IsServer);
            UpdateStartButtonState();
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            // Додаємо Хоста (йому дістанеться персонаж 0)
            AddPlayerToList(NetworkManager.Singleton.LocalClientId);
        }

        RefreshPlayerListUI();
    }

    public override void OnNetworkDespawn()
    {
        roomPlayers.OnListChanged -= OnRoomPlayersChanged;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        AddPlayerToList(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;
        RemovePlayerFromList(clientId);
    }

    private void AddPlayerToList(ulong clientId)
    {
        // Логіка в стилі Left 4 Dead: шукаємо перший вільний ID персонажа (від 0 до 3)
        int assignedCharacterId = 0;
        for (int i = 0; i < 4; i++)
        {
            bool isTaken = false;
            foreach (var p in roomPlayers)
            {
                if (p.CharacterId == i) { isTaken = true; break; }
            }
            if (!isTaken)
            {
                assignedCharacterId = i;
                break;
            }
        }

        string defaultName = $"Survivor {clientId + 1}";
        roomPlayers.Add(new PlayerLobbyData
        {
            ClientId = clientId,
            PlayerName = defaultName,
            IsReady = false,
            CharacterId = assignedCharacterId
        });
    }

    private void RemovePlayerFromList(ulong clientId)
    {
        for (int i = 0; i < roomPlayers.Count; i++)
        {
            if (roomPlayers[i].ClientId == clientId)
            {
                roomPlayers.RemoveAt(i);
                break;
            }
        }
    }

    private void OnRoomPlayersChanged(NetworkListEvent<PlayerLobbyData> changeEvent)
    {
        RefreshPlayerListUI();
        if (IsServer)
        {
            UpdateStartButtonState();
        }
    }

    private void RefreshPlayerListUI()
{
    foreach (Transform child in playerListContainer)
    {
        Destroy(child.gameObject);
    }

    foreach (var playerData in roomPlayers)
    {
        GameObject entryGo = Instantiate(playerEntryPrefab, playerListContainer);
        UIPlayerEntry entryScript = entryGo.GetComponent<UIPlayerEntry>();

        bool isHost = playerData.ClientId == NetworkManager.ServerClientId;
        
        entryScript.UpdateEntry(
            playerData.PlayerName.ToString(), 
            playerData.IsReady, 
            isHost,
            playerData.CharacterId,
            characterIcons
        );
    }

    if (playerCountText != null)
    {
        playerCountText.text = $"Players: {roomPlayers.Count} / 4";
    }
}

    private void OnReadyButtonClicked()
    {
        // Клієнт просить сервер змінити його статус готовності
        ToggleReadyStatusServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ToggleReadyStatusServerRpc(ulong clientId)
    {
        for (int i = 0; i < roomPlayers.Count; i++)
        {
            if (roomPlayers[i].ClientId == clientId)
            {
                var data = roomPlayers[i];
                data.IsReady = !data.IsReady; 
                roomPlayers[i] = data; // Мережеве оновлення списку
                break;
            }
        }
    }

    // --- ЗАПУСК ГРИ (ТІЛЬКИ ХОСТ) ---

    private void UpdateStartButtonState()
    {
        if (!IsServer || startGameButton == null) return;

        // Перевіряємо, чи ВСІ гравці (крім самого хоста, або включно з ним — зазвичай всі) натиснули READY
        bool allReady = true;
        foreach (var player in roomPlayers)
        {
            // Якщо це не хост і він НЕ готовий, то старт заблоковано
            if (player.ClientId != NetworkManager.ServerClientId && !player.IsReady)
            {
                allReady = false;
                break;
            }
        }

        startGameButton.interactable = allReady;
    }

    private void OnStartGameButtonClicked()
    {
        if (!IsServer) return;

        startGameButton.interactable = false;

        Debug.Log("Starting Network Game...");
        
        NetworkManager.SceneManager.LoadScene("Level2", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    #region Singleton
    
    private static RelayManager _instance;
    
    public static RelayManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<RelayManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(nameof(RelayManager));
                    _instance = go.AddComponent<RelayManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    #endregion

	#region Properties

	public string JoinCode { get; private set; }
	public Dictionary<ulong, int> SelectedCharacters { get; private set; } = new Dictionary<ulong, int>();

	#endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    #endregion

    #region Initialization
    
    private async Task InitializeUnityServicesAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[RelayManager] Signed in anonymously. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already signing in") || ex.Message.Contains("SigningIn"))
                {
                    Debug.Log("[RelayManager] Already signing in. Waiting for completion...");
                    int attempts = 0;
                    while (!AuthenticationService.Instance.IsSignedIn && attempts < 30)
                    {
                        await Task.Delay(100);
                        attempts++;
                    }
                }
                else
                {
                    throw;
                }
            }
        }
    }
    
    #endregion

    #region Relay Operations
    
    public async Task<string> CreateRelayRoom(int maxConnections)
    {
        try
        {
            await InitializeUnityServicesAsync();

            int relayConnections = Math.Max(1, maxConnections - 1);
            Debug.Log($"[RelayManager] Allocating Relay for {relayConnections} connections...");
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(relayConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );
                
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
                NetworkManager.Singleton.ConnectionApprovalCallback = (request, response) => {
                    response.Approved = true;
                    response.CreatePlayerObject = false; // Disable auto-spawning in lobby
                };

                NetworkManager.Singleton.StartHost();

                if (NetworkManager.Singleton.SceneManager != null)
                {
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
                    NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
                }

                Debug.Log($"[RelayManager] Started Host with Join Code: {joinCode}");
				JoinCode = joinCode;
                return joinCode;
            }
            
            Debug.LogError("[RelayManager] UnityTransport component not found on NetworkManager!");
			JoinCode = null;
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelayManager] Error creating Relay room: {ex.Message}");
			JoinCode = null;
            return null;
        }
    }

    private void OnSceneLoadComplete(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == "MainMenu") return;

        Debug.Log($"[RelayManager] Scene {sceneName} loaded. Spawning players...");

        foreach (ulong clientId in clientsCompleted)
        {
            GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
            if (playerPrefab == null)
            {
                Debug.LogError("[RelayManager] PlayerPrefab is not assigned in NetworkManager!");
                continue;
            }

            Vector3 spawnPos = Vector3.zero;
            GameObject spawnPointObj = GameObject.FindWithTag("PlayerSpawnPoint");
            if (spawnPointObj != null)
            {
                spawnPos = spawnPointObj.transform.position;
            }

            GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            if (playerInstance.TryGetComponent<PlayerVisualSetup>(out var visualSetup))
            {
                int charId = 0;
                if (SelectedCharacters.TryGetValue(clientId, out int selectedId))
                {
                    charId = selectedId;
                }
                visualSetup.SetCharacterIdServer(charId);
            }
        }
    }

    public async Task<bool> JoinRelayRoom(string joinCode)
    {
        try
        {
            await InitializeUnityServicesAsync();
            Debug.Log($"[RelayManager] Joining Relay Room with Code: {joinCode}...");
            
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
            if (transport != null)
            {
                transport.SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                bool isSuccess = NetworkManager.Singleton.StartClient();
                Debug.Log($"[RelayManager] StartClient() returned {isSuccess}");
				if (isSuccess)
				{
					JoinCode = joinCode;
				}
				else
				{
					JoinCode = null;
				}
                return isSuccess;
            }
            
            Debug.LogError("[RelayManager] UnityTransport component not found on NetworkManager!");
			JoinCode = null;
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelayManager] Error joining Relay room: {ex.Message}");
			JoinCode = null;
            return false;
        }
    }
    
    #endregion
}
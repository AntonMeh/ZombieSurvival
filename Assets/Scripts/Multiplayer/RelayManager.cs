using System;
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
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"[RelayManager] Signed in anonymously. Player ID: {AuthenticationService.Instance.PlayerId}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelayManager] Error initializing Unity Services: {ex.Message}");
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
                
                NetworkManager.Singleton.StartHost();
                Debug.Log($"[RelayManager] Started Host with Join Code: {joinCode}");
                return joinCode;
            }
            
            Debug.LogError("[RelayManager] UnityTransport component not found on NetworkManager!");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelayManager] Error creating Relay room: {ex.Message}");
            return null;
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
                return isSuccess;
            }
            
            Debug.LogError("[RelayManager] UnityTransport component not found on NetworkManager!");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RelayManager] Error joining Relay room: {ex.Message}");
            return false;
        }
    }
    
    #endregion
}
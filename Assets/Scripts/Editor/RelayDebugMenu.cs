#if UNITY_EDITOR
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

public static class RelayDebugMenu
{
	#region Public Methods

	[MenuItem("Tools/Test Relay Connection")]
	public static async void TestRelayConnection()
	{
		Debug.Log("[RelayDebugMenu] Starting Unity Services and Relay Test...");
		try
		{
			if (UnityServices.State == ServicesInitializationState.Uninitialized)
			{
				Debug.Log("[RelayDebugMenu] Initializing Unity Services...");
				await UnityServices.InitializeAsync();
				Debug.Log("[RelayDebugMenu] Unity Services Initialized successfully.");
			}
			else
			{
				Debug.Log("[RelayDebugMenu] Unity Services already initialized.");
			}

			if (!AuthenticationService.Instance.IsSignedIn)
			{
				Debug.Log("[RelayDebugMenu] Signing in anonymously...");
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
				Debug.Log($"[RelayDebugMenu] Signed in anonymously. Player ID: {AuthenticationService.Instance.PlayerId}");
			}
			else
			{
				Debug.Log($"[RelayDebugMenu] Already signed in. Player ID: {AuthenticationService.Instance.PlayerId}");
			}

			Debug.Log("[RelayDebugMenu] Requesting Relay allocation...");
			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
			Debug.Log($"[RelayDebugMenu] Allocation created! ID: {allocation.AllocationId}");

			Debug.Log("[RelayDebugMenu] Requesting Join Code...");
			string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			Debug.Log($"[RelayDebugMenu] Success! Join Code: {joinCode}");
			
			EditorUtility.DisplayDialog("Relay Test", $"Relay connection and room creation succeeded!\nJoin Code: {joinCode}", "OK");
		}
		catch (Exception e)
		{
			Debug.LogError($"[RelayDebugMenu] Test Failed! Exception: {e.Message}");
			EditorUtility.DisplayDialog("Relay Test Failed", $"Error: {e.Message}", "OK");
		}
	}

	#endregion
}
#endif

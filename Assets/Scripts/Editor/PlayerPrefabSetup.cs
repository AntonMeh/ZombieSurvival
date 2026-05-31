#if UNITY_EDITOR
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerPrefabSetup
{
	private const string PrefabPath = "Assets/Prefabs/Player_1.prefab";

	static PlayerPrefabSetup()
	{
		EditorApplication.delayCall += SetupPrefab;
	}

	[MenuItem("Tools/Configure Player Prefab for Multiplayer")]
	public static void SetupPrefab()
	{
		using (var editingScope = new PrefabUtility.EditPrefabContentsScope(PrefabPath))
		{
			GameObject prefabRoot = editingScope.prefabContentsRoot;
			if (prefabRoot == null)
			{
				Debug.LogError($"[PlayerPrefabSetup] Failed to load prefab at {PrefabPath}");
				return;
			}

			// 1. NetworkObject setup
			NetworkObject netObject = prefabRoot.GetComponent<NetworkObject>();
			if (netObject == null)
			{
				netObject = prefabRoot.AddComponent<NetworkObject>();
				Debug.Log("[PlayerPrefabSetup] Added NetworkObject component.");
			}

			// 2. ClientNetworkTransform setup
			ClientNetworkTransform netTransform = prefabRoot.GetComponent<ClientNetworkTransform>();
			if (netTransform == null)
			{
				netTransform = prefabRoot.AddComponent<ClientNetworkTransform>();
				Debug.Log("[PlayerPrefabSetup] Added ClientNetworkTransform component.");
			}

			// Configure NetworkTransform for 2D smooth movement
			netTransform.SyncPositionX = true;
			netTransform.SyncPositionY = true;
			netTransform.SyncPositionZ = false;
			netTransform.SyncRotAngleX = false;
			netTransform.SyncRotAngleY = false;
			netTransform.SyncRotAngleZ = false;
			netTransform.SyncScaleX = false;
			netTransform.SyncScaleY = false;
			netTransform.SyncScaleZ = false;
			netTransform.Interpolate = true;
			Debug.Log("[PlayerPrefabSetup] Configured ClientNetworkTransform sync settings (2D Interpolated).");

			// 3. OwnerNetworkAnimator setup
			OwnerNetworkAnimator netAnimator = prefabRoot.GetComponent<OwnerNetworkAnimator>();
			if (netAnimator == null)
			{
				netAnimator = prefabRoot.AddComponent<OwnerNetworkAnimator>();
				Debug.Log("[PlayerPrefabSetup] Added OwnerNetworkAnimator component.");
			}

			Animator animator = prefabRoot.GetComponent<Animator>();
			if (animator != null)
			{
				if (netAnimator.Animator != animator)
				{
					netAnimator.Animator = animator;
					Debug.Log("[PlayerPrefabSetup] Assigned Animator reference to OwnerNetworkAnimator.");
				}
			}
			else
			{
				Debug.LogWarning("[PlayerPrefabSetup] Animator component not found on Player prefab root!");
			}
		}
	}
}
#endif

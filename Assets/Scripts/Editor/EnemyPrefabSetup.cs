#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class EnemyPrefabSetup
{
	private static readonly string[] EnemyPrefabPaths = new string[]
	{
		"Assets/Prefabs/Zombie.prefab",
		"Assets/Prefabs/Bat.prefab",
		"Assets/Prefabs/Golem_Blue.prefab",
		"Assets/Prefabs/GolemBoss.prefab"
	};

	private const string PlayerPrefabPath = "Assets/Prefabs/Player_1.prefab";
	private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

	static EnemyPrefabSetup()
	{
		EditorApplication.delayCall += SetupEnemyPrefabs;
	}

	[MenuItem("Tools/Configure Enemy Prefabs for Multiplayer")]
	public static void SetupEnemyPrefabs()
	{
		List<GameObject> configuredPrefabs = new List<GameObject>();

		// Configure player prefab just in case
		GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
		if (playerPrefab != null)
		{
			configuredPrefabs.Add(playerPrefab);
		}

		// Configure all enemy prefabs
		foreach (string path in EnemyPrefabPaths)
		{
			using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
			{
				GameObject prefabRoot = editingScope.prefabContentsRoot;
				if (prefabRoot == null)
				{
					Debug.LogError($"[EnemyPrefabSetup] Failed to load prefab at {path}");
					continue;
				}

				// 1. NetworkObject
				NetworkObject netObject = prefabRoot.GetComponent<NetworkObject>();
				if (netObject == null)
				{
					netObject = prefabRoot.AddComponent<NetworkObject>();
					Debug.Log($"[EnemyPrefabSetup] Added NetworkObject to {prefabRoot.name}");
				}

				// 2. NetworkTransform (Server-authoritative)
				NetworkTransform netTransform = prefabRoot.GetComponent<NetworkTransform>();
				if (netTransform == null)
				{
					netTransform = prefabRoot.AddComponent<NetworkTransform>();
					Debug.Log($"[EnemyPrefabSetup] Added NetworkTransform to {prefabRoot.name}");
				}

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

				// 3. NetworkAnimator
				NetworkAnimator netAnimator = prefabRoot.GetComponent<NetworkAnimator>();
				if (netAnimator == null)
				{
					netAnimator = prefabRoot.AddComponent<NetworkAnimator>();
					Debug.Log($"[EnemyPrefabSetup] Added NetworkAnimator to {prefabRoot.name}");
				}

				Animator animator = prefabRoot.GetComponent<Animator>();
				if (animator != null)
				{
					netAnimator.Animator = animator;
				}
			}

			// Load the asset post-configuration to store in the list
			GameObject configuredAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (configuredAsset != null)
			{
				configuredPrefabs.Add(configuredAsset);
			}
		}

		// Add prefabs to NetworkManager in MainMenu scene
		RegisterPrefabsInScene(configuredPrefabs);
	}

	private static void RegisterPrefabsInScene(List<GameObject> prefabs)
	{
		string currentScenePath = EditorSceneManager.GetActiveScene().path;

		// Load MainMenu scene
		var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
		NetworkManager netManager = Object.FindFirstObjectByType<NetworkManager>();

		if (netManager != null)
		{
			bool dirty = false;
			foreach (GameObject prefab in prefabs)
			{
				if (!netManager.NetworkConfig.Prefabs.Contains(prefab))
				{
					NetworkPrefab newNP = new NetworkPrefab();
					newNP.Prefab = prefab;
					netManager.NetworkConfig.Prefabs.Add(newNP);
					Debug.Log($"[EnemyPrefabSetup] Registered network prefab {prefab.name} in NetworkManager.");
					dirty = true;
				}
			}

			if (dirty)
			{
				EditorUtility.SetDirty(netManager);
				EditorSceneManager.MarkSceneDirty(scene);
				EditorSceneManager.SaveScene(scene);
				Debug.Log("[EnemyPrefabSetup] NetworkManager prefabs successfully saved to MainMenu scene.");
			}
		}
		else
		{
			Debug.LogError("[EnemyPrefabSetup] NetworkManager not found in MainMenu scene!");
		}

		// Restore previous scene if it was different
		if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != MainMenuScenePath)
		{
			EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
		}
	}
}
#endif

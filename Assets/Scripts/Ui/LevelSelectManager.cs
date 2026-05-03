using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Level Settings")]
    public LevelEntry[] levels;

    [Header("UI")]
    public Transform tilesContainer; 
    public GameObject levelTilePrefab;

    private List<LevelTile> spawnedTiles = new List<LevelTile>();

    void OnEnable()
    {
        GenerateTiles();
    }

    void GenerateTiles()
    {

        foreach (var tile in spawnedTiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        spawnedTiles.Clear();

        foreach (var entry in levels)
        {
            GameObject tileObj = Instantiate(levelTilePrefab, tilesContainer);
            LevelTile tile = tileObj.GetComponent<LevelTile>();
            tile.Setup(entry.levelNumber, entry.sceneName);
            spawnedTiles.Add(tile);
        }
    }
}

[System.Serializable]
public class LevelEntry
{
    public int levelNumber;
    public string sceneName;
}

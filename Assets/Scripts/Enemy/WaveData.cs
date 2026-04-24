using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "ZombieSurvival/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Enemies")]
    [Tooltip("Кількість ворогів у хвилі")]
    public int enemyCount = 10;

    [Tooltip("Інтервал між спавнами (секунди)")]
    public float spawnInterval = 2f;

    [Header("Timing")]
    [Tooltip("Пауза перед початком цієї хвилі")]
    public float intermissionDuration = 5f;
}

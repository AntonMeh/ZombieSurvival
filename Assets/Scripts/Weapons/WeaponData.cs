using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int price = 100;
    public GameObject bulletPrefab;
    public Sprite weaponSprite;
    public float fireRate = 0.5f;
    public float bulletForce = 20f;
    public int damage = 1;
    public Vector3 firePointOffset;

    [Header("Shotgun Settings")]
    [Tooltip("Кількість куль за один постріл (1 = звичайна зброя, 5+ = дробовик)")]
    public int pelletsPerShot = 1;

    [Tooltip("Кут розсіювання в градусах (наприклад, 30 = ±15°)")]
    public float spreadAngle = 0f;

    [Tooltip("Час життя кулі в секундах (менше = коротша дальність)")]
    public float bulletLifetime = 2f;

    [Header("Visual Settings")]
    public float weaponDistance = 1.2f;

    public AudioClip shootSound;
    public float volume = 0.7f; 
}
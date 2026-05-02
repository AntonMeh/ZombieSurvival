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

    [Header("Visual Settings")]
    public float weaponDistance = 1.2f;

    public AudioClip shootSound;
    public float volume = 0.7f; 
}
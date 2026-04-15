using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject bulletPrefab;
    public float fireRate = 0.5f;
    public float bulletForce = 20f;
    public int damage = 1;

    public AudioClip shootSound;
    public float volume = 0.7f; 
}
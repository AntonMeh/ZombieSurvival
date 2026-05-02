using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public WeaponData currentWeapon;
    public WeaponData[] allWeapons; // Масив всієї зброї для завантаження екіпірованої
    public Transform firePoint;
    public SpriteRenderer weaponVisual; 

    private float nextFireTime = 0f;

    void Start()
    {
        // Завантажуємо активну зброю з налаштувань магазину
        string activeWeaponName = PlayerPrefs.GetString("ActiveWeapon", "");
        if (!string.IsNullOrEmpty(activeWeaponName) && allWeapons != null)
        {
            foreach (WeaponData wd in allWeapons)
            {
                if (wd.weaponName == activeWeaponName)
                {
                    currentWeapon = wd;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Блокуємо стрільбу під час паузи
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + currentWeapon.fireRate;
        }
    }
    public void UpdateWeaponVisuals()
    {
        if (currentWeapon == null) return;

        weaponVisual.sprite = currentWeapon.weaponSprite;

        weaponVisual.transform.localPosition = new Vector3(currentWeapon.weaponDistance, 0, 0); 

        firePoint.localPosition = currentWeapon.firePointOffset;
    }
    private void OnValidate()
{
    if (weaponVisual != null && currentWeapon != null)
    {
        UpdateWeaponVisuals();
    }
}

    void Shoot()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("BulletPool.Instance відсутній на сцені! Створи GameObject з BulletPool.");
            return;
        }

        // Беремо готову кулю з пулу
        Bullet bulletScript = BulletPool.Instance.GetBullet(currentWeapon.bulletPrefab, firePoint.position, firePoint.rotation);
        
        bulletScript.SetDamage(currentWeapon.damage);
        
        // Використовуємо закешований Rigidbody
        bulletScript.rb.AddForce(firePoint.right * currentWeapon.bulletForce, ForceMode2D.Impulse);

        if (currentWeapon.shootSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(currentWeapon.shootSound, currentWeapon.volume);
        }
    }
}
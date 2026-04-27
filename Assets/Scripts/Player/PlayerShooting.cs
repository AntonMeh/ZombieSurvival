using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public WeaponData currentWeapon;
    public Transform firePoint;

    private float nextFireTime = 0f;

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

    void Shoot()
    {
        GameObject bulletObj = Instantiate(currentWeapon.bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Передаємо шкоду кулі
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDamage(currentWeapon.damage);
        }

        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * currentWeapon.bulletForce, ForceMode2D.Impulse);

        if (currentWeapon.shootSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(currentWeapon.shootSound, currentWeapon.volume);
        }
    }
}
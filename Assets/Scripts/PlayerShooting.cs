using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public WeaponData currentWeapon;
    public Transform firePoint;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + currentWeapon.fireRate;
        }
    }

    void Shoot()
    {
        // 1. Створюємо кулю. ВАЖЛИВО: передаємо firePoint.rotation
        GameObject bullet = Instantiate(currentWeapon.bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // 2. Додаємо силу в ЛОКАЛЬНОМУ напрямку точки пострілу
        // Спробуй спочатку .right, якщо не вийде - .up
        rb.AddForce(firePoint.right * currentWeapon.bulletForce, ForceMode2D.Impulse);
    }
}
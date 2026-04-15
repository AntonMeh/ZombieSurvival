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
        GameObject bullet = Instantiate(currentWeapon.bulletPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        rb.AddForce(firePoint.right * currentWeapon.bulletForce, ForceMode2D.Impulse);

        if (currentWeapon.shootSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(currentWeapon.shootSound, currentWeapon.volume);
        }
    }
}
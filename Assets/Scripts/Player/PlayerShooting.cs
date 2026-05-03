using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public WeaponData currentWeapon;
    public WeaponData[] allWeapons; 
    public Transform firePoint;
    public SpriteRenderer weaponVisual; 

    private float nextFireTime = 0f;

    void Start()
    {

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

        UpdateWeaponVisuals();
    }

    void Update()
    {
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused) return;

        bool isShooting = false;

        // 1. Власний надійний джойстик
        if (SimpleJoystick.AimJoy != null)
        {
            Vector2 rightStick = SimpleJoystick.AimJoy.InputVector;
            if (rightStick.magnitude > 0.5f)
            {
                isShooting = true;
            }
        }
        
        // 2. Мишка на ПК
        if (!Application.isMobilePlatform && Mouse.current != null && Mouse.current.leftButton != null && Mouse.current.leftButton.isPressed)
        {
            isShooting = true;
        }

        if (isShooting && Time.time >= nextFireTime)
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
            Debug.LogWarning("BulletPool.Instance відсутній на сцені!");
            return;
        }

        int pellets = Mathf.Max(1, currentWeapon.pelletsPerShot);
        float halfSpread = currentWeapon.spreadAngle / 2f;

        for (int i = 0; i < pellets; i++)
        {

            float angle;
            if (pellets == 1)
            {
                angle = 0f; 
            }
            else
            {

                float step = currentWeapon.spreadAngle / (pellets - 1);
                angle = -halfSpread + step * i;
                angle += Random.Range(-step * 0.2f, step * 0.2f); 
            }

            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, 0, angle);

            Bullet bulletScript = BulletPool.Instance.GetBullet(
                currentWeapon.bulletPrefab, firePoint.position, rotation);

            bulletScript.SetDamage(currentWeapon.damage);
            bulletScript.SetLifetime(currentWeapon.bulletLifetime);

            Vector2 direction = rotation * Vector2.right;
            bulletScript.rb.AddForce(direction * currentWeapon.bulletForce, ForceMode2D.Impulse);
        }

        if (currentWeapon.shootSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(currentWeapon.shootSound, currentWeapon.volume);
        }
    }
}
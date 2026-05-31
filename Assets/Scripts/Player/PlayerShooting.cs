using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : NetworkBehaviour
{
	public WeaponData currentWeapon;
	public WeaponData[] allWeapons; 
	public Transform firePoint;
	public SpriteRenderer weaponVisual; 

	[Header("Multiplayer VFX")]
	public GameObject muzzleFlashPrefab;

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
		if (!CanControl()) return;

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
			int weaponIndex = GetCurrentWeaponIndex();
			if (weaponIndex >= 0)
			{
				if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
				{
					ShootServerRpc(weaponIndex, firePoint.position, firePoint.rotation);
				}
				else
				{
					ShootLocal(weaponIndex, firePoint.position, firePoint.rotation);
				}
				nextFireTime = Time.time + currentWeapon.fireRate;
			}
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

	private bool CanControl()
	{
		if (NetworkManager.Singleton == null || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient))
		{
			return true;
		}
		return IsOwner;
	}

	private int GetCurrentWeaponIndex()
	{
		if (allWeapons == null) return -1;
		for (int i = 0; i < allWeapons.Length; i++)
		{
			if (allWeapons[i] == currentWeapon)
			{
				return i;
			}
		}
		return -1;
	}

	[Rpc(SendTo.Server)]
	private void ShootServerRpc(int weaponIndex, Vector3 pos, Quaternion rot)
	{
		PlayShootEffectsClientRpc(weaponIndex, pos, rot);
	}

	[Rpc(SendTo.ClientsAndHost)]
	private void PlayShootEffectsClientRpc(int weaponIndex, Vector3 pos, Quaternion rot)
	{
		ShootLocal(weaponIndex, pos, rot);
	}

	private void ShootLocal(int weaponIndex, Vector3 pos, Quaternion rot)
	{
		if (weaponIndex < 0 || allWeapons == null || weaponIndex >= allWeapons.Length) return;
		WeaponData weapon = allWeapons[weaponIndex];

		if (BulletPool.Instance == null)
		{
			Debug.LogWarning("BulletPool.Instance відсутній на сцені!");
			return;
		}

		int pellets = Mathf.Max(1, weapon.pelletsPerShot);
		float halfSpread = weapon.spreadAngle / 2f;

		for (int i = 0; i < pellets; i++)
		{
			float angle;
			if (pellets == 1)
			{
				angle = 0f;
			}
			else
			{
				float step = weapon.spreadAngle / (pellets - 1);
				angle = -halfSpread + step * i;
				angle += Random.Range(-step * 0.2f, step * 0.2f);
			}

			Quaternion bulletRotation = rot * Quaternion.Euler(0, 0, angle);

			Bullet bulletScript = BulletPool.Instance.GetBullet(
				weapon.bulletPrefab, pos, bulletRotation);

			if (bulletScript != null)
			{
				bulletScript.SetDamage(weapon.damage);
				bulletScript.SetLifetime(weapon.bulletLifetime);

				Vector2 direction = bulletRotation * Vector2.right;
				bulletScript.rb.AddForce(direction * weapon.bulletForce, ForceMode2D.Impulse);
			}
		}

		if (weapon.shootSound != null && SoundManager.Instance != null)
		{
			SoundManager.Instance.PlaySound(weapon.shootSound, weapon.volume);
		}

		if (muzzleFlashPrefab != null && firePoint != null)
		{
			GameObject flash = Instantiate(muzzleFlashPrefab, firePoint);
			Destroy(flash, 0.1f);
		}
	}
}
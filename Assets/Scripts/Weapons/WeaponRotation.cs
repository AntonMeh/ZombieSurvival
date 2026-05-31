using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponRotation : NetworkBehaviour
{
	#region Public Fields

	public Camera cam;

	#endregion

	#region Unity Lifecycle

	private void Update()
	{
		if (!CanControl())
		{
			return;
		}

		if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
		{
			return;
		}

		float angle = 0f;
		bool hasInput = false;

		if (SimpleJoystick.AimJoy != null)
		{
			Vector2 rightStick = SimpleJoystick.AimJoy.InputVector;
			if (rightStick.sqrMagnitude > 0.01f)
			{
				angle = Mathf.Atan2(rightStick.y, rightStick.x) * Mathf.Rad2Deg;
				hasInput = true;
			}
		}
        
		if (!hasInput && !Application.isMobilePlatform && Mouse.current != null)
		{
			var posControl = Mouse.current.position;
			if (posControl != null)
			{
				if (cam == null)
				{
					cam = Camera.main;
				}

				if (cam != null)
				{
					Vector3 mousePos = cam.ScreenToWorldPoint(posControl.ReadValue());
					Vector2 lookDir = mousePos - transform.position;
					angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
					hasInput = true;
				}
			}
		}

		if (hasInput)
		{
			transform.rotation = Quaternion.Euler(0, 0, angle);

			Vector3 scale = Vector3.one;
			if (angle > 90 || angle < -90)
			{
				scale.y = -1f;
			}
			else
			{
				scale.y = 1f;
			}
			transform.localScale = scale;
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

	#endregion
}
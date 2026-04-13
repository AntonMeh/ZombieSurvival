using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponRotation : MonoBehaviour
{
    public Camera cam;

    void Update()
    {
        // Отримуємо позицію миші
        Vector3 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 lookDir = mousePos - transform.position;

        // Вираховуємо кут
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // Повертаємо Pivot
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Фікс: щоб зброя не переверталася догори дриґом, коли ми цілимось вліво
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
using UnityEngine;

public class HideOnPC : MonoBehaviour
{
    void Awake()
    {
        if (!Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
        }
    }
}

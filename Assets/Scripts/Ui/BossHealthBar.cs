using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance { get; private set; }

    [Header("UI References")]
    public GameObject bossHUDPanel;       
    public TMP_Text bossNameText;         
    public Image healthBarFill;           
    public TMP_Text healthText;           

    private int maxHealth;
    private int currentHealth;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {

        if (bossHUDPanel != null)
            bossHUDPanel.SetActive(false);
    }

    public void ShowBoss(string name, int hp)
    {
        maxHealth = hp;
        currentHealth = hp;

        if (bossNameText != null)
            bossNameText.text = name;

        UpdateBar();

        if (bossHUDPanel != null)
            bossHUDPanel.SetActive(true);
    }

    public void UpdateHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        UpdateBar();
    }

    public void HideBoss()
    {
        if (bossHUDPanel != null)
            bossHUDPanel.SetActive(false);
    }

    void UpdateBar()
    {
        if (maxHealth <= 0) return;

        float fill = (float)currentHealth / maxHealth;

        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Clamp01(fill);

        if (healthText != null)
            healthText.text = $"{Mathf.Max(0, currentHealth)} / {maxHealth}";
    }
}

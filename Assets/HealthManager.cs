using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;  
    public float healthAmount = 100f;

    void Update()
    {
        // Press Enter to take damage
        if (Input.GetKeyDown(KeyCode.R))
        {
            TakeDamage(20);
        }

        // Press H to heal
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(15);
        }
    }

    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        healthAmount = Mathf.Clamp(healthAmount, 0, 100);
        UpdateHealthBar();
    }

    public void Heal(float healingAmount)
    {
        healthAmount += healingAmount;
        healthAmount = Mathf.Clamp(healthAmount, 0, 100);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.fillAmount = healthAmount / 100f; 
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Character playerCharacter;

    [Header("UI Elements")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Text healthText;

    [Header("Damage Feedback")]
    [SerializeField] private float overlayDuration = 0.5f;
    [SerializeField] private float overlayMaxAlpha = 0.5f;

    private float healthAmount = 100f;
    private Coroutine damageEffectCoroutine;

    private void Awake()
    {
        // Find player character if not assigned
        if (playerCharacter == null)
        {
            playerCharacter = FindObjectOfType<Character>();
        }
    }

    private void Start()
    {
        // Initialize UI elements
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0;
            damageOverlay.color = overlayColor;
        }

        // Subscribe to player health events
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged += UpdateHealthUI;
            playerCharacter.OnPlayerDeath += HandlePlayerDeath;
        }

        // Initialize health UI
        UpdateHealthUI(playerCharacter.currentHealth, 100f);
    }

    void Update()
    {
        // Debug controls (can be removed in final version)
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerCharacter.TakeDamage(20);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            playerCharacter.Heal(15);
        }
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        healthAmount = currentHealth;

        // Update health bar
        if (healthBar != null)
        {
            healthBar.fillAmount = healthAmount / maxHealth;
        }

        // Update health text if available
        if (healthText != null)
        {
            healthText.text = Mathf.Ceil(healthAmount).ToString() + "/" + maxHealth.ToString();
        }

        // Show damage effect when health decreases
        if (damageOverlay != null && currentHealth < healthAmount)
        {
            ShowDamageEffect();
        }
    }

    private void ShowDamageEffect()
    {
        // Stop existing coroutine if running
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }

        // Start new coroutine
        damageEffectCoroutine = StartCoroutine(DamageOverlayEffect());
    }

    private IEnumerator DamageOverlayEffect()
    {
        // Fade in
        float elapsed = 0f;

        // Instantly show overlay at max alpha
        Color overlayColor = damageOverlay.color;
        overlayColor.a = overlayMaxAlpha;
        damageOverlay.color = overlayColor;

        // Fade out
        elapsed = 0f;
        while (elapsed < overlayDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / overlayDuration;

            overlayColor.a = Mathf.Lerp(overlayMaxAlpha, 0f, normalizedTime);
            damageOverlay.color = overlayColor;

            yield return null;
        }

        // Ensure overlay is fully transparent
        overlayColor.a = 0f;
        damageOverlay.color = overlayColor;

        damageEffectCoroutine = null;
    }

    private void HandlePlayerDeath()
    {
        // Show death screen or game over UI
        // This would typically trigger a game manager to handle restart/respawn
        Debug.Log("Player has died!");

        // Example: Show overlay at full alpha
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 1f;
            damageOverlay.color = overlayColor;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged -= UpdateHealthUI;
            playerCharacter.OnPlayerDeath -= HandlePlayerDeath;
        }
    }
}
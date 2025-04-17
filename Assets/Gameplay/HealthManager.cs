using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthManager : MonoBehaviour
{
    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    [Header("UI Elements")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image damageOverlay;
    [SerializeField] private Text healthText;

    [Header("Damage Feedback")]
    [SerializeField] private float overlayDuration = 0.5f;
    [SerializeField] private float overlayMaxAlpha = 0.5f;

    private float healthAmount = 100f;
    private Coroutine damageEffectCoroutine;

    private Character playerCharacter;

    private void Awake()
    {
        playerCharacter = FindObjectOfType<Character>();

        if (playerCharacter == null)
        {
            Debug.LogError("No GameObject with Character script found in the scene!");
        }
    }

    private void Start()
    {
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0;
            damageOverlay.color = overlayColor;
        }

        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged += UpdateHealthUI;
            playerCharacter.OnPlayerDeath += HandlePlayerDeath;
        }

        UpdateHealthUI(playerCharacter.currentHealth, 100f);
    }

    void Update()
    {

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
        Debug.Log("Player has died!");

        // Show overlay at full alpha
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 1f;
            damageOverlay.color = overlayColor;
        }

        // Transition to game over screen after a short delay
        StartCoroutine(TransitionToGameOver());
    }

    private IEnumerator TransitionToGameOver()
    {
        // Wait for a moment so player can see death
        yield return new WaitForSeconds(1.5f);

        // Load game over scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameOverSceneName);
    }

    private void OnDestroy()
    {
        if (playerCharacter != null)
        {
            playerCharacter.OnHealthChanged -= UpdateHealthUI;
            playerCharacter.OnPlayerDeath -= HandlePlayerDeath;
        }
    }
}
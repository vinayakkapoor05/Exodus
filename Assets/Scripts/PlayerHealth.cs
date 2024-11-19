// PlayerHealth.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour 
{
    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;
    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBar;
    
    [Header("Healing Settings")]
    [SerializeField] private float asteroidKillHealAmount = 30f;
    [SerializeField] private float moonHealTickRate = 1f;
    

    
    private bool isHealing = false;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (currentHealth >= maxHealth) return;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        if (currentHealth > previousHealth)
        {
            UpdateHealthUI();
        }
    }
    
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
    }
    
    private void Die()
    {
        gameObject.SetActive(false);
        OnPlayerDeath?.Invoke();
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        UpdateHealthUI();
    }
    
    
    public void OnAsteroidDestroyed()
    {
        Heal(asteroidKillHealAmount);
    }
}

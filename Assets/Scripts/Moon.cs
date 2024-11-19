// Moon.cs
using UnityEngine;
using System.Collections;

public class Moon : MonoBehaviour 
{
    [Header("Moon Settings")]
    [SerializeField] private float healAmountPerSecond = 20f;
    [SerializeField] private float healTickRate = 0.1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip healAudioClip;
    [SerializeField] private float audioVolume = 0.5f;

    private bool isPlayerOnMoon = false;
    private PlayerHealth playerHealth;
    private Coroutine healingCoroutine;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = audioVolume;
        audioSource.clip = healAudioClip;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                isPlayerOnMoon = true;

                if (healingCoroutine != null)
                {
                    StopCoroutine(healingCoroutine);
                }
                healingCoroutine = StartCoroutine(HealPlayer());

                PlayHealingSound();
                    if (playerHealth.currentHealth == playerHealth.maxHealth){
                StopHealingSound();

            }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnMoon = false;

            if (healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
                healingCoroutine = null;
            }
          
            StopHealingSound();
            playerHealth = null;
        }
    }

    private void PlayHealingSound()
    {
        if (audioSource != null && healAudioClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void StopHealingSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator HealPlayer()
    {
        while (isPlayerOnMoon && playerHealth != null)
        {
            float healPerTick = healAmountPerSecond * healTickRate;
            playerHealth.Heal(healPerTick);
            yield return new WaitForSeconds(healTickRate);
        }
    }

    private void OnDisable()
    {
        StopHealingSound();
    }
}
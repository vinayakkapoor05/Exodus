// Blackhole.cs
using UnityEngine;

public class BlackHole : MonoBehaviour
{
    [Header("Black Hole Settings")]
    [SerializeField] private float attractionForce = 18f;
    [SerializeField] private float attractionRadius = 9f;
    [SerializeField] private float energyDrainRate = 10f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip blackHoleAmbientSound;
    [SerializeField] private float ambientVolume = 0.5f;
    [SerializeField] private float maxAudioDistance = 15f;

    private AudioSource audioSource;
    private RocketController rocket;
    private float baseVolume;

    private void Start()
    {
        rocket = FindObjectOfType<RocketController>();
        SetupAudio();
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = blackHoleAmbientSound;
        audioSource.loop = true;
        audioSource.volume = 0;  // Start silent
        audioSource.spatialBlend = 1f;  // Full 3D sound
        audioSource.maxDistance = maxAudioDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        baseVolume = ambientVolume;

        if (blackHoleAmbientSound != null)
        {
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (rocket == null) return;

        float distance = Vector2.Distance(transform.position, rocket.transform.position);

        if (distance < attractionRadius)
        {
            Vector2 direction = (transform.position - rocket.transform.position).normalized;

            Rigidbody2D rocketRb = rocket.GetComponent<Rigidbody2D>();
            if (rocketRb != null)
            {
                rocketRb.AddForce(direction * (attractionForce / distance));
            }

            rocket.DrainEnergy(energyDrainRate * Time.deltaTime);

            // Update audio volume based on distance
            if (audioSource != null)
            {
                float volumeMultiplier = 1f - (distance / attractionRadius);
                audioSource.volume = baseVolume * volumeMultiplier;
            }
        }
        else
        {
            // Fade out audio when outside attraction radius
            if (audioSource != null && audioSource.volume > 0)
            {
                audioSource.volume = Mathf.Max(0, audioSource.volume - Time.deltaTime);
            }
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    
}

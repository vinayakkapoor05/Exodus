// Planet.cs
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float gravitationalPull = 5f;
    [SerializeField] private float orbitRadius = 5f;
    [SerializeField] private float orbitSpeed = 120f;
    [SerializeField] private float radiusDecreaseRate = 0.5f;
    [SerializeField] private float minOrbitRadius = 1f;
    [SerializeField] private float attractionRadius = 8f;
        [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip orbitSound;

    [Header("Escape Settings")]
    [SerializeField] private KeyCode escapeKey = KeyCode.E;
    [SerializeField] private float baseEscapeForce = 15f;
    [SerializeField] private float escapeForceMultiplier = 2f;
    [SerializeField] private float escapeCooldown = 2f;  

    [Header("Energy Regeneration")]
    [SerializeField] private float orbitEnergyRegenRate = 20f; 

    private RocketController rocket;
    private bool isOrbiting = false;
    private float currentOrbitRadius;
    private float orbitAngle;
    private Vector3 orbitCenter;
    private float escapeTimer = 0f;
    private bool canAttract = true;
    
   private GameManager gameManager;

private void Start()
{
    rocket = FindObjectOfType<RocketController>();
    gameManager = FindObjectOfType<GameManager>();
    currentOrbitRadius = orbitRadius;
            if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

}

    
    private void Update()
    {
        if (rocket == null) return;

        if (!canAttract)
        {
            escapeTimer += Time.deltaTime;
            if (escapeTimer >= escapeCooldown)
            {
                canAttract = true;
                escapeTimer = 0f;
            }
        }

        float distanceToRocket = Vector2.Distance(transform.position, rocket.transform.position);

        if (!isOrbiting && canAttract && distanceToRocket < attractionRadius)
        {
            StartOrbit();
        }
        
        if (isOrbiting)
        {
            HandleOrbit();
            HandleEscape();
            DecreaseOrbitRadius();
            rocket.RechargeEnergy(orbitEnergyRegenRate * Time.deltaTime);

        }
    }
    
    private void StartOrbit()
    {
        isOrbiting = true;
        orbitCenter = transform.position;
        
        Vector2 directionToRocket = (rocket.transform.position - transform.position).normalized;
        orbitAngle = Mathf.Atan2(directionToRocket.y, directionToRocket.x) * Mathf.Rad2Deg;
                if (audioSource != null && orbitSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(orbitSound);
        }

        rocket.enabled = false;
    }
    
    private void HandleOrbit()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        
        float x = orbitCenter.x + currentOrbitRadius * Mathf.Cos(orbitAngle * Mathf.Deg2Rad);
        float y = orbitCenter.y + currentOrbitRadius * Mathf.Sin(orbitAngle * Mathf.Deg2Rad);
        
        rocket.transform.position = new Vector3(x, y, rocket.transform.position.z);
    }
    
    private void DecreaseOrbitRadius()
    {
        currentOrbitRadius = Mathf.Max(minOrbitRadius, 
            currentOrbitRadius - (radiusDecreaseRate * Time.deltaTime));
    }
    
    private void HandleEscape()
    {
        if (Input.GetKeyDown(escapeKey))
        {
            float escapeForce = baseEscapeForce + 
                (escapeForceMultiplier * (orbitRadius - currentOrbitRadius));
            
            Vector2 escapeDirection = new Vector2(
                -Mathf.Sin(orbitAngle * Mathf.Deg2Rad),
                Mathf.Cos(orbitAngle * Mathf.Deg2Rad)
            );
            
            Rigidbody2D rocketRb = rocket.GetComponent<Rigidbody2D>();
            rocketRb.velocity = escapeDirection * escapeForce;
            
            rocket.enabled = true;
            isOrbiting = false;
                        if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            canAttract = false;
            escapeTimer = 0f;
            
            currentOrbitRadius = orbitRadius;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
        gameManager.HandleCelestialCollision("Planet");
        
        currentOrbitRadius = orbitRadius;
        isOrbiting = false;
        canAttract = true;
        escapeTimer = 0f;
        }
                    if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

    }
    
   
}
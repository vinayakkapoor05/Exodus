// RocketController.cs
using UnityEngine;
using UnityEngine.UI;

public class RocketController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float thrustForce = 700f;
    [SerializeField] private float maxVelocity = 15f;
    [SerializeField] private float horizontalSpeed = 8f;
    [SerializeField] private float horizontalSmoothTime = 0.1f;

    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyConsumptionRate = 10f;
    [SerializeField] private float energyRegenerationRate = 5f;
    [SerializeField] private Slider energySlider;

    [Header("Visual Effects")]
    [SerializeField] private Animator engineAnimator;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootCooldown = 0.2f;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private Vector2 bulletOffset = new Vector2(0f, 0.5f);

    [Header("Energy Cooldown Settings")]
    [SerializeField] private float energyCooldownDuration = 1.5f;

    [Header("Screen Boundaries")]
    [SerializeField] private float screenEdgeBuffer = 0.5f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip shootBullet;
    [SerializeField] private AudioClip thrust;
    [SerializeField] [Range(0f, 1f)] private float thrustVolume = 0.3f; 
    private AudioSource bulletAudioSource;  
    private AudioSource thrustAudioSource;

    private bool isEnergyCoolingDown = false;
    private float energyCooldownTimer = 0f;
    private float lastShootTime = 0f;
    private Rigidbody2D rb;
    private float currentEnergy;
    private Vector3 startPosition;
    private float currentHorizontalVelocity;
    private Camera mainCamera;
    private float minX;
    private float maxX;
    private float rocketWidth;

    public bool IsThrusting { get; private set; }

    private void Start()
    {
        InitializeComponents();
        SetupRocket();
        SetupBoundaries();
        SetupVisuals();
    }

    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        thrustAudioSource = gameObject.AddComponent<AudioSource>();
        thrustAudioSource.loop = true; 
        thrustAudioSource.clip = thrust;
        thrustAudioSource.volume = thrustVolume;

        bulletAudioSource = gameObject.AddComponent<AudioSource>();
        bulletAudioSource.loop = false;  
    }

    private void SetupRocket()
    {
        startPosition = transform.position;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        ResetRocket();
    }

    private void SetupBoundaries()
    {
        CalculateScreenBounds();
    }

    private void SetupVisuals()
    {
      

        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = maxEnergy;
        }
    }

    private void CalculateScreenBounds()
    {
        float verticalExtent = mainCamera.orthographicSize;
        float horizontalExtent = verticalExtent * mainCamera.aspect;
        
        minX = -horizontalExtent + rocketWidth + screenEdgeBuffer;
        maxX = horizontalExtent - rocketWidth - screenEdgeBuffer;
    }

    public void ResetRocket()
    {
        currentEnergy = maxEnergy;
        transform.position = startPosition;
        rb.velocity = Vector2.zero;
        currentHorizontalVelocity = 0f;
        rb.gravityScale = 2f;
        rb.drag = 0.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        IsThrusting = false;

        UpdateEnergyUI();
        UpdateAnimationState();
    }

    private void Update()
    {
        HandleInput();
        HandleEnergy();
        HandleShooting();
        UpdateVisuals();
    }

    private void HandleInput()
    {
        if (currentEnergy <= 0 || isEnergyCoolingDown)
        {
            IsThrusting = false;
            return;
        }

        IsThrusting = Input.GetKey(KeyCode.W);

        if (isEnergyCoolingDown)
        {
            energyCooldownTimer += Time.deltaTime;
            
            if (energyCooldownTimer >= energyCooldownDuration && !Input.GetKey(KeyCode.W))
            {
                isEnergyCoolingDown = false;
                energyCooldownTimer = 0f;
            }
        }
    }

    private void HandleShooting()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShootTime + shootCooldown)
        {
            Vector2 spawnPosition = (Vector2)transform.position + bulletOffset;
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePosition - (Vector2)transform.position).normalized;
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0f, 0f, angle));
            
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.Initialize(direction);
            }
            
            bulletAudioSource.PlayOneShot(shootBullet);
            lastShootTime = Time.time;
        }
    }

    private void HandleEnergy()
    {
        if (IsThrusting)
        {
            if (!thrustAudioSource.isPlaying)
            {
                thrustAudioSource.Play();
            }

            currentEnergy = Mathf.Max(0f, currentEnergy - energyConsumptionRate * Time.deltaTime);
        }
        else
        {
            thrustAudioSource.Stop();
            if (currentEnergy < maxEnergy)
            {
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRegenerationRate * Time.deltaTime);
            }
        }

        UpdateEnergyUI();
    }

    private void UpdateVisuals()
    {
        
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (engineAnimator != null)
        {
            engineAnimator.SetBool("IsPowered", IsThrusting);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        Vector3 currentPos = transform.position;
        bool canMoveLeft = currentPos.x > minX || horizontalInput > 0;
        bool canMoveRight = currentPos.x < maxX || horizontalInput < 0;

        if ((canMoveLeft && canMoveRight) || 
            (canMoveLeft && horizontalInput > 0) || 
            (canMoveRight && horizontalInput < 0))
        {
            float targetHorizontalVelocity = horizontalInput * horizontalSpeed;
            float smoothedHorizontalVelocity = Mathf.SmoothDamp(
                rb.velocity.x,
                targetHorizontalVelocity,
                ref currentHorizontalVelocity,
                horizontalSmoothTime
            );
            rb.velocity = new Vector2(smoothedHorizontalVelocity, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (IsThrusting)
        {
            float thrustDelta = thrustForce * Time.fixedDeltaTime;
            Vector2 newVelocity = rb.velocity;
            newVelocity.y += thrustDelta;
            newVelocity.y = Mathf.Min(newVelocity.y, maxVelocity);
            rb.velocity = newVelocity;
        }

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        transform.position = clampedPosition;
    }

    private void UpdateEnergyUI()
    {
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }
    }

    public void RechargeEnergy(float amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        UpdateEnergyUI();
    }

    public void DrainEnergy(float amount)
    {
        currentEnergy = Mathf.Max(0f, currentEnergy - amount);
        UpdateEnergyUI();
    }

    public float GetEnergyPercentage()
    {
        return currentEnergy / maxEnergy;
    }
   

}

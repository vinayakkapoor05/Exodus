// Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private float speed = 20f;
    private Rigidbody2D rb;
    private GameManager gameManager;
    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.bodyType = RigidbodyType2D.Dynamic;
        
        if (GetComponent<CircleCollider2D>() == null)
        {
            var collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = false;
            collider.radius = 0.1f;
        }
        
        gameManager = FindObjectOfType<GameManager>();
        playerHealth = FindObjectOfType<PlayerHealth>();
    }
    
    public void Initialize(Vector2 direction)
    {
        rb.velocity = direction * speed;
    }
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            Asteroid asteroid = collision.gameObject.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.DestroyAsteroid();
                if (playerHealth != null)
                {
                    playerHealth.OnAsteroidDestroyed();
                }
            }
            Destroy(gameObject);
        }
        else if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
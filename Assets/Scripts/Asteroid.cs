// Asteroid.cs
using UnityEngine;

public class Asteroid : MonoBehaviour 
{
    [Header("Asteroid Settings")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private int scoreValue = 10;

    private GameManager gameManager;
    private Rigidbody2D rb;

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
        rb.angularDrag = 0f;
        rb.drag = 0f;

    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    public void DestroyAsteroid()
    {
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}
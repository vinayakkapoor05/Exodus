// Supernova.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class Supernova : MonoBehaviour 
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private float accelerationRate = 0.1f;
    [SerializeField] private float initialDistance = 10f;
    
    private float currentSpeed;
    private Vector3 startPosition;
    private float timeSinceStart;
    private bool isPaused;
    private float pauseStartTime;
    private Vector3 pausePosition;

    private void Start()
    {
        currentSpeed = followSpeed;
        startPosition = transform.position;
        timeSinceStart = 0f;
        isPaused = false;

        if (target != null)
        {
            transform.position = new Vector3(0, target.position.y - initialDistance, 0);
        }
    }

    private void Update()
    {
        if (target == null) return;

        if (isPaused)
        {
            transform.position = pausePosition;
            return;
        }

        timeSinceStart += Time.deltaTime;
        currentSpeed = followSpeed + (accelerationRate * timeSinceStart);

        float newY = Mathf.MoveTowards(transform.position.y,
                                   target.position.y,
                                   currentSpeed * Time.deltaTime);
        transform.position = new Vector3(0, newY, 0);
    }

    public void PauseMovement()
    {
        if (isPaused) return;
        
        isPaused = true;
        pausePosition = transform.position;  
    }

    public void ResumeMovement()
    {
        if (!isPaused) return;
        
        isPaused = false;
        timeSinceStart += Time.time - pauseStartTime;  
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        currentSpeed = followSpeed;
        timeSinceStart = 0f;
        isPaused = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.HandleSupernovaCollision();
            }
        }
    }
}

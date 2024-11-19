// GameManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
        [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button restartButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private float gameOverVolume = 1f;

    [Header("Game State")]
    private bool isGameOver = false;
    private float score;
    private float highScore;
    private const string HIGH_SCORE_KEY = "HighScore";


    [System.Serializable]
        public class SpawnSettings
    {
        public GameObject[] prefabVariants;
        public float spawnChance = 0.5f;
        public float minHorizontalSpawn = -8f;
        public float maxHorizontalSpawn = 8f;
        public float minHorizontalGap = 3f;
        public bool requiresExclusiveLayer = false;
        
        public GameObject GetRandomPrefab()
        {
            if (prefabVariants == null || prefabVariants.Length == 0)
            {
                return null;
            }
            return prefabVariants[Random.Range(0, prefabVariants.Length)];
        }
    }


    [Header("Spawn Settings")]
    [SerializeField] private SpawnSettings planetSettings;
    [SerializeField] private SpawnSettings moonSettings;
    [SerializeField] private SpawnSettings sunSettings;
    [SerializeField] private SpawnSettings blackHoleSettings;
    [SerializeField] private SpawnSettings asteroidSettings;
    
    [Header("Layer Settings")]
    [SerializeField] private float layerHeight = 10f;
    [SerializeField] private float initialLayerY = 10f;
    [SerializeField] private int maxActiveLayers = 10;
    [SerializeField] private float minDangerousObjectSpacing = 3f;
    
    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyIncreaseRate = 0.1f;
    [SerializeField] private float maxDifficultyMultiplier = 2f;
    [SerializeField] private float baseAsteroidSpawnRate = 0.8f;
    
    [Header("Score Settings")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float scoreMultiplier = 1f;
    
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
        [Header("References")]
    [SerializeField] private RocketController rocket;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Supernova supernova;

    private float screenMinX;
    private float screenMaxX;
    private float currentDifficulty = 1f;
    private float highestGeneratedY;
    private float lastDangerousObjectY = float.MinValue;
    private System.Collections.Generic.List<Vector2> spawnedPositions = new System.Collections.Generic.List<Vector2>();

    private void OnValidate()
    {
        ValidatePrefabArray(planetSettings.prefabVariants, "Planet");
        ValidatePrefabArray(asteroidSettings.prefabVariants, "Asteroid");
        ValidatePrefabArray(moonSettings.prefabVariants, "Moon");
        ValidatePrefabArray(sunSettings.prefabVariants, "Sun");
        ValidatePrefabArray(blackHoleSettings.prefabVariants, "Black Hole");
    }

    private void ValidatePrefabArray(GameObject[] prefabs, string type)
    {
        if (prefabs == null || prefabs.Length == 0)
        {
        }
    }

        private void Start()
    {
        InitializeGame();
                if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

    }

    private void InitializeGame()
    {
        highScore = PlayerPrefs.GetFloat(HIGH_SCORE_KEY, 0);
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        if (highScoreText != null)
            highScoreText.text = $"High Score: {Mathf.FloorToInt(highScore):N0}";
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            

        isGameOver = false;
        score = 0;
        UpdateScoreUI();

        if (playerHealth != null)
            playerHealth.OnPlayerDeath += HandlePlayerDeath;

        InitializeSettings();
        GenerateInitialLayers();
        InvokeRepeating(nameof(CleanupObjects), 5f, 5f);
    }



    private void InitializeSettings()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }
        CalculateScreenBounds();
        UpdateSpawnBoundaries();

        moonSettings.spawnChance = 0.4f;
        planetSettings.spawnChance = 0.3f;
        sunSettings.spawnChance = 0.15f;
        blackHoleSettings.spawnChance = 0.05f;
        
        sunSettings.requiresExclusiveLayer = true;
        blackHoleSettings.requiresExclusiveLayer = true;
        
        highestGeneratedY = initialLayerY;
        spawnedPositions.Clear();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            UpdateScore();
            CheckAndGenerateNewLayers();
            UpdateDifficulty();
        }
    }

    public void GameOver(string reason)
    {
        if (isGameOver) return;
        
        isGameOver = true;
                if (audioSource != null && gameOverSound != null)
        {
            audioSource.volume = gameOverVolume;
            audioSource.PlayOneShot(gameOverSound);
        }

        if (supernova != null)
            supernova.PauseMovement();

        if (rocket != null)
            rocket.enabled = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetFloat(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {Mathf.FloorToInt(score):N0}";
            if (highScoreText != null)
                highScoreText.text = $"High Score: {Mathf.FloorToInt(highScore):N0}";
        }

        if (scoreText != null)
            scoreText.text = $"GAME OVER - {reason}";
    }

    private void HandlePlayerDeath()
    {
        GameOver("Health Depleted");
    }

    public void HandleSupernovaCollision()
    {
        GameOver("Caught by Supernova");
    }

    public void HandleCelestialCollision(string objectName)
    {
        GameOver($"Crashed into {objectName}");
    }

    private void RestartGame()
    {
        isGameOver = false;
        score = 0;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        UpdateScoreUI();
        
        if (rocket != null)
        {
            rocket.ResetRocket();
            rocket.enabled = true;
        }
        
        if (playerHealth != null)
            playerHealth.ResetHealth();
        
        if (supernova != null)
        {
            supernova.ResetPosition();
            supernova.ResumeMovement();
        }
        
        ClearAllCelestialBodies();
        GenerateInitialLayers();
    }
    private void ClearAllCelestialBodies()
    {
        GameObject[] celestialBodies = GameObject.FindGameObjectsWithTag("CelestialBody");
        foreach (GameObject obj in celestialBodies)
        {
            Destroy(obj);
        }
        
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            Destroy(asteroid);
        }
        
        spawnedPositions.Clear();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(score):N0}";
        }
    }

    private void GenerateInitialLayers()
    {
        for (int i = 0; i < maxActiveLayers; i++)
        {
            GenerateLayer(initialLayerY + (i * layerHeight));
        }
    }

    private void GenerateLayer(float yPosition)
    {
        spawnedPositions.Clear();

        bool canSpawnDangerous = (yPosition - lastDangerousObjectY) >= minDangerousObjectSpacing * layerHeight;
        
        if (Random.value < baseAsteroidSpawnRate * currentDifficulty)
        {
            SpawnAsteroids(yPosition);
        }

        if (canSpawnDangerous)
        {
            float spawnRoll = Random.value;
            float cumulativeChance = 0f;

            cumulativeChance += blackHoleSettings.spawnChance;
            if (spawnRoll < cumulativeChance)
            {
                SpawnCelestialBody(blackHoleSettings, yPosition);
                lastDangerousObjectY = yPosition;
                return;
            }

            cumulativeChance += sunSettings.spawnChance;
            if (spawnRoll < cumulativeChance)
            {
                SpawnCelestialBody(sunSettings, yPosition);
                lastDangerousObjectY = yPosition;
                return;
            }
        }

        int maxBodiesPerLayer = Mathf.FloorToInt(2 + currentDifficulty);
        int bodiesInLayer = 0;

        if (Random.value < moonSettings.spawnChance && bodiesInLayer < maxBodiesPerLayer)
        {
            SpawnCelestialBody(moonSettings, yPosition);
            bodiesInLayer++;
        }

        if (Random.value < planetSettings.spawnChance && bodiesInLayer < maxBodiesPerLayer)
        {
            SpawnCelestialBody(planetSettings, yPosition, true);
            bodiesInLayer++;
        }

        if (bodiesInLayer == 0 && Random.value < 0.5f)
        {
            SpawnCelestialBody(planetSettings, yPosition);
        }

        highestGeneratedY = yPosition;
    }

    private void SpawnCelestialBody(SpawnSettings settings, float yPosition, bool offsetFromExisting = false)
    {
        GameObject prefabToSpawn = settings.GetRandomPrefab();
        if (prefabToSpawn == null) return;

        for (int attempts = 0; attempts < 10; attempts++)
        {
            float xPos;
            if (offsetFromExisting && spawnedPositions.Count > 0)
            {
                xPos = Random.value < 0.5f ? 
                    Random.Range(settings.minHorizontalSpawn, -settings.minHorizontalGap) :
                    Random.Range(settings.minHorizontalGap, settings.maxHorizontalSpawn);
            }
            else
            {
                xPos = Random.Range(settings.minHorizontalSpawn, settings.maxHorizontalSpawn);
            }

            Vector2 spawnPos = new Vector2(xPos, yPosition);

            if (IsValidSpawnPosition(spawnPos, settings.minHorizontalGap))
            {
                GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                obj.tag = "CelestialBody";
                spawnedPositions.Add(spawnPos);
                break;
            }
        }
    }

    private void SpawnAsteroids(float yPosition)
    {
        int asteroidCount = Random.Range(3, 7);
        float spawnWidth = screenMaxX - screenMinX - 2f;
        float sectorWidth = spawnWidth / asteroidCount;
        
        for (int i = 0; i < asteroidCount; i++)
        {
            GameObject asteroidPrefab = asteroidSettings.GetRandomPrefab();
            if (asteroidPrefab == null) continue;

            float sectorStart = screenMinX + 1f + (i * sectorWidth);
            float xPos = Random.Range(sectorStart, sectorStart + sectorWidth);
            Vector3 spawnPos = new Vector3(xPos, yPosition + Random.Range(-1f, 1f), 0);
            
            if (IsValidSpawnPosition(spawnPos, asteroidSettings.minHorizontalGap))
            {
                GameObject asteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
                asteroid.tag = "Asteroid";
                spawnedPositions.Add(spawnPos);
            }
        }
    }

    private void CheckAndGenerateNewLayers()
    {
        float cameraTopY = mainCamera.transform.position.y + mainCamera.orthographicSize;
        while (highestGeneratedY < cameraTopY + (layerHeight * 2))
        {
            GenerateLayer(highestGeneratedY + layerHeight);
        }
    }

    private void UpdateScore()
    {
        if (player != null)
        {
            float newScore = Mathf.Max(score, player.position.y * scoreMultiplier);
            if (newScore > score)
            {
                score = newScore;
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {Mathf.FloorToInt(score):N0}";
                }
            }
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null)
        {
            scoreText.text = $"Score: {Mathf.FloorToInt(score):N0}";
        }
    }

    private void UpdateDifficulty()
    {
        float targetDifficulty = 1f + (score / 1000f) * difficultyIncreaseRate;
        currentDifficulty = Mathf.Min(targetDifficulty, maxDifficultyMultiplier);
        
        UpdateSpawnRatesWithDifficulty();
    }

    private void UpdateSpawnRatesWithDifficulty()
    {
        sunSettings.spawnChance = Mathf.Min(0.15f * currentDifficulty, 0.25f);
        blackHoleSettings.spawnChance = Mathf.Min(0.05f * currentDifficulty, 0.1f);
        
        float difficultySpacingMultiplier = Mathf.Max(0.7f, 1f - ((currentDifficulty - 1f) * 0.15f));
        planetSettings.minHorizontalGap *= difficultySpacingMultiplier;
        moonSettings.minHorizontalGap *= difficultySpacingMultiplier;
        asteroidSettings.minHorizontalGap *= difficultySpacingMultiplier;
    }

    private void CleanupObjects()
    {
        float cleanupY = mainCamera.transform.position.y - mainCamera.orthographicSize * 2;
        
        GameObject[] celestialBodies = GameObject.FindGameObjectsWithTag("CelestialBody");
        foreach (GameObject obj in celestialBodies)
        {
            if (obj.transform.position.y < cleanupY)
            {
                Destroy(obj);
            }
        }
        
        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");
        foreach (GameObject asteroid in asteroids)
        {
            if (asteroid.transform.position.y < cleanupY)
            {
                Destroy(asteroid);
            }
        }
        
        spawnedPositions.RemoveAll(pos => pos.y < cleanupY);
    }

    private void CalculateScreenBounds()
    {
        float verticalExtent = mainCamera.orthographicSize;
        float horizontalExtent = verticalExtent * mainCamera.aspect;
        
        screenMinX = -horizontalExtent;
        screenMaxX = horizontalExtent;
    }

    private void UpdateSpawnBoundaries()
    {
        float spawnBuffer = 1f;
        SpawnSettings[] allSettings = {
            planetSettings,
            moonSettings,
            sunSettings,
            blackHoleSettings,
            asteroidSettings
        };

        foreach (var settings in allSettings)
        {
            settings.minHorizontalSpawn = screenMinX + spawnBuffer;
            settings.maxHorizontalSpawn = screenMaxX - spawnBuffer;
        }
    }

    private bool IsValidSpawnPosition(Vector2 position, float minGap)
    {
        foreach (Vector2 existingPos in spawnedPositions)
        {
            if (Vector2.Distance(position, existingPos) < minGap)
            {
                return false;
            }
        }
        return true;
    }
}
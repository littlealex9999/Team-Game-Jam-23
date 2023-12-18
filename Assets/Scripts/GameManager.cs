using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Player player;

    [Header("Scoring")]
    public int scoreOnKill = 20;
    public int scoreOnBullet = 10;
    public int scoreOnHeadshot = 50;

    [Header("UI")]
    public Image healthUI;
    public Image ammoUI;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI ammoHeldText;
    public Image hurtSplatter;
    public AnimationCurve hurtSplatterCurve;

    [Space]
    public float gameOverFadeDuration = 3.0f;
    public Image fadeToDeathScreen;
    public GameObject gameOverScreen;

    [Header("Spawning")]
    public Enemy enemyPrefab;
    public Transform enemySpawnFocus;
    public float enemySpawnDistanceMin;
    public float enemySpawnDistanceMax;
    [Space]
    public float enemySpawnIntervalMin = 1.0f;
    public float enemySpawnIntervalMax = 15.0f;
    public float enemySpawnIntervalTimeToMax = 600.0f;
    public AnimationCurve enemySpawnIntervalCurve;
    [Space]
    public float enemySpawnCount = 2.0f;
    public float enemySpawnCountRange = 1;
    public float enemySpawnCountPerMinute = 1.0f;
    public float enemySpawnCountRangePerMinute = 0.5f;
    float enemySpawnCountBase;
    float enemySpawnCountRangeBase;
    float enemySpawnCountPerMinuteBase;
    float enemySpawnCountRangePerMinuteBase;
    float enemySpawnTimer;

    List<Enemy> enemies = new List<Enemy>();

    float elapsedGameTime = 0;
    bool gameOver = false;
    bool gamePaused = false;
    public bool gameStopped { get { return gameOver || gamePaused; } }

    void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;

        enemySpawnCountBase = enemySpawnCount;
        enemySpawnCountRangeBase = enemySpawnCountRange;
        enemySpawnCountPerMinuteBase = enemySpawnCountPerMinute;
        enemySpawnCountRangePerMinuteBase = enemySpawnCountRangePerMinute;
        enemySpawnTimer = enemySpawnIntervalMax;
    }

    void Update()
    {
        UpdateUI();

        if (!gameStopped) {
            elapsedGameTime += Time.deltaTime;
            UpdateEnemies();
        }
    }

    void UpdateUI()
    {
        if (player != null) {
            if (healthUI) healthUI.fillAmount = player.health / player.maxHealth;
            if (ammoUI) ammoUI.fillAmount = player.currentAmmoClip / player.maxAmmoClip;
            if (ammoText) ammoText.text = player.currentAmmoClip + " / " + player.maxAmmoClip;
            if (ammoHeldText) ammoHeldText.text = player.currentAmmoHeld.ToString();

            if (hurtSplatter) {
                Color c = hurtSplatter.color;
                c.a = hurtSplatterCurve.Evaluate(1 - player.health / player.maxHealth);
                hurtSplatter.color = c;
            }
        }
    }

    void UpdateEnemies()
    {
        enemySpawnTimer -= Time.deltaTime;
        enemySpawnCount += enemySpawnCountPerMinute * Time.deltaTime / 60.0f;
        enemySpawnCountRange += enemySpawnCountRangePerMinute * Time.deltaTime / 60.0f;

        if (enemySpawnTimer <= 0) {
            int numEnemiesSpawned = (int)(enemySpawnCount + Random.Range(-enemySpawnCountRange, enemySpawnCountRange));
            for (int i = 0; i < numEnemiesSpawned; i++) {
                SpawnEnemy();
            }

            enemySpawnTimer = Mathf.Lerp(enemySpawnIntervalMax, enemySpawnIntervalMin, enemySpawnIntervalCurve.Evaluate(Mathf.Clamp(elapsedGameTime / enemySpawnIntervalTimeToMax, 0, 1)));
        }
    }

    public void RestartGame()
    {
        elapsedGameTime = 0;
        enemySpawnCount = enemySpawnCountBase;
        enemySpawnCountRange = enemySpawnCountRangeBase;
        enemySpawnCountPerMinute = enemySpawnCountPerMinuteBase;
        enemySpawnCountRangePerMinute = enemySpawnCountRangePerMinuteBase;
        enemySpawnTimer = enemySpawnIntervalMax;

        for (int i = 0; i < enemies.Count; i++) {
            if (enemies[i] != null) {
                Destroy(enemies[i]);
            }
        }

        enemies.Clear();

        gameOverScreen.SetActive(false);
    }

    public void GameOver()
    {
#if UNITY_EDITOR
        Debug.Log("Game Over");
#endif

        gameOver = true;
        StartCoroutine(GameOverFade(gameOverFadeDuration));
    }

    public void PauseGame(bool pauseState)
    {
        if (gamePaused == pauseState) return;

        gamePaused = pauseState;
        if (pauseState) {

        } else {

        }
    }

    public void AddScore(int score)
    {
        player.score += score;
    }

    void SpawnEnemy()
    {
        Vector2 spawnDir = Random.insideUnitCircle * Random.Range(enemySpawnDistanceMin, enemySpawnDistanceMax);
        Vector3 spawnPos;
        spawnPos.x = enemySpawnFocus.position.x + spawnDir.x;
        spawnPos.y = enemySpawnFocus.position.y;
        spawnPos.z = enemySpawnFocus.position.z + spawnDir.y;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    IEnumerator GameOverFade(float duration)
    {
        fadeToDeathScreen.gameObject.SetActive(true);

        float timer = 0;
        while ((timer += Time.deltaTime) < duration) {
            float completion = timer / duration;
            Color c = fadeToDeathScreen.color;
            c.a = completion;

            fadeToDeathScreen.color = c;
            yield return new WaitForEndOfFrame();
        }

        gameOverScreen.SetActive(true);
        fadeToDeathScreen.gameObject.SetActive(false);

        yield break;
    }
}

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
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI healthMaxText;

    [Space]
    public Image ammoUI;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI ammoMaxText;
    public TextMeshProUGUI ammoHeldText;

    [Space]
    public UFOFollow ufoFollow;
    public Image timerImage;
    public TextMeshProUGUI timerText;

    [Space]
    public TextMeshProUGUI scoreText;

    [Space]
    public Image interactImage;
    public TextMeshProUGUI interactText;

    [Space]
    public Image hitmarkerImage;
    public float hitmarkerStayTime = 0.1f;
    Coroutine hitmarkerCoroutine;

    [Space]
    public Image hurtSplatter;
    public AnimationCurve hurtSplatterCurve;

    [Space]
    public float gameOverFadeDuration = 3.0f;
    public Image fadeToDeathScreen;
    public GameObject gameOverScreen;
    public GameObject mainMenu;
    public GameObject pauseMenu;

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
    float enemySpawnTimerMax;

    List<Enemy> enemies = new List<Enemy>();

    float elapsedGameTime = 0;
    public bool gameOver { get; private set; } = true;
    public bool gamePaused { get; private set; } = false;
    public bool gameStopped { get { return gameOver || gamePaused; } }

    enum MenuState
    {
        MAIN,
        PAUSE,
        DEATH,
        GAME,
    }

    void Awake()
    {
        if (instance != null) Destroy(this);
        else instance = this;

        enemySpawnCountBase = enemySpawnCount;
        enemySpawnCountRangeBase = enemySpawnCountRange;
        enemySpawnCountPerMinuteBase = enemySpawnCountPerMinute;
        enemySpawnCountRangePerMinuteBase = enemySpawnCountRangePerMinute;
        enemySpawnTimer = enemySpawnIntervalMax;
        enemySpawnTimerMax = enemySpawnTimer;

        gameOver = true;
        ChangeMenuState(MenuState.MAIN);
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
            if (healthText) healthText.text = player.health.ToString("0");
            if (healthMaxText) healthMaxText.text = player.maxHealth.ToString("0");
            if (ammoUI) ammoUI.fillAmount = (float)player.currentAmmoClip / (float)player.maxAmmoClip;
            if (ammoText) ammoText.text = player.currentAmmoClip.ToString();
            if (ammoMaxText) ammoMaxText.text = player.maxAmmoClip.ToString();
            if (ammoHeldText) ammoHeldText.text = player.currentAmmoHeld.ToString();
            if (timerImage && ufoFollow) timerImage.fillAmount = ufoFollow.suckTime / ufoFollow.timeToSuckPlayer;
            if (timerText) timerText.text = elapsedGameTime.ToString("0");
            if (scoreText) scoreText.text = player.score.ToString();

            if (hurtSplatter) {
                Color c = hurtSplatter.color;
                c.a = hurtSplatterCurve.Evaluate(1 - player.health / player.maxHealth);
                hurtSplatter.color = c;
            }
        }
    }

    public void UpdateInteractUI(Interactable interactable)
    {
        if (interactable == null || !interactable.CanInteract(player)) {
            interactText.text = "";
            interactImage.fillAmount = 0;
            return;
        }

        switch (interactable.restorationType) {
            case Interactable.RestorationType.HEALTH:
                interactText.text = "Hold \"E\" to gain " + interactable.amountToRestore + " health for " + interactable.cost + " points";
                break;
            case Interactable.RestorationType.AMMO:
                interactText.text = "Hold \"E\" to gain " + interactable.amountToRestore + " ammo for " + interactable.cost + " points";
                break;
            case Interactable.RestorationType.BOARD:
                interactText.text = "Hold \"E\" to rebuild a board for " + interactable.cost + " points";
                break;
        }

        interactImage.fillAmount = 0;
    }

    public void UpdateInteractUI(Interactable interactable, float duration, float currentTime)
    {
        UpdateInteractUI(interactable);

        interactImage.fillAmount = currentTime / duration;
    }

    public void FlashHitmarker()
    {
        if (hitmarkerCoroutine != null) {
            StopCoroutine(hitmarkerCoroutine);
        }

        hitmarkerCoroutine = StartCoroutine(HitmarkerFlash(hitmarkerStayTime));
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
            enemySpawnTimerMax = enemySpawnTimer;
        }
    }

    void ChangeMenuState(MenuState state)
    {
        fadeToDeathScreen.gameObject.SetActive(false);
        gameOverScreen.SetActive(false);

        mainMenu.SetActive(false);
        pauseMenu.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        switch (state) {
            case MenuState.MAIN:
                mainMenu.SetActive(true);
                break;
            case MenuState.PAUSE:
                pauseMenu.SetActive(true);
                break;
            case MenuState.DEATH:
                Color c = fadeToDeathScreen.color;
                c.a = 1.0f;
                fadeToDeathScreen.color = c;

                fadeToDeathScreen.gameObject.SetActive(true);
                gameOverScreen.SetActive(true);
                break;
            case MenuState.GAME:
            default:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
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
                Destroy(enemies[i].gameObject);
            }
        }

        enemies.Clear();

        ChangeMenuState(MenuState.GAME);

        player.Restart();
        gameOver = false;
    }

    public void GameOver()
    {
#if UNITY_EDITOR
        Debug.Log("Game Over");
#endif
        if (gameOver) return;

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

        enemies.Add(Instantiate(enemyPrefab, spawnPos, Quaternion.identity));
    }

    IEnumerator GameOverFade(float duration)
    {
        fadeToDeathScreen.gameObject.SetActive(true);

        float timer = 0;
        while ((timer += Time.deltaTime) < duration) {
            float completion = timer / duration;
            Color col = fadeToDeathScreen.color;
            col.a = completion;

            fadeToDeathScreen.color = col;
            yield return new WaitForEndOfFrame();
        }

        ChangeMenuState(MenuState.DEATH);

        yield break;
    }

    IEnumerator HitmarkerFlash(float duration)
    {
        Color c = hitmarkerImage.color;
        c.a = 1;
        hitmarkerImage.color = c;

        float timer = duration;
        while ((timer -= Time.deltaTime) > 0) {
            c.a = timer / duration;
            hitmarkerImage.color = c;

            yield return new WaitForEndOfFrame();
        }

        c.a = 0;
        hitmarkerImage.color = c;

        yield break;
    }
}

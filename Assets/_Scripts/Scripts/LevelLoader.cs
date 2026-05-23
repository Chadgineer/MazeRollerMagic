using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class LevelLoader : MonoBehaviour
{
    [Header("Grid Ayarları")]
    [SerializeField] private float cellSize = 1f;

    [Header("Blok Prefabları")]
    [SerializeField] private GameObject[] blockPrefabs;

    [Header("Görsel Efekt Ayarları (DOTween)")]
    [SerializeField] private float objectSpawnDuration = 0.4f;
    [SerializeField] private float minRandomScaleRange = 0.0f;
    [SerializeField] private float maxRandomScaleRange = 0.4f;

    [Header("Oyuncu Ayarları")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float playerSpawnDuration = 0.5f;

    [Header("Mobil Arayüz Referansları")]
    [SerializeField] private Button jumpButton;

    [Header("Işınlanma (Respawn) Ayarı")]
    [SerializeField] private string restartTag = "FallZone";

    [Header("Bölüm Bitirme (Finish) Ayarı")]
    [SerializeField] private string finishTag = "Finish";
    [SerializeField] private string menuSceneName = "MainMenuScene";

    [Header("Zıplama Küresi (Jump Orb) Ayarı")]
    [SerializeField] private string jumpOrbTag = "JumpOrb";

    private Vector3 calculatedSpawnPosition;

    void Start()
    {
        if (jumpButton != null)
        {
            jumpButton.gameObject.SetActive(false);
        }

        if (PlayerPrefs.HasKey("SelectedLevelToLoad"))
        {
            string levelName = PlayerPrefs.GetString("SelectedLevelToLoad");
            LoadAndGenerateLevel(levelName);
            PlayerPrefs.DeleteKey("SelectedLevelToLoad");
        }
        else
        {
            Debug.LogWarning("Menüden herhangi bir bölüm seçilmedi! Test için varsayılan harita yüklenebilir.");
        }
    }

    void LoadAndGenerateLevel(string levelName)
    {
        string path = Path.Combine(Application.persistentDataPath, levelName + ".json");

        if (!File.Exists(path))
        {
            TextAsset levelAsset = Resources.Load<TextAsset>("Levels/" + levelName);
            if (levelAsset != null)
            {
                GenerateLevelFromJson(levelAsset.text);
            }
            else
            {
                Debug.LogError($"Böyle bir level dosyası bulunamadı! {levelName}");
            }
            return;
        }

        string json = File.ReadAllText(path);
        GenerateLevelFromJson(json);
    }

    void GenerateLevelFromJson(string json)
    {
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);
        Vector3 spawnPlatformWorldPos = Vector3.zero;
        bool isSpawnPlatformFound = false;

        foreach (BlockData block in levelData.blocks)
        {
            GameObject prefabToSpawn = FindPrefabByName(block.prefabName);

            if (prefabToSpawn != null)
            {
                BlockProperty prefabProp = prefabToSpawn.GetComponent<BlockProperty>();
                bool isPivotAtBottom = prefabProp != null ? prefabProp.pivotAtBottom : false;
                float yOffset = isPivotAtBottom ? -(cellSize * 0.5f) : 0f;

                Vector3 worldPos = new Vector3(
                    (block.x * cellSize) + (cellSize * 0.5f),
                    (block.y * cellSize) + (cellSize * 0.5f) + yOffset,
                    (block.z * cellSize) + (cellSize * 0.5f)
                );

                GameObject spawnedBlock = Instantiate(prefabToSpawn, worldPos, prefabToSpawn.transform.rotation, transform);

                if (!spawnedBlock.GetComponent<Collider>())
                {
                    spawnedBlock.AddComponent<BoxCollider>();
                }

                if (block.prefabName == "MazeJumpOrb" || spawnedBlock.CompareTag(jumpOrbTag))
                {
                    if (!spawnedBlock.GetComponent<JumpOrb>())
                    {
                        spawnedBlock.AddComponent<JumpOrb>();
                    }
                }

                Vector3 originalObjectScale = prefabToSpawn.transform.localScale;
                float startScaleMultiplier = Random.Range(minRandomScaleRange, maxRandomScaleRange);
                spawnedBlock.transform.localScale = originalObjectScale * startScaleMultiplier;

                spawnedBlock.transform.DOScale(originalObjectScale, objectSpawnDuration).SetEase(Ease.OutBack);

                if (block.prefabName == "MazeSpawn")
                {
                    spawnPlatformWorldPos = worldPos;
                    isSpawnPlatformFound = true;
                }
            }
        }

        if (isSpawnPlatformFound)
        {
            calculatedSpawnPosition = spawnPlatformWorldPos + new Vector3(0f, cellSize * 0.5f, 0f);
            SpawnPlayerWithEffect(calculatedSpawnPosition);
        }
    }

    private void SpawnPlayerWithEffect(Vector3 spawnPos)
    {
        if (playerPrefab == null) return;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        BallMovement movementScript = playerInstance.GetComponent<BallMovement>();

        Joystick sceneJoystick = FindFirstObjectByType<Joystick>();
        if (sceneJoystick != null && movementScript != null)
        {
            movementScript.InitJoystick(sceneJoystick);
        }

        if (jumpButton != null && movementScript != null)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(() => movementScript.ExecuteJump());
            movementScript.InitJumpButton(jumpButton);
        }

        Vector3 originalScale = playerPrefab.transform.localScale;
        playerInstance.transform.localScale = Vector3.zero;
        playerInstance.transform.DOScale(originalScale, playerSpawnDuration).SetEase(Ease.OutBack);

        PlayerCollisionDetector detector = playerInstance.AddComponent<PlayerCollisionDetector>();
        detector.Setup(restartTag, calculatedSpawnPosition, originalScale, playerSpawnDuration, finishTag, menuSceneName, jumpOrbTag, jumpButton);
    }

    private GameObject FindPrefabByName(string name)
    {
        foreach (var prefab in blockPrefabs)
        {
            if (prefab != null && prefab.name == name) return prefab;
        }
        return null;
    }
}

public class PlayerCollisionDetector : MonoBehaviour
{
    private string targetRestartTag;
    private Vector3 respawnPoint;
    private Vector3 originalScale;
    private float tweenDuration;

    private string targetFinishTag;
    private string menuScene;
    private string targetJumpOrbTag;
    private Button dynamicJumpButton;

    private Rigidbody rb;
    private BallMovement ballMovement;
    private bool isLevelChanging = false;

    public void Setup(string restartTag, Vector3 spawnPosition, Vector3 targetScale, float duration, string finishTag, string menuSceneName, string jumpOrbTag, Button uiButton)
    {
        targetRestartTag = restartTag;
        respawnPoint = spawnPosition;
        originalScale = targetScale;
        tweenDuration = duration;
        targetFinishTag = finishTag;
        menuScene = menuSceneName;
        targetJumpOrbTag = jumpOrbTag;
        dynamicJumpButton = uiButton;

        rb = GetComponent<Rigidbody>();
        ballMovement = GetComponent<BallMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollisions(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollisions(collision.gameObject);
    }

    private void HandleCollisions(GameObject hitObject)
    {
        if (isLevelChanging) return;

        if (hitObject.CompareTag(targetRestartTag))
        {
            TeleportToSpawn();
        }
        else if (hitObject.CompareTag(targetFinishTag))
        {
            FinishLevelAndGoToMenu();
        }
        else if (hitObject.CompareTag(targetJumpOrbTag))
        {
            JumpOrb orb = hitObject.GetComponentInParent<JumpOrb>();
            if (orb != null)
            {
                orb.CollectOrb();
                if (dynamicJumpButton != null)
                {
                    dynamicJumpButton.gameObject.SetActive(true);
                }
                if (ballMovement != null)
                {
                    ballMovement.GiveJumpReward();
                }
            }
        }
    }

    private void TeleportToSpawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (ballMovement != null) ballMovement.ResetJumpReward();

        if (dynamicJumpButton != null)
        {
            dynamicJumpButton.gameObject.SetActive(false);
        }

        // HATA BURADAYDI: Kapalı (deaktif) olan küreleri de listeye dahil etmesini söyledik
        JumpOrb[] allOrbs = FindObjectsByType<JumpOrb>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (JumpOrb orb in allOrbs)
        {
            orb.ResetOrb();
        }

        transform.DOKill();
        transform.position = respawnPoint;
        transform.localScale = Vector3.zero;
        transform.DOScale(originalScale, tweenDuration).SetEase(Ease.OutBack);
    }

    private void FinishLevelAndGoToMenu()
    {
        isLevelChanging = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.DOKill();
        transform.DOScale(Vector3.zero, tweenDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() => SceneManager.LoadScene(menuScene));
    }
}
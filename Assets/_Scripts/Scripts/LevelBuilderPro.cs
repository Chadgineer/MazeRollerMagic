using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.EventSystems;

public class LevelBuilderPro : MonoBehaviour
{
    public enum EditorMode { Place, Delete }

    [Header("Grid Ayarları")]
    [SerializeField] private float cellSize = 1f;

    [Header("Blok Prefabları")]
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Material ghostMaterial;

    [Header("Dosya Ayarları")]
    [SerializeField] private string levelName = "Level_1";

    private int currentBlockIndex = 0;
    private EditorMode currentMode = EditorMode.Place;
    private Dictionary<Vector3Int, GameObject> gridData = new Dictionary<Vector3Int, GameObject>();

    private GameObject ghostObject;
    private MeshFilter ghostMeshFilter;
    private MeshRenderer ghostMeshRenderer;

    void Start()
    {
        CreateGhostObject();
        if (PlayerPrefs.HasKey("SelectedLevelToLoad"))
        {
            levelName = PlayerPrefs.GetString("SelectedLevelToLoad");

            LoadLevel();

            if (ghostObject != null) Destroy(ghostObject);

            PlayerPrefs.DeleteKey("SelectedLevelToLoad");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentMode = (currentMode == EditorMode.Place) ? EditorMode.Delete : EditorMode.Place;
            UpdateGhostVisual();
            Debug.Log($"<color=magenta>Mod Değişti:</color> {currentMode}");
        }

        if (currentMode == EditorMode.Place)
        {
            HandlePrefabSelection();
        }
        UpdateGhostPositionAndAction();
    }

    void CreateGhostObject()
    {
        ghostObject = new GameObject("Grid_Ghost_Preview");
        ghostMeshFilter = ghostObject.AddComponent<MeshFilter>();
        ghostMeshRenderer = ghostObject.AddComponent<MeshRenderer>();
        ghostMeshRenderer.material = ghostMaterial;
        UpdateGhostVisual();
    }

    void UpdateGhostVisual()
    {
        if (ghostObject == null) return;

        if (currentMode == EditorMode.Delete)
        {
            ghostObject.SetActive(true);
            ghostMeshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);
            ghostMeshRenderer.material.color = new Color(1f, 0f, 0f, 0.4f);
        }
        else
        {
            if (blockPrefabs != null && blockPrefabs.Length > currentBlockIndex)
            {
                ghostObject.SetActive(true);
                MeshFilter prefabMesh = blockPrefabs[currentBlockIndex].GetComponentInChildren<MeshFilter>();
                ghostMeshFilter.sharedMesh = prefabMesh ? prefabMesh.sharedMesh : GetPrimitiveMesh(PrimitiveType.Cube);
                ghostMeshRenderer.material.color = new Color(0f, 1f, 0f, 0.4f);
            }
            else
            {
                ghostObject.SetActive(false);
            }
        }
    }

    void UpdateGhostPositionAndAction()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (ghostObject != null) ghostObject.SetActive(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetWorldPos = Vector3.zero;
        Vector3Int targetCellCoords = Vector3Int.zero;
        bool isPlacementAllowed = true;
        bool hasValidHit = false;

        BlockProperty currentPrefabProp = GetBlockPropertyFromPrefab(blockPrefabs[currentBlockIndex]);
        BlockType currentPlacingType = currentPrefabProp != null ? currentPrefabProp.blockType : BlockType.StandardBlock;
        bool currentPivotAtBottom = currentPrefabProp != null ? currentPrefabProp.pivotAtBottom : false;

        // Özel tipleri de üst üste bir şey koyulamayan genel gruba dahil ediyoruz
        bool isCurrentNonStackable = currentPlacingType == BlockType.NonStackableObject ||
                                     currentPlacingType == BlockType.SpawnPlatform ||
                                     currentPlacingType == BlockType.FinishPlatform;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject != ghostObject)
            {
                hasValidHit = true;

                if (currentMode == EditorMode.Place)
                {
                    BlockProperty hitBlockProperty = hit.collider.GetComponentInParent<BlockProperty>();
                    if (hitBlockProperty != null && (hitBlockProperty.blockType == BlockType.NonStackableObject ||
                                                     hitBlockProperty.blockType == BlockType.SpawnPlatform ||
                                                     hitBlockProperty.blockType == BlockType.FinishPlatform))
                    {
                        isPlacementAllowed = false;
                    }

                    Vector3 positionWithNormal = hit.point + (hit.normal * (cellSize * 0.5f));
                    targetCellCoords = GetCellCoords(positionWithNormal);

                    if (targetCellCoords.y > 0)
                    {
                        Vector3Int bottomCellCoords = targetCellCoords + Vector3Int.down;
                        if (!gridData.ContainsKey(bottomCellCoords))
                        {
                            if (isCurrentNonStackable)
                            {
                                isPlacementAllowed = false;
                            }
                            else
                            {
                                targetCellCoords.y = 0;
                            }
                        }
                    }
                }
                else
                {
                    Vector3 positionInsideBlock = hit.point - (hit.normal * (cellSize * 0.5f));
                    targetCellCoords = GetCellCoords(positionInsideBlock);
                }
            }
        }
        else
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                hasValidHit = true;
                Vector3 hitPoint = ray.GetPoint(enter);
                targetCellCoords = GetCellCoords(hitPoint);

                if (currentMode == EditorMode.Place)
                {
                    targetCellCoords.y = 0;

                    if (isCurrentNonStackable)
                    {
                        isPlacementAllowed = false;
                    }
                }
            }
        }

        if (hasValidHit && isPlacementAllowed)
        {
            ghostObject.SetActive(true);

            float yOffset = currentPivotAtBottom ? -(cellSize * 0.5f) : 0f;

            targetWorldPos = new Vector3(
                (targetCellCoords.x * cellSize) + (cellSize * 0.5f),
                (targetCellCoords.y * cellSize) + (cellSize * 0.5f) + yOffset,
                (targetCellCoords.z * cellSize) + (cellSize * 0.5f)
            );
            ghostObject.transform.position = targetWorldPos;

            if (Input.GetMouseButtonDown(0))
            {
                if (currentMode == EditorMode.Place)
                {
                    // MAX 1 TANE OLMA KURALI: 
                    // Eğer Spawn veya Finish koyuyorsak, sahnede daha önce koyulmuş olanı temizle.
                    if (currentPlacingType == BlockType.SpawnPlatform || currentPlacingType == BlockType.FinishPlatform)
                    {
                        RemoveExistingSpecialBlock(currentPlacingType);
                    }

                    PlaceBlockAt(targetCellCoords, targetWorldPos);
                }
                else if (currentMode == EditorMode.Delete)
                {
                    RemoveBlockAt(targetCellCoords);
                }
            }
        }
        else
        {
            ghostObject.SetActive(false);
        }
    }

    // Sahnede daha önce yerleştirilmiş olan özel platformu bulup silen yardımcı fonksiyon
    private void RemoveExistingSpecialBlock(BlockType typeToRemove)
    {
        Vector3Int keyToRemove = Vector3Int.zero;
        bool found = false;

        foreach (var pair in gridData)
        {
            if (pair.Value == null) continue;
            BlockProperty prop = pair.Value.GetComponent<BlockProperty>();
            if (prop != null && prop.blockType == typeToRemove)
            {
                keyToRemove = pair.Key;
                found = true;
                break;
            }
        }

        if (found)
        {
            Destroy(gridData[keyToRemove]);
            gridData.Remove(keyToRemove);
            Debug.Log($"Eski {typeToRemove} yenisi yerleştirildiği için otomatik silindi.");
        }
    }

    void PlaceBlockAt(Vector3Int coords, Vector3 worldPos)
    {
        if (gridData.ContainsKey(coords)) return;

        GameObject prefabToSpawn = blockPrefabs[currentBlockIndex];
        GameObject newBlock = Instantiate(prefabToSpawn, worldPos, prefabToSpawn.transform.rotation, transform);

        if (!newBlock.GetComponent<Collider>())
        {
            newBlock.AddComponent<BoxCollider>();
        }

        gridData.Add(coords, newBlock);
    }

    void RemoveBlockAt(Vector3Int coords)
    {
        if (gridData.TryGetValue(coords, out GameObject blockToRemove))
        {
            Destroy(blockToRemove);
            gridData.Remove(coords);
        }
    }

    Vector3Int GetCellCoords(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    void HandlePrefabSelection()
    {
        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentBlockIndex = i;
                UpdateGhostVisual();
            }
        }
    }

    private Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        Destroy(gameObject);
        return mesh;
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, levelName + ".json");
    }

    public void SaveLevel()
    {
        LevelData levelData = new LevelData();

        foreach (var pair in gridData)
        {
            if (pair.Value == null) continue;

            BlockData blockData = new BlockData
            {
                prefabName = pair.Value.name.Replace("(Clone)", "").Trim(),
                x = pair.Key.x,
                y = pair.Key.y,
                z = pair.Key.z
            };
            levelData.blocks.Add(blockData);
        }

        string json = JsonUtility.ToJson(levelData, true);

        // 1. Cihazın yerel klasörüne kaydet (Editör testleri için)
        File.WriteAllText(GetSavePath(), json);

        // 2. Projenin Resources klasörüne kaydet (Build'e dahil olması için)
        // NOT: Klasörün Unity içinde Assets/Resources/Levels adında var olduğundan emin ol!
        string resourcesFolderPath = Path.Combine(Application.dataPath, "Resources/Levels");
        if (Directory.Exists(resourcesFolderPath))
        {
            string resourcesFilePath = Path.Combine(resourcesFolderPath, levelName + ".json");
            File.WriteAllText(resourcesFilePath, json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh(); // Unity editörünün dosyayı anında görmesi için
#endif
        }

        Debug.Log($"<color=green>[BAŞARILI]</color> Level iki yola da kaydedildi!");
    }

    public void LoadLevel()
    {
        string path = GetSavePath();

        if (!File.Exists(path))
        {
            Debug.LogError($"Level dosyası bulunamadı! Yol: {path}");
            return;
        }

        ClearLevel();

        string json = File.ReadAllText(path);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        foreach (BlockData block in levelData.blocks)
        {
            GameObject prefabToSpawn = FindPrefabByName(block.prefabName);

            if (prefabToSpawn != null)
            {
                BlockProperty prefabProp = GetBlockPropertyFromPrefab(prefabToSpawn);
                bool isPivotAtBottom = prefabProp != null ? prefabProp.pivotAtBottom : false;

                float yOffset = isPivotAtBottom ? -(cellSize * 0.5f) : 0f;

                Vector3Int coords = new Vector3Int(block.x, block.y, block.z);
                Vector3 worldPos = new Vector3(
                    (coords.x * cellSize) + (cellSize * 0.5f),
                    (coords.y * cellSize) + (cellSize * 0.5f) + yOffset,
                    (coords.z * cellSize) + (cellSize * 0.5f)
                );

                GameObject newBlock = Instantiate(prefabToSpawn, worldPos, prefabToSpawn.transform.rotation, transform);

                if (!newBlock.GetComponent<Collider>())
                    newBlock.AddComponent<BoxCollider>();

                gridData.Add(coords, newBlock);
            }
            else
            {
                Debug.LogWarning($"'{block.prefabName}' isimli prefab listeden bulunamadı!");
            }
        }
        Debug.Log($"<color=cyan>[BAŞARILI]</color> {levelData.blocks.Count} adet blok yüklendi.");
    }

    public void ClearLevel()
    {
        foreach (var pair in gridData)
        {
            if (pair.Value != null) Destroy(pair.Value);
        }
        gridData.Clear();
    }

    private GameObject FindPrefabByName(string name)
    {
        foreach (var prefab in blockPrefabs)
        {
            if (prefab != null && prefab.name == name) return prefab;
        }
        return null;
    }

    private BlockProperty GetBlockPropertyFromPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            return prefab.GetComponent<BlockProperty>();
        }
        return null;
    }
}
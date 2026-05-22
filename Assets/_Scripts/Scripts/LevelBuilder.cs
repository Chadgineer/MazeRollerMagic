using UnityEngine;
using System.Collections.Generic;

public class LevelBuilder : MonoBehaviour
{
    public enum EditorMode { Place, Delete }

    [Header("Grid Ayarları")]
    [SerializeField] private float cellSize = 1f;

    [Header("Blok Prefabları")]
    [SerializeField] private GameObject[] blockPrefabs;
    [SerializeField] private Material ghostMaterial; // Yukarıda oluşturduğun transparan material

    private int currentBlockIndex = 0;
    private EditorMode currentMode = EditorMode.Place;
    private Dictionary<Vector3Int, GameObject> gridData = new Dictionary<Vector3Int, GameObject>();

    // Önizleme (Hayalet) Objesi İçin Değişkenler
    private GameObject ghostObject;
    private MeshFilter ghostMeshFilter;
    private MeshRenderer ghostMeshRenderer;

    void Start()
    {
        CreateGhostObject();
    }

    void Update()
    {
        // 1. Mod Değiştirme (Sağ tık modlar arası geçiş yapar)
        if (Input.GetMouseButtonDown(1))
        {
            currentMode = (currentMode == EditorMode.Place) ? EditorMode.Delete : EditorMode.Place;
            UpdateGhostVisual();
            Debug.Log($"<color=magenta>Mod Değişti:</color> {currentMode}");
        }

        // 2. Sayılarla Prefab Seçimi (Sadece yerleştirme modundaysa)
        if (currentMode == EditorMode.Place)
        {
            HandlePrefabSelection();
        }

        // 3. Mouse Pozisyonunu Takip Et ve Hayaleti Güncelle
        UpdateGhostPositionAndAction();
    }

    void CreateGhostObject()
    {
        // Sahnede görünmez/yarı saydam duracak tek bir hayalet obje yaratıyoruz
        ghostObject = new GameObject("Grid_Ghost_Preview");
        ghostMeshFilter = ghostObject.AddComponent<MeshFilter>();
        ghostMeshRenderer = ghostObject.AddComponent<MeshRenderer>();
        ghostMeshRenderer.material = ghostMaterial;

        // Hayaletin kendi collider'ı olmamalı, yoksa raycast'i engeller
        UpdateGhostVisual();
    }

    void UpdateGhostVisual()
    {
        if (ghostObject == null) return;

        if (currentMode == EditorMode.Delete)
        {
            // Silme modunda hayalet kırmızı bir küp olur (Boyutu grid kadar)
            ghostObject.SetActive(true);
            ghostMeshFilter.sharedMesh = GetPrimitiveMesh(PrimitiveType.Cube);
            ghostMeshRenderer.material.color = new Color(1f, 0f, 0f, 0.4f); // Yarı saydam kırmızı
        }
        else
        {
            // Yerleştirme modunda seçili prefabın şeklini ve rengini alır
            if (blockPrefabs != null && blockPrefabs.Length > currentBlockIndex)
            {
                ghostObject.SetActive(true);
                MeshFilter prefabMesh = blockPrefabs[currentBlockIndex].GetComponentInChildren<MeshFilter>();
                ghostMeshFilter.sharedMesh = prefabMesh ? prefabMesh.sharedMesh : GetPrimitiveMesh(PrimitiveType.Cube);
                ghostMeshRenderer.material.color = new Color(0f, 1f, 0f, 0.4f); // Yarı saydam yeşil/orijinal
            }
            else
            {
                ghostObject.SetActive(false);
            }
        }
    }

    void UpdateGhostPositionAndAction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetWorldPos = Vector3.zero;
        Vector3Int targetCellCoords = Vector3Int.zero;
        bool hasValidHit = false;

        // 1. Sahnede fiziksel bir objeye (başka bir bloğa) çarpıyor muyuz?
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject != ghostObject)
            {
                hasValidHit = true;

                if (currentMode == EditorMode.Place)
                {
                    // Bir bloğa çarptık, yüzey normaline göre pozisyon al
                    Vector3 positionWithNormal = hit.point + (hit.normal * (cellSize * 0.5f));
                    targetCellCoords = GetCellCoords(positionWithNormal);

                    // KRİTİK KONTROL (Havadaki Boşluğu Engelleme):
                    // Eğer yerleştirmek istediğimiz yer zemin (Y=0) DEĞİLSE, 
                    // mutlaka ama mutlaka tam bir alt hücresinde başka bir blok olmak zorunda!
                    if (targetCellCoords.y > 0)
                    {
                        Vector3Int bottomCellCoords = targetCellCoords + Vector3Int.down;
                        if (!gridData.ContainsKey(bottomCellCoords))
                        {
                            // Altı boşsa, bu bloğu dikey düzlemde en alta (Zemine) zorla çekiyoruz
                            targetCellCoords.y = 0;
                        }
                    }
                }
                else // Delete Modu
                {
                    Vector3 positionInsideBlock = hit.point - (hit.normal * (cellSize * 0.5f));
                    targetCellCoords = GetCellCoords(positionInsideBlock);
                }
            }
        }
        else
        {
            // 2. Sahnede hiçbir obje yoksa doğrudan matematiksel düzleme ($Y=0$) çarpıyoruz
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            if (plane.Raycast(ray, out float enter))
            {
                hasValidHit = true;
                Vector3 hitPoint = ray.GetPoint(enter);
                targetCellCoords = GetCellCoords(hitPoint);

                // Boşluğa bakarken hayaletin havaya uçmasını kesin olarak engellemek için:
                if (currentMode == EditorMode.Place)
                {
                    targetCellCoords.y = 0;
                }
            }
        }

        // Hayaleti taşıma ve tıklama aksiyonları (Eski kodun aynısı)
        if (hasValidHit)
        {
            ghostObject.SetActive(true);
            targetWorldPos = new Vector3(
                (targetCellCoords.x * cellSize) + (cellSize * 0.5f),
                (targetCellCoords.y * cellSize) + (cellSize * 0.5f),
                (targetCellCoords.z * cellSize) + (cellSize * 0.5f)
            );
            ghostObject.transform.position = targetWorldPos;

            if (Input.GetMouseButtonDown(0))
            {
                if (currentMode == EditorMode.Place)
                {
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

    void PlaceBlockAt(Vector3Int coords, Vector3 worldPos)
    {
        if (gridData.ContainsKey(coords)) return; // Hücre doluysa koyma

        GameObject newBlock = Instantiate(blockPrefabs[currentBlockIndex], worldPos, Quaternion.identity, transform);

        // ÖNEMLİ: Yeni bloğun üzerine tıklayıp üste çıkabilmek için Collider'ı olmalı!
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

    // Unity'nin dahili mesh'lerini çekmek için yardımcı fonksiyon
    private Mesh GetPrimitiveMesh(PrimitiveType type)
    {
        GameObject gameObject = GameObject.CreatePrimitive(type);
        Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        Destroy(gameObject);
        return mesh;
    }
}
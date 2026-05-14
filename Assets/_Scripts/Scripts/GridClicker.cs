using UnityEngine;

public class GridClicker : MonoBehaviour
{
    [Header("Grid Ayarları")]
    [SerializeField] private float cellSize = 1f; // Blokların boyutu (genelde 1 birim)
    [SerializeField] private LayerMask groundLayer; // Sadece zemini algılamak için opsiyonel

    void Update()
    {
        // Sol tık kontrolü
        if (Input.GetMouseButtonDown(0))
        {
            IdentifyGridCell();
        }
    }

    void IdentifyGridCell()
    {
        // Kameradan farenin olduğu yere bir ışın (Ray) gönderiyoruz
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            // Çarpışma noktasını alıyoruz
            Vector3 hitPoint = hit.point;

            // Dünya pozisyonunu grid koordinatına çevirme (Matematiksel yuvarlama)
            // FloorToInt kullanıyoruz çünkü 0 ile 1 arası 0. cell, 1 ile 2 arası 1. cell'dir.
            int x = Mathf.FloorToInt(hitPoint.x / cellSize);
            int y = Mathf.FloorToInt(hitPoint.y / cellSize);
            int z = Mathf.FloorToInt(hitPoint.z / cellSize);

            Vector3Int cellCoords = new Vector3Int(x, y, z);

            Debug.Log($"<color=cyan>Hücre Tıklandı:</color> {cellCoords} | <color=yellow>Dünya Pozisyonu:</color> {hitPoint}");
        }
    }
}
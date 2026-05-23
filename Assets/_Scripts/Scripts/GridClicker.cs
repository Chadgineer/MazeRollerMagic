using UnityEngine;

public class GridClicker : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                int x = Mathf.FloorToInt(hitPoint.x / cellSize);
                int y = Mathf.FloorToInt(hitPoint.y / cellSize); 
                int z = Mathf.FloorToInt(hitPoint.z / cellSize);

                Vector3Int cellCoords = new Vector3Int(x, y, z);

                Debug.Log($"<color=green>[Grid Başarılı]</color> Tıklanan Hücre: X: {cellCoords.x}, Z: {cellCoords.z} | Dünya Pozisyonu: {hitPoint}");
            }
        }
    }
}
using UnityEngine;

public class GridClicker : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f; // Her bir bloğun boyutu

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Dünyada görünmez, matematiksel bir düzlem tanımlıyoruz (Y koordinatı 0 olan düzlem)
            // Bu düzlem yukarı (Vector3.up) bakıyor ve merkeze (Vector3.zero) konumlu.
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // 2. Kameradan farenin olduğu yere bir ışın (Ray) çıkarıyoruz
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 3. Bu ışın, yarattığımız hayali düzlemi kesiyor mu diye bakıyoruz
            if (plane.Raycast(ray, out float enter))
            {
                // Işının düzlemi kestiği tam 3D nokta (Dünya pozisyonu)
                Vector3 hitPoint = ray.GetPoint(enter);

                // 4. Bu noktayı grid hücrelerine bölüyoruz
                int x = Mathf.FloorToInt(hitPoint.x / cellSize);
                int y = Mathf.FloorToInt(hitPoint.y / cellSize); // Y genellikle 0 kalır zemin için
                int z = Mathf.FloorToInt(hitPoint.z / cellSize);

                Vector3Int cellCoords = new Vector3Int(x, y, z);

                // Konsola yazdırıyoruz
                Debug.Log($"<color=green>[Grid Başarılı]</color> Tıklanan Hücre: X: {cellCoords.x}, Z: {cellCoords.z} | Dünya Pozisyonu: {hitPoint}");
            }
        }
    }
}
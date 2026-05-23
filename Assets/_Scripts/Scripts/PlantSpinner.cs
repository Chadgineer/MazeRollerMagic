using UnityEngine;

public class PlantSpinner : MonoBehaviour
{
    [Header("Dönüş Ayarları")]
    [Tooltip("Objenin saniyedeki dönüş hızı (derece cinsinden).")]
    public float rotationSpeed = 50f;

    void Update()
    {
        // Sadece Z ekseninde, zamanla senkronize bir şekilde döndürür.
        // Pozitif değerler saat yönünün tersine, negatif değerler saat yönüne döndürür.
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
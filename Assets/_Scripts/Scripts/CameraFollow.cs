using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [Tooltip("Eğer boş bırakılırsa sahnedeki Player otomatik bulunmaya çalışılır.")]
    public Transform target;
    public Vector3 offset;

    [Header("Otomatik Bulma Seçenekleri")]
    [SerializeField] private string playerTag = "Player";

    void Start()
    {
        // Eğer editörden elle bir hedef atanmadıysa otomatik aramaya başla
        if (target == null)
        {
            FindPlayerAutomatically();
        }
    }

    // Kamera takipleri için LateUpdate kullanmak en doğrusudur. 
    // Karakter Update veya FixedUpdate içinde hareket ederken kameranın titremesini engeller.
    void LateUpdate()
    {
        // Hedef hala bulunamadıysa (örneğin harita geç yükleniyorsa) tekrar aramayı dene
        if (target == null)
        {
            FindPlayerAutomatically();
            return;
        }

        // Hedefin pozisyonuna offset'i ekleyip direkt kameraya eşitliyoruz
        transform.position = target.position + offset;
    }

    private void FindPlayerAutomatically()
    {
        // 1. Yol: Sahne içindeki "Player" Tag'ine sahip objeyi ara (En standart ve hızlı yöntem)
        GameObject playerObj = GameObject.FindWithTag(playerTag);

        if (playerObj != null)
        {
            target = playerObj.transform;
            Debug.Log("<color=cyan>[Kamera Takip]</color> Oyuncu (Player Tag) otomatik bulundu ve hedefe eklendi!");
            return;
        }

        // 2. Yol: Eğer Tag ayarlanmadıysa, ismi direkt "Ball" veya "Player" olan objeyi aratabilirsin (Yedek Plan)
        playerObj = GameObject.Find("Ball"); // Prefab adın neyse ona göre güncelleyebilirsin
        if (playerObj == null) playerObj = GameObject.Find("Player");

        if (playerObj != null)
        {
            target = playerObj.transform;
            Debug.Log("<color=cyan>[Kamera Takip]</color> Oyuncu (İsim üzerinden) otomatik bulundu!");
        }
    }
}
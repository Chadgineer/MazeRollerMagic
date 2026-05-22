using UnityEngine;

public class EditorCameraController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float shiftMultiplier = 2f; // Shift'e basınca hızlanma

    [Header("Dönüş (Etrafa Bakma) Ayarları")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -85f; // Aşağı bakma sınırı
    [SerializeField] private float maxPitch = 85f;  // Yukarı bakma sınırı

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Kameranın oyun başındaki mevcut açısını hafızaya alıyoruz
        Vector3 currentRotation = transform.localRotation.eulerAngles;
        rotationX = currentRotation.y;
        rotationY = currentRotation.x;
    }

    void Update()
    {
        // 1. Etrafa Bakma (Orta Mouse Tuşu / Scroll Basılı Tutarken)
        HandleRotation();

        // 2. WASD ile Hareket (Kameranın baktığı yöne göre)
        HandleMovement();
    }

    void HandleRotation()
    {
        // 2 numaralı mouse tuşu Orta Mouse (Scroll tıklaması) demektir
        if (Input.GetMouseButton(2))
        {
            // Mouse imlecini gizle ve ekrana kilitle (daha rahat dönebilmek için)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Mouse hareketlerini alıyoruz
            rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;

            // Kameranın takla atmaması için yukarı/aşağı bakışını sınırlıyoruz
            rotationY = Mathf.Clamp(rotationY, minPitch, maxPitch);

            // Yeni rotasyonu uygula
            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
        else
        {
            // Orta mouse bırakıldığında imleci serbest bırak ki blok yerleştirebil
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        // WASD veya Yön Tuşlarından girdileri al (Girdi yoksa 0 döner)
        float moveX = Input.GetAxisRaw("Horizontal"); // A ve D tuşları
        float moveZ = Input.GetAxisRaw("Vertical");   // W ve S tuşları

        // Kameranın tam olarak baktığı yön vektörlerini alıyoruz
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Kameranın hareket ederken havaya uçmasını veya yerin dibine girmesini engellemek istiyorsan,
        // forward.y ve right.y değerlerini sıfırlayıp yönü zemin düzlemine eşitleyebilirsin.
        // Eğer serbest uçuş (uçak gibi) istiyorsan aşağıdaki 3 satırı silebilirsin:
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Yönleri birleştirerek son hareket vektörünü buluyoruz
        Vector3 direction = (forward * moveZ + right * moveX).normalized;

        // Sol Shift tuşuna basılıyorsa kamerayı hızlandır
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= shiftMultiplier;
        }

        // Kamerayı hareket ettir
        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
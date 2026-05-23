using UnityEngine;

public class EditorCameraController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float shiftMultiplier = 2f; 
    [Header("Dönüş (Etrafa Bakma) Ayarları")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -85f; 
    [SerializeField] private float maxPitch = 85f;  

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        Vector3 currentRotation = transform.localRotation.eulerAngles;
        rotationX = currentRotation.y;
        rotationY = currentRotation.x;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(2))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
            rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;

            rotationY = Mathf.Clamp(rotationY, minPitch, maxPitch);

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveZ = Input.GetAxisRaw("Vertical");  

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * moveZ + right * moveX).normalized;

        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= shiftMultiplier;
        }

        transform.position += direction * currentSpeed * Time.deltaTime;
    }
}
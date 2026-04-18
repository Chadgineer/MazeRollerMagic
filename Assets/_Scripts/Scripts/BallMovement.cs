using UnityEngine;
using UnityEngine.InputSystem; // Kütüphaneyi mutlaka ekleyin

public class BallMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 25f;
    }

    void FixedUpdate()
    {
        // Keyboard.current null kontrolü (giriş cihazı bağlı değilse hata vermesin)
        if (Keyboard.current == null) return;

        // Vektör hesaplama
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.dKey.isPressed) moveX += 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.wKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        if (movement != Vector3.zero)
        {
            rb.AddForce(movement * moveSpeed, ForceMode.Acceleration);
        }
    }
}
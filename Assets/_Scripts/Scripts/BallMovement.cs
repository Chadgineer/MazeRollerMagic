using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BallMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpForce = 8f;
    private Rigidbody rb;

    [Header("Mobil Girdi Ayarları")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private Button jumpButton;

    private bool canJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 25f;

        if (joystick == null)
        {
            joystick = FindFirstObjectByType<Joystick>();
        }
    }

    public void InitJoystick(Joystick targetJoystick)
    {
        joystick = targetJoystick;
    }

    public void InitJumpButton(Button targetButton)
    {
        jumpButton = targetButton;
    }

    public void GiveJumpReward()
    {
        canJump = true;
    }

    public void ResetJumpReward()
    {
        canJump = false;
    }

    void Update()
    {
        if (canJump && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ExecuteJump();
        }
    }

    public void ExecuteJump()
    {
        if (!canJump) return;

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
        }

        canJump = false;

        if (jumpButton != null)
        {
            jumpButton.gameObject.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (joystick != null && (joystick.Horizontal != 0f || joystick.Vertical != 0f))
        {
            moveX = joystick.Horizontal;
            moveZ = joystick.Vertical;
        }
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed) moveX -= 1f;
            if (Keyboard.current.wKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        }

        Vector3 movement = new Vector3(moveX, 0f, moveZ).normalized;

        if (movement != Vector3.zero)
        {
            rb.AddForce(movement * moveSpeed, ForceMode.Acceleration);
        }
    }
}
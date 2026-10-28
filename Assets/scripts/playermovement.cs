using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 moveDirection;
    private bool usingGamepadInput;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        usingGamepadInput = input.sqrMagnitude > 0f;
        moveDirection = usingGamepadInput ? input.normalized : Vector2.zero;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        bool hasKeyboardInput = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ||
            Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ||
            Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ||
            Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;

        if (hasKeyboardInput)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                vertical += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                vertical -= 1f;
            }

            usingGamepadInput = false;
            moveDirection = new Vector2(horizontal, vertical);
            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection = moveDirection.normalized;
            }
        }
        else if (!usingGamepadInput)
        {
            moveDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;
    }
}
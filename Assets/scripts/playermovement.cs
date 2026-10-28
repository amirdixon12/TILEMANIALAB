using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpVelocity = 10f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private LayerMask ladderLayers;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer playerSprite;

    private Vector2 moveDirection;
    private bool usingGamepadInput;
    private bool isSprinting;
    private Animator myanimator;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();
    private readonly HashSet<Collider2D> ladderColliders = new HashSet<Collider2D>();
    private float defaultGravityScale;
    private bool isGrounded;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        myanimator = GetComponent<Animator>();
        if (myanimator == null)
        {
            myanimator = GetComponentInChildren<Animator>();
        }

        if (playerSprite == null)
        {
            playerSprite = GetComponentInChildren<SpriteRenderer>();
        }

        defaultGravityScale = rb.gravityScale;
    }

    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        usingGamepadInput = input.sqrMagnitude > 0f;
        moveDirection = usingGamepadInput ? input.normalized : Vector2.zero;
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed || (!isGrounded && ladderColliders.Count == 0))
        {
            return;
        }

        if (ladderColliders.Count > 0)
        {
            ladderColliders.Clear();
            rb.gravityScale = defaultGravityScale;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        groundColliders.Clear();
        isGrounded = false;
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
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

        if (playerSprite != null && Mathf.Abs(moveDirection.x) > 0.01f)
        {
            playerSprite.flipX = moveDirection.x < 0f;
        }

        if (myanimator != null)
        {
            bool isClimbing = ladderColliders.Count > 0 && Mathf.Abs(moveDirection.y) > 0.01f;
            myanimator.SetBool("isCimbing", isClimbing);
            myanimator.SetBool("isRunning", ladderColliders.Count == 0 &&
                Mathf.Abs(moveDirection.x) > 0.01f);
        }
    }

    private void FixedUpdate()
    {
        bool onLadder = ladderColliders.Count > 0;
        float currentSpeed = isSprinting && !onLadder ? sprintSpeed : moveSpeed;

        if (onLadder)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(
                moveDirection.x * currentSpeed,
                moveDirection.y * climbSpeed);
            return;
        }

        rb.gravityScale = defaultGravityScale;
        rb.linearVelocity = new Vector2(moveDirection.x * currentSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
        isGrounded = groundColliders.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsInLayerMask(other.gameObject.layer, ladderLayers))
        {
            ladderColliders.Add(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ladderColliders.Remove(other);
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.gravityScale = defaultGravityScale;
        }

        groundColliders.Clear();
        ladderColliders.Clear();
        isGrounded = false;
    }

    private void UpdateGroundContact(Collision2D collision)
    {
        bool isGroundLayer = IsInLayerMask(collision.gameObject.layer, groundLayers);
        bool hasGroundContact = false;

        if (isGroundLayer)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > 0.5f)
                {
                    hasGroundContact = true;
                    break;
                }
            }
        }

        if (hasGroundContact)
        {
            groundColliders.Add(collision.collider);
        }
        else
        {
            groundColliders.Remove(collision.collider);
        }

        isGrounded = groundColliders.Count > 0;
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}
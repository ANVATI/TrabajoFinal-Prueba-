using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    public float runSpeed = 6.5f;
    public Transform cameraTransform;
    private Vector2 _movement;
    private Rigidbody rb;
    private Animator playerAnimation;
    public float rollForce = 10f;
    private CapsuleCollider capsuleCollider;
    private Vector3 standingColliderCenter;
    private float standingColliderHeight;

    //La pandemia de booleanos
    private bool isRunning = false;
    private bool runButtonPressed = false;
    private bool IsNormalAttacking = false;
    private bool IsCrouched = false;
    private bool canRolling = false;
    private bool IsRolling = false;
    private bool attackStandButtonEnabled = true;
    private bool rollButtonEnabled = true;
    private bool rageModeButton = true;
    private bool inRageMode = false;
    private bool attackCrouchButtonEnabled = true;
    private bool IsCrouchAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAnimation = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        standingColliderCenter = capsuleCollider.center;
        standingColliderHeight = capsuleCollider.height;
    }
    void FixedUpdate()
    {
        PlayerMovement();
        CheckMovement();
    }

    private void Update()
    {
        AnimationLogic();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        _movement = context.ReadValue<Vector2>();
    }

    public void OnRunning(InputAction.CallbackContext context)
    {
        if (!IsNormalAttacking && !IsCrouched && !IsRolling) 
        {
            if (context.started)
            {
                runButtonPressed = true;
                canRolling = true;
            }
            else if (context.canceled)
            {
                speed = 10.0f;
                isRunning = false;
                runButtonPressed = false;
            }
        }
    }

    public void OnRolling(InputAction.CallbackContext context)
    {
        if (isRunning && canRolling && rollButtonEnabled)
        {
            if (context.started)
            {
                StartCoroutine(Rolling());
            }
        }
    }
    public void OnCrouched(InputAction.CallbackContext context)
    {
        if (!IsNormalAttacking && !isRunning && !IsRolling && _movement == Vector2.zero)
        {
            if (context.started)
            {
                if (IsCrouched)
                {
                    if (!Physics.Raycast(transform.position + capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2f), Vector3.up, 1f))
                    {
                        speed = 10;
                        IsCrouched = false;
                        playerAnimation.SetBool("IsCrouched", false);
                        capsuleCollider.height = standingColliderHeight;
                        capsuleCollider.center = standingColliderCenter;
                    }
                }
                else
                {
                    speed = 3;
                    IsCrouched = true;
                    playerAnimation.SetBool("IsCrouched", true);
                    capsuleCollider.height = standingColliderHeight / 1.5f;
                    capsuleCollider.center = new Vector3(capsuleCollider.center.x, capsuleCollider.center.y / 1.5f, capsuleCollider.center.z);
                }
            }
        }
    }
    public void OnAttackStand(InputAction.CallbackContext context)
    {
        if (context.performed && attackStandButtonEnabled)
        {
            if (!IsCrouched && !isRunning)
            {
                StartCoroutine(NormalAttack());
            }
        }
    }
    public void OnAttackCrouch(InputAction.CallbackContext context)
    {
        if (context.performed && attackCrouchButtonEnabled)
        {
            if (IsCrouched && !isRunning)
            {
                StartCoroutine(CrouchAttack());
            }
        }
    }

    public void RageMode(InputAction.CallbackContext context)
    {
        if (context.performed && rageModeButton)
        {
            StartCoroutine(Rage());
        }
    }
    private IEnumerator CrouchAttack()
    {
        IsCrouchAttacking = true;
        playerAnimation.SetBool("AttackCrouch", true);

        attackCrouchButtonEnabled = false;

        yield return new WaitForSeconds(0.8f);
        IsCrouchAttacking = false;
        playerAnimation.SetBool("AttackCrouch", false);

        yield return new WaitForSeconds(0.05f);

        attackCrouchButtonEnabled = true;
    }

    private IEnumerator NormalAttack()
    {
        IsNormalAttacking = true;
        playerAnimation.SetBool("IsAttacking", true);

        attackStandButtonEnabled = false;

        yield return new WaitForSeconds(0.68f);

        IsNormalAttacking = false;
        playerAnimation.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(0.05f);
        attackStandButtonEnabled = true;
    }
    private IEnumerator Rage()
    {
        playerAnimation.SetBool("RageMode", true);
        rageModeButton = false;
        inRageMode = true;

        yield return new WaitForSeconds(2.5f);

        inRageMode = false;
        playerAnimation.SetBool("RageMode", false);

        yield return new WaitForSeconds(5f);
        rageModeButton = true;

    }
    private IEnumerator Rolling()
    {
        playerAnimation.SetBool("IsRolling", true);
        IsRolling = true;
        runButtonPressed = false;
        rollButtonEnabled = false;
        rb.AddForce(transform.forward * rollForce, ForceMode.Impulse);

        yield return new WaitForSeconds(1f);

        playerAnimation.SetBool("IsRolling", false);
        IsRolling = false;
        playerAnimation.SetBool("IsRunning", true);

        yield return new WaitForSeconds(0.8f);

        rollButtonEnabled = true;

        if (Keyboard.current.shiftKey.isPressed)
        {
            runButtonPressed = true;
        }
    }
    private void PlayerMovement()
    {
        if (IsNormalAttacking || inRageMode || IsCrouchAttacking)
        {
            return;
        }
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * _movement.y + right * _movement.x;

        if (!IsRolling && moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), 0.18f);

            rb.MovePosition(transform.position + moveDirection * speed * Time.fixedDeltaTime);
        }
    }
    private void AnimationLogic()
    {
        if (IsNormalAttacking)
        {
            playerAnimation.SetFloat("X", 0);
            playerAnimation.SetFloat("Y", 0);
        }
        else
        {
            playerAnimation.SetFloat("X", _movement.x);
            playerAnimation.SetFloat("Y", _movement.y);
        }
        playerAnimation.SetBool("IsRunning", isRunning);
    }
    private void CheckMovement()
    {
        if (!IsNormalAttacking)
        {
            if (_movement != Vector2.zero && runButtonPressed)
            {
                speed = runSpeed;
                isRunning = true;
                playerAnimation.SetBool("IsAttacking", false);
                playerAnimation.SetBool("IsRunning", true);
            }
            else if (_movement == Vector2.zero && isRunning)
            {
                playerAnimation.SetBool("IsAttacking", false);
                isRunning = false;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimatorTest : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;

    public float walkSpeed = 1f;
    public float runSpeed = 3f;

    private bool isCrouching = false;
    private bool isDead = false;
    private bool isThrowing = false;

    public float moveSpeed = 3f;
    public float rotationSpeed = 720f;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isDead) return; // 死亡后不响应输入

        HandleMovement();
        HandleJump();
        HandleCrouch();
        HandleHit();
        HandleDeath();
        HandleJieMi();
        HandleThrow(); 
    }

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(h, 0, v).normalized;

        float speed = moveDir.magnitude * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        animator.SetFloat("Speed", speed);

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            float realSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 move = moveDir * realSpeed * Time.deltaTime;
            controller.Move(move);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("IsJumping", true);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("IsJumping", false);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isCrouching = !isCrouching;
            animator.SetBool("IsCrouching", isCrouching);
        }
    }

    void HandleHit()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetBool("IsHit", true);
            Invoke(nameof(ResetHit), 0.3f);
        }
    }

    void ResetHit()
    {
        animator.SetBool("IsHit", false);
    }

    void HandleJieMi()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetBool("JieMi", true);
            Invoke(nameof(ResetJieMi), 0.3f);
        }
    }

    void ResetJieMi()
    {
        animator.SetBool("JieMi", false);
    }

    void HandleDeath()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            isDead = true;
            animator.SetBool("IsDead", true);
        }
    }

    void HandleThrow()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isThrowing)
        {
            isThrowing = true;
            animator.SetBool("IsThrow", true);
            Invoke(nameof(ResetThrow), 0.3f); // 1秒为动画时长
        }
    }

    void ResetThrow()
    {
        animator.SetBool("IsThrow", false);
        isThrowing = false;
    }
}

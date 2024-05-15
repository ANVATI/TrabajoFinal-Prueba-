using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastJump : MonoBehaviour
{
    private Rigidbody rb;
    public float JumpHeigth = 200;
    public bool grounded;
    public LayerMask Mask;
    public float raycastOffset = 2f; 

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Checkground();

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Jump();
        }
    }

    private void Checkground()
    {
        Vector3 raycastPosition = transform.position + new Vector3(0, raycastOffset, 0);
        Debug.DrawRay(raycastPosition, Vector3.down * 2f, Color.red);

        if (Physics.Raycast(raycastPosition, -Vector3.up, 2f, Mask))
        {
            grounded = true;
        }
        else
        {
            grounded = false;
        }

    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * JumpHeigth);
    }

}


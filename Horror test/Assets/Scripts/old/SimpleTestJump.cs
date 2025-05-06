using UnityEngine;

public class SimpleJumpTest : MonoBehaviour
{
    private Rigidbody rb;
    public float jumpForce = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Trigger jump with spacebar
        {
            Debug.Log("Jump triggered");
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }
}

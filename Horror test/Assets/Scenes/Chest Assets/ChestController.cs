using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Animator animator;
    public CameraShake cameraShake;
    private bool isOpen = false;

    void Start()
    {
        Debug.Log("✅ ChestController is Running!"); // Check if script starts
    }

    void Update()
    {
        Debug.Log("✅ Update() is running..."); // Check if Update() works

        if (Input.GetKeyDown(KeyCode.E)) 
        {
            Debug.Log("✅ E key pressed!"); // Check if key press is detected

            isOpen = !isOpen;
            animator.SetBool("Open", isOpen);

           if (isOpen)
            {
                cameraShake.Shake(); // Trigger camera shake
            }
    }
}
}
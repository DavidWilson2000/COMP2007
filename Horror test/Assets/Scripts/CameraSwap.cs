using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    public Transform player;
    public Vector3 thirdPersonOffset = new Vector3(0, 2, -4);
    public Vector3 firstPersonOffset = new Vector3(0, 1.6f, 0.2f);
    public float swapSmoothTime = 0.2f; // ✨ Smooth time for view switching

    private bool isFirstPerson = false;
    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;
    private bool isSwitchingView = false;

    private void Start()
    {
        currentOffset = thirdPersonOffset;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleView();
        }

        UpdateCameraPosition();
    }

    void ToggleView()
    {
        isFirstPerson = !isFirstPerson;
        currentOffset = isFirstPerson ? firstPersonOffset : thirdPersonOffset;
        isSwitchingView = true; // ✨ Only smooth when swapping
        Debug.Log(isFirstPerson ? "🎥 Switched to FIRST person" : "🎥 Switched to THIRD person");
    }

    void UpdateCameraPosition()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + player.rotation * currentOffset;

        if (isFirstPerson)
        {
            // 🚀 FIRST PERSON MODE: stick to player instantly
            transform.position = targetPosition;
        }
        else
        {
            if (isSwitchingView)
            {
                // ✨ When just switched to third-person: smooth transition
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, swapSmoothTime);

                // Check if close enough, then stop smoothing
                if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
                {
                    transform.position = targetPosition;
                    isSwitchingView = false;
                }
            }
            else
            {
                // 🧲 Normally stick to third-person offset directly
                transform.position = targetPosition;
            }
        }

        // Always rotate to match player's Y rotation smoothly
        Quaternion targetRotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
    }
}

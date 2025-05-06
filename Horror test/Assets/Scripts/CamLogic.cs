using UnityEngine;

public class CamLogic : MonoBehaviour
{
    public Transform playerTransform;
    public float fixedDistance = 5f;

    public float verticalOffset = 1.8f; // 🎯 new: height above player center

    private void Update()
    {
        // Follow player's rotation
        transform.rotation = playerTransform.rotation;

        Vector3 offset = Vector3.zero;

        if (Mathf.Approximately(transform.eulerAngles.y, 90f))
        {
            offset = new Vector3(-fixedDistance, verticalOffset, 0);
        }
        else if (Mathf.Approximately(transform.eulerAngles.y, 270f))
        {
            offset = new Vector3(fixedDistance, verticalOffset, 0);
        }
        else
        {
            offset = new Vector3(0, verticalOffset, -fixedDistance);
        }

        transform.position = playerTransform.position + offset;
    }
}

using UnityEngine;


public class MenuPlayerLoop : MonoBehaviour
{
    public Transform centerPoint;
    public float radius = 5f;
    public float speed = 1f;
    private float angle;

    void OnEnable()
    {
        // Reset the angle so it always starts moving
        angle = 0f;
    }

    void Update()
    {
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        transform.position = centerPoint.position + new Vector3(x, 0, z);
        transform.LookAt(centerPoint.position);
    }
}

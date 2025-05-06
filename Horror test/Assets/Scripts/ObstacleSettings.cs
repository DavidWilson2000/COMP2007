using UnityEngine;

public class ObstacleSettings : MonoBehaviour
{
    public float heightOffset = 0f;
    public Vector3 rotationOffset = Vector3.zero;
    public string allowedSpawnType = "Any";

    [Header("Advanced Spawn Rules")]
    public int minSegmentsBetweenSpawns = 0; // 🆕 How many segments must pass before it can spawn again
}

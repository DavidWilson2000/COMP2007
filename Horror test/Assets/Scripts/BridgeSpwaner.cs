using UnityEngine;
using System.Collections.Generic;

public class BridgeSpawner : MonoBehaviour
{
    public GameObject[] bridgePrefabs;
    public GameObject[] obstaclePrefabs;
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.3f;

    public Transform player;

    public int maxSegments = 30;
    public float segmentLength = 6f;
    public float despawnDistance = 30f;

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private List<GameObject> activeSegments = new List<GameObject>();
    private Vector3 spawnPosition = Vector3.zero;
    private Vector3 spawnDirection = Vector3.forward;

    private int bridgeLayerMask;

    private int turnCounter = 0;
    private int turnLimit = 1;
    private int segmentWindow = 20;
    private int segmentsSinceWindowStart = 0;

    private Dictionary<GameObject, int> obstacleCooldowns = new Dictionary<GameObject, int>();

    void Start()
    {
        bridgeLayerMask = LayerMask.GetMask("BridgeSegment");

        for (int i = 0; i < maxSegments; i++)
        {
            if (i < 4)
                SpawnSpecificSegment(bridgePrefabs[0]);
            else
                SpawnNextSegment();
        }
    }

    void Update()
    {
        while (activeSegments.Count < maxSegments)
        {
            SpawnNextSegment();
        }

        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = activeSegments[i];
            Vector3 toSegment = segment.transform.position - player.position;

            float dot = Vector3.Dot(toSegment.normalized, player.forward);
            if (dot < -0.5f && toSegment.magnitude > despawnDistance)
            {
                Destroy(segment);
                activeSegments.RemoveAt(i);
            }
        }
    }

    void SpawnSpecificSegment(GameObject prefab)
    {
        Quaternion rotation = Quaternion.LookRotation(spawnDirection);
        GameObject segment = Instantiate(prefab, spawnPosition, rotation);

        Transform exit = segment.transform.Find("ExitPoint");
        if (exit == null)
        {
            Debug.LogWarning($"Missing ExitPoint on: {prefab.name}");
            Destroy(segment);
            return;
        }

        activeSegments.Add(segment);
        occupiedPositions.Add(spawnPosition);
        occupiedPositions.Add(exit.position);

        spawnPosition = exit.position;
        spawnDirection = exit.forward;

        SpawnObstaclesOnSegment(segment);
        TickObstacleCooldowns(); // 🆕 Decrease cooldowns each segment
    }

    void SpawnNextSegment()
    {
        const int maxAttempts = 10;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            float randomTurnChance = 0.1f;
            bool success = false;

            if (Random.value < randomTurnChance)
            {
                success = TrySpawnSegmentType("left") || TrySpawnSegmentType("right") || TrySpawnSegmentType("straight");
            }
            else
            {
                success = TrySpawnSegmentType("straight") || TrySpawnSegmentType("left") || TrySpawnSegmentType("right");
            }

            if (success)
                return;

            attempts++;
        }

        Debug.LogError("🚨 SpawnNextSegment failed after max retries! Path completely blocked!");
    }

    bool TrySpawnSegmentType(string type)
    {
        if ((type == "left" || type == "right") && turnCounter >= turnLimit)
        {
            Debug.Log($"⛔ Turn limit reached ({turnCounter}/{turnLimit}) — skipping {type} turns.");
            return false;
        }

        List<GameObject> candidates = new List<GameObject>();

        foreach (GameObject prefab in bridgePrefabs)
        {
            string name = prefab.name.ToLower();
            if (type == "straight" && !name.Contains("_turn")) candidates.Add(prefab);
            else if (type == "left" && name.Contains("l_turn")) candidates.Add(prefab);
            else if (type == "right" && name.Contains("r_turn")) candidates.Add(prefab);
        }

        if (candidates.Count == 0)
            return false;

        GameObject chosenPrefab = candidates[Random.Range(0, candidates.Count)];
        Quaternion rotation = Quaternion.LookRotation(spawnDirection);
        GameObject temp = Instantiate(chosenPrefab, spawnPosition, rotation);

        Transform exit = temp.transform.Find("ExitPoint");
        if (exit == null)
        {
            Debug.LogWarning($"Missing ExitPoint on: {chosenPrefab.name}");
            Destroy(temp);
            return false;
        }

        Vector3 predictedExit = exit.position;
        Vector3 predictedDirection = spawnDirection;

        string lowerName = chosenPrefab.name.ToLower();
        if (lowerName.Contains("l_turn"))
            predictedDirection = Quaternion.Euler(0, -90, 0) * predictedDirection;
        else if (lowerName.Contains("r_turn"))
            predictedDirection = Quaternion.Euler(0, 90, 0) * predictedDirection;

        Vector3 futureStep = predictedExit + predictedDirection * segmentLength;

        if (occupiedPositions.Contains(predictedExit) || occupiedPositions.Contains(futureStep))
        {
            Destroy(temp);
            return false;
        }

        Collider tempCollider = temp.GetComponent<Collider>();
        if (tempCollider != null)
        {
            Collider[] overlaps = Physics.OverlapBox(tempCollider.bounds.center, tempCollider.bounds.extents, temp.transform.rotation, bridgeLayerMask);
            foreach (Collider col in overlaps)
            {
                if (col != tempCollider)
                {
                    Destroy(temp);
                    return false;
                }
            }
        }

        activeSegments.Add(temp);
        occupiedPositions.Add(spawnPosition);
        occupiedPositions.Add(predictedExit);

        spawnPosition = predictedExit;
        spawnDirection = predictedDirection;

        bool isTurn = lowerName.Contains("_turn");
        if (isTurn) turnCounter++;

        segmentsSinceWindowStart++;
        if (segmentsSinceWindowStart >= segmentWindow)
        {
            segmentsSinceWindowStart = 0;
            turnCounter = 0;
            Debug.Log("🔄 Turn counter reset after segment window completed!");
        }

        Debug.Log($"🛤️ Spawned {type}: {chosenPrefab.name}");
        Debug.DrawLine(spawnPosition, spawnPosition + spawnDirection * 2f, Color.green, 10f);

        SpawnObstaclesOnSegment(temp);
        TickObstacleCooldowns(); // 🆕 Decrease cooldowns each segment

        return true;
    }

    void SpawnObstaclesOnSegment(GameObject segment)
    {
        List<Transform> spawnPoints = new List<Transform>();

        foreach (Transform child in segment.transform)
        {
            if (child.name.StartsWith("SpawnPoint"))
            {
                spawnPoints.Add(child);
            }
        }

        if (spawnPoints.Count == 0 || obstaclePrefabs.Length == 0)
            return;

        if (Random.value > obstacleSpawnChance)
            return;

        Transform chosenPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        string spawnPointType = "Any";
        if (chosenPoint.name.Contains("_Center"))
            spawnPointType = "Center";
        else if (chosenPoint.name.Contains("_Left"))
            spawnPointType = "Left";
        else if (chosenPoint.name.Contains("_Right"))
            spawnPointType = "Right";

        List<GameObject> matchingObstacles = new List<GameObject>();

        foreach (GameObject obstaclePrefab in obstaclePrefabs)
        {
            ObstacleSettings settings = obstaclePrefab.GetComponent<ObstacleSettings>();
            if (settings != null && (settings.allowedSpawnType == "Any" || settings.allowedSpawnType == spawnPointType))
            {
                if (!obstacleCooldowns.ContainsKey(obstaclePrefab) || obstacleCooldowns[obstaclePrefab] <= 0)
                {
                    matchingObstacles.Add(obstaclePrefab);
                }
            }
        }

        if (matchingObstacles.Count == 0)
            return;

        GameObject chosenObstacle = matchingObstacles[Random.Range(0, matchingObstacles.Count)];
        ObstacleSettings chosenSettings = chosenObstacle.GetComponent<ObstacleSettings>();

        float height = chosenSettings != null ? chosenSettings.heightOffset : 0f;
        Vector3 rotOffset = chosenSettings != null ? chosenSettings.rotationOffset : Vector3.zero;

        Vector3 spawnPos = chosenPoint.position + Vector3.up * height;
        Quaternion spawnRot = chosenPoint.rotation * Quaternion.Euler(rotOffset);

        Instantiate(chosenObstacle, spawnPos, spawnRot, segment.transform);

        // 🆕 After spawning, set its cooldown
        if (chosenSettings != null && chosenSettings.minSegmentsBetweenSpawns > 0)
        {
            obstacleCooldowns[chosenObstacle] = chosenSettings.minSegmentsBetweenSpawns;
        }
    }

    void TickObstacleCooldowns()
    {
        List<GameObject> keys = new List<GameObject>(obstacleCooldowns.Keys);
        foreach (GameObject key in keys)
        {
            obstacleCooldowns[key] = Mathf.Max(0, obstacleCooldowns[key] - 1);
        }
    }
}

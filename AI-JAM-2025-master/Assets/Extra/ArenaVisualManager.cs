using UnityEngine;

public class ArenaVisualManager : MonoBehaviour
{
    [SerializeField] private GameObject domPrefababrikaaaat, autoPrefabrikat;

    [Header("Car Spawning")]
    [SerializeField] private Vector3 centerPoint = Vector3.zero; // center to spawn around
    [SerializeField] private float spawnRadius = 20f;            // radius around centerPoint
    [SerializeField] private int initialSpawnCount = 0;          // optional initial cars to spawn

    private float _lockedY;

    private void Start()
    {
        // Lock Y height to current manager position (can be adjusted to any desired baseline)
        _lockedY = transform.position.y;

        // Optionally spawn some cars at startup
        if (initialSpawnCount > 0)
        {
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnCarInRange();
            }
        }
    }

    // Spawns a car at a random position in range of centerPoint (Y locked to _lockedY)
    public GameObject SpawnCarInRange()
    {
        if (autoPrefabrikat == null)
        {
            Debug.LogWarning("ArenaVisualManager: autoPrefabrikat is not assigned.");
            return null;
        }

        Vector3 pos = GetRandomXZPosition(centerPoint, spawnRadius, _lockedY);
        Quaternion rot = Quaternion.Euler(-90f, Random.Range(0f,360f), 0f); // x rotation at -90
        return Instantiate(autoPrefabrikat, pos, rot);
    }

    // Utility: random XZ around center with locked Y
    private static Vector3 GetRandomXZPosition(Vector3 center, float radius, float lockedY)
    {
        Vector2 r = Random.insideUnitCircle * radius;
        return new Vector3(center.x + r.x, lockedY, center.z + r.y);
    }
}
using System.Collections;
using UnityEngine;

public class DroneDropLogic : MonoBehaviour
{
    [Header("Crate")]
    public GameObject cratePrefab;
    public SpringJoint springJoint; // this spring the crate is attached to
    public float attachDistanceBelowDrone = 1.0f;

    [Header("Movement")]
    public Vector3 point;           // center point to move around
    public float moveRadius = 15f;  // radius around point to choose random targets
    public float moveSpeed = 6f;    // movement speed (units/sec)

    [Header("Dropping")]
    public float preDropHangTime = 0.5f;   // time to wait after attaching before dropping
    public float timeBetweenDrops = 2.5f;  // time to wait after drop before next move/drop

    private float _lockedY;
    private Coroutine _runner;

    private void Start()
    {
        // Lock to the drone's initial height
        _lockedY = transform.position.y;
        _runner = StartCoroutine(DroneRoutine());
        AddCrate();
    }

    private IEnumerator DroneRoutine()
    {
        // Infinite loop: move to random target, attach crate, drop, wait, repeat
        var waitBetweenDrops = new WaitForSeconds(timeBetweenDrops);
        while (true)
        {
            Vector3 target = GetRandomTargetOnLockedY();
            yield return MoveTo(target);

            
            if (preDropHangTime > 0f)
            {
                yield return new WaitForSeconds(preDropHangTime);
            }

            DropCrate();
            AddCrate();
            yield return waitBetweenDrops;
        }
    }

    private Vector3 GetRandomTargetOnLockedY()
    {
        // Pick a random position on XZ plane within moveRadius around 'point', with Y locked
        Vector2 r = Random.insideUnitCircle * moveRadius;
        return new Vector3(point.x + r.x, _lockedY, point.z + r.y);
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        // Move smoothly toward destination while keeping Y locked
        while ((transform.position - destination).sqrMagnitude > 0.0004f) // ~2cm
        {
            var current = transform.position;
            var next = Vector3.MoveTowards(current, destination, moveSpeed * Time.deltaTime);
            next.y = _lockedY; // enforce locked height
            transform.position = next;
            yield return null;
        }

        // Snap to exact destination (with locked Y)
        transform.position = new Vector3(destination.x, _lockedY, destination.z);
    }

    private void AddCrate()
    {
        if (cratePrefab == null || springJoint == null)
        {
            Debug.LogWarning("DroneDropLogic: Missing cratePrefab or springJoint.");
            return;
        }

        // If something is already attached, drop it first
        if (springJoint.connectedBody != null)
        {
            DropCrate();
        }

        var spawnPos = transform.position + Vector3.down * Mathf.Max(0.0f, attachDistanceBelowDrone);
        var go = Instantiate(cratePrefab, spawnPos, Quaternion.identity ,transform);
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
        }

        springJoint.connectedBody = rb;
    }

    private void DropCrate() // disconnect the spring so the crate falls
    {
        if (springJoint != null)
        {
            springJoint.connectedBody = null;
        }
    }

    private void OnDisable()
    {
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
    }
}

using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        RobotAgent robot = other.gameObject.GetComponentInParent<RobotAgent>();
        if (robot == null)
            return;

        Debug.Log("Robot triggered a trap", this);
    }
}

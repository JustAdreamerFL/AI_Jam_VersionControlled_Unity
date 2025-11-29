using UnityEngine;

public class TrapTrigger : MonoBehaviour
{
     
    protected virtual void RobotCollided(RobotAgent robot, CollisionZoneBehaviour[] collisionZone)
    {

    }

    protected virtual void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {

    }


    private void OnTriggerEnter(Collider other)
    {
        RobotAgent robot = other.gameObject.GetComponentInParent<RobotAgent>();
        if (robot == null)
            return;

        CollisionZoneBehaviour[] collisionZones = robot.GetComponentsInChildren<CollisionZoneBehaviour>(); 
        RobotCollided(robot, collisionZones);

        if (other.TryGetComponent<CollisionZoneBehaviour>(out CollisionZoneBehaviour zone))
        {
            RobotCollided(robot, zone);
        }
        Debug.Log("Robot triggered a trap", this);
    }
}

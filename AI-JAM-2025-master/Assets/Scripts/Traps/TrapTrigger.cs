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
        Debug.Log("Collision");
        RobotAgent robot = other.gameObject.GetComponentInParent<RobotAgent>();
        if (robot == null)
            return;
        //CollisionZoneBehaviour[]

        

        Debug.Log("Robot triggered a trap", this);
    }
}

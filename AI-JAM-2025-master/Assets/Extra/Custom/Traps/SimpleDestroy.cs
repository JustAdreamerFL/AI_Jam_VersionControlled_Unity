using UnityEngine;

public class SimpleDestroy : TrapTrigger
{
    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        SpawnTriggerParticle();
        Destroy(gameObject);
    }
}

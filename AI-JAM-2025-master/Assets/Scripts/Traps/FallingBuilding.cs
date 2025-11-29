using UnityEngine;

public class FallingBuilding : TrapTrigger
{
    [SerializeField, Range(0, 20)] private float hitDamage;

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        collisionZone.ChangeHealth(-hitDamage);
        Debug.Log("Building destroyed");
        DestroySelf();
    }

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour[] collisionZone)
    {
        base.RobotCollided(robot, collisionZone);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }
}

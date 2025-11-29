using UnityEngine;

public class GoodHeal : TrapTrigger
{
    [SerializeField] private int rewardValue;
    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour[] collisionZone)
    {

    }

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        collisionZone.ChangeHealth(0.1f);
        robot.AddReward(rewardValue);
        Destroy(gameObject);
    }
}

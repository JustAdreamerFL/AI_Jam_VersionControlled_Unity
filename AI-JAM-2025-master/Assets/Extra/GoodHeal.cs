using UnityEngine;

public class GoodHeal : TrapTrigger
{
    [SerializeField] private int rewardValue;
    [SerializeField, Range(0,0.2f)] private float healAmout;

    [SerializeField] private ParticleSystem healtParticles;
    [SerializeField] private Transform particleSpawn;

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        robot.AddReward(rewardValue);
        collisionZone.ChangeHealth(healAmout);
        SpawnTriggerParticle();
        Destroy(gameObject);
    }
}

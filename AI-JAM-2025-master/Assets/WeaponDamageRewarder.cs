using UnityEngine;
using static CollisionZoneBehaviour;

public class WeaponDamageRewarder : MonoBehaviour
{
    [SerializeField] private RobotAgent robot;

    [SerializeField, Range(0, 100)] private float weaponRewardMultiplier;

    //Custom
    private CollisionZoneBehaviour[] weaponCollisionZones;
    //Custom


    private void Awake()
    {
        robot = robot != null ? robot : GetComponent<RobotAgent>();
        FindWeapons();
    }

    private void FindWeapons()
    {
        weaponCollisionZones = new CollisionZoneBehaviour[2];
        int index = 0;
        foreach (CollisionZoneBehaviour collisionZone in robot.zoneComponents)
        {
            if (collisionZone.gameObject.CompareTag("RobotArm"))
            {
                weaponCollisionZones[index++] = collisionZone;
                collisionZone.OnDamageDealt += CollisionZone_OnDamageDealt;
            }
        }
    }

    private void CollisionZone_OnDamageDealt(object sender, DamageDealtEventArgs e)
    {
        float weaponCollisionReward = e.DamageDealt * 1000; // damage * reward multiplier
        Debug.Log($"Calculated weapon reward: {weaponCollisionReward}, {transform.name}");
        robot.AddReward(weaponCollisionReward * weaponRewardMultiplier);
    }

}

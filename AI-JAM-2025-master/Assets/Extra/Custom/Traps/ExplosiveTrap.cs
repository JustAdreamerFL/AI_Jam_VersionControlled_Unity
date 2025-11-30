using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExplosiveTrap : TrapTrigger
{
    [Tooltip("How much of the object will be damaged")]
    [SerializeField, Range(0,100)] private float damagePercentage;

    [SerializeField, Range(0, 0.2f)] private float damage;

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour[] collisionZone)
    {
        if (collisionZone.Length > 0)
        {
            List<CollisionZoneBehaviour> cList = collisionZone.ToList();
            float objectsForDamage = (float)collisionZone.Length * (damagePercentage / 100);
            int count = Mathf.Min(Mathf.RoundToInt(objectsForDamage), cList.Count);
            for (int i = 0; i < count; i++)
            {
                var item = cList[Random.Range(0, cList.Count)];
                cList.Remove(item);
                item.ChangeHealth(damage);
            }
        }
        SpawnTriggerParticle();
        Destroy(gameObject);
    }
}

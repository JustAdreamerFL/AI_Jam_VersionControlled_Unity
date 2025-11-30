using UnityEngine;

public class WeaponDamageTracker : MonoBehaviour
{
    [SerializeField] private CollisionZoneBehaviour[] weaponZoneBehaviour;

    private void Awake()
    {
        foreach (CollisionZoneBehaviour zone in weaponZoneBehaviour)
        {
            zone.OnCollision += Zone_OnCollision;
        }
    }

    private void Zone_OnCollision(object sender, CollisionZoneBehaviour.CollisionEventArgs e)
    {
        //Debug.Log($"Weapon has collided, damage {e.Damage}, hit name {e.colliderHitName} ");
    }

    private void OnDestroy()
    {
        foreach (CollisionZoneBehaviour zone in weaponZoneBehaviour)
        {
            if (zone == null)
                return;
            zone.OnCollision -= Zone_OnCollision;
        }
    }
}

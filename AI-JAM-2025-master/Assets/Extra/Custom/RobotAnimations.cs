using UnityEngine;

public class RobotAnimations : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private ParticleSystem shootParticle;
    [SerializeField] private CollisionZoneBehaviour weaponCollisionZone;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform gun;

    [SerializeField] private float walkSpeedMultiplier = 1f;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (weaponCollisionZone == null)
        {
            Debug.LogWarning("Weapon collision zone is not set, shooting animation wont be played!", this);
            return;
        }
        weaponCollisionZone.OnCollision += WeaponCollisionZone_OnCollision;
    }

    private void WeaponCollisionZone_OnCollision(object sender, CollisionZoneBehaviour.CollisionEventArgs e)
    {
        animator.SetTrigger("shoot");
    }

    void Update()
    {
        Vector3 velocity = rb.linearVelocity;
        animator.SetFloat("walkSpeed", velocity.magnitude);
    }

    public void PlayShootParticle()
    {
        ParticleSystem particle = Instantiate(shootParticle, gun.position, Quaternion.LookRotation(gun.forward, Vector3.up));
        particle.Play();
    }

}

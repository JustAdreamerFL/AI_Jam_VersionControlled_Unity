using UnityEngine;

public class RobotAnimations : MonoBehaviour
{
    [SerializeField] private float walkSpeedMultiplier = 1f;
    [SerializeField] private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 velocity = rb.linearVelocity;
        animator.SetFloat("walkSpeed", velocity.magnitude);
    }
}

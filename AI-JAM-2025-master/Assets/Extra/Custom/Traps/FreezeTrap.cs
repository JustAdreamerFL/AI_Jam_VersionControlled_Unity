using UnityEngine;

public class FreezeTrap : TrapTrigger
{
    [SerializeField] private float freezeDuration;
    [SerializeField] private float freezeUnactiveDuration;

    [SerializeField] private Animator animator;

    private RobotMovement robotMovementRef;
    private bool freezeActive;

    private WaitTimer waitTimer;

    private void Awake()
    {
        waitTimer = new WaitTimer();    
    }

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        if (freezeActive)
            return; 

        robotMovementRef = robot.TryGetComponent<RobotMovement>(out RobotMovement movement) ? movement : robot.GetComponentInChildren<RobotMovement>();
        if (robotMovementRef == null)
        {
            Debug.Log("TÁ VEC NEMÁ MOVEMENT!!");
            return;
        }

        robotMovementRef.stationaryRobot = true;
        freezeActive = true;
        animator.SetTrigger("trigger");
        waitTimer.SetTimer(freezeDuration, () => 
        {
            animator.SetTrigger("trigger");
            if (robotMovementRef != null)
            {
                robotMovementRef.stationaryRobot = false;
                robotMovementRef = null;
            }

            waitTimer.SetTimer(freezeUnactiveDuration, () =>
            {
                freezeActive = false;
            });

        });
    }

    private void Update()
    {
        waitTimer.UpdateTimer();
    }
}

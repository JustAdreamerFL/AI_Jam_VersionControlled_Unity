using UnityEngine;

public class EjectZone : TrapTrigger
{
    [SerializeField] private AnimationCurve curve;
    private float counter;

    private Quaternion startRotation;
    private Quaternion endRotation;

    [SerializeField] private float ejectSpeed;

    [SerializeField] private BoxCollider thisCollider;
    [SerializeField] private Transform triggerVisual;
    [SerializeField] private float sleepingStateDuration = 2f;

    private bool active;

    private State state;

    private enum State
    {
        Ready,
        Raising,
        Falling,
        Sleeping
    }


    private void Start()
    {
        startRotation = Quaternion.identity;
        endRotation = Quaternion.Euler(new Vector3(0, 0, 90));
    }

    protected override void RobotCollided(RobotAgent robot, CollisionZoneBehaviour collisionZone)
    {
        if (state == State.Ready)
        {
            Debug.Log("Triggered");
            state = State.Raising;
            robot.AddReward(-1000);
            active = true;
        }
    }

    private void Update() 
    {
        if (active) 
        {
            Eject();
        }
        if (state == State.Sleeping) 
        {
            counter += Time.deltaTime;
            if (counter > sleepingStateDuration)
            {
                counter = 0;
                transform.rotation = startRotation;
                EnableCollider(true);
                EnableTriggerVisual(true);
                state = State.Ready;
            }
        }
    }

    private void Eject()
    {
        if (counter >= 1)
        {
            transform.rotation = endRotation;
            switch (state)
            {
                case State.Raising:
                    counter = 0;
                    EnableCollider(false);
                    EnableTriggerVisual(false);
                    active = false;
                    state = State.Sleeping;
                    break;
            }
            return;
        }
        transform.rotation = Quaternion.Slerp(startRotation, endRotation, curve.Evaluate(counter));
        counter += Time.deltaTime * ejectSpeed;
    }

    private void FlipRotations()
    {
        (startRotation, endRotation) = (endRotation, startRotation);
    }

    private void EnableCollider(bool enable)
    {
        thisCollider.enabled = enable;
    }

    private void EnableTriggerVisual(bool enable) 
    { 
        triggerVisual.gameObject.SetActive(enable);
    }
}

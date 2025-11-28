using System.Linq;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using static CollisionZoneBehaviour;

public class RobotAgentZaloha : Agent
{

    [SerializeField] private float cumulativeReward = 0f;

    [Header("Reward Info (separate)")]
    [SerializeField] private float movementRwd;
    [SerializeField] private float robotFacingRwd;
    [SerializeField] private float armReleaseRwd;
    [SerializeField] private float enemyDistanceRwd;
    [SerializeField] private float arenaPositionRwd;
    [SerializeField] private float collisionCombinationRwd;

    [SerializeField] private SensorComponent[] sensors = new SensorComponent[7];

    private Rigidbody rb;
    [SerializeField] private RewardEditor rewardEditor;
    private BehaviorParameters behavior;
    private RobotMovement robotMovement;
    private CollisionZoneBehaviour[] zoneComponents;
    private IHealth[] healthComponents;
    [HideInInspector] public RobotAgent enemyRobot;
    private bool isHeuristic = false;

    public event System.EventHandler OnRobotRespawn;
    private bool isStarted = false;

    //[SerializeField] private float tempAngleToEnemy;


    // --------------------------- INITIALIZATION METHODS ---------------------------

    public override void Initialize()
    {
        cumulativeReward = 0f;
        MaxStep = 5000;
    }

    private void Start()
    {
        robotMovement = gameObject.GetComponent<RobotMovement>();
        zoneComponents = GetComponentsInChildren<CollisionZoneBehaviour>();
        healthComponents = GetComponentsInChildren<IHealth>();
        //Debug.Log($"RobotAgent found {zoneComponents.Length} collision zones and {healthComponents.Length} health components.");

        rb = gameObject.GetComponent<Rigidbody>();

        var robots = transform.parent.gameObject.GetComponentsInChildren<RobotAgent>();
        enemyRobot = robots.FirstOrDefault(r => r != this);

        rewardEditor = FindAnyObjectByType<RewardEditor>();
        behavior = GetComponent<BehaviorParameters>();

        isStarted = true;
    }


    public override void OnEpisodeBegin()
    {
        movementRwd = 0f;
        robotFacingRwd = 0f;
        armReleaseRwd = 0f;
        enemyDistanceRwd = 0f;
        arenaPositionRwd = 0f;
        collisionCombinationRwd = 0f;

        SetReward(0f);
        cumulativeReward = GetCumulativeReward();

        OnRobotRespawn?.Invoke(this, System.EventArgs.Empty);

        //robotMovement.armComponent.GetComponent<HingeJoint>().enableCollision = false;
        //robotMovement.armComponent.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        //robotMovement.armComponent.GetComponent<HingeJoint>().enableCollision = true;

        if (!isStarted)
        {
            return;
        }

        foreach (var zone in zoneComponents)
        {
            zone.ResetHealth();
        }
    }

    //public bool IsTraining() {
    //    // BehaviorParameters controls whether we're using trainer or ONNX
    //    var behavior = GetComponent<BehaviorParameters>();

    //    // This is true ONLY when Python is connected (training)
    //    return behavior;
    //}


    // ---------------------------- HANDLE REWARDS ----------------------------

    private void Update()
    {
        //AddReward(0.001f);

        if (transform.localPosition.y < -1f)
        {
            Debug.Log("Robot fell out of arena!");

            foreach (var zone in zoneComponents)
            {
                zone.ChangeHealth(-0.3f);
            }

            OnRobotRespawn?.Invoke(this, System.EventArgs.Empty);
        }

        float rotationUp = Vector3.Dot(transform.up, Vector3.up);
        if (rotationUp < 0.75f)
        { // origin 0.2f
            Debug.Log("Robot flipped over!");

            foreach (var zone in zoneComponents)
            {
                zone.ChangeHealth(-0.2f);
            }

            OnRobotRespawn?.Invoke(this, System.EventArgs.Empty);
        }

        DieCases();

        if (rewardEditor != null)
        {

            float movementRewards = GetRewards_Movement() * 0.5f * Time.deltaTime;
            AddReward(movementRewards * rewardEditor.movementRF);
            movementRwd += (float)System.Math.Round(movementRewards, 2);

            float robotFacingRewards = GetRewards_FacingRobot() * 0.5f * Time.deltaTime;
            AddReward(robotFacingRewards * rewardEditor.facingRF);
            robotFacingRwd += (float)System.Math.Round(robotFacingRewards, 2);

            float armReleaseReward = GetRewards_ArmRelease() * 10f * Time.deltaTime;
            AddReward(armReleaseReward * rewardEditor.armAngleRF);
            armReleaseRwd += (float)System.Math.Round(armReleaseReward, 2);

            float enemyDistanceReward = GetRewards_EnemyDistance() * Time.deltaTime;
            AddReward(enemyDistanceReward * rewardEditor.enemyDistanceRF);
            enemyDistanceRwd += (float)System.Math.Round(enemyDistanceReward, 2);

            float arenaPositionReward = GetRewards_ArenaPosition() * Time.deltaTime;
            AddReward(arenaPositionReward * rewardEditor.arenaPositionRF);
            arenaPositionRwd += (float)System.Math.Round(arenaPositionReward, 2);

            float collisionCombinationReward = GetRewards_collisionCombination() * 100f * Time.deltaTime;
            AddReward(collisionCombinationReward * rewardEditor.collisionCombinationRF);
            collisionCombinationRwd += (float)System.Math.Round(collisionCombinationReward, 2);

            // TODO: If Butt Sensor not null, then add reward for butt marker visibility (maybe could surpress unwanted front collisions?)

            cumulativeReward = GetCumulativeReward();

        }

    }

    private void DieCases()
    {

        if (healthComponents.Count() == 0)
        {
            return;
        }

        foreach (var healthComp in healthComponents)
        {
            switch (healthComp)
            {
                case BodyComponent bodyComp:
                    //Debug.Log($"Body health: {healthComp.CurrentHealth}");

                    if (healthComp.CurrentHealth <= 0.1f)
                    {
                        Debug.Log("Robot body destroyed!");
                        // TODO: Method for robot dying, losing/winning logic
                        AddReward(-1500f);
                        EndEpisode();
                    }
                    break;
                case WheelComponent wheelComp:
                    //Debug.Log($"Wheel health: {healthComp.CurrentHealth}");
                    break;
                case BumperComponent bumperComp:
                    //Debug.Log($"Bumper health: {healthComp.CurrentHealth}");
                    break;
                case ArmComponent armComp:
                    //Debug.Log($"Arm health: {healthComp.CurrentHealth}");
                    break;
            }
        }

        if (healthComponents.Average(e => e.CurrentHealth) <= 0.3f)
        {
            Debug.Log("Robot beyond repair!");
            AddReward(-1000f);
            EndEpisode();
        }
    }

    private float GetRewards_collisionCombination()
    {
        var sumReward = zoneComponents.Average(z => z.RewardOfCollision);
        return Mathf.Clamp(sumReward, -1f, 1f);
    }

    private float GetRewards_ArenaPosition()
    {
        float distanceToCenter = this.transform.localPosition.magnitude;
        float distanceReward = Mathf.Clamp(distanceToCenter / 2f, 0f, 1f);// * -2f + 1f;
        return distanceReward;
    }

    private float GetRewards_EnemyDistance()
    {
        if (enemyRobot == null) return 0f;
        float distanceToEnemy = (enemyRobot.transform.localPosition - this.transform.localPosition).magnitude;
        //Debug.Log("Distance to enemy: " + distanceToEnemy);
        float distanceReward = Mathf.Clamp(distanceToEnemy / 6f, 0f, 1f);// * -2f + 1f;
        return distanceReward;
    }

    private float GetRewards_ArmRelease()
    {
        var reward = (0.02f * robotMovement.armComponent.ArmRaisingReward) + robotMovement.armComponent.ArmDropReward;
        return Mathf.Clamp(reward, 0f, 1f);
    }

    private float GetRewards_Movement()
    {
        //Debug.Log("Velocity: " + rb.linearVelocity.magnitude);

        float forwardDot = Vector3.Dot(rb.linearVelocity.normalized, transform.forward);
        if (forwardDot > 0.2f) // Threshold to ensure it's *mostly* forward
        {
            return Mathf.Clamp(rb.linearVelocity.magnitude / 1.7f, 0f, 1f) * 2f - 1f;
            // Small bonus for moving forward
        }
        else if (forwardDot < -0.2f)
        {
            return Mathf.Clamp(rb.linearVelocity.magnitude / 1.7f, 0f, 1f) * 1.5f - 1f;
            // Penalty for moving backward
        }
        return 0f;
    }

    private float GetRewards_FacingRobot()
    {
        if (enemyRobot != null)
        {
            var vectorToEnemy = enemyRobot.transform.position - this.transform.position;
            var angleToEnemy = Vector3.SignedAngle(this.transform.forward, vectorToEnemy, Vector3.up);

            //tempAngleToEnemy = angleToEnemy;

            // relativna podla toho ako moc? alebo len ano/nie situacia
            if (angleToEnemy >= -45f && angleToEnemy <= 45f)
            {
                return 1f;
            }
            else if (angleToEnemy >= 100f || angleToEnemy <= -100f)
            {
                return -1f;
            }
        }
        return 0f;
    }


    // ---------------------------- ACTIONS AND OBSERVATIONS ---------------------------

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isStarted)
        {
            return;
        }

        // Process the actions received from the neural network
        float leftWheelsValue = actions.ContinuousActions[0];
        float rightWheelsValue = actions.ContinuousActions[1];
        int armLiftValue = actions.DiscreteActions[0];

        if (isHeuristic)
        {
            robotMovement.ApplyBothTrackInputsHeuristic(leftWheelsValue, rightWheelsValue);
            robotMovement.ApplyArmInput(armLiftValue);
        }
        else
        {
            robotMovement.ApplyBothTrackInputs(leftWheelsValue, rightWheelsValue);
            robotMovement.ApplyArmInput(armLiftValue);
        }

    }

    public override void CollectObservations(VectorSensor observations)
    {
        for (int i = 0; i < observations.ObservationSize(); i++)
        {
            var observationValue = sensors?.ElementAtOrDefault(i)?.SensorValue ?? 0f;
            observations.AddObservation(observationValue);
        }
    }

    // ------------------------------- HEURISTIC -------------------------------

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        isHeuristic = true;
        MaxStep = 0; // Disable max step limit for heuristic mode

        if (!isStarted)
        {
            return;
        }

        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;

        float leftWheelsValue = robotMovement.leftWheelsAxis.ReadValue<float>();
        float rightWheelsValue = robotMovement.rightWheelsAxis.ReadValue<float>();
        float armLiftValue = robotMovement.armLiftAction.ReadValue<float>();

        continuousActionsOut[0] = leftWheelsValue;
        continuousActionsOut[1] = rightWheelsValue;
        discreteActionsOut[0] = (int)armLiftValue;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;

        if (enemyRobot != null)
        {
            Gizmos.DrawLine(transform.position, enemyRobot.transform.position);
        }
    }
}

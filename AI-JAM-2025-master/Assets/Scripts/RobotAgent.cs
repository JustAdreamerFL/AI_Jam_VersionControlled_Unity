using System;
using System.Linq;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static CollisionZoneBehaviour;

public class RobotAgent : Agent {

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
    [HideInInspector] public float weightedAverageHealth = 1f;

    public event System.EventHandler OnRobotRespawn;
    public event System.EventHandler OnRobotDie;
    private bool isStarted = false;

    //[SerializeField] private float tempAngleToEnemy;


    //Custom
    private CollisionZoneBehaviour[] weaponCollisionZones;
    //Custom

    // --------------------------- INITIALIZATION METHODS ---------------------------

    public override void Initialize() {
        cumulativeReward = 0f;
        MaxStep = 0; //TODO: 5000; - disable for now
    }

    private void Start() {
        robotMovement = gameObject.GetComponent<RobotMovement>();
        zoneComponents = GetComponentsInChildren<CollisionZoneBehaviour>();
        healthComponents = GetComponentsInChildren<IHealth>();
        //Debug.Log($"RobotAgent found {zoneComponents.Length} collision zones and {healthComponents.Length} health components.");

        FindWeapons();

        rb = gameObject.GetComponent<Rigidbody>();

        var robots = transform.parent.gameObject.GetComponentsInChildren<RobotAgent>();
        enemyRobot = robots.FirstOrDefault(r => r != this);

        rewardEditor = FindAnyObjectByType<RewardEditor>();
        behavior = GetComponent<BehaviorParameters>();

        isStarted = true;



        foreach (var zone in zoneComponents)            // pre kazdy kolider ...
        {   
            zone.OnCollision += HandleZoneCollision;    // prihlad funkciu na event aby sme v okamziku dali velku odmenu za zasah nepriatela
        }
   
    }

    private void FindWeapons()
    {
        weaponCollisionZones = new CollisionZoneBehaviour[2];
        int index = 0;
        foreach (CollisionZoneBehaviour collisionZone in zoneComponents)
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
        float weaponCollisionReward = e.DamageDealt; // damage * reward multiplier
        Debug.Log($"Calculated weapon reward: {weaponCollisionReward}, {transform.name}");
        AddReward(weaponCollisionReward * rewardEditor.weaponDamageRF);
    }

    public void HandleZoneCollision(object sender, CollisionZoneBehaviour.CollisionEventArgs e)
    {
        // You can check what we hit using e.colliderHitName (it's the tag)
        bool hitEnemy = e.colliderHitName == "Robot"; // or whatever tag you use for enemy tank

        if (!hitEnemy)
        {
            // Maybe no reward for walls or non-enemy objects
            return;
        }

        float zoneReward = 0;
        // Option 1: Use the per-zone reward that CollisionZoneBehaviour already computed
        //zoneReward = zone.RewardOfCollision; // This was set in OnCollisionHit
        // Option 2: You could also use e.Damage if you want to scale by actual damage taken
        zoneReward = e.Damage;
        // Scale
        zoneReward *= 10000f;
        // apply
        //Debug.Log(zoneReward);
        AddReward(zoneReward);
    }



    public override void OnEpisodeBegin() {
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

        if (!isStarted) {
            return;
        }

        foreach (var zone in zoneComponents) {
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

    private void Update() {
        //AddReward(0.001f);

        if (transform.localPosition.y < -1f) {
            Debug.Log("Robot fell out of arena!");

            foreach (var zone in zoneComponents) {
                zone.ChangeHealth(-0.3f);
            }

            OnRobotRespawn?.Invoke(this, System.EventArgs.Empty);
        }

        float rotationUp = Vector3.Dot(transform.up, Vector3.up);
        if (rotationUp < 0.75f) { // origin 0.2f
            Debug.Log("Robot flipped over!");

            foreach (var zone in zoneComponents) {
                zone.ChangeHealth(-0.2f);
            }

            OnRobotRespawn?.Invoke(this, System.EventArgs.Empty);
        }

        // Check if robot has died
        if (CheckIfDied()) {
            // Raise event for robot's death
            OnRobotDie?.Invoke(this, System.EventArgs.Empty);

            // End the episode with negative reward (lose)
            AddReward(-1000f);
            EndEpisode();

            // Give positive reward to enemy robot (win)
            enemyRobot?.AddReward(1000f);
            enemyRobot?.EndEpisode();
        }

        /// origin position ***AAA***
        

    }

    /// <summary>
    /// Checks if a robot has died based on the weighted average health of its components.
    /// </summary>
    /// <returns>Returns true if the robot is considered dead, otherwise false.</returns>
    private bool CheckIfDied() {

        if (healthComponents.Count() == 0) {
            return false;
        }


        // Probably not needed, because of the new weighted average method of average health

        //foreach (var healthComp in healthComponents) {
        //    switch (healthComp) {
        //        case BodyComponent bodyComp:
        //            //Debug.Log($"Body health: {healthComp.CurrentHealth}");

        //            if (healthComp.CurrentHealth <= 0.1f) {
        //                Debug.Log("Robot body destroyed!");
        //                // TODO: Method for robot dying, losing/winning logic
        //                return true;
        //            }
        //            break;
        //        case WheelComponent wheelComp:
        //            //Debug.Log($"Wheel health: {healthComp.CurrentHealth}");
        //            break;
        //        case BumperComponent bumperComp:
        //            //Debug.Log($"Bumper health: {healthComp.CurrentHealth}");
        //            break;
        //        case ArmComponent armComp:
        //            //Debug.Log($"Arm health: {healthComp.CurrentHealth}");
        //            break;
        //    }
        //}

        var sumHealth = healthComponents.Sum(e => e.CurrentHealth * e.influenceOnTotalHealth);
        var weightsHealth = healthComponents.Sum(e => e.influenceOnTotalHealth);
        weightedAverageHealth = sumHealth / weightsHealth;

        if (weightedAverageHealth <= 0.2f) {
            Debug.Log("Robot beyond repair!");
            return true;
        }

        return false;
    }

    float lastDamageOrHitTime;
    private float GetRewards_collisionCombination() {
        // original
        //var sumReward = zoneComponents.Average(z => z.RewardOfCollision);
        //return Mathf.Clamp(sumReward, -1f, 1f);

        // NEW
        // average of zone rewards for this frame
        var avgReward = zoneComponents.Average(z => z.RewardOfCollision);

        // If we actually got some reward this frame (positive or negative),
        // consider that as "impact happened"
        if (Mathf.Abs(avgReward) > 0.001f)
        {
            lastDamageOrHitTime = Time.time;
        }

        // Start with the normal collision reward
        float reward = avgReward;

        // Now add "hugging" penalty if we are just stuck together
        if (enemyRobot != null)
        {
            float distanceToEnemy = (enemyRobot.transform.position - transform.position).magnitude;

            bool veryClose = distanceToEnemy < 1.0f;           // tune threshold
            bool noRecentHit = (Time.time - lastDamageOrHitTime) > 0.3f; // no impact for 0.3s

            // optional: check relative speed to detect "just sliding"
            var myVel = rb != null ? rb.linearVelocity : Vector3.zero;
            var enemyRb = enemyRobot.GetComponent<Rigidbody>();
            var enemyVel = enemyRb != null ? enemyRb.linearVelocity : Vector3.zero;
            float relativeSpeed = (myVel - enemyVel).magnitude;
            bool slowRelativeMotion = relativeSpeed < 0.5f;    // tuning value

            if (veryClose && noRecentHit && slowRelativeMotion)
            {
                // Small continuous penalty while hugging
                reward -= 0.02f;   // tune this; not too big per step
            }
        }

        return Mathf.Clamp(reward, -10f, 1f);
    }

    private float GetRewards_ArenaPosition() {
        float distanceToCenter = this.transform.localPosition.magnitude;
        float distanceReward = Mathf.Clamp(distanceToCenter / 2f, 0f, 1f) * -2f + 1f;
        return distanceReward;
    }

    private float GetRewards_EnemyDistance() {
        if (enemyRobot == null) return 0f;
        float distanceToEnemy = (enemyRobot.transform.localPosition - this.transform.localPosition).magnitude;
        //Debug.Log("Distance to enemy: " + distanceToEnemy);
        float distanceReward = Mathf.Clamp(distanceToEnemy / 4f, 0f, 1f) * -2f + 1f;
        return distanceReward;
    }

    private float GetRewards_ArmRelease() {
        var reward = (0.02f * robotMovement.armComponent.ArmRaisingReward) + robotMovement.armComponent.ArmDropReward;
        return Mathf.Clamp(reward, 0f, 1f);
    }

    private float GetRewards_Movement() {
        //Debug.Log("Velocity: " + rb.linearVelocity.magnitude);

        float forwardDot = Vector3.Dot(rb.linearVelocity.normalized, transform.forward);
        if (forwardDot > 0.2f) // Threshold to ensure it's *mostly* forward
        {
            return Mathf.Clamp(rb.linearVelocity.magnitude / 1.7f, 0f, 1f) * 2f - 1f;
            // Small bonus for moving forward
        }
        else if (forwardDot < -0.2f) {
            return Mathf.Clamp(rb.linearVelocity.magnitude / 1.7f, 0f, 1f) * 1.5f - 1f;
            // Penalty for moving backward
        }
        return 0f;
    }

    private float GetRewards_FacingRobot() {
        if (enemyRobot != null) {
            var vectorToEnemy = enemyRobot.transform.position - this.transform.position;
            var angleToEnemy = Vector3.SignedAngle(this.transform.forward, vectorToEnemy, Vector3.up);

            //tempAngleToEnemy = angleToEnemy;

            // relativna podla toho ako moc? alebo len ano/nie situacia
            if (angleToEnemy >= -45f && angleToEnemy <= 45f) {
                return 1f;
            }
            else if (angleToEnemy >= 100f || angleToEnemy <= -100f) {
                return -1f;
            }
        }
        return 0f;
    }

    //returns average weapon collision reward
    private float GetRewards_WeaponDamage()
    {
        if (weaponCollisionZones.Length == 0)
            return 0;

        float collisionReward = zoneComponents.Average(z => z.RewardOfCollision);
        return collisionReward;
    }


    // ---------------------------- ACTIONS AND OBSERVATIONS ---------------------------

    public override void OnActionReceived(ActionBuffers actions) {
        if (!isStarted) {
            return;
        }

        // Process the actions received from the neural network
        float leftWheelsValue = actions.ContinuousActions[0];
        float rightWheelsValue = actions.ContinuousActions[1];
        int armLiftValue = actions.DiscreteActions[0];

        if (isHeuristic) {
            robotMovement.ApplyBothTrackInputsHeuristic(leftWheelsValue, rightWheelsValue);
            robotMovement.ApplyArmInput(armLiftValue);
        }
        else {
            robotMovement.ApplyBothTrackInputs(leftWheelsValue, rightWheelsValue);
            robotMovement.ApplyArmInput(armLiftValue);
        }

        if (rewardEditor != null)
        {

            float movementRewards = GetRewards_Movement() * 0.5f * Time.deltaTime;
            movementRwd = (float)System.Math.Round(movementRewards, 2) * rewardEditor.movementRF;
            AddReward(movementRwd);

            float robotFacingRewards = GetRewards_FacingRobot() * 0.5f * Time.deltaTime;
            //AddReward(robotFacingRewards * rewardEditor.facingRF);
            robotFacingRwd = (float)System.Math.Round(robotFacingRewards, 2) * rewardEditor.facingRF;
            AddReward(robotFacingRwd);

            float armReleaseReward = GetRewards_ArmRelease() * 10f * Time.deltaTime;
            //AddReward(armReleaseReward * rewardEditor.armAngleRF);
            armReleaseRwd = (float)System.Math.Round(armReleaseReward, 2) * rewardEditor.armAngleRF;
            AddReward(armReleaseRwd);

            float enemyDistanceReward = GetRewards_EnemyDistance() * Time.deltaTime;
            //AddReward(enemyDistanceReward * rewardEditor.enemyDistanceRF);
            enemyDistanceRwd = (float)System.Math.Round(enemyDistanceReward, 2) * rewardEditor.enemyDistanceRF;
            AddReward(enemyDistanceRwd);

            float arenaPositionReward = GetRewards_ArenaPosition() * Time.deltaTime;
            //AddReward(arenaPositionReward * rewardEditor.arenaPositionRF);
            arenaPositionRwd = (float)System.Math.Round(arenaPositionReward, 2) * rewardEditor.arenaPositionRF;
            AddReward(arenaPositionRwd);

            float collisionCombinationReward = GetRewards_collisionCombination() * 100f * Time.deltaTime;
            //AddReward(collisionCombinationReward * rewardEditor.collisionCombinationRF);
            collisionCombinationRwd += (float)System.Math.Round(collisionCombinationReward, 2);
            AddReward(collisionCombinationRwd);

            // TODO: If Butt Sensor not null, then add reward for butt marker visibility (maybe could surpress unwanted front collisions?)

            cumulativeReward = GetCumulativeReward();

        }
    }

    [SerializeField] private float maxEnemyDistance = 4f;   
    [SerializeField] private float maxArenaRadius = 4f;
    public override void CollectObservations(VectorSensor observations) {
        // 1) PASSIVE SENSORS (always present, not user-configurable)

        // a) Distance to enemy
        float enemyDistanceNorm = 0f;
        Vector3 relEnemyLocal = Vector3.zero;

        if (enemyRobot != null)
        {
            Vector3 toEnemy = enemyRobot.transform.position - transform.position;
            float distance = toEnemy.magnitude;
            enemyDistanceNorm = Mathf.Clamp01(distance / maxEnemyDistance);

            // enemy position in *your local space* (x,z)
            relEnemyLocal = transform.InverseTransformPoint(enemyRobot.transform.position);
        }

        // Normalize local enemy position by arena size (roughly)
        float relEnemyX = Mathf.Clamp(relEnemyLocal.x / maxArenaRadius, -1f, 1f);
        float relEnemyZ = Mathf.Clamp(relEnemyLocal.z / maxArenaRadius, -1f, 1f);

        // Add passive observations
        observations.AddObservation(enemyDistanceNorm); // 1
        observations.AddObservation(relEnemyX);         // 2
        observations.AddObservation(relEnemyZ);         // 3

        // 2) USER-CHOSEN SENSORS (up to 7 slots)
        for (int i = 0; i < observations.ObservationSize()-3; i++) {
            var observationValue = sensors?.ElementAtOrDefault(i)?.SensorValue ?? 0f;
            observations.AddObservation(observationValue);
        }
    }

    // ------------------------------- HEURISTIC -------------------------------

    public override void Heuristic(in ActionBuffers actionsOut) {
        isHeuristic = true;
        MaxStep = 0; // Disable max step limit for heuristic mode

        if (!isStarted) {
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

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;

        if (enemyRobot != null) {
            Gizmos.DrawLine(transform.position, enemyRobot.transform.position);
        }
    }
}

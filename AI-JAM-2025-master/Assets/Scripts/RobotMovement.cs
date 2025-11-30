using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class RobotMovement : MonoBehaviour {
    [SerializeField] public bool stationaryRobot = false;

    [SerializeField] private InputActionAsset inputActions;
    public InputAction leftWheelsAxis;
    public InputAction rightWheelsAxis;
    public InputAction armLiftAction;

    [SerializeField] private WheelCollider[] rightWheels;
    [SerializeField] private WheelCollider[] leftWheels;
    [SerializeField] public ArmComponent armComponent;


    private HingeJoint armJointControl;

    float motorTorqueMax = 25f;

    private void OnEnable() {
        var actionMap = inputActions.FindActionMap("Robot");
        leftWheelsAxis = actionMap.FindAction("MoveL");
        rightWheelsAxis = actionMap.FindAction("MoveR");
        armLiftAction = actionMap.FindAction("ArmLift");

        actionMap.Enable();
    }

    private void OnDisable() {
        var actionMap = inputActions.FindActionMap("Robot");
        actionMap.Disable();
    }

    private void Start() {
        armComponent = GetComponentInChildren<ArmComponent>();
        armJointControl = armComponent.GetComponent<HingeJoint>();
        armJointControl.connectedBody = gameObject.GetComponent<Rigidbody>();

        armJointControl.useMotor = false;
        armJointControl.useSpring = false;
    }

    private void FixedUpdate() {
        float slowdown = Time.fixedDeltaTime * motorTorqueMax;
        foreach (var wheel in rightWheels.Concat(leftWheels)) {
            if (Mathf.Abs(wheel.motorTorque) < slowdown) {
                wheel.motorTorque = 0f;
            }
            else {
                wheel.motorTorque = Mathf.Clamp(wheel.motorTorque - (slowdown * Mathf.Sign(wheel.motorTorque)), -motorTorqueMax, motorTorqueMax);
            }
        }
    }

    public void ApplyArmInput(int inputValue) {
        if (stationaryRobot) {
            return;
        }
        if (inputValue == 1) {
            if (!armJointControl.useSpring) {
                armJointControl.useSpring = true;
            }
            armJointControl.useMotor = true;
        }
        else {
            armJointControl.useMotor = false;
        }
    }

    public void ApplyBothTrackInputs(float leftTrackValue, float rightTrackValue) {
        //if (leftTrackValue < 0f || rightTrackValue < 0f) {
        //    leftTrackValue *= 0.8f; // Reduce backward speed for left track
        //    rightTrackValue *= 0.8f; // Reduce backward speed for right track
        //}

        if (stationaryRobot) {
            return;
        }
        ApplyTrackInput(leftWheels, leftTrackValue);
        ApplyTrackInput(rightWheels, rightTrackValue);
    }

    void ApplyTrackInput(WheelCollider[] wheels, float inputValue) {
        float torqueStep = motorTorqueMax * 0.3f * inputValue;
        float changedMotorTorque = Mathf.Clamp(wheels[0].motorTorque + torqueStep, -motorTorqueMax, motorTorqueMax);

        foreach (var wheel in wheels) {
            wheel.motorTorque = changedMotorTorque;
        }
    }

    // ------------------------------- HEURISTIC -------------------------------
    public void ApplyBothTrackInputsHeuristic(float leftTrackValue, float rightTrackValue) {
        //if (leftTrackValue < 0f || rightTrackValue < 0f) {
        //    leftTrackValue *= 0.8f; // Reduce backward speed for left track
        //    rightTrackValue *= 0.8f; // Reduce backward speed for right track
        //}

        if (stationaryRobot) {
            return;
        }
        ApplyTrackInputHeuristic(leftWheels, leftTrackValue);
        ApplyTrackInputHeuristic(rightWheels, rightTrackValue);
    }

    void ApplyTrackInputHeuristic(WheelCollider[] wheels, float inputValue) {
        foreach (var wheel in wheels) {
            wheel.motorTorque = inputValue * motorTorqueMax;
        }
    }
}

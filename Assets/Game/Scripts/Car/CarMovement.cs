using UnityEngine;
using TMPro;
using NUnit.Framework;

public class CarMovement : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI dashboardText;

    [Header("InputSetup")]
    [SerializeField] private CarInput inputReader;

    [Header("Vehicle Settings")]
    [SerializeField] private float motorForce = 4500f;
    [SerializeField] private float brakeForce = 9000f;
    [SerializeField] private float maxSteerAngle = 26f;
    [SerializeField] private float steerSpeed = 100f;
    
    [Header("Reverse System")]
    [SerializeField] private bool isReverseOn = false;

    [Header("Park Brake state")]
    [SerializeField] private bool isParkBrakeOn = false;

    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeft;
    [SerializeField] private WheelCollider frontRight;
    [SerializeField] private WheelCollider rearLeft;
    [SerializeField] private WheelCollider rearRight;

    [Header("Engine & Gear Setup")]
    [SerializeField] private float minRPM = 500f;
    [SerializeField] private float maxRPM = 3000f;
    [SerializeField] private float shiftUpRPM = 2800f;
    [SerializeField] private float shiftDownRPM = 1000f;
    [SerializeField] private float[] gearRatios = { 3.5f, 2.5f, 1.8f, 1.3f, 1.0f, 0.8f };
    [SerializeField] private int currentGear = 0; 

    [Header("Live Outputs")]
    [SerializeField] private float currentRPM;
    [SerializeField] private float totalDistanceKM = 0f;

    private Rigidbody rb;
    private Vector3 lastPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if(inputReader == null) return;

        CalculateRPM();
        HandleGearbox();

        HandleMotor();
        HandleSteering();

        TrackDistance();

        UpdateDashboardUI();
    }

    public void OnEnable()
    {
        if(inputReader == null) return;

        inputReader.OnReversePerformed += ToggleReverse;
        inputReader.OnParkPerformed += TogglePark;
    }

    public void OnDisable()
    {
        if(inputReader == null) return;

        inputReader.OnReversePerformed -= ToggleReverse;
        inputReader.OnParkPerformed -= TogglePark;
    }

    private void ToggleReverse()
    {
        isReverseOn = !isReverseOn;
        currentGear = 0; 
    }

    public void TogglePark()
    {
        isParkBrakeOn = !isParkBrakeOn;
    }

    private void CalculateRPM()
    {
        if (rb == null) return;

        float forwardSpeed = Mathf.Abs(Vector3.Dot(rb.linearVelocity, transform.forward));

        float targetRPM = minRPM + (forwardSpeed * gearRatios[currentGear] * 100f);

        currentRPM = Mathf.Clamp(targetRPM, minRPM, maxRPM);
    }

    private void HandleGearbox()
    {
        if (isReverseOn) return;

        if (currentRPM > shiftUpRPM && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
        }
        else if (currentRPM < shiftDownRPM && currentGear > 0)
        {
            currentGear--;
        }
    }

    private void HandleMotor()
    {
        float currentThrottle = inputReader.ThrottleInput;
        float currentBrake = inputReader.BrakeInput;

        float torqueMultiplier = 1f;
        if (currentRPM >= maxRPM)
        {
            torqueMultiplier = 0f;
        }
        else if (currentRPM > shiftUpRPM)
        {
            float t = (currentRPM - shiftUpRPM) / (maxRPM - shiftUpRPM);
            torqueMultiplier = Mathf.Lerp(1f, 0f, t);
        }

        float finalTorque = currentThrottle * motorForce * gearRatios[currentGear] * torqueMultiplier;

        if (isReverseOn)
        {
            frontLeft.motorTorque = currentThrottle * -motorForce;
            frontRight.motorTorque = currentThrottle * -motorForce;
        } 
        else
        {
            frontLeft.motorTorque = finalTorque;
            frontRight.motorTorque = finalTorque;
        }

        float appliedBrakeTorque = 0f;
        if (isParkBrakeOn)
        {
            appliedBrakeTorque = brakeForce;
            frontLeft.motorTorque = 0f;
            frontRight.motorTorque = 0f;
        }
        else if (inputReader.BrakeInput > 0.05f)
        {
            appliedBrakeTorque = currentBrake * brakeForce;
        } else if (currentThrottle < 0.01f)
        {
            appliedBrakeTorque = 450f * gearRatios[currentGear];
        }

        frontLeft.brakeTorque = appliedBrakeTorque;
        frontRight.brakeTorque = appliedBrakeTorque;
        rearLeft.brakeTorque = appliedBrakeTorque * 0.7f;
        rearRight.brakeTorque = appliedBrakeTorque * 0.7f;
    }

    private void HandleSteering()
    {
        float targetSteerAngle = inputReader.SteerInput * maxSteerAngle;

        float currentSteerAngle = Mathf.MoveTowards(frontLeft.steerAngle, targetSteerAngle, steerSpeed * Time.fixedDeltaTime);

        frontLeft.steerAngle = currentSteerAngle;
        frontRight.steerAngle = currentSteerAngle;
    }

    private void TrackDistance()
    {
        float distanceThisFrameMeters = Vector3.Distance(transform.position, lastPosition);

        float distanceThisFrameKM = distanceThisFrameMeters/1000f;

        totalDistanceKM += distanceThisFrameKM;

        lastPosition = transform.position;
    }

    private void UpdateDashboardUI()
    {
        if(dashboardText == null || rb == null) return;

        float forwardSpeedMPS = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedKMH = Mathf.Abs(forwardSpeedMPS) * 3.6f;

        string displayGear = isReverseOn ? "R" : (currentGear + 1).ToString();
        if(isParkBrakeOn) displayGear = "P";

        dashboardText.text = $"SPEED: {speedKMH:F0} KM/H\n" + 
                             $"RPM: {currentRPM:F0}\n" +
                             $"GEAR: {displayGear}\n" + 
                             $"Distance: {totalDistanceKM:F1}";
    }
}
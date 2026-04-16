using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
[AddComponentMenu("Game/Boats/BoatController")]
public class BoatController : MonoBehaviour
{

    public SplineContainer currentSegment;
    public SplineContainer queuedSegment;
    public SplineContainer pendingSegment;

    [Header("Datafiles")]
    public LockData lockData;
    public JuntionData juntionData;
    public BoatData BoatData;

    [Header("Movement Settings")]
    [Range(0, 4)]
    public float speed = 2.0f;
    [Range(0, 1)]
    public float progress = 0f;

    [Header("Squeeze/Passing Settings")]
    public float currentOffset = 0f;
    public float targetOffset = 0f;
    public float squeezeSpeed = 1.5f;

    [Header("Physics Settings")]
    public float splineGravityStrength = 10f;
    public float forwardForceStrength = 5f;
    public float rotationSpeed = 5f;

    [Header("Mooring")]
    [SerializeField] private SplineContainer originalSpline;
    [SerializeField] private float originalProgress;
    private MooringSpot currentMooringSpot;

    private Rigidbody rb;

    void Start()
{
    rb = GetComponent<Rigidbody>();
    rb.useGravity = false;
    rb.linearDamping = 2f;
    rb.angularDamping = 5f;
    rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    // Fixed teleporter
    if (currentSegment != null)
    {
        // EvaluatePosition returns world position directly when using SplineContainer
        Vector3 worldStart = currentSegment.EvaluatePosition(0f);
        transform.position = worldStart;
        
        // Optional: Also reset progress to 0
        progress = 0f;
    }
}

    void Update()
    {
        if (currentSegment == null) return;
        //if (BoatData.IsMoored && currentMooringSpot.moorPoint >= progress && currentSegment == currentMooringSpot.mooringline) speed = 0.0f;

        float splineLength = currentSegment.CalculateLength();
        if (splineLength > 0)
        {
            float movement = (speed * Time.deltaTime) / splineLength;
            progress += BoatData.HeadingUp ? movement : -movement;
        }

        if (BoatData.HeadingUp && progress >= 1f)
            Splineswitch();
        else if (!BoatData.HeadingUp && progress <= 0f)
            Splineswitch();

        CheckForIncomingBoats();


    }

    void FixedUpdate()
    {
        if (currentSegment == null) return;

        currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.fixedDeltaTime * squeezeSpeed);

        // Get spline position and direction at current progress
        float3 worldPos;
        float3 worldTangent;
        float3 worldUp;
        currentSegment.Evaluate(progress, out worldPos, out worldTangent, out worldUp);

        Vector3 forward = Vector3.Normalize((Vector3)worldTangent);
        if (!BoatData.HeadingUp) forward = -forward;

        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 targetPosition = (Vector3)worldPos + (right * currentOffset);

        // Pull boat toward spline (the "gravity")
        Vector3 toTarget = targetPosition - transform.position;
        rb.AddForce(toTarget * splineGravityStrength, ForceMode.Acceleration);

        // Push boat forward along spline
        rb.AddForce(forward * forwardForceStrength, ForceMode.Acceleration);

        // Smooth rotation to face forward
        if (forward != Vector3.zero)
        {
            Vector3 flatForward = new Vector3(forward.x, 0, forward.z);
            if (flatForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatForward);
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
            }
        }
    }

    void CheckForIncomingBoats()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 1.5f, transform.forward, out hit, 10f))
        {
            if (hit.collider.CompareTag("Boat") && hit.collider.gameObject != this.gameObject)
            {
                targetOffset = 1.2f;
                return;
            }
        }
        targetOffset = 0f;
    }

    void Splineswitch()
    {
        if (queuedSegment != null)
        {
            currentSegment = queuedSegment;
            progress = BoatData.HeadingUp ? 0f : 1f;
            queuedSegment = null;
            if (pendingSegment != null)
            {
                queuedSegment = pendingSegment;
                pendingSegment = null;
            }
        }
        else
        {
            progress = BoatData.HeadingUp ? 1f : 0f;
        }
    Debug.Log("starting" + currentSegment);
    }



}
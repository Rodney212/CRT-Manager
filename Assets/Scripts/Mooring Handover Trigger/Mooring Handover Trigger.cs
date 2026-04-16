using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class MooringHandoverTrigger : MonoBehaviour
{

    public MooringSpot mooring;
    public LockData lockData;
    public bool isup;
    public bool lockSide;
    
    [Tooltip("If true, only boats finishing their spline (progress > 0.9) will trigger this.")]
    public bool triggerAtEndOfPath = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("triggered");
        BoatController boat = other.GetComponent<BoatController>();
        bool inbound = isup != boat.BoatData.HeadingUp;
        mooring.Trigger(boat, inbound, lockSide);

        //if (isup != boat.BoatData.HeadingUp) mooring.Trigger(boat, notLockSide);
        //else mooring.Outbound(boat);
    }

    
    
}
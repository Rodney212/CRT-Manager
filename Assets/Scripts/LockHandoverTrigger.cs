using UnityEngine;
using UnityEngine.Splines;

public class LockHandoverTrigger : MonoBehaviour
{
    public SplineContainer nextSplineSegment;
    public LockData lockData;
    
    [Tooltip("If true, only boats finishing their spline (progress > 0.9) will trigger this.")]
    public bool triggerAtEndOfPath = true;

    private void OnTriggerEnter(Collider other)
    {
        var boat = other.GetComponent<BoatController>();
        
        if (boat != null)
        {
            // DIRECTIONAL CHECK: 
            // If this is the 'Exit' trigger of a canal segment, 
            // the boat must be nearly finished with its current spline.
            if (triggerAtEndOfPath && boat.progress > 0.8f)
            {

                boat.lockData = lockData;
                splineControl(boat);
                //Debug.Log("Handing over to next segment: " + nextSplineSegment.name);
            }
            // If this is an 'Entrance' trigger (e.g. boat coming from opposite way)
            else if (!triggerAtEndOfPath && boat.progress < 0.2f)
            {
                boat.queuedSegment = nextSplineSegment;
                //Debug.Log("Handing over to previous segment: " + nextSplineSegment.name);
            }
        }
    }
    void splineControl(BoatController boat) //moved 16/4/26
    {
        boat.BoatData.NeedToMoor = true;
        

        boat.queuedSegment = lockData.LockSpline;
        if (boat.BoatData.HeadingUp) boat.pendingSegment = lockData.UpSpline;
        else boat.pendingSegment = lockData.DownSpline;
    }
    
}
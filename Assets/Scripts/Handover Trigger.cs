using UnityEngine;
using UnityEngine.Splines;

public class HandoverTrigger : MonoBehaviour
{

    public BoatController boat;
    public SplineContainer spline;
public SplineData SplineData;


    


private void OnTriggerEnter(Collider other)
{
    boat = other.GetComponent<BoatController>();
    
    if (boat != null)
    {
        splineControl();
    }
}
    void splineControl()
    {
        if (boat.BoatData.HeadingUp){
        boat.queuedSegment = SplineData.thisSegment;    
        boat.pendingSegment = SplineData.nextSegment;
        }
        else if (boat.BoatData.HeadingUp == false)
        {
            boat.queuedSegment = SplineData.previousSegment;
        } 
    }
    
}
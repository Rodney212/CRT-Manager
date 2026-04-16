using System;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class MooringSpot : MonoBehaviour
{
    [Header("Identification")]
    public int spotID;
    public string spotName = "Mooring Spot";
    public bool isLock = false;
    
    [Header("Status")]
    public bool IsOccupied = false;
    
    [Header("Optional")]
    public float mooringPriority = 0f; // For future use (higher = preferred)
    public SplineContainer mooringline;
    public SplineContainer straightSpline;
    public SplineContainer upSpline;
    public SplineContainer downSpline;
    public float moorPoint;




    public void Trigger(BoatController boat, bool inbound, bool lockSide)
    {
        if(inbound && boat != null)
        {
            if(isLock == true && lockSide == false) boat.BoatData.NeedToMoor = true;
            //Debug.Log(boat.BoatData.NeedToMoor);
            if (inbound) Inbound(boat);
            
            //Debug.Log(boat.currentSegment, boat.queuedSegment);
        }
        else if (boat != null) Outbound(boat);

    }
    
    public void Inbound(BoatController boat)
    {

        IsOccupied = true; //testonly
        if (boat.BoatData.NeedToMoor) boat.queuedSegment = mooringline;
        else boat.queuedSegment = straightSpline;
        //Debug.Log(boat.queuedSegment);
        boat.BoatData.IsMoored = true;
        boat.speed *= 0.5f;
        if (boat.currentSegment == downSpline) boat.pendingSegment = upSpline;
        else boat.pendingSegment = downSpline;
    }
    
    public void Outbound(BoatController boat)
    {
        //Debug.Log("Outbound Triggered");
        IsOccupied = false; //TESTONLY
        boat.BoatData.IsMoored = false;
        boat.BoatData.NeedToMoor = false;
        boat.speed *= 2;
    }
    
    // Visual feedback in editor
    private void OnDrawGizmos()
    {
        Gizmos.color = IsOccupied ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
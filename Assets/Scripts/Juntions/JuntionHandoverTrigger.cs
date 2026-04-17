using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;


public class JuntionHandoverTrigger : MonoBehaviour
{
    public int BranchNumber;
    public JuntionData juntionData;
    
    [Tooltip("If true, only boats finishing their spline (progress > 0.9) will trigger this.")]
    public bool triggerAtEndOfPath = true;

    private void OnTriggerEnter(Collider other)
    {
        var boat = other.GetComponent<BoatController>();
        
        if (boat != null)
        {
            Debug.Log("collision detected at:" + juntionData.JunctionName);
            // DIRECTIONAL CHECK: 
            // If this is the 'Exit' trigger of a canal segment, 
            // the boat must be nearly finished with its current spline.
            boat.juntionData = juntionData;
            splineControl(BranchNumber, boat);

        }
    }

    public void splineControl(int fromsection, BoatController boat) //moved 16/04/26
    {
        Dictionary<string, int> Route = new Dictionary<string, int>();
        Route.Add("Ruby", 2); //testing only
        

        Route.TryGetValue(juntionData.JunctionName, out int tosection);
        juntionData.nextSection.TryGetValue(tosection, out boat.pendingSegment);
        
        if(fromsection != tosection)
        {
            if (boat.BoatData.HeadingUp == false)
            {
                int x = fromsection;
                int y = tosection;
                fromsection = y;
                tosection = x;
            }

            int pathchosen = (fromsection * 10) + tosection;
            Debug.Log($"pathchosen: {pathchosen}");
            juntionData.route.TryGetValue(pathchosen, out boat.queuedSegment);
        }
    }

    
}
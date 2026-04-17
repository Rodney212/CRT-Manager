using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class JunctionManager : MonoBehaviour
{
    public JuntionData myData;

    public JuntionHandoverTrigger trigger1;
    public JuntionHandoverTrigger trigger2;
    public JuntionHandoverTrigger trigger3;
    void Start()
    {
        trigger1.BranchNumber = 1;
        trigger2.BranchNumber = 2;
        trigger3.BranchNumber = 3;
    }

    public void Triggered(BoatController boat, int fromsection)
    {
            Debug.Log("Boat at" + myData.JunctionName);
            splineControl(fromsection, boat);

    }

        public void splineControl(int fromsection, BoatController boat) //moved 16/04/26
    {
        Dictionary<string, int> Route = new Dictionary<string, int>(); // testing only
        Route.Add("Ruby", 2); //testing only
        

        Route.TryGetValue("Ruby", out int tosection);
        myData.nextSection.TryGetValue(tosection, out boat.pendingSegment);
        
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
            myData.route.TryGetValue(pathchosen, out boat.queuedSegment);
        }
    }
}

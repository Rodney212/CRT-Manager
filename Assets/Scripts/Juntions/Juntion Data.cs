using System;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class JuntionData : MonoBehaviour
{

    [Header("Identity")]
    public String JunctionName;
    public String CanalName;
    [Header("connected Splines")]
    

    [Header("Juntion Splines")]
    public SplineContainer Spline1to2;
    public SplineContainer Spline1to3;
    public SplineContainer Spline2to3;
    [Header("Section Splines")]
    public SplineContainer Spline1;
    public SplineContainer Spline2;
    public SplineContainer Spline3;

    public Dictionary<int, SplineContainer> route;
    public Dictionary<int, SplineContainer> nextSection;

        private void Awake()
        {
            // 2. Initialize and fill it here, after Unity has linked the Splines
            route = new Dictionary<int, SplineContainer>
            {
                { 12, Spline1to2 }, // Using 12 as a key for Spline 1 to 2
                { 13, Spline1to3 },
                { 23, Spline2to3 }
            };

            nextSection = new Dictionary<int, SplineContainer>
            {
                { 1, Spline1 }, // Using 12 as a key for Spline 1 to 2
                { 2, Spline2 },
                { 3, Spline3 }
            };
        }
    

}
using System;
using UnityEngine;
using UnityEngine.Splines;
public class LockData : MonoBehaviour
{
    [Header("Identity")]
    public int LockNumber;
    public String Section;
    public String CanalName;
    [Header("Splines")]
    public SplineContainer LockSpline;
    public SplineContainer UpSpline;
    public SplineContainer DownSpline;
    public SplineContainer PersonDownPath;
    public SplineContainer PersonUpPath;
    

}
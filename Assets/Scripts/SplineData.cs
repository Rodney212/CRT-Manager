// Assets/Scripts/SplineData.cs
using UnityEngine;
using UnityEngine.Splines;

public class SplineData : MonoBehaviour
{
    [Header("Identity")]
    public int objectID;
    public string functionalLocation;
    public string sapCanalCode;
    public string canalName;
    public string region;

    [Header("Navigation")]
    public string sapWidth;
    public string navStatus;
    public int lockQuantity;

    [Header("Technical")]
    public string globalID;

    [Header("Segment Links")]
    public SplineContainer thisSegment;
    public SplineContainer previousSegment;
    public SplineContainer nextSegment;
}
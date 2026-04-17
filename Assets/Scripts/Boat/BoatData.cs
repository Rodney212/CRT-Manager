using UnityEngine;

[CreateAssetMenu(fileName = "NewBoatData", menuName = "CanalManager/BoatData")]
public class BoatData : ScriptableObject
{
    [Header("Identity")]
    public string boatName;
    public float boatLength;
    public float maxSpeed;

    [Header("Economy")]
    public float fuelLevel;
    public int cargoTons;
    
    [Header("State")]
    public bool hasCrewOnBoard;
    public float engineHealth = 1.0f;
    public bool HeadingUp;

     public bool NeedToMoor = false;
    public bool IsMoored = false;
    public int CurrentMooringSpotID = -1;
}
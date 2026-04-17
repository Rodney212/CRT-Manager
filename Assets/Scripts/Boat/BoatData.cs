using UnityEngine;

[CreateAssetMenu(fileName = "NewBoatData", menuName = "CanalManager/BoatData")]
public class BoatData : ScriptableObject
{
    [Header("Identity")]
    public string boatName; //editable
    public float boatLength; //not editable
    public float maxSpeed; //not editable

    [Header("Economy")]
    public float fuelLevel; //not editable
    public int cargoTons; //not editable
    
    [Header("State")]
    public bool hasCrewOnBoard; //not editable
    public float engineHealth = 1.0f; //not editable
    public bool HeadingUp; //not editable

     public bool NeedToMoor = false; //editable
    public bool IsMoored = false; //not editable
    public int CurrentMooringSpotID = -1; //not editable
}
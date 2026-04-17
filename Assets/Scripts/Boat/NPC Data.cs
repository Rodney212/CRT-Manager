using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Data", menuName = "CanalManager/NPCData")]
public class NPCData : ScriptableObject
{
    [Header("Identity")]
    public string firstName; //editable
    public string lastName; //editable
    public float related; //not editable


    [Header("Economy")]
    public int Money; //not editable    
    [Header("Skill")]
    public bool skilLevel;
}
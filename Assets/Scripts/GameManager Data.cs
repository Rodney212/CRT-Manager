using UnityEngine;

[CreateAssetMenu(fileName = "New GameManager Data", menuName = "CanalManager/GameMagerData")]
public class GameManagerData : ScriptableObject
{
    [Header("Day/Time")]
    public float time;
    public int date; //editable

}
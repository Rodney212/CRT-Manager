using UnityEngine;

public class gameManager : MonoBehaviour
{
    [Header("DayNight Cycle")]
    public int day;

    public float gameSpeed;

    public GameManagerData Data;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Data.time += gameSpeed;
        if (Data.time >= 24.0) endOfDay();

    }

    private void endOfDay()
    {
    Data.time = 0; 
        

    }



}

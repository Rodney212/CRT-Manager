using UnityEngine;

public class BoatClickable : MonoBehaviour
{
    public BoatUiController ui;
    public BoatController boat;

    void OnEnable()
    {

    }
    // This triggers automatically when the object is clicked
    public void OnClick()
    {
        Debug.Log("Object clicked!");
        GetComponent<Renderer>().material.color = Color.red; // Change color as an example
        ui.Display(boat);
    }
}
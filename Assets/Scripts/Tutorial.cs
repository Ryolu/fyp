using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Available states of the tutorial
/// </summary>
public enum STATE
{
    CONTROLS,
    ENEMIES,
    POWERUPS
}

/// <summary>
/// Tutorial class just to handle the tutorial part of the game
/// </summary>
public class Tutorial : MonoBehaviour
{
    public STATE state = STATE.CONTROLS;
    public GameObject[] overlays;
    public AudioSource select;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("z"))
        {
            // always play this sound when Z is pressed
            select.Play();

            // increment the state of the tutorial phase
            if (state < STATE.POWERUPS)
                state++;
        }

        // sets the overlays to active based on the current state of the tutorial
        for(int i = 0; i < overlays.Length; i++)
        {
            if (i != (int)state)
                overlays[i].SetActive(false);
            else
                overlays[i].SetActive(true);
        }
    }
}

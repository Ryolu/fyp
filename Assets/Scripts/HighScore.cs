using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScore : MonoBehaviour
{
    public bool glow = false;
    
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        // oscillate between green and white if its supposed to
        if (glow)
        {
            float t = (Mathf.Sin(Time.time * 5) + 1) / 2;
            text.color = Color.Lerp(Color.white, Color.green, t);
        }
    }

    public void White()
    {
        // reset back to white
        glow = false;
        if(text)
            text.color = Color.white;
    }
}

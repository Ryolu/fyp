using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public Sprite stump;

    int hp = 2;

    // Update is called once per frame
    public void Hit()
    {
        // reduce thee plant health
        hp--;

        // changes the sprite to the stump if it still has hp
        if (hp > 0)
        {
            GetComponent<SpriteRenderer>().sprite = stump;
            GetComponent<CircleCollider2D>().radius = 0.25f;

        }
        // destroys it otherwise
        else
            Destroy(gameObject);
    }
}

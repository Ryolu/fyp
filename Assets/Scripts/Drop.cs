using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DROP_TYPE
{
    TIME,
    RICOCHET
}

/// <summary>
/// Powerup drops that benefit the player upon pickup.
/// </summary>
public class Drop : MonoBehaviour
{
    public DROP_TYPE type;

    /// <summary>
    /// Set the drop's sprite.
    /// </summary>
    /// <param name="image">Sprite for the drop.</param>
    public void SetImage(Sprite image)
    {
        GetComponent<SpriteRenderer>().sprite = image;
    }
}

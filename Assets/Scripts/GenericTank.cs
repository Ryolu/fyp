using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic tank class.
/// </summary>
public class GenericTank : MonoBehaviour
{
    public GameObject explosion;
    public AudioSource sound;

    /// <summary>
    /// Create the explosion upon death.
    /// </summary>
    public void Die()
    {
        Destroy(gameObject);
        Instantiate(explosion, transform.position, Quaternion.identity);
        sound.Play();
    }
}

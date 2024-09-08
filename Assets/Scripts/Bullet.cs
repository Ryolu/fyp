using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bullet class for all tank projectiles.
/// </summary>
public class Bullet : MonoBehaviour
{
    public int bouncesLeft;

    Rigidbody2D rb;
    // latest non zero velocity holder variable to circumvent a glitch
    Vector2 nonZeroVel;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // updates holder variable with the last nonzero velocity
        if (rb.velocity != Vector2.zero)
            nonZeroVel = rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "wall")
        {
            bouncesLeft--;

            // destroy bullet after colliding with a wall when having no bounces
            if (bouncesLeft < 0)
                Destroy(gameObject);
            else
            {
                // reflect bullet based on normal of the surface it bounced off of
                Vector2 velocity = Vector2.Reflect(nonZeroVel, collision.GetContact(0).normal);
                rb.velocity = velocity;
            }
        }
        else if (collision.collider.tag == "tank")
        {
            GameObject tank = collision.collider.gameObject;

            // add score
            if (tank.GetComponent<Enemy>())
                tank.GetComponent<Enemy>().Killed();

            // game over
            if (tank.GetComponent<Player>())
                tank.GetComponent<Player>().Killed();

            // animate death
            tank.GetComponent<GenericTank>().Die();

            Destroy(gameObject);
        }
        else if (collision.collider.tag == "plant")
        {
            collision.collider.gameObject.GetComponent<Plant>().Hit();
            Destroy(gameObject);
        }
        else if (collision.collider.tag == "drop" || collision.collider.tag == "bullet")
            Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Different types of enemies.
/// </summary>
public enum ENEMY_TYPE
{
    STATIC,
    PATROL,
    ELITE
}

public class Enemy : MonoBehaviour
{
    public GameObject drop;
    public GameObject dropParent;
    public GameObject dropArtHolder;
    public GameObject bullet;
    public GameObject magazine;
    public LayerMask playerLayer;
    public ENEMY_TYPE type;

    float rayLength = 0.15f;
    float moveSpeed = 1f;

    float fireRate = 2f;
    float fireTimer = 0f;
    bool fired = false;

    bool alerted = false;
    Quaternion originalDir;

    // Update is called once per frame
    void Update()
    {
        // find enemy direction
        Vector2 front = transform.up;

        if (type == ENEMY_TYPE.PATROL || type == ENEMY_TYPE.ELITE)
        {
            if (type == ENEMY_TYPE.ELITE)
            {
                // raycasting
                RaycastHit2D frontCheck = Physics2D.Raycast(transform.position, front, Mathf.Infinity, LayerMask.GetMask("player", "wall"));
                RaycastHit2D leftCheck = Physics2D.Raycast(transform.position - transform.up * 0.25f, -transform.right, Mathf.Infinity, LayerMask.GetMask("player", "wall"));
                RaycastHit2D rightCheck = Physics2D.Raycast(transform.position - transform.up * 0.25f, - transform.right, Mathf.Infinity, LayerMask.GetMask("player", "wall"));

                // if player hasnt been detected
                if (!alerted)
                {
                    if (leftCheck.collider.name.Contains("Player"))
                    {
                        alerted = true;
                        originalDir = transform.rotation;
                        transform.rotation *= Quaternion.Euler(0, 0, 90);
                    }
                    else if (rightCheck.collider.name.Contains("Player"))
                    {
                        alerted = true;
                        originalDir = transform.rotation;
                        transform.rotation *= Quaternion.Euler(0, 0, 90);
                    }
                    else if (frontCheck.collider.name.Contains("Player"))
                    {
                        alerted = true;
                        originalDir = transform.rotation;
                    }
                }
                else
                {
                    // if player cannot be found
                    if (!leftCheck.collider.name.Contains("Player") &&
                        !rightCheck.collider.name.Contains("Player") &&
                        !frontCheck.collider.name.Contains("Player"))
                    {
                        alerted = false;
                        transform.rotation = originalDir;
                    }
                }
            }
            
            // patrol and elite type enemies will patrol an area when not alerted
            if(!alerted)
            {
                // raycast a short distance forward
                RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * 0.6f, front, rayLength, LayerMask.GetMask("enemy", "plant", "wall"));
                // turn the enemy tank when it hits a wall, plant or another enemy
                if (hit.collider != null)
                    transform.Rotate(0, 0, 180);
            }

            // move forward if it can
            transform.position += transform.up * moveSpeed * Time.deltaTime;
        }

        // allow the walls to block enemy vision
        RaycastHit2D lineOfSight = Physics2D.Raycast(transform.position, front, Mathf.Infinity, LayerMask.GetMask("player", "wall"));

        // only fire when the player is in line of sight
        if (lineOfSight.collider != null && lineOfSight.collider.gameObject.name.Contains("Player"))
            Fire();

        // firing logic
        if (fired)
        {
            if (fireTimer < fireRate)
                fireTimer += Time.deltaTime;
            else
            {
                fired = false;
                fireTimer = 0;
            }
        }
    }

    /// <summary>
    /// Adds score, might spawn a drop.
    /// </summary>
    public void Killed()
    {
        // stronger enemy tanks give more score
        transform.parent.GetComponent<EnemyManager>().AddScore(1000 * ((int)type + 1));
        // 15% chance of a power up spawning after killing an enemy, as long as it's not the last enemy
        // stronger enemies have a higher chance of dropping a powerup
        if(Random.value < 0.15f * ((int)type + 1) && transform.parent.childCount > 1)
        {
            GameObject newObj = Instantiate(drop, transform.position, Quaternion.identity, dropParent.transform);
            Drop newDrop = newObj.GetComponent<Drop>();
            // 50% chance of either a clock or ricochet
            newDrop.type = Random.value < 0.5f ? DROP_TYPE.TIME : DROP_TYPE.RICOCHET;
            newDrop.SetImage(dropArtHolder.GetComponent<ArtHolder>().sprites[(int)newDrop.type]);
        }
    }

    /// <summary>
    /// Try to fire a bullet.
    /// Fires a bullet based on the type of enemy that fired it.
    /// </summary>
    private void Fire()
    {
        if(!fired)
        {
            fired = true;
            // create a new bullet and give it the correct params
            GameObject newBullet = Instantiate(bullet, transform.position + transform.up * 0.75f, transform.rotation, magazine.transform);
            float multiplier = type == ENEMY_TYPE.STATIC ? 1f : 3f;
            newBullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector3(0, moveSpeed * multiplier, 0));
            newBullet.GetComponent<Bullet>().bouncesLeft = 0;
        }
    }
}

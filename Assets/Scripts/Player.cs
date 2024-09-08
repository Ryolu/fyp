using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject bullet;
    public GameManager GM;
    public AudioSource shootSound;

    Animator animator;
    float moveSpeed = 2f;

    public bool fired = false;
    float bulletVelocity = 3f;
    float fireRate = 0.2f;
    float fireTimer = 0f;
    int numBounces = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        shootSound = GM.shootSound;
        GetComponent<GenericTank>().sound = GM.explosion;
    }

    void FixedUpdate()
    {
        // directional stuff
        animator.SetBool("isMoving", false);
        Vector3 dir = transform.eulerAngles;

        if (Input.GetKey("up") && Input.GetKey("left"))
            dir = new Vector3(0, 0, 45);
        else if (Input.GetKey("up") && Input.GetKey("right"))
            dir = new Vector3(0, 0, 315);
        else if (Input.GetKey("down") && Input.GetKey("left"))
            dir = new Vector3(0, 0, 135);
        else if (Input.GetKey("down") && Input.GetKey("right"))
            dir = new Vector3(0, 0, 225);
        else if (Input.GetKey("up"))
            dir = new Vector3(0, 0, 0);
        else if (Input.GetKey("down"))
            dir = new Vector3(0, 0, 180);
        else if (Input.GetKey("left"))
            dir = new Vector3(0, 0, 90);
        else if (Input.GetKey("right"))
            dir = new Vector3(0, 0, 270);

        // animate and update position if it's moving
        if (Input.GetKey("up") || Input.GetKey("down") || Input.GetKey("left") || Input.GetKey("right"))
        {
            animator.SetBool("isMoving", true);
            transform.eulerAngles = dir;
            transform.position += transform.up * moveSpeed * Time.deltaTime;
        }

        // firing
        if (Input.GetKey("z"))
            Fire();

        if(fired)
        {
            if(fireTimer < fireRate)
                fireTimer += Time.deltaTime;
            else
            {
                fired = false;
                fireTimer = 0;
            }
        }
    }

    /// <summary>
    /// Fire the bullet
    /// </summary>
    void Fire()
    {
        if (!fired)
        {
            GameObject newBullet = Instantiate(bullet, transform.position + transform.up * 0.75f, transform.rotation, GM.magazine.transform);
            newBullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector3(0, bulletVelocity, 0));
            newBullet.GetComponent<Bullet>().bouncesLeft = numBounces;
            fired = true;
            shootSound.Play();
        }
    }

    /// <summary>
    /// Update player variables when going to the next level
    /// </summary>
    public void OnLevelNext()
    {
        moveSpeed += 0.1f;
    }

    /// <summary>
    /// Update player position
    /// </summary>
    /// <param name="xPos">New X coordinate</param>
    /// <param name="yPos">New X coordinate</param>
    public void MovePlayer(int xPos, int yPos)
    {
        transform.position = new Vector3(xPos, yPos, 0);
    }

    /// <summary>
    /// Tell GM that the player has died
    /// </summary>
    public void Killed()
    {
        GM.Lose();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // handle powerups
        if (collision.collider.tag == "drop")
        {
            Destroy(collision.collider.gameObject);
            if (collision.collider.gameObject.GetComponent<Drop>().type == DROP_TYPE.TIME)
                GM.levelManager.levelTimer += 15f;
            else if (collision.collider.gameObject.GetComponent<Drop>().type == DROP_TYPE.RICOCHET)
                numBounces++;
        }
    }
}

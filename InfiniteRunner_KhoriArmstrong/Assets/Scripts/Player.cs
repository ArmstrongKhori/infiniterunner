using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public float health = 100;
    public float maxHealth = 100;
    public int canJump = 0;
    public bool isDead = false;



    Rigidbody2D rb;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isDead)
        {
            int tilt = (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? +1 : 0);
            if (tilt != 0)
            {
                GameManager gm = GameManager.Instance();
                transform.Translate(new Vector3(tilt * gm.PLAYERMOVEMENTSPEED * Time.deltaTime * Time.deltaTime, 0, 0));
            }
        }
    }

    private void LateUpdate()
    {
        if (!isDead)
        {
            if (canJump > 0 && Input.GetKeyDown(KeyCode.Space))
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
                //
                rb.AddForce(new Vector2(0f, 0.5f), ForceMode2D.Impulse);

                transform.Translate(rb.velocity * Time.deltaTime);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (!isDead)
        {
            health += 1 * Time.deltaTime;
            health = Mathf.Min(health, maxHealth);
        }
    }


    void OnCollisionStay2D(Collision2D col)
    {
        SolidBlock sb = col.gameObject.GetComponent<SolidBlock>();
        if (sb != null)
        {
            switch (sb.type)
            {
                case SolidBlock.BlockTypes.Solid:
                    // *** Nothing really.
                    break;
                case SolidBlock.BlockTypes.Damage:
                    TakeDamage(30 * Time.deltaTime);
                    break;
                case SolidBlock.BlockTypes.Death:
                    TakeDamage(100);
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        SolidBlock sb = col.gameObject.GetComponent<SolidBlock>();
        if (sb != null)
        {
            canJump++;
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        SolidBlock sb = col.gameObject.GetComponent<SolidBlock>();
        if (sb != null)
        {
            canJump--;
        }
    }

    private void TakeDamage(float v)
    {
        GameManager gm = GameManager.Instance();


        health -= v * (1 + 0.3f * (1.0f * Mathf.Min(gm.obstacleCounter, gm.LEVELCAP) / gm.LEVELUPTHRESHOLD));
        //
        if (health <= 0)
        {
            isDead = true;
            //
            gm.GameOver();
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        EventArea ev = col.gameObject.GetComponent<EventArea>();
        if (ev != null)
        {
            GameManager gm = GameManager.Instance();
            switch (ev.type)
            {
                case EventArea.EventType.None:
                    break;
                case EventArea.EventType.Death:
                    gm.GameOver();
                    break;
                case EventArea.EventType.Bump:
                    transform.Translate(new Vector3(-5 * gm.runSpeed * Time.deltaTime * Time.deltaTime, 0, 0));
                    break;
            }
        }
    }
}

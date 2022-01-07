﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private AudioSource SFX;
    public AudioClip[] effects;
    public Text ScoreText, HealthText, MissileNumber;
    public GameObject GameOverScreen, projectile, missile, heart;
    public int score, health, currScore, currHealth, missileCount;
    public float playerSpeed, shootVolume, explosionVolume;
    private Rigidbody2D rb2d;
    public bool CanTakeDamage;
    private Coroutine temporaryImmunity = null;
    private Animator anim;

    private IEnumerator tempImmune(float timer)
    {
        this.GetComponent<SpriteRenderer>().color = new Color(0, 132, 1, 0.5F);
        yield return new WaitForSeconds(timer);
        this.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255, 255);
        this.GetComponent<Collider2D>().isTrigger = false;
        CanTakeDamage = true;
        yield break;
    }


    void Start()
    {
        anim = GetComponent<Animator>();
        SFX = GetComponent<AudioSource>();
        rb2d = GetComponent<Rigidbody2D>();
        health = 5;
        missileCount = 5;
        HealthText.text = "Lives: ";
        score = 0;
        CanTakeDamage = true;
    }

    void FixedUpdate()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");
        rb2d.velocity = new Vector2(Horizontal, Vertical) * playerSpeed;
        if (rb2d.velocity.x != 0 || rb2d.velocity.y != 0) 
        {
            anim.SetBool("isMoving", true);
        }
        else 
        {
            anim.SetBool("isMoving", false);
        }
    }

    void Update()
    {
        lookAtMouse();
        if(currScore != score)
        {
            addScoreSound();
            currScore = score;
        }

        if(currHealth != health)
        {
            updateHealthBar();
            currHealth = health;
        }

        ScoreText.text = "Score: " + score.ToString();
        MissileNumber.text = "X" + missileCount.ToString();

        if (health > 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                shootBullet(transform.rotation);
            }
            if (Input.GetMouseButtonDown(1) && missileCount > 0)
            {
                shootMissile(transform.rotation);
                missileCount--;
            }
        }

        if (health == 0)
        {
            GameOverScreen.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }       

        checkOutOfBounds();
    }

    void updateHealthBar() 
    {
        foreach (Transform child in HealthText.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for(int i = 0; i < health; i++)
        {
            GameObject hitPoint = Instantiate(heart, HealthText.transform);
        }
    }

    void checkOutOfBounds()
    {
        Vector2 currentPosition = transform.position;
        if (currentPosition.x < -10)
        {
            Vector2 leftToRight = new Vector2(currentPosition.x + 20, currentPosition.y);
            transform.position = leftToRight;
        }
        if (currentPosition.x > 10)
        {
            Vector2 rightToLeft = new Vector2(currentPosition.x - 20, currentPosition.y);
            transform.position = rightToLeft;
        }
        if (currentPosition.y < -6)
        {
            Vector2 bottomToTop = new Vector2(currentPosition.x, currentPosition.y + 12);
            transform.position = bottomToTop;
        }
        if (currentPosition.y > 6)
        {
            Vector2 topToBottom = new Vector2(currentPosition.x, currentPosition.y - 12);
            transform.position = topToBottom;
        }
    }

    private bool powerupActive () 
    {
        return true;
    }

    void lookAtMouse()
    {
        Vector3 MousePos = Input.mousePosition;
        MousePos = Camera.main.ScreenToWorldPoint(MousePos);

        Vector2 direction = new Vector2(MousePos.x - transform.position.x, MousePos.y - transform.position.y);
        transform.up = direction;
    }

    void shootBullet(Quaternion angle)
    {
        shootSound();
        Instantiate(projectile, transform.position, angle);
    }

    void shootMissile(Quaternion angle)
    {
        missileSound();
        Instantiate(missile, transform.position, angle);
    }

    public void takeDamage()
    {
        health -= 1;
        playerHurtSound();
        CanTakeDamage = false;
        temporaryImmunity = StartCoroutine(tempImmune(2.0f));
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {            
            if (CanTakeDamage == true)
            {
                if (health > 0)
                {
                    takeDamage();
                    Destroy(collision.gameObject);
                }              
            }                      
        } 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("HealthPickup"))
        {
            healSound();
            Destroy(collision.gameObject);
            if (health < 5)
            {               
                health += 1;                
            }   
        }

        if (collision.gameObject.CompareTag("GhostPickup"))
        {
            if(temporaryImmunity != null)
            {
                StopCoroutine(temporaryImmunity);
            }
            ghostSound();
            Destroy(collision.gameObject);
            this.GetComponent<Collider2D>().isTrigger = true;
            temporaryImmunity = StartCoroutine(tempImmune(12.0f));         
        }

        if (collision.gameObject.CompareTag("WipePickup"))
        {
            deathSound();
            Destroy(collision.gameObject);
            GameObject[] activeEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (activeEnemies.Length != 0)
            {
                foreach (GameObject enemy in activeEnemies)
                {
                    Destroy(enemy.gameObject);
                }
            }
            
        }

        if (collision.gameObject.CompareTag("MissilePickup"))
        {
            healSound();
            Destroy(collision.gameObject);
            missileCount += Random.Range(3, 6);
        }
    }

    void playerHurtSound()
    {
        SFX.clip = effects[0];
        SFX.PlayOneShot(SFX.clip);
    }

    void addScoreSound()
    {
        SFX.clip = effects[1];
        SFX.PlayOneShot(SFX.clip);
    }

    void shootSound()
    {
        SFX.clip = effects[2];
        SFX.volume = shootVolume;
        SFX.PlayOneShot(SFX.clip);
    }

    void healSound()
    {
        SFX.clip = effects[3];
        SFX.PlayOneShot(SFX.clip);
    }

    void ghostSound()
    {
        SFX.clip = effects[4];
        SFX.PlayOneShot(SFX.clip);
    }

    void deathSound()
    {
        SFX.clip = effects[5];
        SFX.volume = explosionVolume;
        SFX.PlayOneShot(SFX.clip);
    }

    void missileSound()
    {
        SFX.clip = effects[6];
        SFX.volume = shootVolume;
        SFX.PlayOneShot(SFX.clip);
    }
}


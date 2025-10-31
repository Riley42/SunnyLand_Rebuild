using System;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    int maxHealth = 5;
    int currentHealth;
    [SerializeField] float knockbackForce;
    [SerializeField] GameObject deathFX;

    Rigidbody2D rb;
    Animator anim;
    PossumMovement possMovement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        possMovement = GetComponent<PossumMovement>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amt)
    {
        currentHealth -= amt;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(possMovement.StunEnemy(3f));
        }
    }

    private void Die()
    {
        GameObject go = Instantiate(deathFX, transform.position, Quaternion.identity);
        Destroy(go, .6f);
        Destroy(gameObject);
    }
}

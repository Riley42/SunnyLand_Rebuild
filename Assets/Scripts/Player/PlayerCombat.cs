using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{

    int maxHealth = 5;
    int currentHealth;
    int atkPower = 3;
    float stunDuration = 2;
    [SerializeField] float knockbackForce = 5f;
    [SerializeField] float bounceForce = 5f;

    Rigidbody2D rb;
    Animator anim;
    PlayerMovement pMovement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pMovement = GetComponent<PlayerMovement>();
    }
    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.TryGetComponent<EnemyCombat>(out EnemyCombat enemy))
        {
            if (pMovement.CurrentJumpState == FoxJumpState.FALL)
            {
                enemy.TakeDamage(atkPower);
                StompEnemy();
            }
            else
            {
                TakeDamage(1);
                HandleKnockback(other.transform);
            }
        }
    }

    private void HandleKnockback(Transform other)
    {
        var knockbackDirection = (this.transform.position - other.position).normalized;
        rb.AddForce(new Vector2(knockbackDirection.x * knockbackForce, knockbackForce /2), ForceMode2D.Impulse);
        other.GetComponent<Rigidbody2D>().AddForce(new Vector2(-knockbackDirection.x * knockbackForce, knockbackForce /2), ForceMode2D.Impulse);
        StartCoroutine(StunPlayer(stunDuration));
    }

    public void TakeDamage(int amt)
    {
        currentHealth -= amt;

        if (currentHealth <= 0)
        {
            Die();
        }

    }
    private void StompEnemy()
    {
        rb.AddForce(new Vector2(rb.linearVelocity.x, bounceForce), ForceMode2D.Impulse);
    }
    private IEnumerator StunPlayer(float duration)
    {

        pMovement.CanMove = false;
        anim.SetBool("isHurt", true);

        yield return new WaitForSeconds(duration);
        anim.SetBool("isHurt", false);
        pMovement.CanMove = true;

    }


    private void Die()
    {
        print("Dead");
    }
}

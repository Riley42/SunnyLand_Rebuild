using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PossumState
{
    PATROL,
    CHASE,
    RESET,
    STUN
}

public class PossumMovement : MonoBehaviour
{
    [Header("Data")]
    int currentPoint;
    float currentSpeed;
    Transform target;
    PossumState currentState;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed;
    [SerializeField] List<Transform> patrolPoints = new List<Transform>();
    bool reachedDestination;

    [Header("Combat")]
    [SerializeField] private float combatCheckDistance = 5f;
    [SerializeField] private float chaseSpeed;

    [Header("Reset")]
    [SerializeField] private float chaseDuration = 2.5f;
    float chaseTimer;
    Vector2 resetPosition;
    Transform resetTarget;


    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        currentSpeed = patrolSpeed;
        target = patrolPoints[0];
    }
    private void FixedUpdate()
    {
        var player = CheckForPlayer();

        switch (currentState)
        {
            case PossumState.PATROL:
                
                Patrol();

                if (player != null)
                {
                    SetChase(player);
                }
                break;
            
            case PossumState.CHASE:
                
                Chase();
                
                if (player == null)
                {
                    if (chaseTimer > 0)
                    {
                        chaseTimer -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        SetReset();
                    }
                }
                else
                {
                    chaseTimer = chaseDuration;
                }

                    break;
            case PossumState.RESET:
                
                ResetPossum();

                break;
            case PossumState.STUN:
                //Stun();
                break;
        }

        
    }

    private void SetPatrol()
    {
        currentState = PossumState.PATROL;
        currentSpeed = patrolSpeed;
        resetTarget = null;
        resetPosition = Vector2.zero;
    }
    private void SetChase(Transform playerToChase)
    {
        resetPosition = transform.position;
        resetTarget = target;

        target = playerToChase;
        currentState = PossumState.CHASE;
        currentSpeed = chaseSpeed;
    }
    private void SetReset()
    {
        target = resetTarget;
        currentState = PossumState.RESET;
    }
    private void Patrol()
    {
        if (reachedDestination)
        {
            MoveToNextPoint();
        }

        Vector3 destPos = new Vector3(target.position.x, transform.position.y, transform.position.z);

        reachedDestination = Vector2.Distance(destPos, transform.position) < 0.2f;
        transform.position = Vector2.MoveTowards(transform.position, destPos, currentSpeed * Time.fixedDeltaTime);

        UpdateFacing(destPos);
    }
    private void Chase()
    {
        Vector3 destPos = new Vector3(target.position.x, transform.position.y, transform.position.z);
        transform.position = Vector2.MoveTowards(transform.position, destPos, currentSpeed * Time.fixedDeltaTime);

        UpdateFacing(destPos);
    }
    private void ResetPossum()
    {
        if (reachedDestination)
        {
            SetPatrol();
        }

        Vector3 destPos = new Vector3(resetPosition.x, transform.position.y, transform.position.z);
        reachedDestination = Vector2.Distance(destPos, transform.position) < 0.2f;
        transform.position = Vector2.MoveTowards(transform.position, destPos, currentSpeed * Time.fixedDeltaTime);

        UpdateFacing(destPos);
    }
    private Transform CheckForPlayer()
    {
        Debug.DrawRay(transform.position, Vector2.left * transform.localScale.x * combatCheckDistance, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, combatCheckDistance, GameLayers.i.PlayerLayer);
        if (hit)
        {
            return hit.transform;
        }
        return null;
    }
    private void UpdateFacing(Vector3 destPos)
    {
        Vector3 facing = destPos - transform.position;

        if (facing.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (facing.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void MoveToNextPoint()
    {
        currentPoint = (currentPoint + 1) % patrolPoints.Count;
        target = patrolPoints[currentPoint];
    }

    public IEnumerator StunEnemy(float duration)
    {
        var prevState = currentState;
        var prevSpeed = currentSpeed;

        currentState = PossumState.STUN;
        currentSpeed = 0;
        anim.SetBool("isStunned", true);

        Physics2D.IgnoreLayerCollision(GameLayers.i.EnemyLayerInt, GameLayers.i.PlayerLayerInt, true);

         yield return new WaitForSeconds(duration);

        Physics2D.IgnoreLayerCollision(GameLayers.i.EnemyLayerInt, GameLayers.i.PlayerLayerInt, false);

        anim.SetBool("isStunned", false);
        currentState = prevState;
        currentSpeed = prevSpeed;
    }
}

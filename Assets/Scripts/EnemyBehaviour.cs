using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : EnemyAI
{
    NavMeshAgent navMeshAgent;
    public Transform[] roundPoses;
    Coroutine roundAroundCoroutine;
    Coroutine goStoneCoroutine;
    PlayerBehaviour _playerBehavior;
    Vector3 startPos;
    Quaternion startRot;
    public LayerMask whatIsTrapTrigger;
    public LayerMask whatIsTrap;
    public float checkTrapRange;
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        _playerBehavior = FindObjectOfType<PlayerBehaviour>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        roundAroundCoroutine = StartCoroutine(roundAroundPoses());
    }
    public void CheckTrapTrigger()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, checkTrapRange, whatIsTrapTrigger);
        if (c.Length > 0)
        {
            lineOfSight.gameObject.SetActive(false);
            Destroy(this.gameObject,0.4f);
            // navMeshAgent.enabled = false;
            // this.gameObject.SetActive(false);
            // Other method this Script enabled false
            // this.gameObject.GetComponent<EnemyBehaviour>().enabled = false;      
            StopAllCoroutines();
            c[0].GetComponent<ITrapTrigger>().trigger();
        }
    }
    public void CheckTrap()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, checkTrapRange, whatIsTrap);
        if (c.Length > 0)
        {
            Destroy(gameObject);
        }
    }
    IEnumerator roundAroundPoses()
    {
        if (roundPoses.Length > 1)
        {
            for (int i = 0; i < roundPoses.Length; i++)
            {
                navMeshAgent.SetDestination(roundPoses[i].position);

                while (Vector3.Distance(roundPoses[i].position, transform.position) > 1f)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(1);
            }
            roundAroundCoroutine = StartCoroutine(roundAroundPoses());
        }
        else
        {
            navMeshAgent.SetDestination(startPos);
            while (navMeshAgent.remainingDistance > 0.1f)
            {
                yield return null;
            }
            navMeshAgent.ResetPath();
            transform.rotation = startRot;
        }
    }
    // Update is called once per frame
    void Update()
    {
        CheckTrapTrigger();
        CheckTrap();
    }
    bool isFollowing;
    internal override void DoAttack()
    {
        Debug.Log("enemy attacked");
    }

    internal override void DoFollow()
    {
        if (isTriggered && this.gameObject != null)
        {
            isFollowing = true;
            StopCoroutine(goStoneCoroutine);
            navMeshAgent.SetDestination(_playerBehavior.transform.position);
            Debug.Log("Follow");
        }
        else
        {
            isFollowing = true;
            if (roundAroundCoroutine != null)
                StopCoroutine(roundAroundCoroutine);
            navMeshAgent.SetDestination(_playerBehavior.transform.position);

            Debug.Log("Follow");
        }
    }
    internal override void DoIdle()
    {
        if (isFollowing)
        {
            isFollowing = false;
            roundAroundCoroutine = StartCoroutine(roundAroundPoses());
        }
        Debug.Log("idle");
    }

    internal override void DoPatrol()
    {
        Debug.Log("patrol");
    }
    bool isTriggered;
    public void trigger(GameObject g)
    {
        Debug.Log("triggered");
        isTriggered = true;
        if (roundAroundCoroutine != null)
            StopCoroutine(roundAroundCoroutine);

        goStoneCoroutine = StartCoroutine(goToStone(g));

    }
    IEnumerator goToStone(GameObject stone)
    {
        navMeshAgent.SetDestination(stone.transform.position);
        while (navMeshAgent.remainingDistance > 0.1f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(4);
        roundAroundCoroutine = StartCoroutine(roundAroundPoses());
        isTriggered = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,checkTrapRange);
    }
}

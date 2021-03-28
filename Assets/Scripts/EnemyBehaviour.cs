using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : EnemyAI
{
    public LayerMask whatIsTrapTrigger;
    public LayerMask whatIsTrap;
    public float checkTrapRange;
    public Transform[] roundPoses;
    private NavMeshAgent navMeshAgent;
    private Coroutine roundAroundCoroutine;
    private Coroutine goStoneCoroutine;
    private PlayerBehaviour _playerBehavior;
    private Vector3 startPos;
    private Quaternion startRot;
    private Animator _enemyAnim;
    private bool isTriggered;
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        _playerBehavior = FindObjectOfType<PlayerBehaviour>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        roundAroundCoroutine = StartCoroutine(roundAroundPoses());
        _enemyAnim = transform.GetChild(0).GetComponent<Animator>();
    }
    public void CheckTrapTrigger()
    {
        Collider[] c = Physics.OverlapSphere(transform.position, checkTrapRange, whatIsTrapTrigger);
        if (c.Length > 0)
        {
            lineOfSight.gameObject.SetActive(false);
            navMeshAgent.enabled = false;
            this.gameObject.SetActive(false);
            // Other method this Script enabled false
            // this.gameObject.GetComponent<EnemyBehaviour>().enabled = false;      
            StopAllCoroutines();
            c[0].GetComponent<ITrapTrigger>().trigger();
            // Destroy(this.gameObject,0.4f);
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

                //while (Vector3.Distance(roundPoses[i].position, transform.position) > 1f)
                //{
                    // yield return null;
                // }
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
        Debug.Log(isTriggered);
        CheckTrapTrigger();
        CheckTrap();
    }
    bool isFollowing;
    internal override void DoAttack()
    {
        Debug.Log("enemy attacked");
        // Enemy attacked
    }
    internal  override void DoFollow()
    {
        if (isTriggered)
        {
            Debug.Log("Working");
            _enemyAnim.SetBool("enemyIdle",false);
            _enemyAnim.SetBool("enemyIsRun",true);
            StopCoroutine(goStoneCoroutine);
            navMeshAgent.SetDestination(_playerBehavior.transform.position);
            isFollowing = true;
            Debug.Log("Follow");
        }
        else
        {
            _enemyAnim.SetBool("enemyIdle",false);
            _enemyAnim.SetBool("enemyIsRun",true);
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
        if ((startPos.x) - (this.gameObject.transform.position.x) < 0.05f && (startPos.z) - (this.gameObject.transform.position.z) < 0.05f)
        {
            _enemyAnim.SetBool("enemyIsRun",false);
            _enemyAnim.SetBool("enemyIdle",true);
        }
    }
    internal override void DoPatrol()
    {
        Debug.Log("patrol");
    }
    public void trigger(GameObject g)
    {
        Debug.Log("triggered");
        isTriggered = true;
        if (roundAroundCoroutine != null)
        {
            StopCoroutine(roundAroundCoroutine);
        }
        goStoneCoroutine = StartCoroutine(goToStone(g));
    }

    private void GoStone()
    {
        _enemyAnim.SetBool("enemyIdle",false);
        _enemyAnim.SetBool("enemyIsRun",true);
    }
    IEnumerator goToStone(GameObject stone)
    {
        yield return new WaitForSeconds(0.1f);
        navMeshAgent.SetDestination(stone.transform.position);
        while (navMeshAgent.remainingDistance > 0.1f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(4);
        Destroy(stone);
        roundAroundCoroutine = StartCoroutine(roundAroundPoses());
        isTriggered = false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,checkTrapRange);
    }
}

using Helper.Extension;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float moveSpeed;
    public float rayDist;

    public bool isFrontBlocked;
    Vector3 targetPos;
    public bool isBypassing;

    public float detectionArea;
    float step;

    [ReadOnlyLUFI] public bool isPlayerDetected;
    bool flag;

    private void Start()
    {
        SetTargetPos(player.position);
    }


    private void Update()
    {
        if (isPlayerDetected)
        {
            if (!isFrontBlocked && !isBypassing)
            {
                SetTargetPos(player.position);
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                if (!isBypassing)
                {
                    CheckBypass();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        isPlayerDetected = Vector3.Distance(transform.position, player.position) < detectionArea;

        isFrontBlocked = Physics.Raycast(transform.position, transform.forward, rayDist);

        PlayChaseMusic(isPlayerDetected);
    }

    void PlayChaseMusic(bool isChasing)
    {
        if (isChasing == flag) return;
        flag = isChasing;

        if (isChasing)
        {
            SoundManager.self.PlayEnemyChaseMusic();
        }
        else
        {
            SoundManager.self.PlayGamePlayMusic();
        }
    }

    void CheckBypass()
    {
        if (Mathf.Abs(player.position.z - transform.position.z) > Mathf.Abs(player.position.x - transform.position.x))
        {
            if (player.position.z > transform.position.z)
            {
                transform.SetRotY(0);
            }
            else
            {
                transform.SetRotY(180);
            }
        }
        else
        {
            if (player.position.x > transform.position.x)
            {
                transform.SetRotY(90);
            }
            else
            {
                transform.SetRotY(-90);
            }
        }

        step = 0;
        bool isLeftBlocked = true;
        bool isRightBlocked = true;

        while (isLeftBlocked && isRightBlocked)
        {
            step += 0.5f;
            isLeftBlocked = Physics.Raycast(transform.position - (transform.right * step), transform.forward, rayDist);
            isRightBlocked = Physics.Raycast(transform.position + (transform.right * step), transform.forward, rayDist);
        }

        step = Mathf.CeilToInt(step);

        if (!isLeftBlocked)
        {
            Vector3 pos = transform.position - (transform.right * step);
            StartCoroutine(MoveToSide(pos, pos + (transform.forward * rayDist * 3)));
        }
        else if (!isRightBlocked)
        {
            Vector3 pos = transform.position + (transform.right * step);
            StartCoroutine(MoveToSide(pos, pos + (transform.forward * rayDist * 3)));
        }
    }

    IEnumerator MoveToSide(Vector3 sidePos, Vector3 offset)
    {
        isBypassing = true;
        SetTargetPos(sidePos);
        while (Vector3.Distance(transform.position, sidePos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, sidePos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        SetTargetPos(offset);
        while (Vector3.Distance(transform.position, offset) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, offset, moveSpeed * Time.deltaTime);
            yield return null;
        }

        SetTargetPos(player.position);

        isBypassing = false;
    }

    

    void SetTargetPos(Vector3 pos)
    {
        targetPos = pos;
        transform.LookAt(targetPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * rayDist);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position - (transform.right * step), transform.forward * rayDist);
        Gizmos.DrawRay(transform.position + (transform.right * step), transform.forward * rayDist);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionArea);
    }
}

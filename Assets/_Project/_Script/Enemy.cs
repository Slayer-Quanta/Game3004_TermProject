using Helper.Extension;
using Helper.Tween;
using Helper.Waiter;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float moveSpeed;
    public float rayDist;
    public float chunkSize = 1;

    public bool isFrontBlocked;
    public bool canJump;
    Vector3 targetPos;
    public bool isBypassing;

    public float jumpHeight = 1.5f;
    public float jumpDuration = 0.3f;
    public float jumpHeightDetect;
    public float detectionArea;
    float step;
    public LayerMask blockLayer;
    [ReadOnlyLUFI] public bool isPlayerDetected;
    bool isJumping;
    
    public void Init(Transform player)
    {
        this.player = player;
        enabled = true;
    }

    private void Update()
    {
        if (isPlayerDetected)
        {
            if (isFrontBlocked)
            {
                if (canJump)
                {
                    if (!isJumping) Jump();

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
            else
            {
                if (!isBypassing)
                {
                    SetTargetPos(player.position);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                }
            }
        }
    }

    void Jump()
    {
        SetTargetPos(player.position);
        isJumping = true;
        transform.DoMove(transform.position + new Vector3(0, jumpHeight, 0) + transform.forward/2, jumpDuration);
        Waiter.Wait(jumpDuration + 0.5f, () => isJumping = false);
    }

    private void FixedUpdate()
    {
        isPlayerDetected = Vector3.Distance(transform.position, player.position) < detectionArea;

        isFrontBlocked = Physics.Raycast(transform.position + (Vector3.up * 0.25f), transform.forward, rayDist, blockLayer);
        canJump = !Physics.Raycast(transform.position + (Vector3.up * jumpHeightDetect), transform.forward, rayDist, blockLayer);

        // fix rotation
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
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
            step += chunkSize;
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
        offset.y = transform.position.y;
        isBypassing = true;
        SetTargetPos(sidePos);
        while (Vector3.Distance(transform.position, sidePos) > 0.05f)
        {
            if (IsPlayerBehind())
            {
                SetTargetPos(player.position);
                isBypassing = false;
                yield break;
            }

            offset.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, sidePos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (isBypassing) SetTargetPos(offset);
        while (Vector3.Distance(transform.position, offset) > 0.05f && isBypassing)
        {
            if (IsPlayerBehind())
            {
                SetTargetPos(player.position);
                isBypassing = false;
                yield break;
            }

            offset.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, offset, moveSpeed * Time.deltaTime);
            yield return null;
        }

        SetTargetPos(player.position);

        isBypassing = false;
    }

    bool IsPlayerBehind()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toPlayer);

        return dot < 0; // If dot is negative, the player is behind
    }

    void SetTargetPos(Vector3 pos)
    {
        targetPos = new Vector3(pos.x, transform.position.y, pos.z);
        transform.LookAt(targetPos);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, transform.forward * rayDist);

        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(transform.position - (transform.right * step), transform.forward * rayDist);
        //Gizmos.DrawRay(transform.position + (transform.right * step), transform.forward * rayDist);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + (Vector3.up * jumpHeightDetect), transform.forward * rayDist);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionArea);
    }
}

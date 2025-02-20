using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent agent;
    public float updateRate = 1f;

    public void Init(Transform player)
    {
        this.player = player;
        enabled = true;

        InvokeRepeating(nameof(UpdatePlayerDetection), 0, updateRate);
    }

    private void UpdatePlayerDetection()
    {
        agent.SetDestination(player.position);
    }
}

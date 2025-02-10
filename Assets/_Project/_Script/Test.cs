using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Transform player;
    public Transform enemy;

    public float dir;

    
    public void Update()
    {
        dir = Vector3.SignedAngle(enemy.forward, player.position, Vector3.up);
    }
}

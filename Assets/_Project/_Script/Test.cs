using Helper.Waiter;
using Unity.AI.Navigation;
using UnityEngine;


public class Test : MonoBehaviour
{
    public Transform player;
    public Enemy enemy;
    public NavMeshSurface navMeshSurface;

    [ButtonLUFI]
    void T()
    {
        navMeshSurface.BuildNavMesh();

        Waiter.WaitEndOffFrame(1, () =>
        {
            enemy.Init(player);
        });
    }
}

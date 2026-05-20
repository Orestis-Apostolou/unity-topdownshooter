using UnityEngine;
using UnityEngine.AI;

public class agent : MonoBehaviour
{
    private NavMeshAgent nvAgent;
    public GameObject tar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        nvAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        nvAgent.SetDestination(tar.transform.position);
    }
}

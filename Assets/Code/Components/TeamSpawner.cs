using System;
using Code.Components;
using Code.Factories;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class TeamSpawner : MonoBehaviour
{
    public RelationshipsActor npcPrefab;
    public int count = 40;
    public Teams.TeamType team = Teams.TeamType.None;
    public float radius = 50f;
    public bool spawnOnStart = true;

    private ActorFactory _factory;
    private Arsenal _arsenal;

    [Inject]
    public void Construct(ActorFactory factory, Arsenal arsenal)
    {
        _factory = factory;
    }

    public void Start()
    {
        if (spawnOnStart)
        {
            SpawnAll();
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Teams.ToColor(team);
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void SpawnAll()
    {
        for (int i = 0; i < count; i++)
        {
            SpawnOne();
        }
    }

    public void SpawnOne()
    {
        Vector2 pos2 = Random.insideUnitCircle * radius;
        Vector3 pos3 = transform.position + new Vector3(pos2.x, 0f, pos2.y);

        _factory.Create(npcPrefab, team, pos3, Quaternion.identity);
    }
}
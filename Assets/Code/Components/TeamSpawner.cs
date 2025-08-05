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

    [Inject]
    public void Construct(ActorFactory factory)
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
        Vector3 pos3 = new(pos2.x, 1.5f, pos2.y);

        _factory.Create(npcPrefab, pos3, Quaternion.identity);
    }
}
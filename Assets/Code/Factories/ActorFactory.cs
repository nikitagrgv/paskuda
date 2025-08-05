using System;
using Code.Components;
using UnityEngine;
using Zenject;

namespace Code.Factories
{
    public class ActorFactory : MonoBehaviour
    {
        public event Action<RelationshipsActor> Spawned;

        private DiContainer _container;

        [Inject]
        public void Construct(DiContainer container)
        {
            _container = container;
        }

        public RelationshipsActor Create(RelationshipsActor prefab, Teams.TeamType team, Vector3 pos, Quaternion rot)
        {
            RelationshipsActor actor =
                _container.InstantiatePrefabForComponent<RelationshipsActor>(prefab, pos, rot, null);
            actor.Team = team;
            Spawned?.Invoke(actor);
            return actor;
        }
    }
}
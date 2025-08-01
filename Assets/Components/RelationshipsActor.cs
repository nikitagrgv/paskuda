using System;
using UnityEngine;

namespace Components
{
    public class RelationshipsActor : MonoBehaviour
    {
        public Teams.TeamType Team
        {
            get => team;
            set => team = value;
        }

        public event Action Died;

        private void OnDestroy()
        {
            NotifyDied();
        }

        [SerializeField]
        private Teams.TeamType team = Teams.TeamType.None;

        private void NotifyDied()
        {
            Died?.Invoke();
        }
    }
}
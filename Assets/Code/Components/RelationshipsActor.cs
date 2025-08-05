using System;
using UnityEngine;

namespace Code.Components
{
    public class RelationshipsActor : MonoBehaviour
    {
        public event Action TeamChanged;

        public Teams.TeamType Team
        {
            get => team;
            set
            {
                team = value;
                TeamChanged?.Invoke();
            }
        }

        [SerializeField]
        private Teams.TeamType team = Teams.TeamType.None;
    }
}
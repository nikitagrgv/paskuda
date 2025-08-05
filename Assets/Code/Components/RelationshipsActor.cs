using System;
using UnityEngine;

namespace Code.Components
{
    public class RelationshipsActor : MonoBehaviour
    {
        public Teams.TeamType Team
        {
            get => team;
            set => team = value;
        }

        [SerializeField]
        private Teams.TeamType team = Teams.TeamType.None;
    }
}
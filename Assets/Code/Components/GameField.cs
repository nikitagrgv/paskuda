using System;
using UnityEngine;

namespace Code.Components
{
    public class GameField : MonoBehaviour
    {
        public float gameFieldRadius = 200;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, gameFieldRadius);
        }
    }
}
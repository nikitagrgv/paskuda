using UnityEngine;

namespace Code.Components
{
    public class ColorByTeam : MonoBehaviour
    {
        public RelationshipsActor relationshipsActor;

        private void Start()
        {
            relationshipsActor.TeamChanged += UpdateColor;
            UpdateColor();
        }

        private void UpdateColor()
        {
            Renderer rnd = GetComponent<Renderer>();
            if (!rnd)
                return;

            Color color = Teams.ToColor(relationshipsActor.Team);
            rnd.material.color = color;
        }
    }
}
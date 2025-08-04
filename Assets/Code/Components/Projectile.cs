using UnityEngine;

namespace Code.Components
{
    public class Projectile : MonoBehaviour
    {
        public GameObject projectileBase;
        public Renderer projectileBaseRenderer;
        public TrailRenderer projectileTrail;
        public ParticleSystemRenderer projectileExplosion;

        public void SetColor(Color color)
        {
            Material baseMat = projectileBaseRenderer.material;
            baseMat.color = color;
            ApplyEmissionForLitMaterial(baseMat, color, 8f);

            Material trailMat = projectileTrail.material;
            trailMat.color = color;
            ApplyEmissionForLitMaterial(trailMat, color, 4.5f);

            Material explosionMat = projectileExplosion.material;
            ApplyEmissionForLitMaterial(explosionMat, color, 5f);
        }

        private void ApplyEmissionForLitMaterial(Material material, Color color, float intensity)
        {
            const string emissionName = "_EmissionColor"; // TODO: Cache, PropertyToID
            material.SetColor(emissionName, ApplyIntensity(color, intensity));
        }

        private static Color ApplyIntensity(Color color, float intensity)
        {
            float factor = Mathf.Pow(2, intensity);
            return new Color(color.r * factor, color.g * factor, color.b * factor, color.a);
        }
    }
}
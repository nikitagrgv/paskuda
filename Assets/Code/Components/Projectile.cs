using UnityEngine;

namespace Code.Components
{
    public class Projectile : MonoBehaviour
    {
        public GameObject projectileHead;

        public Renderer projectileHeadRenderer;
        public TrailRenderer projectileTrailRenderer;
        public ParticleSystemRenderer projectileExplosionRenderer;
        public ParticleSystem projectileExplosionParticles;

        public void SetColor(Color color)
        {
            Material baseMat = projectileHeadRenderer.material;
            baseMat.color = color;
            ApplyEmissionForLitMaterial(baseMat, color, 8f);

            Material trailMat = projectileTrailRenderer.material;
            trailMat.color = color;
            ApplyEmissionForLitMaterial(trailMat, color, 4.5f);

            Material explosionMat = projectileExplosionRenderer.material;
            ApplyEmissionForLitMaterial(explosionMat, color, 5f);
        }

        public void MakeHidden()
        {
            SetHeadVisible(false);
            SetTrailVisible(false);
            SetExplosionVisible(false);
            gameObject.SetActive(false);
        }

        public void MakeVisible()
        {
            SetHeadVisible(true);
            SetTrailVisible(true);
            gameObject.SetActive(true);
        }

        public void MakeCrashed()
        {
            SetHeadVisible(false);
            SetExplosionVisible(true);
        }

        private void SetHeadVisible(bool visible)
        {
            projectileHead.gameObject.SetActive(visible);
        }

        private void SetExplosionVisible(bool visible)
        {
            projectileExplosionParticles.gameObject.SetActive(visible);
            if (visible)
            {
                projectileExplosionParticles.Play();
            }
            else
            {
                projectileExplosionParticles.Stop();
                projectileExplosionParticles.Clear();
            }
        }

        private void SetTrailVisible(bool visible)
        {
            projectileTrailRenderer.gameObject.SetActive(visible);
            projectileTrailRenderer.enabled = visible;
            if (!visible)
            {
                projectileTrailRenderer.Clear();
            }
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
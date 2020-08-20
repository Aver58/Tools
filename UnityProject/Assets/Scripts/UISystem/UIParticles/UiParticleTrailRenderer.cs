using UnityEngine;
using UnityEngine.UI;

namespace UiParticles
{
    public class UiParticleTrailRenderer : MaskableGraphic
    {
        
        /// <summary>
        /// ParticleSystem used for generate particles
        /// </summary>
        /// <value>The particle system.</value>
        public UiParticles ParentParticleModule {
            get { return m_partenParticlesModule; }
            set {
                if (SetPropertyUtility.SetClass (ref m_partenParticlesModule, value))
                    SetAllDirty ();
            }
        }
        
        /// <summary>
        /// Texture used by the particles
        /// </summary>
        public override Texture mainTexture {
            get {
                if (material != null && material.mainTexture != null) {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }
        }
        
        private UiParticles m_partenParticlesModule;
        
        private void Start()
        {
            var parent = transform.parent;
            if (parent != null)
            {
                var parentParticleModule = parent.GetComponent<UiParticles>();
                if (parentParticleModule == null)
                {
                    Debug.LogError("UiParticleTrailRenderer doesn't contains UiParticles in parent object");
                    enabled = false;
                    return;
                }

                ParentParticleModule = parentParticleModule;
            }
            else
            {
                Debug.LogError("UiParticleTrailRenderer doesn't contains parent object");
                return;
            }
            var trails = ParentParticleModule.ParticleSystem.trails;
            
            if (m_Material == null && trails.enabled) 
            {
                m_Material = ParentParticleModule.GetComponent<ParticleSystemRenderer>().trailMaterial;
            }
        }
        
        
        
    }
}
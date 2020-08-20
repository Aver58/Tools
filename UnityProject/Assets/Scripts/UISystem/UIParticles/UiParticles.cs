using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UiParticles
{
    /// <summary>
    /// Ui Parcticles, requiere ParticleSystem component
    /// </summary>
    [RequireComponent (typeof(ParticleSystem))]
	public class UiParticles : MaskableGraphic
	{

		#region InspectorFields
	    
		/// <summary>
		/// ParticleSystem used for generate particles
		/// </summary>
		[SerializeField]
	    [FormerlySerializedAs ("m_ParticleSystem")]
		private ParticleSystem m_ParticleSystem;

	    /// <summary>
	    /// If true, particles renders in streched mode
	    /// </summary>
	    [FormerlySerializedAs ("m_RenderMode")]
	    [SerializeField]
	    [Tooltip("Render mode of particles")]
	    private UiParticleRenderMode m_RenderMode  = UiParticleRenderMode.Billboard;

	    /// <summary>
	    /// Scale particle size, depends on particle velocity
	    /// </summary>
	    [FormerlySerializedAs ("m_StretchedSpeedScale")]
	    [SerializeField]
	    [Tooltip("Speed Scale for streched billboards")]
	    private float m_StretchedSpeedScale = 1f;

	    /// <summary>
	    /// Sclae particle length in streched mode
	    /// </summary>
	    [FormerlySerializedAs ("m_StretchedLenghScale")]
	    [SerializeField]
	    [Tooltip("Speed Scale for streched billboards")]
	    private float m_StretchedLenghScale = 1f;


		[FormerlySerializedAs ("m_IgnoreTimescale")]
		[SerializeField]
		[Tooltip("If true, particles ignore timescale")]
		private bool m_IgnoreTimescale = false;

		[SerializeField]
		private Mesh m_RenderedMesh;

	    #endregion
		
		
		#region Public properties
		/// <summary>
		/// ParticleSystem used for generate particles
		/// </summary>
		/// <value>The particle system.</value>
		public ParticleSystem ParticleSystem {
			get { return m_ParticleSystem; }
			set {
				if (SetPropertyUtility.SetClass (ref m_ParticleSystem, value))
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

        /// <summary>
        /// Particle system render mode (billboard, strechedBillobard)
        /// </summary>
	    public UiParticleRenderMode RenderMode
	    {
	        get { return m_RenderMode; }
	        set
	        {
	            if(SetPropertyUtility.SetStruct(ref m_RenderMode, value))
	                SetAllDirty();
	        }
	    }

		public Mesh RenderedMesh
		{
			get { return m_RenderedMesh; }
			set
			{
				if (SetPropertyUtility.SetClass(ref m_RenderedMesh, value))
				{
					InitMeshData();
					SetAllDirty();
				}
			}
		}
			
		#endregion

		
		private ParticleSystemRenderer m_ParticleSystemRenderer;
		private ParticleSystem.Particle[] m_Particles;

		private Mesh _cachedMesh;
		private Vector3[] m_MeshVerts;
		private int[] m_MeshTriangles;
		private Vector2[] m_MeshUvs; 

		ParticleSystem.MinMaxCurve frameOverTime = new ParticleSystem.MinMaxCurve();
		ParticleSystem.MinMaxCurve velocityOverTimeX = new ParticleSystem.MinMaxCurve();
		ParticleSystem.MinMaxCurve velocityOverTimeY = new ParticleSystem.MinMaxCurve();
		ParticleSystem.MinMaxCurve velocityOverTimeZ = new ParticleSystem.MinMaxCurve();

		protected override void Awake ()
		{
			var _particleSystem = GetComponent<ParticleSystem> ();
			var _particleSystemRenderer = GetComponent<ParticleSystemRenderer> ();
			if (m_Material == null) {
				m_Material = _particleSystemRenderer.sharedMaterial;
			}
		    if(_particleSystemRenderer.renderMode == ParticleSystemRenderMode.Stretch)
		        RenderMode = UiParticleRenderMode.StreachedBillboard;
			
			base.Awake ();
			ParticleSystem = _particleSystem;
			m_ParticleSystemRenderer = _particleSystemRenderer;
			InitMeshData();

			// 这段代码如果在GenerateParticlesBillboards，每帧会产生gc，移到awake可以避免这个问题，但是美术不可以动画k这两组值
			//!NOTE sample curves before render particles, because it produces allocations
			if(ParticleSystem.textureSheetAnimation.enabled)
			{
				frameOverTime = ParticleSystem.textureSheetAnimation.frameOverTime;
			}
			if(m_RenderMode == UiParticleRenderMode.StreachedBillboard)
			{
				velocityOverTimeX = ParticleSystem.velocityOverLifetime.x;
				velocityOverTimeY = ParticleSystem.velocityOverLifetime.y;
				velocityOverTimeZ = ParticleSystem.velocityOverLifetime.z;
			}
		}

		private void InitMeshData()
		{
			if (RenderedMesh != null && RenderedMesh != _cachedMesh)
			{
				m_MeshVerts = RenderedMesh.vertices;
				m_MeshTriangles = RenderedMesh.triangles;
				m_MeshUvs = RenderedMesh.uv;
				_cachedMesh = RenderedMesh;
			}
		}


		public override void SetMaterialDirty ()
		{
			base.SetMaterialDirty ();
			if (m_ParticleSystemRenderer != null)
				m_ParticleSystemRenderer.sharedMaterial = m_Material;
		}

		protected override void OnPopulateMesh (VertexHelper toFill)
		{
			Profiler.BeginSample("UIParticles OnPopulateMesh");
			if (ParticleSystem == null) {
				base.OnPopulateMesh (toFill);
				return;
			}
			GenerateParticlesBillboards (toFill);
			Profiler.EndSample();
		}
		
		protected virtual void Update ()
		{
			if (!m_IgnoreTimescale)
			{
				if (ParticleSystem != null && ParticleSystem.isPlaying)
				{
					SetVerticesDirty();
				}
			}
			else
			{
				if (ParticleSystem != null)
				{
					ParticleSystem.Simulate(Time.unscaledDeltaTime, true, false);
					SetVerticesDirty();
				}
			}

			// disable default particle renderer, we using our custom
			if (m_ParticleSystemRenderer != null && m_ParticleSystemRenderer.enabled)
				m_ParticleSystemRenderer.enabled = false;
		}
			

		private void InitParticlesBuffer (ParticleSystem.MainModule mainModule)
		{
			if (m_Particles == null || m_Particles.Length < mainModule.maxParticles)
			{
				m_Particles = new ParticleSystem.Particle[mainModule.maxParticles];
			}
			
		}
	
		private void GenerateParticlesBillboards (VertexHelper vh)
		{
			//read modules ones, cause they produce allocations when read.
			var mainModule = ParticleSystem.main;
			
			var textureSheetAnimationModule = ParticleSystem.textureSheetAnimation;
			
			InitParticlesBuffer (mainModule);
			int numParticlesAlive = ParticleSystem.GetParticles (m_Particles);
			
			vh.Clear ();
			
			var isWorldSimulationSpace = mainModule.simulationSpace == ParticleSystemSimulationSpace.World;

			if (RenderMode == UiParticleRenderMode.Mesh)
			{
				if (RenderedMesh != null)
				{
					InitMeshData();
					for (int i = 0; i < numParticlesAlive; i++)
					{
						DrawParticleMesh(m_Particles[i], vh, frameOverTime, isWorldSimulationSpace,
							textureSheetAnimationModule, m_MeshVerts, m_MeshTriangles, m_MeshUvs);
					}
				}
			}
			else
			{
				for (int i = 0; i < numParticlesAlive; i++) 
				{
					DrawParticleBillboard (m_Particles [i], vh, frameOverTime, 
						velocityOverTimeX, velocityOverTimeY, velocityOverTimeZ, isWorldSimulationSpace, 
						textureSheetAnimationModule);
				}
			}
		}

		private void DrawParticleMesh(
			ParticleSystem.Particle particle,
			VertexHelper vh,
			ParticleSystem.MinMaxCurve frameOverTime,
			bool isWorldSimulationSpace,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule, Vector3[] verts, int[] triangles,
			Vector2[] uvs)
		{
			var center =  particle.position; 
			var rotation = Quaternion.Euler (particle.rotation3D);

			if (isWorldSimulationSpace)
			{
				center = rectTransform.InverseTransformPoint (center);
			}

			float timeAlive = particle.startLifetime - particle.remainingLifetime;
			float globalTimeAlive = timeAlive / particle.startLifetime;

			Vector3 size3D = particle.GetCurrentSize3D (ParticleSystem);
			Color32 color32 = particle.GetCurrentColor (ParticleSystem);
			
			Vector2 uv0;
			Vector2 uv1;
			Vector2 uv2;
			Vector2 uv3;

			CalculateUvs(particle, frameOverTime, textureSheetAnimationModule, timeAlive, out uv0, out uv1, out uv2, out uv3);
			
			var currentVertCount = vh.currentVertCount;
			for (int j = 0; j < verts.Length; j++)
			{
				Vector3 pos = verts[j];
				pos.x *= size3D.x;
				pos.y *= size3D.y;
				pos.z *= size3D.z;
				pos = rotation * pos + center;

				var uvXpercent = uvs[j].x;
				var uvYpercent = uvs[j].y;

				var newUvx = Mathf.Lerp(uv0.x, uv2.x, uvXpercent);
				var newUvy = Mathf.Lerp(uv0.y, uv2.y, uvYpercent);
				
				vh.AddVert(pos, color32, new Vector2(newUvx, newUvy));
			}
			
			for (int i = 0; i < triangles.Length; i+=3)
			{
				vh.AddTriangle(currentVertCount+triangles[i], 
					currentVertCount+triangles[i+1], 
					currentVertCount+triangles[i+2]);
			}
		}

		private void DrawParticleBillboard (
			ParticleSystem.Particle particle, 
			VertexHelper vh, 
			ParticleSystem.MinMaxCurve frameOverTime, 
			ParticleSystem.MinMaxCurve velocityOverTimeX, 
			ParticleSystem.MinMaxCurve velocityOverTimeY, 
			ParticleSystem.MinMaxCurve velocityOverTimeZ,
			bool isWorldSimulationSpace,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule)
		{
			var center =  particle.position; 
			var rotation = Quaternion.Euler (particle.rotation3D);

			if (isWorldSimulationSpace)
			{
				center = rectTransform.InverseTransformPoint (center);
			}

			float timeAlive = particle.startLifetime - particle.remainingLifetime;
			float globalTimeAlive = timeAlive / particle.startLifetime;

			Vector3 size3D = particle.GetCurrentSize3D (ParticleSystem);
			
			if(m_RenderMode == UiParticleRenderMode.StreachedBillboard)
			{
				GetStrechedBillboardsSizeAndRotation(particle,globalTimeAlive,ref size3D, out rotation,
					velocityOverTimeX, velocityOverTimeY, velocityOverTimeZ);
			}

			var leftTop = new Vector3 (-size3D.x * 0.5f, size3D.y * 0.5f);
			var rightTop = new Vector3 (size3D.x * 0.5f, size3D.y * 0.5f);
			var rightBottom = new Vector3 (size3D.x * 0.5f, -size3D.y * 0.5f);
			var leftBottom = new Vector3 (-size3D.x * 0.5f, -size3D.y * 0.5f);


			leftTop = rotation * leftTop + center;
			rightTop = rotation * rightTop + center;
			rightBottom = rotation * rightBottom + center;
			leftBottom = rotation * leftBottom + center;

			Color32 color32 = particle.GetCurrentColor (ParticleSystem);
			var i = vh.currentVertCount;

			Vector2 uv0;
			Vector2 uv1;
			Vector2 uv2;
			Vector2 uv3;

			CalculateUvs(particle, frameOverTime, textureSheetAnimationModule, timeAlive, out uv0, out uv1, out uv2, out uv3);

			vh.AddVert (leftBottom, color32, uv0);
			vh.AddVert (leftTop, color32, uv1);
			vh.AddVert (rightTop, color32, uv2);
			vh.AddVert (rightBottom, color32, uv3);

			vh.AddTriangle (i, i + 1, i + 2);
			vh.AddTriangle (i + 2, i + 3, i);
		}

		private static void CalculateUvs(ParticleSystem.Particle particle, ParticleSystem.MinMaxCurve frameOverTime,
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule, float timeAlive, out Vector2 uv0, out Vector2 uv1, out Vector2 uv2,
			out Vector2 uv3)
		{
			if (!textureSheetAnimationModule.enabled)
			{
				uv0 = new Vector2(0f, 0f);
				uv1 = new Vector2(0f, 1f);
				uv2 = new Vector2(1f, 1f);
				uv3 = new Vector2(1f, 0f);
			}
			else
			{
				float lifeTimePerCycle = particle.startLifetime / textureSheetAnimationModule.cycleCount;
				float timePerCycle = timeAlive % lifeTimePerCycle;
				float timeAliveAnim01 = timePerCycle / lifeTimePerCycle; // in percents


				var totalFramesCount = textureSheetAnimationModule.numTilesY * textureSheetAnimationModule.numTilesX;
				var frame01 = frameOverTime.Evaluate(timeAliveAnim01);

				var frame = 0f;
				switch (textureSheetAnimationModule.animation)
				{
					case ParticleSystemAnimationType.WholeSheet:
					{
						frame = Mathf.Clamp(Mathf.Floor(frame01 * totalFramesCount), 0, totalFramesCount - 1);
						break;
					}
					case ParticleSystemAnimationType.SingleRow:
					{
						frame = Mathf.Clamp(Mathf.Floor(frame01 * textureSheetAnimationModule.numTilesX), 0,
							textureSheetAnimationModule.numTilesX - 1);
						int row = textureSheetAnimationModule.rowIndex;
						if (textureSheetAnimationModule.useRandomRow)
						{
							Random.InitState((int) particle.randomSeed);
							row = Random.Range(0, textureSheetAnimationModule.numTilesY);
						}
						frame += row * textureSheetAnimationModule.numTilesX;
						break;
					}
				}

				int x = (int) frame % textureSheetAnimationModule.numTilesX;
				int y = (int) frame / textureSheetAnimationModule.numTilesX;

				var xDelta = 1f / textureSheetAnimationModule.numTilesX;
				var yDelta = 1f / textureSheetAnimationModule.numTilesY;
				y = textureSheetAnimationModule.numTilesY - 1 - y;
				var sX = x * xDelta;
				var sY = y * yDelta;
				var eX = sX + xDelta;
				var eY = sY + yDelta;

				uv0 = new Vector2(sX, sY);
				uv1 = new Vector2(sX, eY);
				uv2 = new Vector2(eX, eY);
				uv3 = new Vector2(eX, sY);
			}
		}


		/// <summary>
		/// Evaluate size and roatation of particle in streched billboard mode
		/// </summary>
		/// <param name="particle">particle</param>
		/// <param name="timeAlive01">current life time percent [0,1] range</param>
		/// <param name="size3D">particle size</param>
		/// <param name="rotation">particle rotation</param>
		private void GetStrechedBillboardsSizeAndRotation(ParticleSystem.Particle particle, float timeAlive01,
			ref Vector3 size3D, out Quaternion rotation, 
			ParticleSystem.MinMaxCurve x, ParticleSystem.MinMaxCurve y, ParticleSystem.MinMaxCurve z)
		{
			var velocityOverLifeTime = Vector3.zero;

			if (ParticleSystem.velocityOverLifetime.enabled)
			{
				velocityOverLifeTime.x = x.Evaluate(timeAlive01);
				velocityOverLifeTime.y = y.Evaluate(timeAlive01);
				velocityOverLifeTime.z = z.Evaluate(timeAlive01);
			}
		    
			var finalVelocity = particle.velocity + velocityOverLifeTime;
			var ang = Vector3.Angle(finalVelocity,  Vector3.up);
			var horizontalDirection = finalVelocity.x < 0 ? 1 : -1;
			rotation = Quaternion.Euler(new Vector3(0,0, ang*horizontalDirection));
			size3D.y *=m_StretchedLenghScale;
			size3D+= new Vector3(0, m_StretchedSpeedScale*finalVelocity.magnitude);
		}
	}


	/// <summary>
	/// Particles Render Modes
	/// </summary>
    public enum UiParticleRenderMode
    {
        Billboard,
        StreachedBillboard,
	    Mesh
    }
}

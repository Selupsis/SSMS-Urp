using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.SSMS
{
    public class SSMSGlobalFogRenderPass : ScriptableRenderPass
    {
        private bool createdFogRT = false;

        private Material fogMaterial = null;

        private SSMSGlobalFogSettings settings;

        private RenderTexture source { get; set; }
        private RenderTargetIdentifier destination { get; set; }
        private string profilerTag;
        private bool saveFogRT = true;
        private float currentFogHeightPercentage = 0f;
        private float currentFogHeight = 0f;

        private float currentHeightDensityPercentage = 0f;
        private float currentHeightDensity = 0f;


        [HideInInspector]
        public RenderTexture fogRT;

        public SSMSGlobalFogRenderPass(string profilerTag, SSMSGlobalFogSettings settings)
        {
            this.profilerTag = profilerTag;
            this.settings = settings;
            this.renderPassEvent = settings.Event;

            this.settings.fogShader = Shader.Find("Hidden/SSMS Global Fog");

            if (fogMaterial == null)
            {
                fogMaterial = new Material(this.settings.fogShader);
                fogMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        public void Setup( RenderTargetIdentifier destination)
        {
            //this.source = source;
            this.destination = destination;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //this.source = Shader.PropertyToID("_SSMS Global Fog");
            //cmd.GetTemporaryRT(this.source, cameraTextureDescriptor);
            this.source = RenderTexture.GetTemporary(cameraTextureDescriptor);

#if UNITY_EDITOR
            this.renderPassEvent = Application.isPlaying ? this.settings.Event : this.settings.EditorRenderPassEvent;
#else
            this.renderPassEvent = this.settings.Event;
#endif
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(this.profilerTag);
            Camera cam = null;
            // Note: Do NOT use the camera of rendingeringdata, it will return the wrong one!
#if UNITY_EDITOR
            cam = !Application.isPlaying ? SceneView.lastActiveSceneView.camera : Camera.main;

#endif
            if (cam == null && !Application.isEditor)
            {
                cam = Camera.main;
            }

            if (cam != null)
            {
                // Update the fogheight-animation if set
                if (Application.isPlaying && this.settings.useFogHeightAnimation && this.settings.fogHeightAnimationCurve != null)
                {
                    // Advance percentage
                    this.currentFogHeightPercentage += Time.deltaTime * this.settings.heightFogAnimationSpeed;
                    if (currentFogHeightPercentage > 100f)
                    {
                        currentFogHeightPercentage = 0;
                    }

                    float fogModifier = this.settings.fogHeightAnimationCurve.Evaluate(this.currentFogHeightPercentage) * this.settings.fogHeightAnimationModifier;
                    this.currentFogHeight = this.settings.fogHeight + fogModifier;
                }
                else
                { 
                    this.currentFogHeight = this.settings.fogHeight;
                    this.currentFogHeightPercentage = 0;
                }

                // Update the fogheightdensity-animation if set
                if (Application.isPlaying && this.settings.useHeightDensityAnimation && this.settings.heightDensityAnimationCurve != null)
                {
                    // Advance percentage
                    this.currentHeightDensityPercentage += Time.deltaTime * this.settings.heightDensityAnimationSpeed;
                    if (currentHeightDensityPercentage > 100f)
                    {
                        currentHeightDensityPercentage = 0;

                    }
                    float densityModifier = this.settings.heightDensityAnimationCurve.Evaluate(this.currentHeightDensityPercentage) * this.settings.heightDensityAnimationModifier;
                    this.currentHeightDensity = this.settings.heightDensity + densityModifier;
                }
                else
                {
                    this.currentHeightDensity = this.settings.heightDensity;
                    this.currentHeightDensityPercentage = 0;
                }

                // Copy the content of the screen to source
                cmd.Blit(BuiltinRenderTextureType.CurrentActive, this.source);

                if (this.settings.setGlobalSettings)
                {
                    if (this.settings.fogStart < 0) { this.settings.fogStart = 0; }
                    if (this.settings.fogEnd < 0) { this.settings.fogEnd = 0; }

                    RenderSettings.fogColor = this.settings.fogColor;
                    RenderSettings.fogMode = this.settings.fogMode;
                    RenderSettings.fogDensity = this.settings.fogDensity;
                    RenderSettings.fogStartDistance = this.settings.fogStart;
                    RenderSettings.fogEndDistance = this.settings.fogEnd;
                }


                //Camera.main;//renderingData.cameraData.camera;
                int sourceWidth = cam.scaledPixelWidth;
                int sourceHeight = cam.scaledPixelHeight;

                if (/*CheckResources() == false ||*/ (!this.settings.useDistanceFog && !this.settings.useHeightFog))
                {
                    cmd.Blit(this.source, this.destination);
                    return;
                }

                //Create a new FogRT
                if (!createdFogRT && this.saveFogRT && (fogRT == null || fogRT.height < sourceHeight || fogRT.width < sourceWidth))
                {
                    fogRT = new RenderTexture(sourceWidth, sourceHeight, 0, RenderTextureFormat.Default);
                    createdFogRT = !Application.IsPlaying(cam);
                }

                Transform camtr = cam.transform;

                Vector3[] frustumCorners = new Vector3[4];
                cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, cam.stereoActiveEye, frustumCorners);
                var bottomLeft = camtr.TransformVector(frustumCorners[0]);
                var topLeft = camtr.TransformVector(frustumCorners[1]);
                var topRight = camtr.TransformVector(frustumCorners[2]);
                var bottomRight = camtr.TransformVector(frustumCorners[3]);

                Matrix4x4 frustumCornersArray = Matrix4x4.identity;
                frustumCornersArray.SetRow(0, bottomLeft);
                frustumCornersArray.SetRow(1, bottomRight);
                frustumCornersArray.SetRow(2, topLeft);
                frustumCornersArray.SetRow(3, topRight);

                var camPos = camtr.position;
                float FdotC = camPos.y - this.currentFogHeight;
                float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
                float excludeDepth = (this.settings.excludeFarPixels ? 1.0f : 2.0f);
                fogMaterial.SetMatrix("_FrustumCornersWS", frustumCornersArray);
                fogMaterial.SetVector("_CameraWS", camPos);
                fogMaterial.SetVector("_HeightParams", new Vector4(this.currentFogHeight, FdotC, paramK, this.currentHeightDensity * 0.5f));
                fogMaterial.SetVector("_DistanceParams", new Vector4(-Mathf.Max(this.settings.startDistance, 0.0f), excludeDepth, 0, 0));

                var sceneMode = RenderSettings.fogMode;
                var sceneDensity = RenderSettings.fogDensity;
                var sceneStart = RenderSettings.fogStartDistance;
                var sceneEnd = RenderSettings.fogEndDistance;
                Vector4 sceneParams;
                bool linear = (sceneMode == FogMode.Linear);
                float diff = linear ? sceneEnd - sceneStart : 0.0f;
                float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
                sceneParams.x = sceneDensity * 1.2011224087f; // density / sqrt(ln(2)), used by Exp2 fog mode
                sceneParams.y = sceneDensity * 1.4426950408f; // density / ln(2), used by Exp fog mode
                sceneParams.z = linear ? -invDiff : 0.0f;
                sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;
                fogMaterial.SetVector("_SceneFogParams", sceneParams);
                fogMaterial.SetVector("_SceneFogMode", new Vector4((int)sceneMode, this.settings.useRadialDistance ? 1 : 0, 0, 0));

                fogMaterial.SetColor("_FogTint", this.settings.fogTint);
                fogMaterial.SetFloat("_MaxValue", this.settings.maxDensity);
                fogMaterial.SetFloat("_EnLoss", this.settings.energyLoss);

                int pass = 0;
                if (this.settings.useDistanceFog && this.settings.useHeightFog)
                {
                    pass = 0; // distance + height
                    if (this.saveFogRT)
                    {
                        cmd.Blit(this.source, fogRT, this.fogMaterial, 3);
                    }
                }
                else if (this.settings.useDistanceFog)
                {
                    pass = 1; // distance only
                    if (this.saveFogRT)
                    {
                        cmd.Blit(this.source, fogRT, this.fogMaterial, 4);
                    }
                }
                else
                {
                    pass = 2; // height only
                    if (saveFogRT)
                    {
                        cmd.Blit(this.source, fogRT, this.fogMaterial, 5);
                    }
                }

                cmd.Blit(this.source, this.destination, this.fogMaterial, pass);
                Shader.SetGlobalTexture("_FogTex", fogRT);

                if (!this.saveFogRT && fogRT != null)
                {
                    fogRT.Release();
                }
            }
            else
            {
                Debug.LogError("No camera found to render fog to?");
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            //cmd.ReleaseTemporaryRT(this.source);
            RenderTexture.ReleaseTemporary(this.source);
        }
    }
}

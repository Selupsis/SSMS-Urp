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
    internal class SSMSRenderPass : ScriptableRenderPass
    {
        #region Private Members

        private RenderTexture source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        private SSMSSettings settings;

        private Shader _shader;

        private Material _material;

        private const int kMaxIterations = 16;
        private Tuple<int, int, RenderTexture>[] _blurBuffer1 = new Tuple<int, int, RenderTexture>[kMaxIterations];
        private Tuple<int, int, RenderTexture>[] _blurBuffer2 = new Tuple<int, int, RenderTexture>[kMaxIterations];
        private string profilerTag;

        float LinearToGamma(float x)
        {
#if UNITY_5_3_OR_NEWER
            return Mathf.LinearToGammaSpace(x);
#else
            if (x <= 0.0031308f)
                return 12.92f * x;
            else
                return 1.055f * Mathf.Pow(x, 1 / 2.4f) - 0.055f;
#endif
        }

        float GammaToLinear(float x)
        {
#if UNITY_5_3_OR_NEWER
            return Mathf.GammaToLinearSpace(x);
#else
            if (x <= 0.04045f)
                return x / 12.92f;
            else
                return Mathf.Pow((x + 0.055f) / 1.055f, 2.4f);
#endif
        }

        #endregion

        public SSMSRenderPass(string profilerTag, SSMSSettings settings)
        {
            this.profilerTag = profilerTag;
            this.settings = settings;
            this.renderPassEvent = settings.Event;
            var shader = _shader ? _shader : Shader.Find("Hidden/SSMS");
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;

            // SMSS
            if (this.settings.fadeRamp == null)
            {
                this.settings.fadeRamp = Resources.Load("Textures/nonLinear2", typeof(Texture2D)) as Texture2D;
            };
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //this.source = Shader.PropertyToID("_SSMS");
            //cmd.GetTemporaryRT(this.source, cameraTextureDescriptor);
            this.source = RenderTexture.GetTemporary(cameraTextureDescriptor);
        }

        public void Setup(RenderTargetIdentifier destination)
        {
            this.destination = destination;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(this.profilerTag);

            if (_material != null)
            {
                // Copy the content of the screen to source
                cmd.Blit(BuiltinRenderTextureType.CurrentActive, this.source);

                var useRGBM = Application.isMobilePlatform;
                //CameraData cameraData = renderingData.cameraData;
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
                    // source texture size
                    var tw = cam.scaledPixelWidth;//this.source.width;
                    var th = cam.scaledPixelHeight; //this.source.height;

                    // halve the texture size for the low quality mode
                    if (!this.settings.highQuality)
                    {
                        tw /= 2;
                        th /= 2;
                    }

                    // blur buffer format
                    var rtFormat = useRGBM ?
                        RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

                    // determine the iteration count
                    var logh = Mathf.Log(th, 2) + this.settings.radius - 8;
                    var logh_i = (int)logh;
                    var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);

                    // update the shader properties
                    var lthresh = this.settings.thresholdLinear;
                    _material.SetFloat("_Threshold", lthresh);

                    var knee = lthresh * this.settings.softKnee + 1e-5f;
                    var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
                    _material.SetVector("_Curve", curve);

                    var pfo = !this.settings.highQuality && this.settings.antiFlicker;
                    _material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

                    _material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
                    _material.SetFloat("_Intensity", this.settings.intensity);

                    _material.SetTexture("_FadeTex", this.settings.fadeRamp);
                    _material.SetFloat("_BlurWeight", this.settings.blurWeight);
                    _material.SetFloat("_Radius", this.settings.radius);
                    _material.SetColor("_BlurTint", this.settings.blurTint);

                    // prefilter pass
                    var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
                    var pass = this.settings.antiFlicker ? 1 : 0;
                    cmd.Blit(source, prefiltered, _material, pass);

                    // construct a mip pyramid
                    var last = prefiltered;
                    int lastWidth = tw;
                    int lastHeight = th;
                    for (var level = 0; level < iterations; level++)
                    {
                        _blurBuffer1[level] = new Tuple<int, int, RenderTexture>(
                            lastWidth / 2,
                            lastHeight / 2,
                            RenderTexture.GetTemporary(lastWidth / 2, lastHeight / 2, 0, rtFormat));
                        
                        pass = (level == 0) ? (this.settings.antiFlicker ? 3 : 2) : 4;
                        cmd.Blit(last, _blurBuffer1[level].Item3, _material, pass);

                        last = _blurBuffer1[level].Item3;
                        lastWidth = _blurBuffer1[level].Item1;
                        lastHeight = _blurBuffer1[level].Item2;
                    }

                    // upsample and combine loop
                    for (var level = iterations - 2; level >= 0; level--)
                    {
                        var basetex = _blurBuffer1[level].Item3;
                        _material.SetTexture("_BaseTex", basetex);

                        _blurBuffer2[level] = new Tuple<int, int, RenderTexture>(
                            basetex.width, 
                            basetex.height,
                            RenderTexture.GetTemporary(basetex.width, basetex.height, 0, rtFormat));

                        pass = this.settings.highQuality ? 6 : 5;
                        cmd.Blit(last, _blurBuffer2[level].Item3, _material, pass);
                        last = _blurBuffer2[level].Item3;
                    }

                    // finish process
                    _material.SetTexture("_BaseTex", this.source);
                    pass = this.settings.highQuality ? 8 : 7;
                    cmd.Blit(last, destination, _material, pass);

                    // release the temporary buffers
                    for (var i = 0; i < kMaxIterations; i++)
                    {
                        if (_blurBuffer1[i] != null)
                        {
                            RenderTexture.ReleaseTemporary(_blurBuffer1[i].Item3);
                        }

                        if (_blurBuffer2[i] != null)
                        {
                            RenderTexture.ReleaseTemporary(_blurBuffer2[i].Item3);
                        }

                        _blurBuffer1[i] = null;
                        _blurBuffer2[i] = null;
                    }

                    RenderTexture.ReleaseTemporary(prefiltered);
                }
                else
                {
                    Debug.LogError("No camera found to render SSMS to?");
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
            RenderTexture.ReleaseTemporary(this.source);
        }
    }
}

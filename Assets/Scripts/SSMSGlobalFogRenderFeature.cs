using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.SSMS
{
    public class SSMSGlobalFogRenderFeature : ScriptableRendererFeature
    {
        /// <summary>
        /// Settings for the renderer. Must be named "settings" to be shown in the editor!
        /// </summary>
        public SSMSGlobalFogSettings settings = new SSMSGlobalFogSettings();
        RenderTargetHandle renderTextureHandle;
        SSMSGlobalFogRenderPass ssmsPass;


        public override void Create()
        {
            ssmsPass = new SSMSGlobalFogRenderPass("SSMSGlobalFog", settings);
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Get source and target and set it to the pass before rendering starts
            this.ssmsPass.Setup(renderer.cameraColorTarget);

            renderer.EnqueuePass(ssmsPass);
        }
    }
}

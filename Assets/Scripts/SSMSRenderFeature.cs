using Assets.Scripts.SSMS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSMSRenderFeature : ScriptableRendererFeature
{
    /// <summary>
    /// Settings for the renderer. Must be named "settings" to be shown in the editor!
    /// </summary>
    public SSMSSettings settings = new SSMSSettings();
    RenderTargetHandle renderTextureHandle;
    SSMSRenderPass ssmsPass;


    public override void Create()
    {
        ssmsPass = new SSMSRenderPass("SSMS", settings);
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



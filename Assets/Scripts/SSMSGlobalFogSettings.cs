using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.SSMS
{
    [Serializable]
    public class SSMSGlobalFogSettings
    {
        [Tooltip("Defines at which point in the pipeline the effect is rendered")]
        public RenderPassEvent Event = RenderPassEvent.AfterRendering;

        [Tooltip("Defines at which point in the pipeline the effect is rendered (in editor only, while not playing!)")]
        public RenderPassEvent EditorRenderPassEvent = RenderPassEvent.BeforeRendering;

        [Tooltip("Apply distance-based fog?")]
        public bool useDistanceFog = true;

        [Tooltip("Exclude far plane pixels from distance-based fog? (Skybox or clear color)")]
        public bool excludeFarPixels = true;

        [Tooltip("Distance fog is based on radial distance from camera when checked")]
        public bool useRadialDistance = false;

        [Tooltip("Apply height-based fog?")]
        public bool useHeightFog = true;

        [Tooltip("Fog top Y coordinate")]
        public float fogHeight = 1.0f;

        [Range(0.001f, 100.0f)]
        public float heightDensity = 2.0f;

        [Tooltip("Push fog away from the camera by this amount")]
        public float startDistance = 0.0f;

        [Tooltip("Clips max fog value. Allows bright lights to shine through.")]
        [Range(0, 1)]
        public float maxDensity = 0.999f;

        [Tooltip("How much light is absorbed by the fog. Not physically correct at all.")]
        [Range(0, 100)]
        public float energyLoss = 0f;

        [Tooltip("Tints the color of this instance of Global Fog.")]
        public Color fogTint = Color.white;

        public Shader fogShader = null;

        [Header("Global Fog Settings")]
        [Tooltip("Overrides global settings.")]
        public bool setGlobalSettings = false;

        public Color fogColor;

        public FogMode fogMode;

        [Tooltip("For exponential modes only.")]
        [Range(0, 1)]
        public float fogDensity;

        [Tooltip("For linear mode only.")]
        public float fogStart;

        [Tooltip("For linear mode only.")]
        public float fogEnd;

        [Header("Fog Animation Settings")]
        [Tooltip("Enable fog animation (Only considered if an animation-curve is set!)")]
        public bool useFogHeightAnimation = true;

        [Tooltip("Optional animation curve to apply to the fog height")]
        public AnimationCurve fogHeightAnimationCurve;

        [Tooltip("Speed modifier applied to the fogheight, in combination with the animationcurve")]
        public float heightFogAnimationSpeed = 0.01f;

        [Tooltip("Value modifier for the height fog animation")]
        public float fogHeightAnimationModifier = 1.0f;


        [Tooltip("Enable fog animation (Only considered if an animation-curve is set!)")]
        public bool useHeightDensityAnimation = true;

        [Tooltip("Optional animation curve to apply to the fog density")]
        public AnimationCurve heightDensityAnimationCurve;

        [Tooltip("Speed modifier applied to the heightdensity, in combination with the animationcurve")]
        public float heightDensityAnimationSpeed = 0.01f;

        [Tooltip("Value modifier for the height fog density animation")]
        public float heightDensityAnimationModifier = 1.0f;

    }
}

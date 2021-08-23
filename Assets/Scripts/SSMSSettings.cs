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
    public class SSMSSettings
    {
        public RenderPassEvent Event = RenderPassEvent.AfterRendering;

        /// Prefilter threshold (gamma-encoded)
        /// Filters out pixels under this level of brightness.
        public float thresholdGamma
        {
            get { return Mathf.Max(_threshold, 0); }
            set { _threshold = value; }
        }

        /// Prefilter threshold (linearly-encoded)
        /// Filters out pixels under this level of brightness.
        public float thresholdLinear
        {
            get { return GammaToLinear(thresholdGamma); }
            set { _threshold = LinearToGamma(value); }
        }

        [HideInInspector]
        [SerializeField]
        [Tooltip("Filters out pixels under this level of brightness.")]
        float _threshold = 0f;

        /// Soft-knee coefficient
        /// Makes transition between under/over-threshold gradual.
        public float softKnee
        {
            get { return _softKnee; }
            set { _softKnee = value; }
        }

        [HideInInspector]
        [SerializeField, Range(0, 1)]
        [Tooltip("Makes transition between under/over-threshold gradual.")]
        float _softKnee = 0.5f;

        /// Bloom radius
        /// Changes extent of veiling effects in a screen
        /// resolution-independent fashion.
        public float radius
        {
            get { return _radius; }
            set { _radius = value; }
        }

        [Header("Scattering")]
        [SerializeField, Range(1, 7)]
        [Tooltip("Changes extent of veiling effects\n" +
                 "in a screen resolution-independent fashion.")]

        float _radius = 7f;

        /// Blur Weight
        /// Gives more strength to the blur texture during the combiner loop.
        public float blurWeight
        {
            get { return _blurWeight; }
            set { _blurWeight = value; }
        }

        [SerializeField, Range(0.1f, 100)]
        [Tooltip("Higher number creates a softer look but artifacts are more pronounced.")] // TODO Better description.
        float _blurWeight = 1f;

        /// Bloom intensity
        /// Blend factor of the result image.
        public float intensity
        {
            get { return Mathf.Max(_intensity, 0); }
            set { _intensity = value; }
        }

        [SerializeField]
        [Tooltip("Blend factor of the result image.")]
        [Range(0, 1)]
        float _intensity = 1f;

        /// High quality mode
        /// Controls filter quality and buffer resolution.
        public bool highQuality
        {
            get { return _highQuality; }
            set { _highQuality = value; }
        }

        [SerializeField]
        [Tooltip("Controls filter quality and buffer resolution.")]
        bool _highQuality = true;

        /// Anti-flicker filter
        /// Reduces flashing noise with an additional filter.
        [SerializeField]
        [Tooltip("Reduces flashing noise with an additional filter.")]
        bool _antiFlicker = true;

        public bool antiFlicker
        {
            get { return _antiFlicker; }
            set { _antiFlicker = value; }
        }

        /// Distribution texture
        [SerializeField]
        [Tooltip("1D gradient. Determines how the effect fades across distance.")]
        Texture2D _fadeRamp;

        public Texture2D fadeRamp
        {
            get { return _fadeRamp; }
            set { _fadeRamp = value; }
        }

        /// Blur tint
        [SerializeField]
        [Tooltip("Tints the resulting blur. ")]
        Color _blurTint = Color.white;

        public Color blurTint
        {
            get { return _blurTint; }
            set { _blurTint = value; }
        }

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


    }
}

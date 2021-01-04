using UnityEngine;

namespace IrradianceGI
{
    public class IrradianceGridSettings : MonoBehaviour
    {
        public int irradianceSmoothing = 1;
        public bool useAmbientColorIfNoHit = true;
        public Color ambientColorToBool = Color.white;
        public bool useDefaultAmbientColor;
        public Color ambientColor;
        public bool useLightIntensity = true;
        public float lightIntensity = 1f;

        public float raycastLength = 2.5f;
        public float otherVoxelRadiance = 1.1f;

        private void Start()
        {
            if (useDefaultAmbientColor)
                ambientColor = RenderSettings.ambientLight;
            if (useAmbientColorIfNoHit)
                ambientColorToBool = RenderSettings.ambientEquatorColor;
            if (useLightIntensity)
                lightIntensity = RenderSettings.sun.intensity;
        }
    }
}
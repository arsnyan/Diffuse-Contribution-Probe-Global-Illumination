using System;
using UnityEngine;

namespace IrradianceGI
{
    public class IrradianceGridSettings : MonoBehaviour
    {
        public int irradianceSmoothing = 1;
        public bool useAmbientColorIfNoHit = true;
        public Color ambientColorToBool = Color.white;
        public bool useDefaultAmbientColor = false;
        public Color ambientColor;

        private void Start()
        {
            if (useDefaultAmbientColor)
                ambientColor = RenderSettings.ambientLight;
            if (useAmbientColorIfNoHit)
                ambientColorToBool = RenderSettings.ambientEquatorColor;
        }
    }
}
using UnityEditor;
using UnityEngine;

namespace IrradianceGI
{
    public class IrradiancePoint : MonoBehaviour
    {
        /**
         * <summary>
         * <param name="voxelColor"> used as a value of irradiance smoothing result</param>
         * <param name="blendingAmount"> determines how much colors for smoothing
         * should blend with each other</param>
         * <param name="isInShadow"> determines if the point is in shadow. Public just for debug</param>
         * </summary>
         */
        public Color voxelColor;
        public float blendingAmount = 0.5f;
        public bool isInShadow = false;

        /**
         * <summary>
         * <param name="m_ignoreLayer"> determines which layers should not affect shadow calculation</param>
         * <param name="m_sideColors"> used to store all colors for further blending</param>
         * <param name="m_vectors"> determines all vectors for ray casting</param>
         * </summary>
         */
        private int m_ignoreLayer;
        private Color[] m_sideColors = new Color[6];
        private readonly Vector3[] m_vectors = new Vector3[6]
            {
                Vector3.up,
                Vector3.down,
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right
            };
        private IrradianceGridSettings m_gridSettings;

        private void Start()
        {
            m_gridSettings = transform.GetComponentInParent<IrradianceGridSettings>();
            
            /*
             * TODO() m_ignoreLayer = LayerMask.GetMask("IgnoreShadows");
             * For some reason LayerMask.GetMask($) doesn't work, maybe change later,
             * though it's absolutely unnecessary
             *
             * At the moment, I use bit shifting. 8 - number of layer mask "Ignore Shadows"
             */
            m_ignoreLayer = 1 << 8;
            m_ignoreLayer = ~m_ignoreLayer;
            
            /*
             * First and necessary color calculation
             * Assigns resulted color to material
             *
             * TODO() Not using materials and mesh renderers
             */
            voxelColor = CalculateLighting() * CalculateShadedLighting();
            transform.GetComponent<MeshRenderer>().sharedMaterial.color = voxelColor;
        }

        private void LateUpdate()
        {
            /*
             * Calculating voxel colors again in a time period, do not use multiplication
             * as it would be multiplying forever giving clear white color
             * Assigns resulted color to material
             * 
             * TODO() Not using materials and mesh renderers
             */
            voxelColor = CalculateLighting() * CalculateShadedLighting();
            transform.GetComponent<MeshRenderer>().sharedMaterial.color = voxelColor;
        }

        private Color LerpColors(Color[] colors)
        {
            Color finalColor = colors[0];
            for (int i = 1; i < colors.Length; i++)
            {
                finalColor = Color.Lerp(finalColor, colors[i], blendingAmount);
            }
            
            return finalColor;
        }

        /**
         * This function calculates the colors by hitting other objects and points.
         *
         * TODO() Do not use raycasts further away, for now better use collisions
         */
        private Color CalculateLighting()
        {
            for (int i = 0; i < 6; i++)
            {
                /*
                 * This raycast checks 6 sides of irradiance point to lerp them later
                 * TODO() Implement finding algorithm which allows for multiple points to be found,
                 * TODO() whether it's 2 points or a million
                 * TODO() Or for now it must have some distance it casts a ray, to prevent any wrong
                 * TODO() color info. Even if it's gonna be implemented in grid-like pattern
                 */
                if (Physics.Raycast(transform.position, m_vectors[i], out var hit))
                {
                    var meshRenderer = hit.collider.GetComponent<MeshRenderer>();
                    var texture = (Texture2D) meshRenderer.material.mainTexture;
                    
                    /*
                     * Checks if object's material even has texture
                     * If it does, then it checks for coordinates the ray hit
                     * and uses it to get pixel color from these UV coordinates.
                     * Then it assigns the colors to special blending array by given
                     * loop index.
                     *
                     * TODO() Actually implement it without using any materials
                     * TODO() irradiance point's side.
                     *
                     * TODO(Non-important) Maybe not use this method to get color
                     */
                    if (texture != null)
                    {
                        var uvTextureCoordinates = hit.textureCoord;
                        uvTextureCoordinates.x *= texture.width;
                        uvTextureCoordinates.y *= texture.height;
                        var tiling = meshRenderer.sharedMaterial.mainTextureScale;

                        m_sideColors[i] = texture.GetPixel(
                            Mathf.FloorToInt(uvTextureCoordinates.x * tiling.x),
                            Mathf.FloorToInt(uvTextureCoordinates.y * tiling.y)
                        );
                    }
                    /*
                     * If it doesn't have textures but it does have color value,
                     * then it's used for array value, given by loop index.
                     */
                    else
                    {
                        m_sideColors[i] = meshRenderer.sharedMaterial.color;
                    }
                }
                /*
                 * If nothing gets in way to raycast, it uses ambientEquatorColor
                 * as a value of array by given loop index. It's most unnoticeable
                 * change to lighting, talking about colors.
                 */
                else
                {
                    m_sideColors[i] = m_gridSettings.ambientColorToBool;
                }
            }
            
            return LerpColors(m_sideColors);
        }

        /**
         * This function calculates if the irradiance point is being
         * in shadow and if it is, it gives ambient light color as a result (shadow color).
         * Then given color should be multiplied with main.
         *
         * TODO() Replace RenderSettings.ambientLight with something else
         * TODO() Do not use raycasts further away, for now better use collisions
         */
        private Color CalculateShadedLighting()
        {
            /*
             * I'm not totally sure but here it is;
             * Light direction is local space front rotation of sun
             *
             * It should be normalized then and multiplied by 100 for better ray casting
             * as it needs to refer to target's position. The bigger the number, the
             * more accurate calculation is;
             *
             * TODO() Implement support for any other nearby lights. Possible with collisions
             */
            var lightDir = RenderSettings.sun.transform.forward;
            lightDir.Normalize();
            lightDir *= 100;
            
            /*
             * Raycasts from origin of irradiance point opposite to light direction. Does not
             * depend on distance. Also it ignores "IgnoreShadows" layer mask
             *
             * If it hit something, then the point is in shadow, and the return color is shadow color.
             */
            if (Physics.Raycast(
                transform.position,
                -1f * lightDir,
                Mathf.Infinity,
                layerMask: m_ignoreLayer
                ))
            {
                isInShadow = true;
                return m_gridSettings.ambientColor;
            }

            /*
             * If it's not then just return white color as it doesn't affect anything by multiplying.
             * No 'else' block is used as the previous return value would terminate method.
             */
            isInShadow = false;
            return Color.white;
        }
    }
}

/*using UnityEngine;

namespace SU.IrradianceGI
{
    public class IrradianceGrid : MonoBehaviour
    {
        public Transform irradiancePoint;
        public int gridResolution = 10;
        public float blendingValue = 0.5f;

        private Transform[] grid;

        private void Awake()
        {
            grid = new Transform[gridResolution * gridResolution * gridResolution];
            for (int i = 0, z = 0; z < gridResolution; z++) {
                for (int y = 0; y < gridResolution; y++) {
                    for (int x = 0; x < gridResolution; x++, i++) {
                        grid[i] = CreateGridPoint(x, y, z);
                    }
                }
            }
        }
    
        private Transform CreateGridPoint (int x, int y, int z) {
            Transform point = Instantiate<Transform>(irradiancePoint);
            point.localPosition = GetCoordinates(x, y, z);
            point.gameObject.isStatic = true;
            return point;
        }
    
        private Vector3 GetCoordinates (int x, int y, int z) {
            return new Vector3(
                x - (gridResolution - 1) * 0.5f,
                y - (gridResolution - 1) * 0.5f,
                z - (gridResolution - 1) * 0.5f
            );
        }

        private IrradianceCubemap BlendCubemaps(IrradianceCubemap[] cubemaps)
        {
            IrradianceCubemap blendedCubemap = new IrradianceCubemap();
            blendedCubemap.xpos = Blend3Colors(new Color[3]
            {
                cubemaps[0].xpos, cubemaps[1].xpos, cubemaps[2].xpos
            });
            blendedCubemap.xneg = Blend3Colors(new Color[3]
            {
                cubemaps[0].xneg, cubemaps[1].xneg, cubemaps[2].xneg
            });
            blendedCubemap.ypos = Blend3Colors(new Color[3]
            {
                cubemaps[0].ypos, cubemaps[1].ypos, cubemaps[2].ypos
            });
            blendedCubemap.yneg = Blend3Colors(new Color[3]
            {
                cubemaps[0].yneg, cubemaps[1].yneg, cubemaps[2].yneg
            });
            blendedCubemap.zpos = Blend3Colors(new Color[3]
            {
                cubemaps[0].zpos, cubemaps[1].zpos, cubemaps[2].zpos
            });
            blendedCubemap.zneg = Blend3Colors(new Color[3]
            {
                cubemaps[0].zneg, cubemaps[1].zneg, cubemaps[2].zneg
            });
            return blendedCubemap;
        }

        private Color Blend3Colors(Color[] colors)
        {
            var temp = Color.Lerp(colors[0], colors[1], blendingValue);
            return Color.Lerp(temp, colors[2], blendingValue);
        }
    }
}
*/


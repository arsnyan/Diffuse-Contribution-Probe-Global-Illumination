/**************************************************************************
 * Copyright 2020 ArsMania
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
**************************************************************************/

using UnityEngine;

namespace DCPGI
{
    public class VoxelGrid : MonoBehaviour
    {
        public int width, length, height;
        [Range(0.1f, 16384f)]
        public float size = 1;
    
        private void Awake()
        {
            for (float x = 0; x < width; x += size)
            {
                for (float y = 0; y < height; y += size)
                {
                    for (float z = 0; z < length; z += size)
                    {
                        var obj = new GameObject($"Voxel Probe x: {x}, y: {y}, z: {z}");
                        obj.transform.position = transform.position + new Vector3(x, y, z);
                        obj.AddComponent<VoxelProbe>();
                        obj.transform.parent = this.transform;
                    }
                }
            }
        }
    
        private void OnDrawGizmos()
        {
            for (float x = 0; x < width; x += size)
            {
                for (float y = 0; y < height; y += size)
                {
                    for (float z = 0; z < length; z += size)
                    {
                        Gizmos.DrawWireCube(transform.position + new Vector3(x, y, z), new Vector3(size, size, size));
                    }
                }
            }
        }
    }
}

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

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DCPGI
{
    public class VoxelProbe : SerializedMonoBehaviour
    {
        public int voxelProbeResolution = 4;
        public float maxRayDistance = 4f;
        public int bounces = 1;
        [Range(1, 6)]
        public int rayBounceCount = 1;
        
        public RenderTexture samplerTexture;

        private Dictionary<GameObject, List<Color>> ColorsByObject;

        private readonly Vector3[] _mVectors = new Vector3[6]
        {
            Vector3.down,
            Vector3.up,
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };
    
        private Color _voxelColor;
        private int _mIgnoreLayer;
        private float _rayDst;
        private float _rayColor;
        private float _rayPower;
        private bool _isInShadow = true;
        private bool _hasReachedLight = false;
        
        private void Start()
        {
            ColorsByObject = new Dictionary<GameObject, List<Color>>();

            _mIgnoreLayer = 1 << 8;
            _mIgnoreLayer = ~_mIgnoreLayer;
            
            ProcessColorEnvironment();
            SendProbesToReceivers();
        }

        private void LateUpdate()
        {
            _isInShadow = IsInShadow();
        }

        /**
         * <summary>
         * Return true if the object's origin intersects any object, meaning it's in shadow.
         * Because of this, it can be not in shadow, but the method will return true.
         * </summary>
         */
        private bool IsInShadow()
        {
            var lightDir = RenderSettings.sun.transform.forward;
            lightDir.Normalize();
            lightDir *= 100;
            
            return Physics.Raycast(
                transform.position,
                -1f * lightDir,
                Mathf.Infinity,
                _mIgnoreLayer
            );
        }
        
        /**
         * <summary>
         *  Adds processed color to a dictionary called ColorsByObject.
         * It stores all values of voxel probe for each object separately.
         * It's made this way, cause color contribution differs from one object to another.
         * </summary>
         * <param name="hit">Specify ray from which method gets information about environmental colors.</param>
         */
        private void AddColorsToDict(RaycastHit hit)
        {
            // temporary list variable to add colors which then
            // will be sent to probe receiver
            var tmpColor = new List<Color>();
            // game object which ray hit
            var obj = hit.collider.gameObject;
            
            // hue, saturation and value variables to store hit color
            // in such color space, in order to then desaturate final color by s value
            Color.RGBToHSV(GetColorFromRay(hit), out var h, out var s, out var v);
            // applied color is final color, which saturation depends on the distance
            // between voxel probe and it's receiver
            var appliedColor = Color.HSVToRGB(h, s / hit.distance, v);
            
            // there might be no objects ray hit, so in order to
            // prevent exceptions, this was added
            if (obj == null) return;
            // if dictionary doesn't have such game object object yet,
            // it will be created
            if (!ColorsByObject.ContainsKey(obj))
            {
                // final color is added to temporary list
                tmpColor.Add(appliedColor);
                // temporary list is added to dictionary with game object as a key
                ColorsByObject.Add(obj, tmpColor);
            }
            // if there already is existing game object key in dictionary,
            // final color' list will be merged with existing one
            else
            {
                tmpColor.Add(appliedColor);
                // existing color list by key is merging with temporary list
                ColorsByObject[obj] = ColorsByObject[obj].Concat(tmpColor).ToList();
            }
        }

        /**
         * <summary>
         *  Returns Color value from objects which intersected with ray.
         * </summary>
         * <param name="hit">Ray that used for intersection with colliders.</param>
         */
        private Color GetColorFromRay(RaycastHit hit)
        {
            // mesh renderer's non-shared material to get color from texture by uv or main color
            var meshRenderer = hit.collider.GetComponent<MeshRenderer>().material;
            // texture from main material's, which might be null
            var texture = (Texture2D) meshRenderer.mainTexture;

            /*
             * Checks if object's material even has texture
             * If it does, then it checks for coordinates the ray hit
             * and uses it to get pixel color from these UV coordinates.
             * Then it assigns the colors to special blending array by given
             * loop index.
             *
             * TODO() Actually implement it without using any materials
             *
             * TODO(Non-important) Maybe not use this method to get color
             */
            if (texture != null)
            {
                return GetColorFromTextureHit(hit, meshRenderer);
            }
            /*
             * If it doesn't have textures but it does have color value,
             * then it's used for array value, given by loop index.
             */
            else
                return meshRenderer.color;
        }

        /**
         * <summary>
         * Gets color from texture if one exists, using UV point
         * from ray.
         * </summary>
         * <param name="hit">Ray that declares UVs for color point detection.</param>
         * <param name="material">Material to get texture from.</param>
         */
        private Color GetColorFromTextureHit(RaycastHit hit, Material material)
        {
            // gets main texture of provided material
            var texture = (Texture2D) material.mainTexture;
            // normalized uv coordinates from raycast hit, which declare point in texture to get color from
            var uvTextureCoordinates = hit.textureCoord;
            // non-normalizing uv coordinates (probably)
            uvTextureCoordinates.x *= texture.width;
            uvTextureCoordinates.y *= texture.height;
            
            var tiling = material.mainTextureScale;

            // the equation is too long, so it's a variable now
            // get's pixel from given uv coordinates
            var uvColor = texture.GetPixel(
                Mathf.FloorToInt(uvTextureCoordinates.x * tiling.x),
                Mathf.FloorToInt(uvTextureCoordinates.y * tiling.y)
            );
            return uvColor;
        }

        /**
         * <summary>
         * Processes environment with raycasts to get colors from it.
         * </summary>
         */
        private void ProcessColorEnvironment()
        {
            // called vector to indicate that its used in _mVectors array
            foreach (var vector in _mVectors)
            {
                // raycasting main ray to all directions from transform.position by maxRayDistance
                if (Physics.Raycast
                (transform.position,
                    vector,
                    out var diffuseRay,
                    maxDistance: maxRayDistance))
                {
                    // depending on ray bounce count, may activate secondary rays for each original ray
                    for (int i = 0; i < rayBounceCount; i++)
                    {
                        // reflected vector for bouncing of ray
                        var reflect = Vector3.Reflect(_mVectors[i], diffuseRay.normal);
                        // raycasting secondary ray to given direction by 2 * maxRayDistance
                        if (Physics.Raycast
                        (diffuseRay.point,
                            reflect,
                            out var bounceRay,
                            maxRayDistance * 2))
                        {
                            // adds colors from secondary rays to dictionary
                            AddColorsToDict(bounceRay);
                        }
                    }

                    // adds colors from main rays to dictionary
                    AddColorsToDict(diffuseRay);
                }
            }
        }

        /**
         * <summary>
         * Sends dictionary data to voxel probe receiver, including
         * voxel probe instance and the colors it generated.
         * Actually separates made dictionary to all receivers.
         * </summary>
         */
        private void SendProbesToReceivers()
        {
            // gets key value pairs from dictionary for sorting
            foreach (var pair in ColorsByObject)
            {
                // gets object to exclude from sending to receiver (usually it's the receiver itself)
                var obj = pair.Key;
                
                // receiver instance
                // if the object doesn't have it, then it get it attached.
                var receiver = obj.GetComponent<VoxelProbeReceiver>() == null ?
                    obj.AddComponent<VoxelProbeReceiver>()
                    : obj.GetComponent<VoxelProbeReceiver>();

                // temporary list for all colors from dictionary, except excluded
                var colorsForSpecificObject = new List<Color>();
                foreach (var wrongPair in ColorsByObject)
                {
                    // if key value pair doesn't match original one's, then all good
                    if (wrongPair.Key != pair.Key)
                    {
                        // merging temporary list with color list from dictionary
                        colorsForSpecificObject = colorsForSpecificObject
                            .Concat(ColorsByObject[wrongPair.Key]).ToList();
                    }
                }

                // receiver's 'probes' variable instance, for faster code writing
                var probes = receiver.Probes;
                // if receiver doesn't contain this voxel probe, then it will from now on
                // with specific colors it should receive
                if (!probes.ContainsKey(this))
                {
                    probes.Add(this, colorsForSpecificObject);
                }
                // if receiver already has this probe instance, then it's value gets merged with temporary list
                else
                {
                    probes[this] = colorsForSpecificObject.Concat(probes[this]).ToList();
                }
            }
        }
    }
}
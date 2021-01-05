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
    public class VoxelProbeReceiver : SerializedMonoBehaviour
    {
        public Dictionary<VoxelProbe, List<Color>> Probes;
        public Color[] finalGatherColors;

        private int _counter;
        private Color _finalGatherColor;

        private void Awake()
        {
            Probes = new Dictionary<VoxelProbe, List<Color>>();
            finalGatherColors = new Color[1];
            
            foreach (var pair in Probes)
            {
                if (Probes[pair.Key].Count == 0)
                    Probes.Remove(pair.Key);
            }
        }

        private void Start()
        {
            _counter = 0;
            Mix4Colors();
        }

        private void Mix4Colors()
        {
            var bounceColors = new List<Color>();

            foreach (var pair in Probes)
            {
                bounceColors = bounceColors.Concat(Probes[pair.Key]).ToList();
                
                for (int color = 0; color < bounceColors.Count; color++)
                {
                    if (color == 0)
                        _finalGatherColor = bounceColors[color];
                    else
                        _finalGatherColor += bounceColors[color];
                }

                finalGatherColors[_counter] = _finalGatherColor;
            }
        }
    }
}
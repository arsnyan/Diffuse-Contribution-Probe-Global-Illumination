using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
 * Here a little commentary of how I want to release it.
 * 1. Draw 3D grid with equal quads to demonstrate voxel grid size.
 * To change size, need to use handles.
 *
 * 2. To calculate perfect quads, we have w - width of grid, h - height and l - length &&
 * also sw - size multiplier of voxel grid.
 * To get perfect cube size, perform this GCD with multiplier of cube size (Or use Fractional side lengths for cubes, whatever it is)
 *
 * 3. Place cubes with cores in all of them next to each other.
 */
[ExecuteInEditMode]
public class VoxelGrid : MonoBehaviour
{
    public int columns, rows, height;
    public float size = 1;
    
    void Start()
    {
    }

    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        for (float x = 0; x < columns; x += size)
        {
            for (float y = 0; y < height; y += size)
            {
                for (float z = 0; z < rows; z += size)
                {
                    Gizmos.DrawWireCube(transform.position + new Vector3(x, y, z), new Vector3(size, size, size));
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class VoxelProbe : MonoBehaviour
{
    public int voxelProbeResolution = 4;
    public float maxRayDistance = 1f;
    public int bounces = 1;
    [Range(1, 6)]
    public int rayBounceCount = 1;

    public ComputeShader samplerShader;
    public RenderTexture samplerTexture;

    private readonly Vector3[] m_vectors = new Vector3[6]
    {
        Vector3.down,
        Vector3.up,
        Vector3.forward,
        Vector3.back,
        Vector3.left,
        Vector3.right
    };
    
    private Color _mVoxelColor;
    private float _mRayDistance;
    private float _mRayColor;
    private float _mRayPower;
    private bool _mIsInShadow = true;
    private bool _mHasReachedLight = false;

    private int _mIgnoreLayer;

    private Camera _mSamplerCamera;
    
    private void Start()
    {
        //samplerTexture = new RenderTexture(256, 256, 24);
        //samplerTexture.enableRandomWrite = true;
        //samplerTexture.Create();
        
        //samplerShader.SetTexture(0, "Result", samplerTexture);
        //samplerShader.Dispatch(0, samplerTexture.width / 8, samplerTexture.height / 8, 1);

        GameObject obj = new GameObject("CubemapCamera", typeof(Camera));
        obj.hideFlags = HideFlags.HideAndDontSave;
        obj.transform.position = transform.position;
        obj.transform.rotation = Quaternion.identity;
        _mSamplerCamera = obj.GetComponent<Camera>();
        _mSamplerCamera.farClipPlane = 100;
        
        _mIgnoreLayer = 1 << 8;
        _mIgnoreLayer = ~_mIgnoreLayer;
    }

    void Update()
    {
        var lightDir = RenderSettings.sun.transform.forward;
        lightDir.Normalize();
        lightDir *= 100;

        var position = transform.position;
        _mIsInShadow = Physics.Raycast(
            position,
            -1f * lightDir,
            Mathf.Infinity,
            _mIgnoreLayer
        );

        for (int vectorsCount = 0; vectorsCount < m_vectors.Length; vectorsCount++)
        {
            var bounceColors = new List<Color>();

            RaycastHit diffuseRay;
            if (Physics.Raycast(position, m_vectors[vectorsCount], out diffuseRay, maxDistance: maxRayDistance))
            {
                for (int i = 0; i < rayBounceCount; i++)
                {
                    var reflect = Vector3.Reflect(m_vectors[i], diffuseRay.normal);
                    if (Physics.Raycast(diffuseRay.point, reflect, out var bounceRay, maxRayDistance / 2))
                    {
                        var meshMaterial = bounceRay.collider.GetComponent<MeshRenderer>().sharedMaterial;
                        bounceColors.Add(meshMaterial.color);
                    }
                }

                var diffuseMeshMaterial = diffuseRay.collider.GetComponent<MeshRenderer>().sharedMaterial;
                bounceColors.Add(diffuseMeshMaterial.color);
            }
            
            Color finalGatherColor = Color.white;
            for (int color = 0; color < bounceColors.Count; color++)
            {
                if (color == 0)
                    finalGatherColor = bounceColors[color];
                else
                    finalGatherColor += bounceColors[color];
            }

            //finalGatherColor /= 2;

            var emission = GetComponent<MeshRenderer>();
            emission.sharedMaterial.color = finalGatherColor;
        }
    }

    private void LateUpdate()
    {
        UpdateCubemap(63);
    }

    private void UpdateCubemap(int faceMask)
    {
        samplerTexture = new RenderTexture(voxelProbeResolution, voxelProbeResolution, 16);
        samplerTexture.dimension = TextureDimension.Cube;
        samplerTexture.hideFlags = HideFlags.HideAndDontSave;

        _mSamplerCamera.enabled = true;
        _mSamplerCamera.transform.position = transform.position;
        _mSamplerCamera.RenderToCubemap(samplerTexture, faceMask);
        GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", samplerTexture);
        _mSamplerCamera.enabled = false;
    }
}

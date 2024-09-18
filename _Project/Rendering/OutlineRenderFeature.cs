using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static OutlineRenderFeature;
using UnityEngine.UIElements;
using static UnityEngine.XR.XRDisplaySubsystem;

public class OutlineRenderFeature : ScriptableRendererFeature, IDisposable
{
    public enum OutlineType
    {
        ON_TOP_OF_SCENE,
        IN_SCENE
    };

    private OutlinePass _outlinePass;
    public Material material;
    public Material materialOutlineInScene;

    static MeshesList _onTopOfSceneMeshes;// = new MeshesList();
    static MeshesList _inSceneMeshes;// = new MeshesList();
    static Mesh s_QuadMesh;
    static MaterialPropertyBlock s_PropertyBlock;// = new MaterialPropertyBlock();

    class MeshesList
    {
        public struct MeshToOutline
        {
            public int hash;
            public Mesh mesh;
            public Renderer renderer;
            public Vector3 position;
            public Vector3 scale;
            public Quaternion rotation;
        }

        List<MeshToOutline> _meshesToOutline = new List<MeshToOutline>();
        Dictionary<int, int> _meshesHashes = new Dictionary<int, int>();

        public int MeshesCount => _meshesToOutline.Count;

        public MeshToOutline GetMesh(int i)
        {
            return _meshesToOutline[i];
        }

        public void AddMeshForOutline(int hash, Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!_meshesHashes.ContainsKey(hash))
            {
                _meshesHashes.Add(hash, _meshesToOutline.Count);
                _meshesToOutline.Add(new MeshToOutline
                {
                    hash = hash,
                    mesh = mesh,
                    renderer = null,
                    position = position,
                    scale = scale,
                    rotation = rotation,
                });
            }
            else
            {
                //throw new System.InvalidOperationException("Mesh with same already in list to outline");
            }
        }

        public void RemoveMeshForOutline(int hash)
        {
            if (_meshesHashes.TryGetValue(hash, out int index))
            {
                //1. move last renderer to new "empty" position
                var lastIdx = _meshesToOutline.Count - 1;
                var lstHash = _meshesToOutline[lastIdx].hash;

                _meshesToOutline[index] = _meshesToOutline[lastIdx];
                _meshesHashes[lstHash] = index;

                //2. remove reference copy to moved renderer
                _meshesToOutline.RemoveAt(lastIdx);

                //3. remove record about renderer
                _meshesHashes.Remove(hash);
            }
        }

        public void AddRendererForOutline(int hash, Renderer renderer, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!_meshesHashes.ContainsKey(hash))
            {
                _meshesHashes.Add(hash, _meshesToOutline.Count);
                _meshesToOutline.Add(new MeshToOutline
                {
                    hash = hash,
                    mesh = null,
                    renderer = renderer,
                    position = position,
                    scale = scale,
                    rotation = rotation,
                });
            }
            else
            {
                //throw new System.InvalidOperationException("Mesh with same already in list to outline");
            }
        }

        public void RemoveRendererForOutline(int hash)
        {
            if (_meshesHashes.TryGetValue(hash, out int index))
            {
                //1. move last renderer to new "empty" position
                var lastIdx = _meshesToOutline.Count - 1;
                var lstHash = _meshesToOutline[lastIdx].hash;

                _meshesToOutline[index] = _meshesToOutline[lastIdx];
                _meshesHashes[lstHash] = index;

                //2. remove reference copy to moved renderer
                _meshesToOutline.RemoveAt(lastIdx);

                //3. remove record about renderer
                _meshesHashes.Remove(hash);
            }
        }

        public void ClearMeshesForOutline()
        {
            _meshesHashes.Clear();
            _meshesToOutline.Clear();
        }
    }


    // Should match Common.hlsl
    static Vector3[] GetQuadVertexPosition(float z /*= UNITY_NEAR_CLIP_VALUE*/)
    {
        var r = new Vector3[4];
        for (uint i = 0; i < 4; i++)
        {
            uint topBit = i >> 1;
            uint botBit = (i & 1);
            float x = topBit;
            float y = 1 - (topBit + botBit) & 1; // produces 1 for indices 0,3 and 0 for 1,2
            r[i] = new Vector3(x, y, z);
        }
        return r;
    }

    // Should match Common.hlsl
    static Vector2[] GetQuadTexCoord()
    {
        var r = new Vector2[4];
        for (uint i = 0; i < 4; i++)
        {
            uint topBit = i >> 1;
            uint botBit = (i & 1);
            float u = topBit;
            float v = (topBit + botBit) & 1; // produces 0 for indices 0,3 and 1 for 1,2
            if (SystemInfo.graphicsUVStartsAtTop)
                v = 1.0f - v;

            r[i] = new Vector2(u, v);
        }
        return r;
    }


    class OutlinePass : ScriptableRenderPass
    {
        private Material _materialTop;
        private Material _materialInScene;

        RTHandle _CameraDepthTarget;
        RTHandle _CameraColorTarget;

        public OutlinePass(Material material, Material materialInScene)
        {
            _materialTop = material;
            _materialInScene = materialInScene;
        }

        public void SetTargets(RTHandle colorHandle, RTHandle depthHandle)
        {
            _CameraColorTarget = colorHandle;
            _CameraDepthTarget = depthHandle;
        }

        public override void Execute(ScriptableRenderContext context,
            ref RenderingData renderingData)
        {
            // Get the Camera data from the renderingData argument.
            Camera camera = renderingData.cameraData.camera;
            if (camera.name == "UICamera")
                return;

            if (!s_QuadMesh)
            {
                float nearClipZ = -1;
                if (SystemInfo.usesReversedZBuffer)
                    nearClipZ = 1;

                s_QuadMesh = new Mesh();
                s_QuadMesh.vertices = GetQuadVertexPosition(nearClipZ);
                s_QuadMesh.uv = GetQuadTexCoord();
                s_QuadMesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
            }


            CommandBuffer cmd = CommandBufferPool.Get(name: "OutlinePass");

            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            cmd.SetRenderTarget(renderingData.cameraData.renderer.cameraColorTargetHandle, temporaryDepthTexture);
            cmd.ClearRenderTarget(true, false, Color.black);
            
            cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialTop, 0, 2, s_PropertyBlock);

            if (_onTopOfSceneMeshes != null)
            {
                //Render onTopOfScene outlines
                for (int i = 0; i < _onTopOfSceneMeshes.MeshesCount; ++i)
                {
                    var m = _onTopOfSceneMeshes.GetMesh(i);
                    if (m.renderer != null)
                    {
                        cmd.DrawRenderer(m.renderer, _materialTop, 0, 0);
                        cmd.DrawRenderer(m.renderer, _materialTop, 0, 1);
                        cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialTop, 0, 2, s_PropertyBlock);
                    }
                    else
                    {
                        if (m.mesh != null)
                        {
                            cmd.DrawMesh(m.mesh, Matrix4x4.TRS(m.position, m.rotation, m.scale), _materialTop, 0, 0);
                            cmd.DrawMesh(m.mesh, Matrix4x4.TRS(m.position, m.rotation, m.scale), _materialTop, 0, 1);
                            cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialTop, 0, 2, s_PropertyBlock);
                        }
                    }
                }
            }

            cmd.ReleaseTemporaryRT(Shader.PropertyToID(temporaryDepthTexture.name));

            cmd.SetRenderTarget(_CameraColorTarget, _CameraDepthTarget);
            //cleanup stencil. In case it is durty
            cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialInScene, 0, 2, s_PropertyBlock);

            //Render _inScene outlines
            if (_inSceneMeshes != null)
            {
                for (int i = 0; i < _inSceneMeshes.MeshesCount; ++i)
                {
                    var m = _inSceneMeshes.GetMesh(i);
                    if (m.renderer != null)
                    {
                        cmd.DrawRenderer(m.renderer, _materialInScene, 0, 0);
                        cmd.DrawRenderer(m.renderer, _materialInScene, 0, 1);
                        cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialInScene, 0, 2, s_PropertyBlock);
                    }
                    else
                    {
                        if (m.mesh != null)
                        {
                            cmd.DrawMesh(m.mesh, Matrix4x4.TRS(m.position, m.rotation, m.scale), _materialInScene, 0, 0);
                            cmd.DrawMesh(m.mesh, Matrix4x4.TRS(m.position, m.rotation, m.scale), _materialInScene, 0, 1);
                            cmd.DrawMesh(s_QuadMesh, Matrix4x4.identity, _materialInScene, 0, 2, s_PropertyBlock);
                        }
                    }
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        RTHandle temporaryDepthTexture;

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            base.Configure(cmd, cameraTextureDescriptor);

            if (temporaryDepthTexture == null)
                temporaryDepthTexture = RTHandles.Alloc("temporaryDepthTexture", name: "temporaryDepthTexture");

            cmd.GetTemporaryRT(
                Shader.PropertyToID(temporaryDepthTexture.name), cameraTextureDescriptor.width, cameraTextureDescriptor.height, 24, 
                FilterMode.Bilinear, RenderTextureFormat.Depth
            );

        }
    }

    public override void Create()
    {
        _outlinePass = new OutlinePass(material, materialOutlineInScene);
        _outlinePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        //_outlinePass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material != null)
        {
            renderer.EnqueuePass(_outlinePass);
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                    in RenderingData renderingData)
    {
        //if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            _outlinePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);
            _outlinePass.SetTargets(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
    }

    private static void CheckMeshesList()
    {
        if (_onTopOfSceneMeshes == null)
            _onTopOfSceneMeshes = new MeshesList();
        if (_inSceneMeshes == null)
            _inSceneMeshes = new MeshesList();
    }

    public static void AddMeshForOutline(int hash, Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, OutlineType outlineType)
    {
        CheckMeshesList();
        switch (outlineType)
        {
            case (OutlineType.ON_TOP_OF_SCENE):
                _onTopOfSceneMeshes.AddMeshForOutline(hash, mesh, position, rotation, scale);
                break;
            case (OutlineType.IN_SCENE):
                _inSceneMeshes.AddMeshForOutline(hash, mesh, position, rotation, scale);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void RemoveMeshForOutline(int hash, OutlineType outlineType)
    {
        CheckMeshesList();
        switch (outlineType)
        {
            case (OutlineType.ON_TOP_OF_SCENE):
                _onTopOfSceneMeshes.RemoveMeshForOutline(hash);
                break;
            case (OutlineType.IN_SCENE):
                _inSceneMeshes.RemoveMeshForOutline(hash);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void AddRendererForOutline(int hash, Renderer renderer, Vector3 position, Quaternion rotation, Vector3 scale, OutlineType outlineType)
    {
        CheckMeshesList();
        switch (outlineType)
        {
            case (OutlineType.ON_TOP_OF_SCENE):
                _onTopOfSceneMeshes.AddRendererForOutline(hash, renderer, position, rotation, scale);
                break;
            case (OutlineType.IN_SCENE):
                _inSceneMeshes.AddRendererForOutline(hash, renderer, position, rotation, scale);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void RemoveRendererForOutline(int hash, OutlineType outlineType)
    {
        CheckMeshesList();
        switch (outlineType)
        {
            case (OutlineType.ON_TOP_OF_SCENE):
                _onTopOfSceneMeshes.RemoveRendererForOutline(hash);
                break;
            case (OutlineType.IN_SCENE):
                _inSceneMeshes.RemoveRendererForOutline(hash);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static void ClearMeshesForOutline()
    {
        if (_onTopOfSceneMeshes != null)
        {
            _onTopOfSceneMeshes.ClearMeshesForOutline();
            _inSceneMeshes.ClearMeshesForOutline();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            CoreUtils.Destroy(s_QuadMesh);
            s_QuadMesh = null;
        }
    }


}

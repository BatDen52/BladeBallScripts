using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class TestMeshOutline : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        var mf = GetComponent<MeshFilter>();
        var m = mf.sharedMesh;

        var position = transform.position;
        var rotation = transform.rotation;
        var scale = transform.lossyScale;
        OutlineRenderFeature.AddMeshForOutline(GetInstanceID(), m, position, rotation, scale, OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);
    }

    private void OnDisable()
    {
        OutlineRenderFeature.RemoveMeshForOutline(GetInstanceID(), OutlineRenderFeature.OutlineType.ON_TOP_OF_SCENE);

        OutlineRenderFeature.ClearMeshesForOutline();
    }


}

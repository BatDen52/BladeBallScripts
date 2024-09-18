using System.Linq;
using UnityEngine;

namespace _Project
{
    public static class SkinnedMeshRendererExtensions
    {
        public static void AddMaterial(this SkinnedMeshRenderer skinnedMeshRenderer, Material newMaterial)
        {
            var materials = skinnedMeshRenderer.materials;
            var newMaterials = new Material[materials.Length + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[newMaterials.Length - 1] = newMaterial;
            skinnedMeshRenderer.materials = newMaterials;
        }   

        public static void RemoveLastMaterial(this SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var materials = skinnedMeshRenderer.materials;
            if (materials.Length > 0)
            {
                var newMaterials = new Material[materials.Length - 1];
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = materials[i];
                }
                skinnedMeshRenderer.materials = newMaterials;
            }
        }
        
        public static void RemoveMaterialLINQ(this SkinnedMeshRenderer skinnedMeshRenderer, Material materialToRemove)
        {
            var materials = skinnedMeshRenderer.materials.ToList();
            if (materials.Contains(materialToRemove))
            {
                materials.Remove(materialToRemove);
                skinnedMeshRenderer.materials = materials.ToArray();
            }
        }
        
        public static void RemoveMaterial(this SkinnedMeshRenderer skinnedMeshRenderer, Material materialToRemove)
        {
            var materials = skinnedMeshRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].name == materialToRemove.name)
                {
                    var newMaterials = new Material[materials.Length - 1];
                    for (int j = 0; j < i; j++)
                    {
                        newMaterials[j] = materials[j];
                    }
                    for (int j = i + 1; j < materials.Length; j++)
                    {
                        newMaterials[j - 1] = materials[j];
                    }
                    skinnedMeshRenderer.materials = newMaterials;
                    break;
                }
            }
        }
    }
}
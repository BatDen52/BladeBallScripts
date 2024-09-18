using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _Project.Editor
{
    public class RotateTowardsLastSelected : ScriptableObject
    {
        [MenuItem("Utils/Rotate Towards Last Selected")]
        static void Rotate()
        {
            List<GameObject> selectedObjects = new List<GameObject>();
            foreach (GameObject go in Selection.gameObjects)
            {
                selectedObjects.Add(go);
            }

            if (selectedObjects.Count > 0)
            {
                GameObject target = selectedObjects[selectedObjects.Count - 1];

                foreach (GameObject go in selectedObjects)
                {
                    if (go != target)
                    {
                        Vector3 relativePos = target.transform.position - go.transform.position;
                        Quaternion rotation = Quaternion.LookRotation(relativePos);
                        go.transform.rotation = rotation;
                    }
                }
            }
        }
    }
}
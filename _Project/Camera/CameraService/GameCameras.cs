using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace _Project
{
    public class GameCameras : MonoBehaviour
    {
        public List<CinemachineVirtualCameraBase> VirtualCameras;
        public Camera MainCamera;
        public Camera UICamera;
        public CharacterCamera CharacterCamera;
    }
}
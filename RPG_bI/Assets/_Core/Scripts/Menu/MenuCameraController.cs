using Unity.Cinemachine;
using UnityEngine;

public class MenuCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera mainMenuCam;
    [SerializeField] private CinemachineCamera optionsCam;

    public void OpenSettings()
    {
        mainMenuCam.Priority = 1;
        optionsCam.Priority = 2;
    }

    public void BackToMenu()
    {
        mainMenuCam.Priority = 2;
        optionsCam.Priority = 1;
    }

}

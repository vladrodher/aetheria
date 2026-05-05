using UnityEngine;

/// <summary>
/// Puente entre los joysticks virtuales de la UI y el controlador del jugador.
/// Lee los valores de cada VirtualJoystick y los inyecta al controller cada frame.
/// </summary>
public class MobileInputProvider : MonoBehaviour
{
    [SerializeField] private VirtualJoystick moveJoystick;
    [SerializeField] private VirtualJoystick aimJoystick;
    [SerializeField] private IsometricPlayerController playerController;

    private void Update()
    {
        playerController.SetMoveInput(moveJoystick.Value);
        playerController.SetAimInput(aimJoystick.Value);
    }
}

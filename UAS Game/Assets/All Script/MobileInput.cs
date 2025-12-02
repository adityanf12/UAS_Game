using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInput : MonoBehaviour
{
    public void OnLeftButtonDown(BaseEventData eventData)
    {
        GameInput.SetMoveLeft(true);
        Debug.Log("Left Down: Mulai Bergerak Kiri");
    }

    public void OnRightButtonDown(BaseEventData eventData)
    {
        GameInput.SetMoveRight(true);
        Debug.Log("Right Down: Mulai Bergerak Kanan");
    }
 
    public void OnJumpButton(BaseEventData eventData)
    {
        GameInput.DoJump();
        Debug.Log("Jump Pressed");
    }

    public void OnDashButton(BaseEventData eventData)
    {
        GameInput.DoDash();
        Debug.Log("Dash Pressed");
    }

    public void OnAttackButton(BaseEventData eventData)
    {
        GameInput.DoAttack();
        Debug.Log("Attack Pressed");
    }

    public void OnLeftButtonUp(BaseEventData eventData)
    {
        GameInput.SetMoveLeft(false);
        Debug.Log("Left Up: Berhenti Bergerak Kiri");
    }
    
    public void OnRightButtonUp(BaseEventData eventData)
    {
        GameInput.SetMoveRight(false);
        Debug.Log("Right Up: Berhenti Bergerak Kanan");
    }
}
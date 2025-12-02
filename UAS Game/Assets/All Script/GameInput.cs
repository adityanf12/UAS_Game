using UnityEngine;

public static class GameInput
{
    public static bool MoveLeft { get; private set; }
    public static bool MoveRight { get; private set; }

    public static bool JumpPressed { get; private set; }
    public static bool DashPressed { get; private set; }
    public static bool AttackPressed { get; private set; }

    public static void SetMoveLeft(bool value) { MoveLeft = value; }
    public static void SetMoveRight(bool value) { MoveRight = value; }

    public static void DoJump() { JumpPressed = true; }
    public static void DoDash() { DashPressed = true; }
    public static void DoAttack() { AttackPressed = true; }

    public static void ResetAksi()
    {
        JumpPressed = false;
        DashPressed = false;
        AttackPressed = false;
    }
}
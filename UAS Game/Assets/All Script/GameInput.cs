using UnityEngine;

public static class GameInput
{
    // Properti untuk gerakan: True selama tombol Down (berlaku untuk Left/Right)
    public static bool MoveLeft { get; private set; }
    public static bool MoveRight { get; private set; }

    // Properti untuk aksi sekali tekan:
    // Nilai ini diatur True oleh MobileInput, dan di-reset False oleh PlayerController setelah dibaca.
    public static bool JumpPressed { get; private set; }
    public static bool DashPressed { get; private set; }
    public static bool AttackPressed { get; private set; }

    // --- Method untuk di Panggil oleh Tombol UI (MobileInput.cs) ---

    public static void SetMoveLeft(bool value) { MoveLeft = value; }
    public static void SetMoveRight(bool value) { MoveRight = value; }

    public static void DoJump() { JumpPressed = true; }
    public static void DoDash() { DashPressed = true; }
    public static void DoAttack() { AttackPressed = true; }

    // PENTING: Method ini harus dipanggil di FixedUpdate() PlayerController
    public static void ResetAksi()
    {
        // Reset hanya aksi yang bertipe 'satu kali tekan'
        JumpPressed = false;
        DashPressed = false;
        AttackPressed = false;
    }
}
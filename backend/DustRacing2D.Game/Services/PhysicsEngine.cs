using DustRacing2D.Game.Models;

namespace DustRacing2D.Game.Services;

/// <summary>
/// Server-authoritative physics simulation for a single player.
/// Constants derived from game designer review (┬º12 of spec).
/// </summary>
public static class PhysicsEngine
{
    public const double MaxSpeed    = 380.0;  // units/s
    public const double Acceleration = 280.0;
    public const double BrakeForce  = 520.0;
    public const double Friction    = 0.96;   // per-second multiplier (applied via pow)
    public const double TurnRateMax = 2.8;    // rad/s at full speed
    public const double TurnRateMin = 1.2;    // rad/s at low speed
    public const double CarWidth    = 18.0;
    public const double CarHeight   = 30.0;

    public static void Step(PlayerState player, double deltaTime)
    {
        var input = player.Input;

        // Acceleration / braking
        if (input.Accelerate)
            player.Speed = Math.Min(player.Speed + Acceleration * deltaTime, MaxSpeed);
        else if (input.Brake)
            player.Speed = Math.Max(player.Speed - BrakeForce * deltaTime, 0);
        else
            player.Speed *= Math.Pow(Friction, deltaTime);

        // Speed-proportional steering
        double speedRatio = player.Speed / MaxSpeed;
        double turnRate = TurnRateMin + (TurnRateMax - TurnRateMin) * speedRatio;

        if (input.TurnLeft)  player.Angle -= turnRate * deltaTime;
        if (input.TurnRight) player.Angle += turnRate * deltaTime;

        // Move
        player.X += Math.Cos(player.Angle) * player.Speed * deltaTime;
        player.Y += Math.Sin(player.Angle) * player.Speed * deltaTime;
    }
}

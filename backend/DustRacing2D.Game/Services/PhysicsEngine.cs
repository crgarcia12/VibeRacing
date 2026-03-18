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

    // Radius used for circle-vs-circle collision (half the diagonal of the car bounding box).
    public const double CarRadius = 18.0;

    /// <summary>
    /// Detects and resolves overlapping cars for all pairs in the provided collection.
    /// Each car is treated as a circle of radius <see cref="CarRadius"/>.
    /// Overlapping cars are pushed apart along the collision normal and exchange
    /// velocity components along that normal (elastic collision).
    /// </summary>
    public static void ResolveCollisions(IEnumerable<PlayerState> players)
    {
        var list = players.ToList();
        double diameter = CarRadius * 2.0;

        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                var a = list[i];
                var b = list[j];

                double dx = b.X - a.X;
                double dy = b.Y - a.Y;
                double distSq = dx * dx + dy * dy;

                if (distSq >= diameter * diameter || distSq < 1e-10)
                    continue;

                double dist = Math.Sqrt(distSq);
                // Unit normal pointing from a to b
                double nx = dx / dist;
                double ny = dy / dist;

                // Separate: push each car out by half the overlap
                double overlap = diameter - dist;
                double half = overlap * 0.5;
                a.X -= nx * half;
                a.Y -= ny * half;
                b.X += nx * half;
                b.Y += ny * half;

                // Velocity vectors along heading
                double avx = Math.Cos(a.Angle) * a.Speed;
                double avy = Math.Sin(a.Angle) * a.Speed;
                double bvx = Math.Cos(b.Angle) * b.Speed;
                double bvy = Math.Sin(b.Angle) * b.Speed;

                // Relative velocity along the collision normal
                double relVn = (avx - bvx) * nx + (avy - bvy) * ny;

                // Only resolve if cars are approaching each other
                if (relVn > 0)
                {
                    // Swap the normal components of velocity (equal-mass elastic)
                    double aVn = avx * nx + avy * ny;
                    double bVn = bvx * nx + bvy * ny;

                    // Replace normal components
                    avx += (bVn - aVn) * nx;
                    avy += (bVn - aVn) * ny;
                    bvx += (aVn - bVn) * nx;
                    bvy += (aVn - bVn) * ny;

                    a.Speed = Math.Sqrt(avx * avx + avy * avy);
                    b.Speed = Math.Sqrt(bvx * bvx + bvy * bvy);

                    if (a.Speed > 1e-6)
                        a.Angle = Math.Atan2(avy, avx);
                    if (b.Speed > 1e-6)
                        b.Angle = Math.Atan2(bvy, bvx);

                    // Clamp to MaxSpeed
                    a.Speed = Math.Min(a.Speed, MaxSpeed);
                    b.Speed = Math.Min(b.Speed, MaxSpeed);
                }
            }
        }
    }

    /// <summary>Wall-collision bounce damping: speed is multiplied by this on impact.</summary>
    public const double WallBounceDamping = 0.4;

    public static void Step(PlayerState player, double deltaTime, double trackWidth = double.MaxValue, double trackHeight = double.MaxValue)
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

        // Wall collision: keep the car (half-size bounding box) inside the track canvas
        ApplyWallCollision(player, trackWidth, trackHeight);
    }

    private static void ApplyWallCollision(PlayerState player, double trackWidth, double trackHeight)
    {
        double halfW = CarWidth  / 2.0;
        double halfH = CarHeight / 2.0;

        bool bounced = false;

        if (player.X - halfW < 0)
        {
            player.X = halfW;
            bounced = true;
            // Reflect horizontal velocity component by flipping the angle across the vertical wall
            player.Angle = Math.PI - player.Angle;
        }
        else if (player.X + halfW > trackWidth)
        {
            player.X = trackWidth - halfW;
            bounced = true;
            player.Angle = Math.PI - player.Angle;
        }

        if (player.Y - halfH < 0)
        {
            player.Y = halfH;
            bounced = true;
            player.Angle = -player.Angle;
        }
        else if (player.Y + halfH > trackHeight)
        {
            player.Y = trackHeight - halfH;
            bounced = true;
            player.Angle = -player.Angle;
        }

        if (bounced)
            player.Speed *= WallBounceDamping;
    }
}

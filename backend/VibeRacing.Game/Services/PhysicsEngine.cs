using VibeRacing.Game.Models;

namespace VibeRacing.Game.Services;

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
    public const double OffRoadFriction = 0.82;
    public const double OffRoadAccelerationMultiplier = 0.45;
    public const double OffRoadMaxSpeedMultiplier = 0.55;
    public const double TurnRateMax = 2.8;    // rad/s at full speed
    public const double TurnRateMin = 1.2;    // rad/s at low speed
    public const double CarWidth    = 18.0;
    public const double CarHeight   = 30.0;

    // Radius used for circle-vs-circle collision (half the diagonal of the car bounding box).
    public const double CarRadius = 18.0;

    private const double CarCollisionRestitution = 0.22;
    private const double MinimumImpactForwardDot = 0.35;
    private const double VelocityEpsilon = 1e-6;

    /// <summary>
    /// Detects and resolves overlapping cars for all pairs in the provided collection.
    /// Each car is treated as a circle of radius <see cref="CarRadius"/>.
    /// Overlapping cars are pushed apart along the collision normal and lose some
    /// closing speed along that normal so crashes feel weighty without snapping a
    /// car into a full heading reversal.
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
                    double impulse = ((1.0 + CarCollisionRestitution) * relVn) * 0.5;

                    avx -= impulse * nx;
                    avy -= impulse * ny;
                    bvx += impulse * nx;
                    bvy += impulse * ny;

                    ApplyImpactMotion(a, avx, avy);
                    ApplyImpactMotion(b, bvx, bvy);
                }
            }
        }
    }

    /// <summary>Crash damping applied after wall impacts.</summary>
    public const double WallBounceDamping = 0.4;

    public static double GetPlayableMargin(TrackData track)
    {
        return Math.Clamp(track.TileSize * 0.30, CarRadius, track.TileSize * 0.45);
    }

    public static void Step(PlayerState player, double deltaTime, TrackData track)
    {
        var input = player.Input;
        bool onRoad = track.IsOnRoad(player.X, player.Y);
        double effectiveAcceleration = onRoad ? Acceleration : Acceleration * OffRoadAccelerationMultiplier;
        double effectiveMaxSpeed = onRoad ? MaxSpeed : MaxSpeed * OffRoadMaxSpeedMultiplier;
        double effectiveFriction = onRoad ? Friction : OffRoadFriction;

        // Acceleration / braking
        if (input.Accelerate)
            player.Speed = Math.Min(player.Speed + effectiveAcceleration * deltaTime, effectiveMaxSpeed);
        else if (input.Brake)
            player.Speed = Math.Max(player.Speed - BrakeForce * deltaTime, 0);
        else
            player.Speed *= Math.Pow(effectiveFriction, deltaTime);

        player.Speed = Math.Min(player.Speed, effectiveMaxSpeed);

        // Speed-proportional steering
        double speedRatio = player.Speed / MaxSpeed;
        double turnRate = TurnRateMin + (TurnRateMax - TurnRateMin) * speedRatio;

        if (input.TurnLeft)  player.Angle -= turnRate * deltaTime;
        if (input.TurnRight) player.Angle += turnRate * deltaTime;

        // Move
        player.X += Math.Cos(player.Angle) * player.Speed * deltaTime;
        player.Y += Math.Sin(player.Angle) * player.Speed * deltaTime;

        // Keep cars close to the course and inside the track canvas.
        ApplyTrackCollision(player, track);

        if (!track.IsOnRoad(player.X, player.Y))
            player.Speed = Math.Min(player.Speed, MaxSpeed * OffRoadMaxSpeedMultiplier);
    }

    private static void ApplyTrackCollision(PlayerState player, TrackData track)
    {
        ApplyCanvasWallCollision(player, track.Width, track.Height);

        double margin = GetPlayableMargin(track);
        if (!track.TryClampToPlayableArea(player.X, player.Y, margin, out var clampedX, out var clampedY, out var normalX, out var normalY))
            return;

        player.X = clampedX;
        player.Y = clampedY;
        if (ClipVelocityAgainstNormal(player, normalX, normalY))
            player.Speed *= WallBounceDamping;
    }

    private static void ApplyCanvasWallCollision(PlayerState player, double trackWidth, double trackHeight)
    {
        double halfW = CarWidth  / 2.0;
        double halfH = CarHeight / 2.0;

        bool bounced = false;

        if (player.X - halfW < 0)
        {
            player.X = halfW;
            bounced |= ClipVelocityAgainstNormal(player, -1.0, 0.0);
        }
        else if (player.X + halfW > trackWidth)
        {
            player.X = trackWidth - halfW;
            bounced |= ClipVelocityAgainstNormal(player, 1.0, 0.0);
        }

        if (player.Y - halfH < 0)
        {
            player.Y = halfH;
            bounced |= ClipVelocityAgainstNormal(player, 0.0, -1.0);
        }
        else if (player.Y + halfH > trackHeight)
        {
            player.Y = trackHeight - halfH;
            bounced |= ClipVelocityAgainstNormal(player, 0.0, 1.0);
        }

        if (bounced)
            player.Speed *= WallBounceDamping;
    }

    private static bool ClipVelocityAgainstNormal(PlayerState player, double normalX, double normalY)
    {
        if (normalX == 0 && normalY == 0)
            return false;

        double vx = Math.Cos(player.Angle) * player.Speed;
        double vy = Math.Sin(player.Angle) * player.Speed;
        double normalVelocity = (vx * normalX) + (vy * normalY);

        if (normalVelocity <= 0)
            return false;

        vx -= normalVelocity * normalX;
        vy -= normalVelocity * normalY;

        ApplyImpactMotion(player, vx, vy);
        return true;
    }

    private static void ApplyImpactMotion(PlayerState player, double vx, double vy)
    {
        double speed = Math.Sqrt((vx * vx) + (vy * vy));
        player.Speed = Math.Min(speed, MaxSpeed);

        if (player.Speed <= VelocityEpsilon)
        {
            player.Speed = 0;
            return;
        }

        player.Angle = GetNaturalImpactHeading(player.Angle, vx, vy);
    }

    private static double GetNaturalImpactHeading(double currentAngle, double vx, double vy)
    {
        double magnitudeSq = (vx * vx) + (vy * vy);
        if (magnitudeSq <= VelocityEpsilon * VelocityEpsilon)
            return currentAngle;

        double inverseMagnitude = 1.0 / Math.Sqrt(magnitudeSq);
        double targetX = vx * inverseMagnitude;
        double targetY = vy * inverseMagnitude;

        double forwardX = Math.Cos(currentAngle);
        double forwardY = Math.Sin(currentAngle);
        double forwardDot = (targetX * forwardX) + (targetY * forwardY);

        if (forwardDot >= MinimumImpactForwardDot)
            return Math.Atan2(targetY, targetX);

        double sideX = -forwardY;
        double sideY = forwardX;
        double sideDot = (targetX * sideX) + (targetY * sideY);

        if (Math.Abs(sideDot) <= VelocityEpsilon)
            return currentAngle;

        double adjustedX = (forwardX * MinimumImpactForwardDot) + (sideX * sideDot);
        double adjustedY = (forwardY * MinimumImpactForwardDot) + (sideY * sideDot);
        return Math.Atan2(adjustedY, adjustedX);
    }
}

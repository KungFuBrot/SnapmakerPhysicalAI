namespace Snapmaker.Core.Machine;

/// <summary>Position im Maschinenkoordinatensystem, alle Werte in Millimetern.</summary>
public readonly record struct Position(double X, double Y, double Z)
{
    public static Position Origin { get; } = new(0, 0, 0);

    public override string ToString() => $"X{X:0.###} Y{Y:0.###} Z{Z:0.###}";
}

/// <summary>Quaderförmiger Arbeitsraum, beginnend im Nullpunkt.</summary>
public sealed record WorkVolume(double X, double Y, double Z)
{
    public bool Contains(Position p) =>
        p.X >= 0 && p.X <= X &&
        p.Y >= 0 && p.Y <= Y &&
        p.Z >= 0 && p.Z <= Z;
}

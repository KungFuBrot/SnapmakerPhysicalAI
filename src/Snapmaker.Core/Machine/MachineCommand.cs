using System.Globalization;

namespace Snapmaker.Core.Machine;

/// <summary>
/// Befehle, die eine KI oder ein Mensch an die Maschine schicken darf. Bewusst ein kleiner,
/// geschlossener Satz statt freiem G-Code: Nur was hier modelliert ist, kann geprüft werden.
/// </summary>
public abstract record MachineCommand
{
    public abstract string ToGCode();

    protected static string F(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>Referenzfahrt aller Achsen.</summary>
public sealed record Home : MachineCommand
{
    public override string ToGCode() => "G28";
}

/// <summary>Lineare Bewegung auf eine absolute Position.</summary>
public sealed record MoveTo(Position Target, double FeedrateMmPerMin) : MachineCommand
{
    public override string ToGCode() =>
        $"G90\nG1 X{F(Target.X)} Y{F(Target.Y)} Z{F(Target.Z)} F{F(FeedrateMmPerMin)}";
}

/// <summary>Laser einschalten, Leistung in Prozent (0 bis 100).</summary>
public sealed record LaserOn(double PowerPercent) : MachineCommand
{
    public override string ToGCode() => $"M3 P{F(PowerPercent)}";
}

public sealed record LaserOff : MachineCommand
{
    public override string ToGCode() => "M5";
}

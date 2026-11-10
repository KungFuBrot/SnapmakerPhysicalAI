namespace Snapmaker.Core.Machine;

public enum Toolhead
{
    Printer3D,
    Laser,
    Cnc,
}

/// <summary>
/// Physische Grenzen einer Maschine. Die Sicherheitsschicht prüft jeden Befehl gegen dieses Profil.
/// </summary>
public sealed record MachineProfile(
    string Name,
    IReadOnlyDictionary<Toolhead, WorkVolume> WorkVolumes,
    double MaxFeedrateMmPerMin,
    double MaxLaserPowerPercent)
{
    /// <summary>
    /// Snapmaker 2.0 A350T. Arbeitsräume laut Herstellerangaben; vor dem ersten echten Lauf
    /// an der eigenen Maschine nachmessen. Die Leistungsgrenze für den Laser ist bewusst
    /// konservativ und wird erst nach Tests angehoben.
    /// </summary>
    public static MachineProfile SnapmakerA350T { get; } = new(
        Name: "Snapmaker 2.0 A350T",
        WorkVolumes: new Dictionary<Toolhead, WorkVolume>
        {
            [Toolhead.Printer3D] = new(320, 350, 330),
            [Toolhead.Laser] = new(320, 350, 330),
            [Toolhead.Cnc] = new(320, 350, 275),
        },
        MaxFeedrateMmPerMin: 6000,
        MaxLaserPowerPercent: 30);

    public WorkVolume VolumeFor(Toolhead toolhead) => WorkVolumes[toolhead];
}

/// <summary>Zustand der Maschine, wie ihn die Verbindung meldet.</summary>
public sealed record MachineState(
    Toolhead Toolhead,
    bool IsHomed,
    Position Position,
    double LaserPowerPercent,
    bool EnclosureClosed)
{
    public bool LaserActive => LaserPowerPercent > 0;
}

using Snapmaker.Core.Machine;

namespace Snapmaker.Core.Safety;

public sealed record SafetyVerdict(bool Allowed, string Reason)
{
    public static SafetyVerdict Ok { get; } = new(true, "ok");

    public static SafetyVerdict Deny(string reason) => new(false, reason);
}

/// <summary>
/// Deterministische Prüfschicht zwischen Planung (Mensch oder KI) und Maschine.
/// Sie kennt keine Absicht, nur Regeln: Was gegen eine Regel verstößt, wird abgelehnt.
/// </summary>
public sealed class SafetyGuard(MachineProfile profile)
{
    public MachineProfile Profile { get; } = profile;

    public SafetyVerdict Evaluate(MachineState state, MachineCommand command) => command switch
    {
        Home => SafetyVerdict.Ok,
        LaserOff => SafetyVerdict.Ok,
        MoveTo move => CheckMove(state, move),
        LaserOn laser => CheckLaser(state, laser),
        _ => SafetyVerdict.Deny($"Unbekannter Befehl {command.GetType().Name}."),
    };

    private SafetyVerdict CheckMove(MachineState state, MoveTo move)
    {
        if (!state.IsHomed)
            return SafetyVerdict.Deny("Maschine ist nicht referenziert. Zuerst Home ausführen.");
        if (move.FeedrateMmPerMin <= 0 || move.FeedrateMmPerMin > Profile.MaxFeedrateMmPerMin)
            return SafetyVerdict.Deny(
                $"Vorschub {move.FeedrateMmPerMin} mm/min liegt außerhalb von 0 bis {Profile.MaxFeedrateMmPerMin}.");
        if (!Profile.VolumeFor(state.Toolhead).Contains(move.Target))
            return SafetyVerdict.Deny($"Ziel {move.Target} liegt außerhalb des Arbeitsraums für {state.Toolhead}.");
        return SafetyVerdict.Ok;
    }

    private SafetyVerdict CheckLaser(MachineState state, LaserOn laser)
    {
        if (state.Toolhead != Toolhead.Laser)
            return SafetyVerdict.Deny($"Laser angefordert, montiert ist aber {state.Toolhead}.");
        if (!state.EnclosureClosed)
            return SafetyVerdict.Deny("Gehäuse ist offen. Laser nur bei geschlossenem Gehäuse.");
        if (!state.IsHomed)
            return SafetyVerdict.Deny("Maschine ist nicht referenziert.");
        if (laser.PowerPercent <= 0 || laser.PowerPercent > Profile.MaxLaserPowerPercent)
            return SafetyVerdict.Deny(
                $"Laserleistung {laser.PowerPercent} % liegt außerhalb von 0 bis {Profile.MaxLaserPowerPercent} %.");
        return SafetyVerdict.Ok;
    }
}

public sealed class SafetyViolationException(MachineCommand command, string reason)
    : InvalidOperationException($"Abgelehnt: {command} ({reason})")
{
    public MachineCommand Command { get; } = command;
    public string Reason { get; } = reason;
}

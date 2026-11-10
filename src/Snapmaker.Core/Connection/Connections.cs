using Snapmaker.Core.Machine;
using Snapmaker.Core.Safety;

namespace Snapmaker.Core.Connection;

/// <summary>
/// Eine Verbindung zu einer Maschine: echt (WLAN/USB) oder simuliert.
/// Höhere Schichten sprechen ausschließlich über <see cref="SafeMachine"/> mit ihr.
/// </summary>
public interface IMachineConnection
{
    Task<MachineState> GetStateAsync(CancellationToken ct = default);

    Task ExecuteAsync(MachineCommand command, CancellationToken ct = default);
}

/// <summary>
/// Der einzige Weg zur Maschine: Jeder Befehl wird vor dem Senden gegen den aktuellen Zustand geprüft.
/// </summary>
public sealed class SafeMachine(IMachineConnection inner, SafetyGuard guard)
{
    public async Task<SafetyVerdict> TryExecuteAsync(MachineCommand command, CancellationToken ct = default)
    {
        var state = await inner.GetStateAsync(ct);
        var verdict = guard.Evaluate(state, command);
        if (verdict.Allowed)
            await inner.ExecuteAsync(command, ct);
        return verdict;
    }

    public async Task ExecuteAsync(MachineCommand command, CancellationToken ct = default)
    {
        var verdict = await TryExecuteAsync(command, ct);
        if (!verdict.Allowed)
            throw new SafetyViolationException(command, verdict.Reason);
    }

    public Task<MachineState> GetStateAsync(CancellationToken ct = default) => inner.GetStateAsync(ct);
}

/// <summary>
/// Maschine ohne Hardware: führt Befehle aus, protokolliert den erzeugten G-Code
/// und führt den Zustand nach. Grundlage für Tests und für KI-Läufe vor dem ersten echten Job.
/// </summary>
public sealed class SimulatedMachine(Toolhead toolhead = Toolhead.Laser, bool enclosureClosed = true)
    : IMachineConnection
{
    private MachineState _state = new(toolhead, IsHomed: false, Position.Origin, 0, enclosureClosed);
    private readonly List<string> _gcode = [];

    public IReadOnlyList<string> GCodeLog => _gcode;

    public void SetEnclosure(bool closed) => _state = _state with { EnclosureClosed = closed };

    public Task<MachineState> GetStateAsync(CancellationToken ct = default) => Task.FromResult(_state);

    public Task ExecuteAsync(MachineCommand command, CancellationToken ct = default)
    {
        _gcode.Add(command.ToGCode());
        _state = command switch
        {
            Home => _state with { IsHomed = true, Position = Position.Origin },
            MoveTo m => _state with { Position = m.Target },
            LaserOn l => _state with { LaserPowerPercent = l.PowerPercent },
            LaserOff => _state with { LaserPowerPercent = 0 },
            _ => _state,
        };
        return Task.CompletedTask;
    }
}

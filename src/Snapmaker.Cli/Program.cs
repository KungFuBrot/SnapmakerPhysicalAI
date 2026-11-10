using Snapmaker.Core.Connection;
using Snapmaker.Core.Machine;
using Snapmaker.Core.Safety;

// Demo gegen den Simulator: ein Quadrat lasern, dann einen unzulässigen Befehl versuchen.
var sim = new SimulatedMachine(Toolhead.Laser);
var machine = new SafeMachine(sim, new SafetyGuard(MachineProfile.SnapmakerA350T));

MachineCommand[] plan =
[
    new Home(),
    new MoveTo(new Position(50, 50, 10), 3000),
    new LaserOn(20),
    new MoveTo(new Position(100, 50, 10), 1200),
    new MoveTo(new Position(100, 100, 10), 1200),
    new MoveTo(new Position(50, 100, 10), 1200),
    new MoveTo(new Position(50, 50, 10), 1200),
    new LaserOff(),
    new MoveTo(new Position(500, 50, 10), 3000), // außerhalb des Arbeitsraums
];

foreach (var cmd in plan)
{
    var verdict = await machine.TryExecuteAsync(cmd);
    Console.WriteLine($"{(verdict.Allowed ? "OK  " : "STOP")} {cmd,-55} {(verdict.Allowed ? "" : verdict.Reason)}");
}

Console.WriteLine();
Console.WriteLine("Erzeugter G-Code:");
foreach (var line in sim.GCodeLog)
    Console.WriteLine(line);

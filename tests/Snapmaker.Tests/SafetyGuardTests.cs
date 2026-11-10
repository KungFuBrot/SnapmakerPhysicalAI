using Snapmaker.Core.Connection;
using Snapmaker.Core.Machine;
using Snapmaker.Core.Safety;

namespace Snapmaker.Tests;

public class SafetyGuardTests
{
    private static readonly SafetyGuard Guard = new(MachineProfile.SnapmakerA350T);

    private static MachineState Homed(Toolhead toolhead = Toolhead.Laser, bool closed = true) =>
        new(toolhead, IsHomed: true, Position.Origin, 0, closed);

    [Fact]
    public void Move_before_homing_is_denied()
    {
        var state = Homed() with { IsHomed = false };
        Assert.False(Guard.Evaluate(state, new MoveTo(new(10, 10, 10), 1000)).Allowed);
    }

    [Theory]
    [InlineData(-1, 10, 10)]
    [InlineData(321, 10, 10)]
    [InlineData(10, 351, 10)]
    [InlineData(10, 10, 331)]
    public void Move_outside_work_volume_is_denied(double x, double y, double z)
    {
        Assert.False(Guard.Evaluate(Homed(), new MoveTo(new(x, y, z), 1000)).Allowed);
    }

    [Fact]
    public void Cnc_has_lower_z_limit_than_laser()
    {
        var target = new MoveTo(new(10, 10, 300), 1000);
        Assert.True(Guard.Evaluate(Homed(Toolhead.Laser), target).Allowed);
        Assert.False(Guard.Evaluate(Homed(Toolhead.Cnc), target).Allowed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6001)]
    public void Feedrate_out_of_range_is_denied(double feed)
    {
        Assert.False(Guard.Evaluate(Homed(), new MoveTo(new(10, 10, 10), feed)).Allowed);
    }

    [Fact]
    public void Laser_requires_closed_enclosure()
    {
        Assert.False(Guard.Evaluate(Homed(closed: false), new LaserOn(10)).Allowed);
        Assert.True(Guard.Evaluate(Homed(closed: true), new LaserOn(10)).Allowed);
    }

    [Fact]
    public void Laser_requires_laser_toolhead()
    {
        Assert.False(Guard.Evaluate(Homed(Toolhead.Printer3D), new LaserOn(10)).Allowed);
    }

    [Fact]
    public void Laser_power_above_profile_cap_is_denied()
    {
        Assert.False(Guard.Evaluate(Homed(), new LaserOn(31)).Allowed);
    }

    [Fact]
    public async Task SafeMachine_never_sends_a_denied_command()
    {
        var sim = new SimulatedMachine(Toolhead.Laser, enclosureClosed: false);
        var machine = new SafeMachine(sim, Guard);

        await machine.ExecuteAsync(new Home());
        await Assert.ThrowsAsync<SafetyViolationException>(() => machine.ExecuteAsync(new LaserOn(10)));

        Assert.Equal(["G28"], sim.GCodeLog);
    }

    [Fact]
    public void GCode_uses_invariant_culture()
    {
        var gcode = new MoveTo(new(10.5, 20.25, 3), 1200).ToGCode();
        Assert.Contains("X10.5 Y20.25 Z3 F1200", gcode);
    }
}

using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Explosion;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server._Cats.Commands;

[AdminCommand(AdminFlags.Fun)]
public sealed class BSAHitCommand : IConsoleCommand
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IEntitySystemManager _system = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public string Command => "bsahit";
    public string Description => Loc.GetString("bsa-hit-description");
    public string Help => Loc.GetString("bsa-hit-help");
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var player = shell.Player as IPlayerSession;
        if (player?.AttachedEntity == null) { shell.WriteLine(Loc.GetString("shell-only-players-can-run-this-command")); return; }

        Filter filter = Filter.Empty().AddAllPlayers(_playerManager);
        var audioOption = AudioParams.Default; audioOption = audioOption.WithVolume(0);

        _entManager.System<ServerGlobalSoundSystem>().PlayAdminGlobal(filter, "/Audio/Corvax/Adminbuse/artillery.ogg", audioOption, true); //Play audio
        _system.GetEntitySystem<ChatSystem>().DispatchGlobalAnnouncement(
            Loc.GetString($"bsa-hit-announcement"),
            Loc.GetString($"bsa-hit-announcer"),
            playSound: false, colorOverride: Color.Gold); // Write announcement

        MapCoordinates coords;
        if (!_entManager.TryGetComponent(shell.Player?.AttachedEntity, out TransformComponent? xform))
        { shell.WriteError(Loc.GetString($"bsa-hit-coords-error")); return; }
        coords = xform.MapPosition; // Get coords

        Timer.Spawn(4500, () =>
        {
            ExplosionPrototype explosionType = _prototypeManager.Index<ExplosionPrototype>("MicroBomb");
            _system.GetEntitySystem<ExplosionSystem>().QueueExplosion(coords, explosionType.ID, 20000, 5, 50);
        }); //EXPLOSIOOOON!

        _adminLogger.Add(LogType.Action, LogImpact.High, $"{player} use BSA. Coords - {coords}"); //Add log
    }
}
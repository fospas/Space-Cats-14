﻿using System.Globalization;
using Content.Server.Bed.Cryostorage;
using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Objectives.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Objectives.Systems;
using Content.Shared.StationRecords;
using FastAccessors;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Cats.CryoAfk;

public sealed class CryoAfkSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly CryostorageSystem _cryostorage = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialized);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnCompleteSpawn);
        // SubscribeLocalEvent<CryoAFKTargetComponent, BeingGibbedEvent>(OnGibbed);
        SubscribeLocalEvent<CryoAFKTargetComponent, PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<CryoAFKTargetComponent, PlayerAttachedEvent>(OnPlayerAttached);
    }

    public override void Update(float delay)
    {
        var query = AllEntityQuery<CryoAFKTargetComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Log.Info($"uid: {uid}, comp: {comp.Station}");

            if (comp.Station == null)
                continue;

            if (!TryComp<StationCryoTeleportComponent>(comp.Station, out var stationCryoTeleportComponent))
                continue;

            if (comp.ExitTime == null)
                continue;

            if (_timing.CurTime - comp.ExitTime >= stationCryoTeleportComponent.TransferDelay)
            {
                if (HasComp<CryostorageContainedComponent>(uid))
                    continue;

                var containedComp = AddComp<CryostorageContainedComponent>(uid);

                containedComp.Cryostorage = FindCryo(comp.Station.Value, Transform(uid));
                containedComp.GracePeriodEndTime = _timing.CurTime;

                _mind.TryGetMind(uid, out var _, out var mindComponent);

                if (mindComponent == null)
                    continue;

                _entity.SpawnEntity(stationCryoTeleportComponent.PortalPrototype, Transform(uid).Coordinates);
                _audio.PlayPvs(stationCryoTeleportComponent.TransferSound, Transform(uid).Coordinates);

                _cryostorage.HandleEnterCryostorage((uid, containedComp), mindComponent.UserId);
            }
        }
    }

    private void OnStationInitialized(StationInitializedEvent ev)
    {
        if (FindCryo(ev.Station, Transform(ev.Station)) == null)
            return;
        EnsureComp<StationCryoTeleportComponent>(ev.Station);
    }

    private void OnCompleteSpawn(PlayerSpawnCompleteEvent ev)
    {
        if (!TryComp<StationCryoTeleportComponent>(ev.Station, out var cryoTeleportComponent))
            return;
        if (ev.JobId == null)
            return;

        if (ev.Player.AttachedEntity == null)
            return;

        var CryoAFKTargetComponent = EnsureComp<CryoAFKTargetComponent>(ev.Player.AttachedEntity.Value);
        CryoAFKTargetComponent.Station = ev.Station;
    }

    private void OnPlayerDetached(EntityUid uid, CryoAFKTargetComponent comp, PlayerDetachedEvent ev)
    {
        comp.ExitTime = _timing.CurTime;
    }

    private void OnPlayerAttached(EntityUid uid, CryoAFKTargetComponent comp, PlayerAttachedEvent ev)
    {
        comp.ExitTime = null;
    }

    private EntityUid? FindCryo(EntityUid station, TransformComponent entityXform)
    {
        var query = AllEntityQuery<CryostorageComponent>();
        while (query.MoveNext(out var cryoUid, out var cryostorageComponent))
        {
            if (!TryComp<TransformComponent>(cryoUid, out var cryoTransform))
                return null;

            if (entityXform.MapUid != cryoTransform.MapUid)
                continue;

            return cryoUid;
        }

        return null;
    }
}

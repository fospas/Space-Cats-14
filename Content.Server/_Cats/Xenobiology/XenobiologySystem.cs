using System.Numerics;
using Content.Server._Cats.XenoBiology.Components;
using Content.Server._Cats.XenoFood.Components;
using Content.Shared.Polymorph.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Polymorph.Components;
using Content.Shared.Mind;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Server._Cats.XenoBiology.Systems;

public sealed class XenoBiologySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenoBiologyComponent, MeleeHitEvent>(OnSlimeAttack);
    }

    private void OnSlimeAttack(EntityUid uid, XenoBiologyComponent component, ref MeleeHitEvent args)
    {
        foreach (var hitEntity in args.HitEntities)
        {
            if (EntityManager.HasComponent<XenoFoodComponent>(hitEntity))
            {
                if (_mobState.IsIncapacitated(hitEntity))
                return;

                component.Points += component.PointsPerAttack;
                break;
            }
        }
    }

    public override void Update(float frameTime)
    {
        var xenoQuery = EntityQueryEnumerator<XenoBiologyComponent, TransformComponent>();
        while (xenoQuery.MoveNext(out var uid, out var component, out var transform))
        {
            var prototype = MetaData(uid).EntityPrototype?.ID;

            if (component.Points >= component.PointsThreshold)
            {
   
                if (TryComp<MindContainerComponent>(uid, out var mindContainer) && mindContainer.HasMind)
                {
                    _polymorph.PolymorphEntity(uid, component.OnMind);
                }
                else

                if (_robustRandom.Prob(component.Mutationchance))
                {
                    Spawn(component.Mutagen, Transform(uid).Coordinates);
                }
                else
                {
                    Spawn(prototype, Transform(uid).Coordinates);
                }

                if (_robustRandom.Prob(component.Mutationchance))
                {
                    Spawn(component.Mutagen, Transform(uid).Coordinates);
                }
                else
                {
                    Spawn(prototype, Transform(uid).Coordinates);
                }

                if (_robustRandom.Prob(component.Mutationchance))
                {
                    Spawn(component.Mutagen, Transform(uid).Coordinates);
                }
                else
                {
                    Spawn(prototype, Transform(uid).Coordinates);
                }
                EntityManager.DeleteEntity(uid);

                return;
            }

            else if(component.Points > 0 && _robustRandom.Prob(0.001f))
            {
                 component.Points -= 1;
            }
        }
    }
}
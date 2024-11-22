using Content.Server.Modifications.Drug.Components;
using Robust.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Content.Shared.Modifications.Drug.Components;
using Content.Shared.Power;
using Content.Shared.Modifications.Drug;
using Content.Shared.Paper;
using Robust.Shared.Containers;

namespace Content.Server.Modifications.Drug;

public sealed class DrugInitializeMachineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrugInitializeMachineComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<DrugInitializeMachineComponent, StartDrugInitializeEvent>(OnStartInitialize);
        SubscribeLocalEvent<DrugInitializeMachineComponent, MapInitEvent>(OnMapInit);
    }

    protected void OnMapInit(EntityUid uid, DrugInitializeMachineComponent component, MapInitEvent args)
    {
        component.Tube = _container.EnsureContainer<Container>(uid, "tube");
    }

    private void OnPowerChanged(Entity<DrugInitializeMachineComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            SetAppearance(ent, DrugInitializeMachineVisualState.Idle, ent.Comp);
    }

    public void SetAppearance(EntityUid uid, DrugInitializeMachineVisualState state, DrugInitializeMachineComponent? component = null, AppearanceComponent? appearanceComponent = null)
    {
        if (!Resolve(uid, ref component, ref appearanceComponent, false))
            return;

        _appearance.SetData(uid, PowerDeviceVisuals.VisualState, state, appearanceComponent);
    }

    public void OnStartInitialize(EntityUid uid, DrugInitializeMachineComponent component, StartDrugInitializeEvent args)
    {
        if (component.IsRunning)
            return;

        if (!TryComp<DrugTestStickComponent>(args.Stick, out var drugTestStick))
            return;

        _container.Insert(args.Stick, component.Tube);

        var dna = drugTestStick.DNA;
        var dependencyLevel = drugTestStick.DependencyLevel;
        var addictionLevel = (int)Math.Round(drugTestStick.AddictionLevel);
        var tolerance = (int)Math.Round(drugTestStick.Tolerance);
        var withdrawalLevel = (int)Math.Round(drugTestStick.WithdrawalLevel);
        var thresholdTime = (int)Math.Round(drugTestStick.ThresholdTime);


        SetAppearance(uid, DrugInitializeMachineVisualState.Running, component);
        _audio.PlayPvs(component.PrintingSound, uid);
        component.IsRunning = true;

        Timer.Spawn(TimeSpan.FromSeconds(component.RunningTime),
        () =>
        {
            QueueDel(args.Stick);

            if (!EntityManager.EntityExists(uid))
                return;

            SetAppearance(uid, DrugInitializeMachineVisualState.Idle, component);
            var paper = Spawn(component.Paper, Transform(uid).Coordinates);
            if (!TryComp<PaperComponent>(paper, out var paperComp))
            {
                QueueDel(paper);
                return;
            }

            var content = Loc.GetString("drug-test-message-paper",
                ("dna", dna), ("dependency", dependencyLevel), ("addiction", addictionLevel), ("tolerance", tolerance), ("withdrawal", withdrawalLevel), ("time", thresholdTime));

            _paperSystem.SetContent((paper, paperComp), content);
            component.IsRunning = false;
        });

    }
}

[ByRefEvent]
public readonly record struct StartDrugInitializeEvent(EntityUid Stick);
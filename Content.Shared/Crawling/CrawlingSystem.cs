// https://github.com/Mira-Sector/space-station-14/pull/25
using Content.Shared.Buckle.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Explosion;
using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Content.Shared.Standing;
using Robust.Shared.Serialization;
using Content.Shared.Stunnable;
using Robust.Shared.Player;
using Content.Shared.Movement.Systems;
using Content.Shared.Alert;

namespace Content.Shared.Crawling;
public sealed partial class CrawlingSystem : EntitySystem
{
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CrawlerComponent, CrawlStandupDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<CrawlerComponent, StandAttemptEvent>(OnStandUp);
        SubscribeLocalEvent<CrawlerComponent, DownAttemptEvent>(OnFall);
        SubscribeLocalEvent<CrawlerComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<CrawlerComponent, BuckledEvent>(OnBuckled);
        SubscribeLocalEvent<CrawlerComponent, GetExplosionResistanceEvent>(OnGetExplosionResistance);
        SubscribeLocalEvent<CrawlerComponent, CrawlingAlertEvent>(OnCrawlingAlertEvent);
        SubscribeLocalEvent<CrawlerComponent, CrawlingKeybindEvent>(ToggleCrawling);

        SubscribeLocalEvent<CrawlingComponent, ComponentInit>(OnCrawlSlowdownInit);
        SubscribeLocalEvent<CrawlingComponent, ComponentShutdown>(OnCrawlSlowRemove);
        SubscribeLocalEvent<CrawlingComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.ToggleCrawling, InputCmdHandler.FromDelegate(ToggleCrawlingKeybind, handle: false))
            .Register<CrawlingSystem>();
    }

    private bool IsSoftStunned(EntityUid uid)
    {
        if (!TryComp<StaminaComponent>(uid, out var stamComp))
            return false;

        if (stamComp.State != StunnedState.None)
            return true;

        return false;
    }

    private void ToggleCrawlingKeybind(ICommonSession? session)
    {
        if (session?.AttachedEntity == null)
            return;
        var ev = new CrawlingKeybindEvent();
        RaiseLocalEvent(session.AttachedEntity.Value, ev);
    }

    private void ToggleCrawling(EntityUid uid, CrawlerComponent component, CrawlingKeybindEvent args)
    {
        if (args.Cancelled)
            return;

        if (IsSoftStunned(uid) && _standing.IsDown(uid))
        {
            args.Cancelled = true;
            return;
        }

        SetCrawling(uid, component, !_standing.IsDown(uid));
    }
    public void SetCrawling(EntityUid uid, CrawlerComponent component, bool state)
    {
        ///checks players standing state, downing player if they are standding and starts doafter with standing up if they are downed
        switch (state)
        {
            case true:
            {
                _standing.Down(uid, dropHeldItems: false);
                break;
            }
            case false:
            {
                if (HasComp<ActiveStaminaComponent>(uid) &&
                    TryComp<StaminaComponent>(uid, out var staminaComponent) &&
                    staminaComponent.State != StunnedState.None &&
                    staminaComponent.StaminaDamage > staminaComponent.CritThreshold)
                {
                    break;
                }

                _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, uid, component.StandUpTime, new CrawlStandupDoAfterEvent(),
                uid, used: uid)
                {
                    BreakOnDamage = true
                });
                break;
            }
        }
    }
    private void OnCrawlingAlertEvent(EntityUid uid, CrawlerComponent component, CrawlingAlertEvent args)
    {
        var ev = new CrawlingKeybindEvent();
        RaiseLocalEvent(args.User, ev);
    }
    private void OnDoAfter(EntityUid uid, CrawlerComponent component, CrawlStandupDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        _standing.Stand(uid);
    }
    private void OnStandUp(EntityUid uid, CrawlerComponent component, StandAttemptEvent args)
    {
        if (args.Cancelled)
            return;
        RemCompDeferred<CrawlingComponent>(uid);
        _alerts.ClearAlert(uid, component.CtawlingAlert);
    }
    private void OnFall(EntityUid uid, CrawlerComponent component, DownAttemptEvent args)
    {
        if (args.Cancelled)
            return;
        _alerts.ShowAlert(uid, component.CtawlingAlert);
        if (!HasComp<CrawlingComponent>(uid))
            AddComp<CrawlingComponent>(uid);
        //TODO: add hiding under table
    }
    private void OnBuckled(EntityUid uid, CrawlerComponent component, ref BuckledEvent args)
    {
        RemCompDeferred<CrawlingComponent>(uid);
        _alerts.ClearAlert(uid, component.CtawlingAlert);
    }
    private void OnGetExplosionResistance(EntityUid uid, CrawlerComponent component, ref GetExplosionResistanceEvent args)
    {
        // fall on explosion damage and lower explosion damage of crawling
        if (_standing.IsDown(uid))
            args.DamageCoefficient *= component.DownedDamageCoefficient;
        else
            _standing.Down(uid, dropHeldItems: false);
    }
    private void OnCrawlSlowdownInit(EntityUid uid, CrawlingComponent component, ComponentInit args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }
    private void OnCrawlSlowRemove(EntityUid uid, CrawlingComponent component, ComponentShutdown args)
    {
        component.SprintSpeedModifier = 1f;
        component.WalkSpeedModifier = 1f;
        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
    }
    private void OnRefreshMovespeed(EntityUid uid, CrawlingComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
    }
}

[Serializable, NetSerializable]
public sealed partial class CrawlStandupDoAfterEvent : SimpleDoAfterEvent
{
}
public sealed partial class CrawlingAlertEvent : BaseAlertEvent;

[Serializable, NetSerializable]
public sealed partial class CrawlingKeybindEvent
{
    public bool Cancelled;
}
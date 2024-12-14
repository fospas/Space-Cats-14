using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Cats.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed partial class NightVisionComponent : SwitchableOverlayComponent
{
    public override string? ToggleAction { get; set; } = "ToggleNightVision";
}

public sealed partial class ToggleNightVisionEvent : InstantActionEvent;

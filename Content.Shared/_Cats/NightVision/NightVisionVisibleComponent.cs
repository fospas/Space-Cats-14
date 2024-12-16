using Robust.Shared.GameStates;

namespace Content.Shared._Cats.NightVision;

/// <summary>
/// For rendering sprites on top of FOV when the user has a <see cref="NightVisionComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightVisionVisibleComponent : Component
{
    /// <summary>
    /// Priority for rendering order.
    /// Rendered from lowest to highest, which means higher numbers will be rendered above lower numbers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Priority = 0;

    /// <summary>
    /// Transparency of the rendered sprite.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? Transparency = null;
}
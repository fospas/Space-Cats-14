using Content.Client.UserInterface.Controls;
using Content.Shared.Body.Part;
using Content.Shared.Damage.DamageSelector;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Client.Damage.DamageSelector;

[GenerateTypedNameReferences]
public sealed partial class DamageSelectorMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private SpriteSystem _sprites;

    private EntityUid _owner;

    public event Action<BodyPart>? SendDamageSelectorMessageAction;

    public DamageSelectorMenu()
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _sprites = _entManager.System<SpriteSystem>();

        OnChildAdded += AddLimbButtonOnClickActions;
    }

    public void SetEntity(EntityUid uid)
    {
        _owner = uid;
        Refresh();
    }

    public void Refresh()
    {
        // Find the main radial container
        var main = FindControl<RadialContainer>("Main");

        // Populate secondary radial containers
        if (!_entManager.TryGetComponent<DamagePartSelectorComponent>(_owner, out var damageSelector))
            return;

        foreach ((var part, var sprite) in damageSelector.SelectableParts)
        {
            var button = new LimbButton(part)
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64f, 64f),
            };

            var tex = new TextureRect()
            {
                VerticalAlignment = VAlignment.Center,
                HorizontalAlignment = HAlignment.Center,
                Texture = _sprites.Frame0(sprite),
                TextureScale = new Vector2(2f, 2f),
            };

            button.AddChild(tex);

            main.AddChild(button);
        }

        // Set up menu actions
        foreach (var child in Children)
        {
            AddLimbButtonOnClickActions(child);
        }
    }

    private void AddLimbButtonOnClickActions(Control control)
    {
        var radialContainer = control as RadialContainer;

        if (radialContainer == null)
            return;

        foreach (var child in radialContainer.Children)
        {
            var castChild = child as LimbButton;

            if (castChild == null)
                continue;

            castChild.OnButtonUp += _ =>
            {
                SendDamageSelectorMessageAction?.Invoke(castChild.Part);
                Close();
            };
        }
    }
}

public sealed class LimbButton(BodyPart part) : RadialMenuTextureButton
{
    public BodyPart Part = part;
}
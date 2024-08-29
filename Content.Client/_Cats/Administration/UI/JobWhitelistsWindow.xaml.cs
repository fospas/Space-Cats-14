using Content.Client.UserInterface.Controls;
using Content.Shared.Database;
using Content.Shared._Cats.Administration;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client._Cats.Administration.UI;

/// <summary>
/// An admin panel to toggle whitelists for individual jobs or entire departments.
/// This should generally be preferred to a blanket whitelist (Whitelisted: True) since
/// being good with a batong doesn't mean you know engineering and vice versa.
/// </summary>
[GenerateTypedNameReferences]
public sealed partial class JobWhitelistsWindow : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public Action<ProtoId<JobPrototype>, bool>? OnSetJob;

    public JobWhitelistsWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        PlayerName.Text = "???";
    }

    public void HandleState(JobWhitelistsEuiState state)
    {
        PlayerName.Text = state.PlayerName;

        Departments.RemoveAllChildren();
        foreach (var proto in _proto.EnumeratePrototypes<DepartmentPrototype>())
        {
            var panel = new DepartmentWhitelistPanel(proto, _proto, state.Whitelists);
            panel.OnSetJob += (id, whitelisting) => OnSetJob?.Invoke(id, whitelisting);
            Departments.AddChild(panel);
        }
    }
}
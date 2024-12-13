/// Maded by Gorox. Discord - smeshinka112
namespace Content.Shared._Cats.XenoPotion.Components;

[RegisterComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class XenoPotionComponent : Component
{
    [DataField("color"), AutoNetworkedField]
    public Color Color = Color.FromHex("#c62121");

    [DataField("effect"), AutoNetworkedField]
    public string Effect = "Speed";
}
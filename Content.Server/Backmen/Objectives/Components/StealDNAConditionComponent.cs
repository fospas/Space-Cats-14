using Content.Server.Backmen.Changeling;
using Content.Server.Backmen.Objectives.Systems;

namespace Content.Server.Backmen.Objectives.Components;

[RegisterComponent, Access(typeof(ChangelingObjectiveSystem), typeof(ChangelingSystem))]
public sealed partial class StealDNAConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DNAStolen = 0f;
}

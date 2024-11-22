using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Modifications.Drug.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DrugInitializeMachineComponent : Component
{
    public Container Tube = default!;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public bool IsRunning = false;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float RunningTime = 5f;

    [DataField("paper", required: false, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    [ViewVariables(VVAccess.ReadOnly)]
    public string Paper { get; set; } = "DiagnosisReportPaper";

    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public SoundSpecifier? PrintingSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");
}
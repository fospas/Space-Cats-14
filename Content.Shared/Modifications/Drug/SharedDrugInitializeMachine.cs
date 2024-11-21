using Robust.Shared.Serialization;

namespace Content.Shared.Modifications.Drug;

[Serializable, NetSerializable]
public enum DrugInitializeMachineVisualState
{
    Idle,
    Running
}

public enum DrugInitializeMachineVisualizerLayers : byte
{
    Base,
    BaseUnlit
}
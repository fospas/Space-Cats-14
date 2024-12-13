using Content.Server._Cats.XenoBiology.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Server._Cats.XenoBiology.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class XenoBiologyComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// Начальное количество очков для деления
    [DataField("points"), ViewVariables(VVAccess.ReadWrite)]
    public int Points = 100;

    /// Сколько очков получает существо при атаке
    [DataField("pointsPerAttack"), ViewVariables(VVAccess.ReadWrite)]
    public int PointsPerAttack = 10;

    /// Сколько очков необходимо для деления
    [DataField("pointsThreshold"), ViewVariables(VVAccess.ReadWrite)]
    public int PointsThreshold = 200;

    /// Шанс мутации при делении
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Mutationchance = 0.3f;

    /// Прототип при удачной мутации
    [DataField("mutagen", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Mutagen = "MobSlimesPet";

    /// Кем становится существо при делении, если имеет разум. Используйте прототип полиморфа
    [DataField("onMind", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string OnMind = "RandomSlimePerson";
}
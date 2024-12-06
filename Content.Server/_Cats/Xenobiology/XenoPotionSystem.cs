using System.Linq;
using Content.Shared._Cats.XenoPotion.Components;
using Content.Server._Cats.XenoFood.Components;
using Content.Shared._Cats.XenoPotionEffected.Components;
using Content.Server.Atmos.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Clothing;
using Content.Shared.IdentityManagement;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Cats.XenoPotion;

public sealed class XenoPotionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenoPotionComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, XenoPotionComponent component, ref AfterInteractEvent args)
    {
      if (args.Handled)
         return;


      /// Эффект зелья скорости. Убирает любой штраф скорости у одежды
      if (args.Target != null && component.Effect == "Speed" && !EntityManager.HasComponent<XenoPotionEffectedComponent>(args.Target.Value))
      {

         if (args.Target != null && TryComp<ClothingSpeedModifierComponent>(args.Target.Value, out var speedComp))
         {
            var meta = MetaData(args.Target.Value);
            var name = meta.EntityName;

            if (speedComp.SprintModifier > 1.0)
            return;

            EnsureComp<XenoPotionEffectedComponent>(args.Target.Value, out XenoPotionEffectedComponent? color);

            _metaData.SetEntityName(args.Target.Value, Loc.GetString("potion-speed-name-prefix", ("target", name)));

            EntityManager.RemoveComponent<ClothingSpeedModifierComponent>(args.Target.Value);

            color.Color = component.Color;
         
            EntityManager.DeleteEntity(args.Used);         
         }
      }

      /// Эффект зелья герметичности. Работает только на верхней одежде и на шляпах(такова система защиты от давления), не защищает от низкой температуры.
      else if (args.Target != null && component.Effect == "Pressure" && !EntityManager.HasComponent<XenoPotionEffectedComponent>(args.Target.Value))
      {
           if (args.Target != null && !EntityManager.HasComponent<PressureProtectionComponent>(args.Target.Value) && EntityManager.HasComponent<ClothingComponent>(args.Target.Value))
           {
              var meta = MetaData(args.Target.Value);
              var name = meta.EntityName;

              EnsureComp<XenoPotionEffectedComponent>(args.Target.Value, out XenoPotionEffectedComponent? color);

              _metaData.SetEntityName(args.Target.Value, Loc.GetString("potion-pressure-name-prefix", ("target", name)));

              EnsureComp<PressureProtectionComponent>(args.Target.Value, out PressureProtectionComponent pressure);

              color.Color = component.Color;
         
              pressure.LowPressureMultiplier = 1000f;
        
              EntityManager.DeleteEntity(args.Used);
           }
      }

      args.Handled = true;
    }
}
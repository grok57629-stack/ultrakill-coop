namespace Jaket.Net.Vendors;

using Sandbox;
using UnityEngine;

using Jaket.Assets;
using Jaket.Content;
using Jaket.Net.Types;
using Jaket.World;

using static Entities;

/// <summary> Vendor responsible for enemies. </summary>
public class Enemies : Vendor
{
    public void Load()
    {
        EntityType counter = EntityType.Filth;
        GameAssets.Enemies.Each(w =>
        {
            byte index = (byte)counter++;
            GameAssets.Prefab(w, p => Vendor.Prefabs[index] = p);
        });

        Events.Post
        (
            () => Vendor.Prefabs[(byte)EntityType.Malicious],
            () => Vendor.Prefabs[(byte)EntityType.Malicious] = Vendor.Prefabs[(byte)EntityType.Malicious].transform.Find("Body").gameObject
        );

        for (EntityType i = EntityType.Filth;           i <= EntityType.Soldier;         i++) Vendor.Suppliers[(byte)i] = (id, type) => new Husk           (id, type);
        for (EntityType i = EntityType.Swordsmachine;   i <= EntityType.Swordsmachine;   i++) Vendor.Suppliers[(byte)i] = (id, type) => new Swordsmachine  (id, type);
        for (EntityType i = EntityType.SecuritySystem;  i <= EntityType.Brain;           i++) Vendor.Suppliers[(byte)i] = (id, type) => new Earthmover     (id, type);
        for (EntityType i = EntityType.Malicious;       i <= EntityType.Malicious;       i++) Vendor.Suppliers[(byte)i] = (id, type) => new Malicious      (id, type);
        for (EntityType i = EntityType.Cerberus;        i <= EntityType.Cerberus;        i++) Vendor.Suppliers[(byte)i] = (id, type) => new Cerberus       (id, type);

        // Universal supplier for all remaining enemy types across Act I, Act II, Act III
        for (EntityType i = EntityType.Filth; i <= EntityType.Sisyphus; i++)
        {
            if (Vendor.Suppliers[(byte)i] == null)
            {
                Vendor.Suppliers[(byte)i] = (id, type) => new GenericEnemy(id, type);
            }
        }

        Events.OnLoad += () =>
        {
            if (LobbyController.Online)
            {
                ResFind<EnemySpawnableInstance>().Each(IsReal, Imdt);
                ResFind<SpiderLegLines        >().Each(IsReal, Imdt);
                ResFind<SpiderLegsController  >().Each(IsReal, Imdt);
            }
        };
    }

    public static EntityType FromEnemyType(EnemyType enemyType) => enemyType switch
    {
        EnemyType.Filth               => EntityType.Filth,
        EnemyType.Stray               => EntityType.Stray,
        EnemyType.Schism              => EntityType.Schism,
        EnemyType.Soldier             => EntityType.Soldier,
        EnemyType.Minos               => EntityType.Hand,
        EnemyType.Stalker             => EntityType.Stalker,
        EnemyType.Sisyphus            => EntityType.Insurrectionist,
        EnemyType.Ferryman            => EntityType.Ferryman,
        EnemyType.MirrorReaper        => EntityType.Reaper,

        EnemyType.Swordsmachine       => EntityType.Swordsmachine,
        EnemyType.Drone               => EntityType.Drone,
        EnemyType.Streetcleaner       => EntityType.Streetcleaner,
        EnemyType.V2                  => EntityType.V2st,
        EnemyType.Mindflayer          => EntityType.Mindflayer,
        EnemyType.V2Second            => EntityType.V2nd,
        EnemyType.Turret              => EntityType.Sentry,
        EnemyType.Gutterman           => EntityType.Gutterman,
        EnemyType.Guttertank          => EntityType.Guttertank,
        EnemyType.Centaur             => EntityType.SecuritySystem,

        EnemyType.MaliciousFace       => EntityType.Malicious,
        EnemyType.Cerberus            => EntityType.Cerberus,
        EnemyType.HideousMass         => EntityType.Hideous,
        EnemyType.Idol                => EntityType.Idol,
        EnemyType.Leviathan           => EntityType.Leviathan,
        EnemyType.Mannequin           => EntityType.Mannequin,
        EnemyType.Minotaur            => EntityType.Minotaur,
        EnemyType.Deathcatcher        => EntityType.Deathcatcher,
        EnemyType.Geryon              => EntityType.Geryon,

        EnemyType.Gabriel             => EntityType.Gabriel1st,
        EnemyType.Virtue              => EntityType.Virtue,
        EnemyType.GabrielSecond       => EntityType.Gabriel2nd,
        EnemyType.Providence          => EntityType.Providence,
        EnemyType.Power               => EntityType.Power,

        EnemyType.Wicked              => EntityType.Wicked,
        EnemyType.CancerousRodent     => EntityType.Rodent1st,
        EnemyType.VeryCancerousRodent => EntityType.Rodent2nd,
        EnemyType.Mandalore           => EntityType.Mandalore,
        EnemyType.BigJohnator         => EntityType.Johninator,
        EnemyType.Puppet              => EntityType.Puppet,

        EnemyType.FleshPrison         => EntityType.FleshPrison,
        EnemyType.FleshPanopticon     => EntityType.FleshPanopticon,
        EnemyType.MinosPrime          => EntityType.Minos,
        EnemyType.SisyphusPrime       => EntityType.Sisyphus,

        _ => EntityType.None,
    };

    public EntityType Type(GameObject obj)
    {
        if (obj == null || !obj.TryGetComponent(out EnemyIdentifier enemyId)) return EntityType.None;

        var type = Vendor.Find
        (
            EntityType.Filth,
            EntityType.Sisyphus,
            p => p && p.TryGetComponent(out EnemyIdentifier e)
                   && e.enemyType        == enemyId.enemyType
                   && e.overrideFullName == enemyId.overrideFullName
                   && e.weakPoint?.name  == enemyId.weakPoint?.name
        );
        if (type != EntityType.None) return type;

        // Fallback by enemyType in prefabs
        type = Vendor.Find
        (
            EntityType.Filth,
            EntityType.Sisyphus,
            p => p && p.TryGetComponent(out EnemyIdentifier e) && e.enemyType == enemyId.enemyType
        );
        if (type != EntityType.None) return type;

        // Fallback enum mapping
        return FromEnemyType(enemyId.enemyType);
    }

    public GameObject Make(EntityType type, Vector3 position = default, Transform parent = null)
    {
        if (!type.IsEnemy()) return null;

        var prefab = Vendor.Prefabs[(byte)type];
        if (!prefab) return null;

        return Inst(prefab, position);
    }

    public void Sync(GameObject obj, params bool[] args)
    {
        var type = Type(obj);
        if (type == EntityType.None || obj.GetComponent<Entity.Agent>()) return;

        if (Gameflow.Mode.NoCommonEnemies()) Imdt(obj);
        else
        if (obj.activeSelf && obj.TryGetComponent(out EnemyIdentifier enemyId) && !enemyId.dead)
        {
            if (LobbyController.IsOwner || (args.Length > 0 && args[0]) || Vendor.Prefabs[(byte)type] == null)
            {
                var entity = Supply(type);

                entity.Owner = AccId;
                entity.Assign(obj.AddComponent<Entity.Agent>());
                entity.Push();
            }
            else Imdt(obj);
        }
    }
}

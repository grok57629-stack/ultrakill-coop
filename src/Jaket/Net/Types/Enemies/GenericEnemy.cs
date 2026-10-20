namespace Jaket.Net.Types;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Jaket.Content;
using Jaket.Harmony;
using Jaket.IO;

/// <summary> Universal tangible entity that synchronizes any enemy or boss type across all acts. </summary>
public class GenericEnemy : Enemy
{
    Agent agent;
    Float x, y, z;
    Float rx, ry, rz;
    NavMeshAgent nma;
    Animator animator;
    EnemyScript[] aiScripts;
    MonoBehaviour[] extraScripts;

    public GenericEnemy(uint id, EntityType type) : base(id, type) { }

    #region snapshot

    public override int BufferSize => 36;

    public override void Write(Writer w)
    {
        WriteOwner(ref w);

        if (IsOwner)
        {
            w.Vector(agent.Position);
            var rot = agent.Rotation;
            w.Float(rot.x);
            w.Float(rot.y);
            w.Float(rot.z);

            w.Byte(Attack);
            w.Bool(Moving);
        }
        else
        {
            w.Floats(x, y, z);
            w.Float(rx.Next);
            w.Float(ry.Next);
            w.Float(rz.Next);

            w.Byte(Attack);
            w.Bool(Moving);
        }
    }

    public override void Read(Reader r)
    {
        if (ReadOwner(ref r)) return;

        r.Floats(ref x, ref y, ref z);
        this.rx.Set(r.Float());
        this.ry.Set(r.Float());
        this.rz.Set(r.Float());

        Attack = r.Byte();
        Moving = r.Bool();
    }

    #endregion
    #region logic

    public override void Create()
    {
        var prefab = Entities.Enemies.Make(Type, new(x.Init, y.Init, z.Init));
        if (prefab) Assign(prefab.AddComponent<Agent>());
    }

    public override void Assign(Agent agent)
    {
        base.Assign(this.agent = agent);

        agent.Get(out nma, true);
        agent.Get(out animator, true);

        aiScripts = agent.GetComponentsInChildren<EnemyScript>(true);

        var allMb = agent.GetComponentsInChildren<MonoBehaviour>(true);
        var extraList = new List<MonoBehaviour>();
        for (int i = 0; i < allMb.Length; i++)
        {
            var mb = allMb[i];
            if (!mb || mb is Agent || mb is EnemyIdentifier || mb is BossHealthBar || mb is EnemyScript) continue;
            var name = mb.GetType().Name;
            if (name.Contains("Audio") || name.Contains("Render") || name.Contains("Gore") || name.Contains("Collider")) continue;
            extraList.Add(mb);
        }
        extraScripts = extraList.ToArray();
    }

    public override void Update(float delta)
    {
        if (Locked)
        {
            if (nma) nma.enabled = false;
            DisableScripts();
            return;
        }

        if (nma) nma.enabled = IsOwner;

        if (aiScripts != null)
        {
            for (int i = 0; i < aiScripts.Length; i++)
            {
                if (aiScripts[i]) aiScripts[i].enabled = IsOwner;
            }
        }

        if (extraScripts != null)
        {
            for (int i = 0; i < extraScripts.Length; i++)
            {
                if (extraScripts[i]) extraScripts[i].enabled = IsOwner;
            }
        }

        if (IsOwner)
        {
            if (nma) Moving = nma.velocity.sqrMagnitude > 0.05f;
            return;
        }

        agent.Position = new(x.GetAware(delta), y.GetAware(delta), z.GetAware(delta));
        agent.Rotation = new(rx.GetAngle(delta), ry.GetAngle(delta), rz.GetAngle(delta));

        if (nma) nma.enabled = false;
    }

    private void DisableScripts()
    {
        if (aiScripts != null)
        {
            for (int i = 0; i < aiScripts.Length; i++)
                if (aiScripts[i]) aiScripts[i].enabled = false;
        }
        if (extraScripts != null)
        {
            for (int i = 0; i < extraScripts.Length; i++)
                if (extraScripts[i]) extraScripts[i].enabled = false;
        }
    }

    #endregion
}

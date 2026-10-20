namespace Jaket.World;

using Logic;
using UnityEngine;
using UnityEngine.UI;

using Jaket.Assets;
using Jaket.Content;
using Jaket.Net;
using Jaket.UI.Fragments;
using Jaket.UI.Lib;

/// <summary> List of all interactions with the world. Will replenish over time. </summary>
public static class ActionList
{
    /// <summary> All actions, including vanilla and custom ones. </summary>
    private static Action[] all;
    /// <summary> Identifiers of last vanilla and custom actions. </summary>
    private static int vanilla, custom;
    /// <summary> Whether all of the vanilla actions are loaded. </summary>
    private static bool locked;

    /// <summary> Adds the given action to the global list, taking vanilla and custom identifiers into account. </summary>
    public static void Add(Action action)
    {
        all ??= new Action[byte.MaxValue + 1];
        int i = locked ? vanilla + custom++ : vanilla++;

        if (i < all.Length)
            (all[i] = action).Identifier = i;
        else
            Log.Warning($"[WRLD] Out of identifiers for actions");
    }

    /// <summary> Iterates each action in the global list that is suitable for the current scene and predicate. </summary>
    public static void Each(Pred<Action> pred, Cons<Action> cons)
    {
        for (int i = 0; i < vanilla + custom; i++) if (all[i].Valid && pred(all[i])) cons(all[i]);
    }

    /// <summary> Returns a dynamic action with the given identifier. </summary>
    public static Action At(int id) => all[id].Valid && all[id].Dynamic ? all[id] : null;

    /// <summary> Adds vanilla actions to the global list, and locks their index to prevent custom ones from interfering. </summary>
    public static void Load()
    {
        string l; // decreases the size of further lines

        #region 0-1
        l = "Level 0-1";

        ActionType.Window(l);

        ActionType.Find(l, "3 - Gun Room/TitleActivator", t => t.Component<ObjectActivator>(o =>
        {
            var prev = o.events.toDisActivateObjects;
            var next = o.events.toDisActivateObjects = [prev[0]];

            o.events.onActivate.AddListener(() =>
            {
                var blck = prev.Find(o => o.name == "Blockers").transform;

                blck.GetChild(0).GetComponentsInChildren<Renderer>().Each(Dest);
                blck.GetChild(1).gameObject.SetActive(false);
                blck.GetChild(2).gameObject.SetActive(false);
            });
            o.events.toActivateObjects.Each(o => o.name == "Cube", o => o.GetComponents<Collider>().Each(Dest));
        }, true));

        ActionType.Find(l, "AltStartFiller/AltStart", t => t.Component<ObjectActivator>(o =>
        {
            var prev = o.events.toDisActivateObjects;
            var next = o.events.toDisActivateObjects = [prev[0], prev[^1]];

            o.events.onActivate.AddListener(() =>
            {
                prev[4].GetComponentsInChildren<Renderer>().Each(Dest);
                prev[5].gameObject.SetActive(false);
                prev[8].gameObject.SetActive(false);
            });
        }, true));

        ActionType.Find(l, "13 - Malicious Face Arena/13 Nonstuff", t => Dest(t.GetChild(0))); // unloader

        ActionType.Act(l, "13 Content/Trigger"); // boss

        #endregion
        #region 0-2
        l = "Level 0-2";

        ActionType.Window(l);

        ActionType.Dest(l, "3 Nonstuff/Swordsmachine Trigger");
        ActionType.Dest(l, "4 - Swordsmachine Hallway/Invisible Wall");

        ActionType.Dest(l, "3B Enemies/Activator");

        ActionType.Find(l, "Enemies/Wave 2 Trigger", t => t.GetComponents<ObjectActivator>().Each(Dest));
        ActionType.Find(l, "Door (Large) With Controllers (3)/Door (Large)", t => t.GetComponent<Door>().Lock());
        ActionType.Dest(l, "7 Nonstuff/Invisible Wall");
        ActionType.Act(l, "7 Contents/PlayerDoorUnlocker");

        ActionType.Dest(l, "5B Nonstuff/Altar"); // duplicate

        ActionType.Find(l, "8 Contents/Trigger", t => t.GetComponents<ObjectActivator>().Each(Dest));

        ActionType.Act(l, "10B Contents/Trigger"); // boss

        ActionType.Find(l, "7B - Bonus Platforming/Cube (48)", t => t.localPosition += Vector3.right * 6f);
        ActionType.Find(l, "7B - Bonus Platforming/Grinders", t => t.localPosition += Vector3.right * .1f);

        #endregion
        #region 0-3
        l = "Level 0-3";

        ActionType.Window(l);

        ActionType.Scr(l, "1 Content/Trigger");
        ActionType.Scr(l, "10 Contents/Trigger");

        ActionType.Turn(l, "Boss Arena Contents/CeilingDoors");
        ActionType.Dest(l, "Boss Arena Contents/NeoBreakable");
        ActionType.Act(l, "Boss Arena Contents/Cube (1)"); // boss

        ActionType.Find(l, "11 Contents/Trigger", t => Dest(t.GetComponent<ObjectActivator>())); // unloader

        #endregion
        #region 0-4
        l = "Level 0-4";

        ActionType.Window(l);

        static void Chamber(Transform t, int i)
        {
            t.name = $"CoolingChamber ({i})";
            t.GetComponentsInChildren<ObjectActivator>(true).Each
            (
                o => o.delay == 2.5f,
                o => o.events.onActivate.AddListener(() => Events.Post(() =>
                {
                    World.Reset(o.Path());
                    t.GetComponentInChildren<ItemTrigger>().requests = 0;
                }))
            );
        }

        ActionType.Find(l, "3 Nonstuff/CoolingChamber", t => Chamber(t, 0));
        ActionType.Find(l, "3 Nonstuff/CoolingChamber (1)", t => Chamber(t, 1));
        ActionType.Find(l, "6 Nonstuff/CoolingChamber", t => Chamber(t, 2));

        ActionType.Act(l, "CoolingChamber (0)/Sequence");
        ActionType.Act(l, "CoolingChamber (1)/Sequence");
        ActionType.Act(l, "CoolingChamber (2)/Sequence");

        ActionType.Act(l, "Enemies/Wave 1 Trigger");

        #endregion
        #region 0-5
        l = "Level 0-5";

        ActionType.Find(l, "4 Contents/Cube", t => t.Component<ObjectActivator>(o => o.events.toDisActivateObjects[0] = null, true));
        ActionType.Find(l, "4 Contents/Enemies", t => t.Component<ActivateNextWaveHP>(o =>
        {
            if (LobbyController.IsOwner)
                o.health = 74f;
            else
                Dest(o);
        }, true));

        ActionType.Act(l, "4 Contents/Cube"); // boss
        ActionType.Act(l, "StatueFake (1)/StatueActivator"); // boss
        ActionType.Act(l, "Hellgate/DelayedDoorActivation");

        ActionType.Turn(l, "/5 - Final Hallway");

        #endregion
        #region 0-S
        l = "Level 0-S";

        ActionType.Run(l, () => NewMovement.Instance.modNoJump = true);

        #endregion
        #region 1-1
        l = "Level 1-1";

        ActionType.Window(l);
        ActionType.Switch(l);

        ActionType.Dest(l, "11 Nonstuff/Altar"); // duplicate

        #endregion
        #region 1-2
        l = "Level 1-2";

        ActionType.Statue(l);
        ActionType.Switch(l);

        #endregion
        #region 1-3
        l = "Level 1-3";

        ActionType.Window(l);
        ActionType.Statue(l);
        ActionType.Switch(l);

        // TODO sync R1 - Courtyard/R1 Stuff(Clone)/Enemies/Wave 3/Trigger 'cause there are two statues

        #endregion
        #region 1-4
        l = "Level 1-4";

        ActionType.Switch(l);

        #endregion
        #region 1-S
        l = "Level 1-S";

        #endregion
        #region 2-1
        l = "Level 2-1";

        ActionType.Window(l);
        ActionType.Statue(l);

        #endregion
        #region 2-2
        l = "Level 2-2";

        ActionType.Statue(l);

        #endregion
        #region 2-3
        l = "Level 2-3";

        ActionType.Window(l);

        #endregion
        #region 2-4
        l = "Level 2-4";

        ActionType.Window(l);

        #endregion
        #region 2-S
        l = "Level 2-S";

        #endregion
        #region 3-1
        l = "Level 3-1";

        ActionType.Statue(l);

        #endregion
        #region 3-2
        l = "Level 3-2";

        #endregion
        #region 4-1
        l = "Level 4-1";

        ActionType.Switch(l);
        ActionType.Flammable(l);

        ActionType.Turn(l, "GreedTorch (2)/Flammable"); // for some reason you cannot set it on fire in vanilla game

        #endregion
        #region 4-2
        l = "Level 4-2";

        ActionType.Window(l);
        ActionType.Statue(l);
        ActionType.Switch(l);

        #endregion
        #region 4-3
        l = "Level 4-3";

        ActionType.Statue(l);
        ActionType.Flammable(l);

        ActionType.Torches(l, new(0f, -10f, 310f));

        #endregion
        #region 4-4
        l = "Level 4-4";

        #endregion
        #region 4-S
        l = "Level 4-S";

        #endregion
        #region 5-1
        l = "Level 5-1";

        ActionType.Statue(l);

        #endregion
        #region 5-2
        l = "Level 5-2";

        ActionType.Statue(l);

        #endregion
        #region 5-3
        l = "Level 5-3";

        ActionType.Window(l);
        ActionType.Statue(l);

        #endregion
        #region 5-4
        l = "Level 5-4";

        #endregion
        #region 5-S
        l = "Level 5-S";

        #endregion
        #region 6-1
        l = "Level 6-1";

        #endregion
        #region 6-2
        l = "Level 6-2";

        #endregion
        #region 7-1
        l = "Level 7-1";

        ActionType.Statue(l);
        ActionType.Switch(l);

        #endregion
        #region 7-2
        l = "Level 7-2";

        ActionType.Window(l);
        ActionType.Statue(l);
        ActionType.Switch(l);

        #endregion
        #region 7-3
        l = "Level 7-3";

        #endregion
        #region 7-4
        l = "Level 7-4";

        ActionType.Window(l);

        #endregion
        #region 7-S
        l = "Level 7-S";

        ActionType.Statue(l);

        #endregion
        #region P-1
        l = "Level P-1";

        ActionType.Arena(l);
        ActionType.Final(l);

        #endregion
        #region P-2
        l = "Level P-2";

        ActionType.Arena(l);
        ActionType.Final(l);

        #endregion
        #region endless
        l = "Endless";

        #endregion
        #region museum
        l = "CreditsMuseum2";

        ActionType.Run(l, () => ResFind<MapIntSetter>().Each(IsReal, Dest));
        ActionType.Dest(l, "/__Gianni nightmare world");

        #endregion
        #region all
        l = "All";

        ActionType.Arena(l);
        ActionType.Final(l);
        ActionType.Window(l);
        ActionType.Switch(l);
        ActionType.Breakable(l);
        ActionType.Zappable(l);
        ActionType.DoorUnlock(l);
        ActionType.DoorOpen(l);
        ActionType.Puzzle(l);
        ActionType.Altar(l);

        ActionType.Find(l, "PauseMenu/Restart Checkpoint", b =>
        {
            if (Gameflow.Mode.NoRestarts() || Spectator.Special)
            {
                b.GetComponentsInParent<PauseMenu>(true).Each(Dest);
                b.GetComponent<Button>().interactable = false;
            }
        });
        ActionType.Find(l, "PauseMenu/Restart Mission", b =>
        {
            if (Gameflow.Mode.NoRestarts() || Spectator.Special)
            {
                b.GetComponentsInParent<PauseMenu>(true).Each(Dest);
                b.GetComponent<Button>().interactable = LobbyController.IsOwner;
            }
        });

        #endregion
        #region beta
        l = "Main Menu";

        string info =
@"[31][b][green]/// JAKET CO-OP — ПОЛНАЯ ВЕРСИЯ ///[][][]

[orange]АКТИВИРОВАНА ПОДДЕРЖКА ВСЕХ ГЛАВ И АКТОВ:[][]
[white]- PRELUDE (Вступление, уровни 0-1 — 0-5)[][]
[white]- ACT I: INFINITE HYPERDEATH (Слои 1, 2, 3)[][]
[white]- ACT II: IMPERFECT HATRED (Слои 4, 5, 6)[][]
[white]- ACT III: GODFIST SUICIDE (Слой 7, P-1, P-2)[][]
[white]- The Cyber Grind, Песочница и Музей[][]

[yellow]Все враги, боссы, арены и двери синхронизированы в мультиплеере![][]

[orange]Управление лобби:[][]
[white]Нажмите [b][cyan]F1[][], чтобы открыть панель мультиплеера.[][]
[white]Вы можете создать лобби, скопировать код или пригласить друзей через Steam![][]

Приятной совместной игры и кровавой резни!";

        ActionType.Find(l, "Main Menu (1)/V1", r =>
        {
            var root = Builder.Rect("Warning", r, UI.Lib.Rect.Fill);

            Builder.Image(root, Tex.Back, Pal.semi, Image.Type.Sliced);

            var text = Builder.Rect("Text", root, new(0f, 0f, -32f, -32f, Vector2.zero, Vector2.one));

            Builder.Text(text, Bundle.Parse(info), 24, Pal.white, TextAnchor.UpperLeft);
        });

        // TODO remove later
        Each(a => a.Scene == "Main Menu", a => a.Perform(default));

        #endregion

        locked = true;

        Log.Info($"[WRLD] Loaded {vanilla} actions");
    }
}

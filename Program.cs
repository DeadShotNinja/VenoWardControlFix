using System;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using SharpDX;

namespace VenoPRO
{
    internal class Program
    {
        private static bool _loaded;
        private static readonly uint[] PlagueWardDamage = { 10, 19, 29, 38 };
        private static void Main()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame || !Utils.SleepCheck("Veno"))
                return;
            Utils.Sleep(125, "Veno");

            var me = ObjectManager.LocalHero;

            if (!_loaded)
            {
                if (!Game.IsInGame || me == null)
                {
                    return;
                }
                _loaded = true;
                Game.PrintMessage(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + " By edwynxero" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version, MessageType.LogMessage);
            }

            if (me == null || me.ClassID != ClassID.CDOTA_Unit_Hero_Venomancer)
                return;

            var plagueWardLevel = me.FindSpell("venomancer_plague_ward").Level - 1;

            var enemies = ObjectManager.GetEntities<Hero>().Where(hero => hero.IsAlive && !hero.IsIllusion && hero.IsVisible && hero.Team == me.GetEnemyTeam()).ToList();
            var creeps = ObjectManager.GetEntities<Creep>().Where(creep => (creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane || creep.ClassID == ClassID.CDOTA_BaseNPC_Creep_Siege) && creep.IsAlive && creep.IsVisible && creep.IsSpawned).ToList();
            var plaguewards = ObjectManager.GetEntities<Unit>().Where(unit => unit.ClassID == ClassID.CDOTA_BaseNPC_Venomancer_PlagueWard && unit.IsAlive && unit.IsVisible).ToList();

            if (!enemies.Any() || !creeps.Any() || !plaguewards.Any() || !(plagueWardLevel > 0))
                return;

            foreach (var enemy in enemies)
            {
                if (enemy.Modifiers.FirstOrDefault(modifier => modifier.Name == "modifier_venomancer_poison_sting_ward") == null && enemy.Health > 0)
                {
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(enemy.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(enemy);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
                }
            }

            foreach (var creep in creeps)
            {
                if (creep.Team == me.GetEnemyTeam() && creep.Health > 0 && creep.Health < (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist) + 20))
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(creep.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(creep);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
                else if (creep.Team == me.Team && creep.Health > (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist)) && creep.Health < (PlagueWardDamage[plagueWardLevel] * (1 - creep.DamageResist) + 88))
                    foreach (var plagueward in plaguewards)
                    {
                        if (GetDistance2D(creep.Position, plagueward.Position) < plagueward.AttackRange && Utils.SleepCheck(plagueward.Handle.ToString()))
                        {
                            plagueward.Attack(creep);
                            Utils.Sleep(1000, plagueward.Handle.ToString());
                        }
                    }
            }

            foreach (var plagueward in plaguewards)
            {
                if (plagueward.Team == me.Team && plagueward.Health > (PlagueWardDamage[plagueWardLevel] * 1) && plagueward.Health < (PlagueWardDamage[plagueWardLevel] * 1 + 88))
                    foreach (var plagueward2 in plaguewards)
                    {
                        if (GetDistance2D(plagueward.Position, plagueward2.Position) < plagueward2.AttackRange && Utils.SleepCheck(plagueward2.Handle.ToString()))
                        {
                            plagueward2.Attack(plagueward);
                            Utils.Sleep(1000, plagueward2.Handle.ToString());
                        }
                    }
            }
        }
        private static float GetDistance2D(Vector3 p1, Vector3 p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }
}

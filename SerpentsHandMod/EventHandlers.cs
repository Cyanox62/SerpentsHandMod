using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace SerpentsHandMod
{
    public partial class EventHandlers
    {
        public static List<int> shPlayers = new List<int>();
        private List<int> shPocketPlayers = new List<int>();

        bool test = false;

        private static System.Random rand = new System.Random();

        private static Vector3 shSpawnPos = new Vector3(0.1f, 1001.3f, -56.8f);

        public void OnRoundStart()
        {
            test = false;
            shPlayers.Clear();
            shPocketPlayers.Clear();

            Timing.CallDelayed(0.3f, () =>
            {
                List<Player> dClassList = Player.List.Where(x => x.Team == Team.CDP).ToList();
                List<Player> toSpawn = new List<Player>();
                for (int i = 0; i < Player.List.Count() / SerpentsHand.instance.Config.SpawnRate; i++)
                {
                    if (dClassList.Count > 0)
                    {
                        Player p = dClassList[Random.Range(0, dClassList.Count)];
                        dClassList.Remove(p);
                        toSpawn.Add(p);
                    }
                }
                SpawnSquad(toSpawn);
            });
        }

        public void OnPocketDimensionEnter(EnteringPocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                shPocketPlayers.Add(ev.Player.Id);
            }
        }

        public void OnSpawn(SpawningEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                ev.Position = shSpawnPos;
            }
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            foreach (Player player in shPlayers.Select(x => Player.Get(x)))
            {
                if (Vector3.Distance(player.Position, ev.Player.Position) <= SerpentsHand.instance.Config.SpawnDistance)
                {
                    ev.IsAllowed = false;
                    Timing.CallDelayed(0.1f, () => SpawnPlayer(ev.Player));
                }
            }
        }

        public void OnPocketDimensionDie(FailingEscapePocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                if (!SerpentsHand.instance.Config.FriendlyFire)
                {
                    ev.IsAllowed = false;
                }
                if (SerpentsHand.instance.Config.TeleportTo106)
                {
                    TeleportTo106(ev.Player);
                }
                shPocketPlayers.Remove(ev.Player.Id);
            }
        }

        public void OnPocketDimensionExit(EscapingPocketDimensionEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                ev.IsAllowed = false;
                if (SerpentsHand.instance.Config.TeleportTo106)
                {
                    TeleportTo106(ev.Player);
                }
                shPocketPlayers.Remove(ev.Player.Id);
            }
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            Player scp035 = null;

            if (SerpentsHand.isScp035)
            {
                scp035 = TryGet035();
            }

            if (((shPlayers.Contains(ev.Target.Id) && (ev.Attacker.Team == Team.SCP || ev.HitInformations.GetDamageType() == DamageTypes.Pocket)) ||
                (shPlayers.Contains(ev.Attacker.Id) && (ev.Target.Team == Team.SCP || (scp035 != null && ev.Target == scp035))) ||
                (shPlayers.Contains(ev.Target.Id) && shPlayers.Contains(ev.Attacker.Id) && ev.Target != ev.Attacker)) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.Amount = 0f;
            }
        }

        public void OnPlayerDying(DyingEventArgs ev)
        {
            /* if (shPlayers.Contains(ev.Target.Id))
             {
                 shPlayers.Remove(ev.Target.Id);
             }

             if (ev.Target.Role == RoleType.Scp106 && !SerpentsHand.instance.Config.FriendlyFire)
             {
                 foreach (Player player in Player.List.Where(x => shPocketPlayers.Contains(x.Id)))
                 {
                     player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(50000, "WORLD", ev.HitInformation.GetDamageType(), player.Id), player.GameObject);
                 }
             }*/
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            if (shPlayers.Contains(ev.Target.Id))
            {
                shPlayers.Remove(ev.Target.Id);
            }

            if (ev.Target.Role == RoleType.Scp106 && !SerpentsHand.instance.Config.FriendlyFire)
            {
                foreach (Player player in Player.List.Where(x => shPocketPlayers.Contains(x.Id)))
                {
                    player.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(50000, "WORLD", ev.HitInformations.GetDamageType(), player.Id), player.GameObject);
                }
            }

            /* for (int i = shPlayers.Count - 1; i >= 0; i--)
             {
                 if (Player.Get(shPlayers[i]).Role == RoleType.Spectator)
                 {
                     shPlayers.RemoveAt(i);
                 }
             }*/
        }

        public void OnCheckRoundEnd(EndingRoundEventArgs ev)
        {
            Player scp035 = null;

            if (SerpentsHand.isScp035)
            {
                scp035 = TryGet035();
            }

            bool MTFAlive = CountRoles(Team.MTF) > 0;
            bool CiAlive = CountRoles(Team.CHI) > 0;
            bool ScpAlive = CountRoles(Team.SCP) + (scp035 != null && scp035.Role != RoleType.Spectator ? 1 : 0) > 0;
            bool DClassAlive = CountRoles(Team.CDP) > 0;
            bool ScientistsAlive = CountRoles(Team.RSC) > 0;
            bool SHAlive = shPlayers.Count > 0;

            if (SHAlive && ((CiAlive && !SerpentsHand.instance.Config.ScpsWinWithChaos) || DClassAlive || MTFAlive || ScientistsAlive))
            {
                ev.IsAllowed = false;
                test = true;
            }
            else if (SHAlive && ScpAlive && !MTFAlive && !DClassAlive && !ScientistsAlive)
            {
                if (!SerpentsHand.instance.Config.ScpsWinWithChaos)
                {
                    if (!CiAlive)
                    {
                        ev.LeadingTeam = Exiled.API.Enums.LeadingTeam.Anomalies;
                        ev.IsAllowed = true;
                        ev.IsRoundEnded = true;
                    }
                }
                else
                {
                    ev.LeadingTeam = Exiled.API.Enums.LeadingTeam.Anomalies;
                    ev.IsAllowed = true;
                    ev.IsRoundEnded = true;
                }
            }
            else
            {
                test = false;
            }
        }

        public void OnSetRole(ChangingRoleEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                if (GetTeam(ev.NewRole) != Team.TUT)
                {
                    shPlayers.Remove(ev.Player.Id);
                }
            }
        }

        public void OnShoot(ShootingEventArgs ev)
        {
            Player target = Player.Get(ev.Target);
            if (target != null && target.Role == RoleType.Scp096 && shPlayers.Contains(ev.Shooter.Id))
            {
                ev.IsAllowed = false;
            }
        }

        public void OnDisconnect(LeftEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id))
            {
                shPlayers.Remove(ev.Player.Id);
            }
        }

        public void OnContain106(ContainingEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnRACommand(SendingRemoteAdminCommandEventArgs ev)
        {
            if (ev.Name == "pos") ev.Sender.RemoteAdminMessage(ev.Sender.Position.ToString(), true);
            string cmd = ev.Name.ToLower();
            if (cmd == "spawnsh")
            {
                ev.IsAllowed = false;

                if (ev.Arguments.Count > 0 && ev.Arguments[0].Length > 0)
                {
                    Player cPlayer = Player.Get(ev.Arguments[0]);
                    if (cPlayer != null)
                    {
                        SpawnPlayer(cPlayer);
                        ev.Sender.RemoteAdminMessage($"Spawned {cPlayer.Nickname} as Serpents Hand.", true);
                        return;
                    }
                    else
                    {
                        ev.Sender.RemoteAdminMessage("Invalid player.", false);
                        return;
                    }
                }
                else
                {
                    ev.Sender.RemoteAdminMessage("SPAWNSH [Player Name / Player ID]", false);
                }
            }
            else if (cmd == "spawnshsquad")
            {
                ev.IsAllowed = false;

                if (ev.Arguments.Count > 0)
                {
                    if (int.TryParse(ev.Arguments[0], out int a))
                    {
                        CreateSquad(a);
                    }
                    else
                    {
                        ev.Sender.RemoteAdminMessage("Error: invalid size.", false);
                        return;
                    }
                }
                else
                {
                    CreateSquad(5);
                }
                Cassie.Message(SerpentsHand.instance.Config.EntryAnnouncement, true, true);
                ev.Sender.RemoteAdminMessage("Spawned squad.", true);
            }
        }

        public void OnGeneratorInsert(InsertingGeneratorTabletEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnFemurEnter(EnteringFemurBreakerEventArgs ev)
        {
            if (shPlayers.Contains(ev.Player.Id) && !SerpentsHand.instance.Config.FriendlyFire)
            {
                ev.IsAllowed = false;
            }
        }

        public void a(SendingConsoleCommandEventArgs ev)
        {
            if (ev.Name.ToLower() == "sh")
            {
                string msg = "1. " + (shPlayers.Count > 0) + "\n2. " + test;
                foreach (int player in shPlayers) msg += "- " + Player.Get(player).Nickname + "\n";
                ev.ReturnMessage = msg;
            }
        }
    }
}
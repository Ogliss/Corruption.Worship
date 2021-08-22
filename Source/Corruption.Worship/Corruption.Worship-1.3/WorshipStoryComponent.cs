using Corruption.Core;
using Corruption.Core.Gods;
using Corruption.Core.Soul;
using Corruption.Worship.Wonders;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class WorshipStoryComponent : CorruptionStoryTrackerComponent
    {
        private static List<Pawn> tmpPawns = new List<Pawn>();

        private static List<Map> tmpMaps = new List<Map>();

        private static List<Caravan> tmpCaravans = new List<Caravan>();

        private DefMap<PantheonDef, GlobalPantheonFollowers> tmpCachedFollowers;

        public override void Notify_PawnCorrupted(CompSoul soul)
        {
        }

        public override void Notify_PawnPantheonChanged(CompSoul soul)
        {
            tmpCachedFollowers = null;
            tmpCachedFollowers = new DefMap<PantheonDef, GlobalPantheonFollowers>();

            var pawns = this.GetAllColonists().ToList();

            for (int i = 0; i < pawns.Count(); i++)
            {
                CompSoul soul2 = pawns[i].Soul();
                if (soul2 != null)
                {
                    var followers = tmpCachedFollowers[soul2.ChosenPantheon];
                    if (followers.Pantheon == null)
                    {
                        followers.Pantheon = soul2.ChosenPantheon;
                    }
                    var tileEntry = followers.GlobalPawns.FirstOrDefault(x => x.Tile == soul2.Pawn.Tile);
                    if (tileEntry.Equals(default(GlobalPantheonFollowers.FollowersPerTile)))
                    {
                        tileEntry = new GlobalPantheonFollowers.FollowersPerTile(soul2.Pawn.Tile);
                        followers.GlobalPawns.Add(tileEntry);
                    }

                    tileEntry.Followers.Add(soul2.Pawn);                    
                }
            }

            var playerPantheon = GlobalWorshipTracker.Current.PlayerPantheon;
            var rivalFollowers = tmpCachedFollowers.Where(x => x.Key != playerPantheon && x.Key.rejectingPantheons.Contains(playerPantheon)).OrderByDescending(x => x.Value.AllPawns.Count).FirstOrDefault();
            var playerFollowers = tmpCachedFollowers[playerPantheon];
            float powerFraction = (float)rivalFollowers.Value.AllPawns.Count / (float)(playerFollowers.AllPawns.Count + rivalFollowers.Value.AllPawns.Count);

            if (!rivalFollowers.Equals(default(GlobalPantheonFollowers.FollowersPerTile)) && rivalFollowers.Key.takeoverThresholds.Evaluate(powerFraction) >= Rand.Value)
            {
                TryStartRevolt(tmpCachedFollowers[playerPantheon], tmpCachedFollowers[rivalFollowers.Key]);
            }
        }

        private IEnumerable<Pawn> GetAllColonists()
        {
            tmpMaps.Clear();
            tmpMaps.AddRange(Find.Maps);
            tmpMaps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
            int num = 0;
            for (int i = 0; i < tmpMaps.Count; i++)
            {
                tmpPawns.Clear();
                tmpPawns.AddRange(tmpMaps[i].mapPawns.FreeColonists);
                List<Thing> list = tmpMaps[i].listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                for (int j = 0; j < list.Count; j++)
                {
                    if (!list[j].IsDessicated())
                    {
                        Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
                        if (innerPawn != null && innerPawn.IsColonist)
                        {
                            yield return innerPawn;
                        }
                    }
                }
                List<Pawn> allPawnsSpawned = tmpMaps[i].mapPawns.AllPawnsSpawned;
                for (int k = 0; k < allPawnsSpawned.Count; k++)
                {
                    Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
                    if (corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
                    {
                        tmpPawns.Add(corpse.InnerPawn);
                    }
                }
                PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
                for (int l = 0; l < tmpPawns.Count; l++)
                {
                    yield return tmpPawns[l];
                }
                num++;
            }
            tmpCaravans.Clear();
            tmpCaravans.AddRange(Find.WorldObjects.Caravans);
            tmpCaravans.SortBy((Caravan x) => x.ID);
            for (int m = 0; m < tmpCaravans.Count; m++)
            {
                if (!tmpCaravans[m].IsPlayerControlled)
                {
                    continue;
                }
                tmpPawns.Clear();
                tmpPawns.AddRange(tmpCaravans[m].PawnsListForReading);
                PlayerPawnsDisplayOrderUtility.Sort(tmpPawns);
                for (int n = 0; n < tmpPawns.Count; n++)
                {
                    if (tmpPawns[n].IsColonist)
                    {
                        yield return tmpPawns[n];
                    }
                }
                num++;
            }
        }


        private void TryStartRevolt(GlobalPantheonFollowers playerFollowers, GlobalPantheonFollowers rivalFollowers)
        {
            if (playerFollowers.AllPawns.Count == 0)
            {
                GlobalWorshipTracker.Current.PlayerPantheon = rivalFollowers.Pantheon;
                Find.LetterStack.ReceiveLetter("PeacefulPantheonChange".Translate(), "PeacefulPantheonChangeDesc".Translate(rivalFollowers.Pantheon.LabelCap), LetterDefOf.NeutralEvent);
            }
            else
            {
                var dialog = new Dialog_ReligiousRiot(playerFollowers, rivalFollowers);
                Find.WindowStack.Add(dialog);
            }
        }

    }
}

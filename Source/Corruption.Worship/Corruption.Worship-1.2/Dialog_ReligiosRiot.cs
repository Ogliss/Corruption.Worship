using Corruption.Core.Gods;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace Corruption.Worship
{
    [StaticConstructorOnStartup]
    public class Dialog_ReligiousRiot : Window
    {
        private static readonly Texture2D IconAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro");
        private GlobalPantheonFollowers rivalFollowers;
        private Pawn riotLeader;
        private GlobalPantheonFollowers currentFollowers;

        private static Vector2 scrollVect;

        private int cachedMaxPawns;
        private float powerFraction;
        private List<Pawn> cachedCurrentFollowers;
        private List<Pawn> cachedRivalFollowers;

        public override Vector2 InitialSize => new Vector2(650f, 550f);

        public Dialog_ReligiousRiot(GlobalPantheonFollowers playerFollowers, GlobalPantheonFollowers rivals)
        {
            SoundDefOf.LetterArrive_BadUrgent.PlayOneShotOnCamera();
            this.rivalFollowers = rivals;
            this.riotLeader = rivals.GlobalPawns.RandomElement().Followers.RandomElementByWeight(x => x.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.Hunting));
            this.currentFollowers = playerFollowers;
            this.forcePause = true; ;
            cachedCurrentFollowers = currentFollowers.AllPawns;
            cachedRivalFollowers = rivalFollowers.AllPawns;
            cachedMaxPawns = Math.Max(cachedRivalFollowers.Count, cachedCurrentFollowers.Count);
            this.powerFraction = (float)rivalFollowers.AllPawns.Count / (float)(playerFollowers.AllPawns.Count + rivals.AllPawns.Count);
            this.closeOnClickedOutside = false;
            this.closeOnCancel = false;
        }
        public override void DoWindowContents(Rect inRect)
        {
            inRect = inRect.ContractedBy(4f);
            GUI.BeginGroup(inRect);

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "ReligiousRiot".Translate());
            Text.Font = GameFont.Small;

            string description = "ReligiousRiotDesc".Translate(rivalFollowers.Pantheon.label, riotLeader.Label);
            Rect labelRect = new Rect(0f, titleRect.yMax + 8f, inRect.width, Text.CalcHeight(description, inRect.width));
            Widgets.Label(labelRect, description);

            Rect currentFollowerRect = new Rect(0f, labelRect.yMax + 4f, inRect.width, 300f);

            this.DrawFollowerRect(currentFollowerRect);

            Rect playerRect = new Rect(0f, currentFollowerRect.yMax + 16f, 208f, 48f);
            if (Widgets.ButtonText(playerRect, currentFollowers.Pantheon.label))
            {
                this.PreserveReligion();
            }

            Rect opposingRect = new Rect(inRect.width - 208f, currentFollowerRect.yMax + 16f, 208f, 48f);
            if (Widgets.ButtonText(opposingRect, rivalFollowers.Pantheon.label))
            {
                this.ChooseNewReligion();
            }

            GUI.EndGroup();
        }

        private void DrawFollowerRect(Rect currentFollowerRect)
        {
            GUI.BeginGroup(currentFollowerRect);
            Rect powerRec = new Rect(24, 8f, currentFollowerRect.width - 48f, 32f);
            Widgets.FillableBar(powerRec, 1f - this.powerFraction, currentFollowers.Pantheon.FillTex, rivalFollowers.Pantheon.FillTex, true);
            Rect iconsRect = new Rect(0f, 0f, 48f, 48f);
            GUI.DrawTexture(iconsRect, currentFollowers.Pantheon.Icon);

            iconsRect.x = currentFollowerRect.width - 48f;
            GUI.DrawTexture(iconsRect, rivalFollowers.Pantheon.Icon);

            iconsRect.x = currentFollowerRect.width / 2f - 24f;

            GUI.DrawTexture(iconsRect, IconAggro);

            Widgets.DrawLineHorizontal(0f, iconsRect.yMax + 2f, currentFollowerRect.width);

            Rect outerRect = new Rect(0f, iconsRect.yMax + 4f, currentFollowerRect.width, currentFollowerRect.height - iconsRect.yMax - 4f);
            Rect viewRect = new Rect(0f, 0f, currentFollowerRect.width - 36f, cachedMaxPawns * 54f);

            Widgets.BeginScrollView(outerRect, ref scrollVect, viewRect);
            Rect iconRect1 = new Rect(0f, 0f, 64f, 64f);
            Rect labelRect1 = new Rect(iconRect1.xMax + 2f, 0f, 200f, 64f);
            Rect iconRect2 = new Rect(viewRect.width - 64f, 0f, 64f, 64f);
            Rect labelRect2 = new Rect(iconRect2.x - 204f, 0f, 200f, 64f);
            float lineOffset = 66f;
            for (int i = 0; i < cachedMaxPawns; i++)
            {
                if (cachedCurrentFollowers.Count > i)
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    GUI.DrawTexture(iconRect1, PortraitsCache.Get(cachedCurrentFollowers[i], iconRect1.size));
                    Widgets.Label(labelRect1, cachedCurrentFollowers[i].Label);
                }
                if (cachedRivalFollowers.Count > i)
                {
                    Text.Anchor = TextAnchor.MiddleRight;
                    GUI.DrawTexture(iconRect2, PortraitsCache.Get(cachedRivalFollowers[i], iconRect2.size));
                    Widgets.Label(labelRect2, cachedRivalFollowers[i].Label);
                }

                iconRect1.y += lineOffset;
                iconRect2.y += lineOffset;
                labelRect2.y += lineOffset;
                labelRect1.y += lineOffset;
            }
            Text.Anchor = TextAnchor.UpperLeft;

            Widgets.EndScrollView();

            GUI.EndGroup();
        }

        private void ChooseNewReligion()
        {
            this.ResolveDecision(rivalFollowers, currentFollowers);
        }

        private void PreserveReligion()
        {
            this.ResolveDecision(currentFollowers, rivalFollowers);
        }

        private void ResolveDecision(GlobalPantheonFollowers playerFollowers, GlobalPantheonFollowers losingFollowers)
        {
            bool choseNewReligion = false;
            bool losersRevolt = false;
            if (GlobalWorshipTracker.Current.PlayerPantheon != playerFollowers.Pantheon)
            {
                GlobalWorshipTracker.Current.PlayerPantheon = playerFollowers.Pantheon;
                choseNewReligion = true;
            }


            var iomFaction = Find.FactionManager.FirstFactionOfDef(Core.FactionsDefOf.IoM_NPC);
            var chaosFaction = Find.FactionManager.FirstFactionOfDef(Core.FactionsDefOf.Chaos_NPC);
            foreach (var globalPawn in losingFollowers.GlobalPawns)
            {
                foreach (Pawn pawn in globalPawn.Followers)
                {
                    PawnBanishUtility.Banish(pawn);
                    if(losingFollowers.Pantheon == PantheonDefOf.Chaos)
                    {
                        pawn.SetFactionDirect(chaosFaction);
                    }
                    else if (pawn.Faction == null)
                    {
                        pawn.SetFactionDirect(iomFaction);
                    }
                }
            }

            if (losingFollowers.Pantheon == PantheonDefOf.Chaos)
            {
                losersRevolt = true;
                if (chaosFaction != null && !chaosFaction.HostileTo(Faction.OfPlayer))
                {
                    chaosFaction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, false);
                }
                foreach (var globalPawns in losingFollowers.GlobalPawns)
                {
                    if (globalPawns.Followers.Count > 0 && Current.Game.FindMap(globalPawns.Tile)?.mapPawns.ColonistCount > 0)
                    {
                        LordJob_AssaultColony lordJob = new LordJob_AssaultColony(chaosFaction, true, false, true, true, false);
                        LordMaker.MakeNewLord(chaosFaction, lordJob, Current.Game.FindMap(globalPawns.Tile), globalPawns.Followers);
                    }
                }
            }
            else if (playerFollowers.Pantheon == PantheonDefOf.Chaos)
            {
                foreach (var globalPawns in losingFollowers.GlobalPawns)
                {
                    foreach (var pawn in globalPawns.Followers.FindAll(x => x.Spawned))
                    {
                        pawn.jobs.TryTakeOrderedJob(new Verse.AI.Job(RimWorld.JobDefOf.Flee), Verse.AI.JobTag.Escaping);
                    }
                }
            }

            string letterTitle = choseNewReligion ? "NewColonyReligion".Translate() : "DefendedColonyReligion".Translate();
            string letter = choseNewReligion ? "NewColonyReligionDesc".Translate(this.rivalFollowers.Pantheon.label) : "DefendedColonyReligionDesc".Translate(this.currentFollowers.Pantheon.label, this.rivalFollowers.Pantheon.label);

            LetterDef letterDef = LetterDefOf.NeutralEvent;

            if (losersRevolt)
            {
                letterDef = LetterDefOf.ThreatBig;
                letter += "\n\n\n" + "CultistUprising".Translate();
            }

            Find.LetterStack.ReceiveLetter(letterTitle, letter, LetterDefOf.NeutralEvent);

            this.Close();
        }
    }
}

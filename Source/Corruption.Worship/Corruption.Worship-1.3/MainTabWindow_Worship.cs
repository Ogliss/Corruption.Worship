using Corruption.Core.Gods;
using Corruption.Core.Soul;
using Corruption.Worship.Wonders;
using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_Worship : MainTabWindow
    {
        private List<Pawn> Worshippers = new List<Pawn>();
        private List<BuildingAltar> Altars = new List<BuildingAltar>();
        private static List<TabRecord> tabsList = new List<TabRecord>();
        private float contributionRecordMaximum;
        private float averagedMaxContribution;
        private static MainTabWindow_Worship.Tab selectedTab = Tab.Favour;

        private enum Tab : Byte
        {
            Favour,
            Pawns
        }

        public MainTabWindow_Worship()
        { 
        }

        public override Vector2 InitialSize => new Vector2(1200f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            CacheWorshippers();
            CacheAltars();

            tabsList.Clear();
            tabsList.Add(new TabRecord("FavourTab".Translate(), delegate
            {
                selectedTab = Tab.Favour;
            }, () => selectedTab == Tab.Favour));
            tabsList.Add(new TabRecord("WorshippersTab".Translate(), delegate
            {
                selectedTab = Tab.Pawns;
            }, () => selectedTab == Tab.Pawns));
        }

        public override void PostOpen()
        {
            base.PostOpen();
            LessonAutoActivator.TeachOpportunity(WorshipConceptDefOf.GlobalFavourKnowledge, OpportunityType.GoodToKnow);
        }

        private void CacheAltars()
        {
            this.Altars.Clear();
            this.Altars = Find.CurrentMap.listerBuildings.allBuildingsColonist.Where(x => x is BuildingAltar).Cast<BuildingAltar>().ToList();
        }

        private void CacheWorshippers()
        {
            this.Worshippers.Clear();

            List<Map> maps = Find.Maps;
            maps.SortBy((Map x) => !x.IsPlayerHome, (Map x) => x.uniqueID);
            foreach (var map in maps)
            {
                Worshippers.AddRange(map.mapPawns.FreeColonists);
                List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
                for (int j = 0; j < list.Count; j++)
                {
                    if (!list[j].IsDessicated())
                    {
                        Pawn innerPawn = ((Corpse)list[j]).InnerPawn;
                        if (innerPawn != null && innerPawn.IsColonist)
                        {
                            Worshippers.Add(innerPawn);
                        }
                    }
                }
                List<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
                for (int k = 0; k < allPawnsSpawned.Count; k++)
                {
                    Corpse corpse = allPawnsSpawned[k].carryTracker.CarriedThing as Corpse;
                    if (corpse != null && !corpse.IsDessicated() && corpse.InnerPawn.IsColonist)
                    {
                        Worshippers.Add(corpse.InnerPawn);
                    }
                }
            }
            foreach (var caravan in Find.WorldObjects.Caravans)
            {
                this.Worshippers.AddRange(caravan.pawns.InnerListForReading.FindAll(x => x.IsColonist));
            }

            this.contributionRecordMaximum = Math.Max(1, this.Worshippers.Sum(x => x.records.GetAsInt(WorshipRecordDefOf.GlobalFavourContributed)));
            this.averagedMaxContribution = this.contributionRecordMaximum / this.Worshippers.Count;
            this.Worshippers = this.Worshippers.OrderByDescending(x => x.records.GetValue(WorshipRecordDefOf.GlobalFavourContributed)).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            inRect.yMin += 32f;

            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, tabsList);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            inRect.x = 0f;
            inRect.y = 0f;
            //inRect.yMax -= 59f;
            switch (selectedTab)
            {
                case Tab.Favour:
                    {
                        WorshipUIUtility.DrawColonyPantheon(inRect, Worshippers);
                        break;
                    }
                case Tab.Pawns:
                    {
                        WorshipUIUtility.DrawWorshippers(inRect, Worshippers, this.Altars, averagedMaxContribution);
                        break;
                    }
            }
            GUI.EndGroup();
        }

    }

    [StaticConstructorOnStartup]
    public static class WorshipUIUtility
    {
        private static Vector2 pantheonScrollPos = Vector2.zero;
        private static readonly Texture2D BackgroundTile = ContentFinder<Texture2D>.Get("UI/Background/SoulmeterTile", true);
        private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(9, 203, 4, 64));

        private static List<WonderDef> cachedWonders = DefDatabase<WonderDef>.AllDefsListForReading;

        public static void DrawColonyPantheon(Rect inRect, List<Pawn> worshippers)
        {
            Rect pantheonHeaderRect = new Rect(inRect);
            pantheonHeaderRect.height = 236f;
            pantheonHeaderRect.width -= 16f;
            Widgets.DrawMenuSection(pantheonHeaderRect);
            Rect innerRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(innerRect);
            var pantheon = GlobalWorshipTracker.Current.PlayerPantheon;
            Rect pantheonRect = new Rect(0f, 0f, innerRect.width, 32f);
            Text.Font = GameFont.Medium;
            Widgets.Label(pantheonRect, "ColonyPantheon".Translate(new NamedArgument(pantheon.label, "RELIGION")));
            Text.Font = GameFont.Small;
            Rect debugRectAdd = new Rect(innerRect.width - 256f, 0f, 256f, 24f);
            if (Prefs.DevMode)
            {
                if (Widgets.ButtonText(debugRectAdd, "Debug: +1000 FavourAll"))
                {
                    foreach (var god in pantheon.members.Select(x => x.god))
                    {
                        GlobalWorshipTracker.Current.TryAddFavor(god, 1000f);
                    }

                    foreach (var worshipper in worshippers)
                    {
                        worshipper.records.AddTo(WorshipRecordDefOf.GlobalFavourContributed, Rand.Range(100, 200));
                    }
                }
                debugRectAdd.y += debugRectAdd.height + 4f;
                if (Widgets.ButtonText(debugRectAdd, "Debug: Change Religion"))
                {
                    Find.WindowStack.Add(new Dialog_SetReligion(GlobalWorshipTracker.Current.PlayerPantheon));
                }
            }

            Rect descriptionRect = new Rect(0f, debugRectAdd.yMax + 2f, pantheonRect.width, Text.LineHeight * 3f);
            Widgets.TextAreaScrollable(descriptionRect, pantheon.description, ref pantheonScrollPos, true);
            Text.Font = GameFont.Medium;
            Rect progressDescRect = new Rect(0f, descriptionRect.yMax + 8f, innerRect.width, 32f);
            Widgets.Label(progressDescRect, "OverallFavour".Translate());
            Text.Font = GameFont.Small;
            Rect progressRect = new Rect(24f, progressDescRect.yMax + 8f, innerRect.width - 80f, 32f);
            GUI.DrawTexture(progressRect, BackgroundTile);
            float overallProgress = GlobalWorshipTracker.Current.PantheonFavourPercentage;
            TooltipHandler.TipRegion(progressRect, new TipSignal(string.Concat(overallProgress * 100f, "%")));
            Rect innerProgressRect = progressRect.ContractedBy(8f);
            Widgets.FillableBar(innerProgressRect, overallProgress, pantheon.FillTex, null, false);
            Rect leftNodeRect = new Rect(0f, progressRect.y - 8f, 48f, 48f);
            GUI.DrawTexture(leftNodeRect, pantheon.Icon);
            Rect rightNodeRect = new Rect(leftNodeRect);
            rightNodeRect.x = progressRect.xMax - 24f;
            GUI.DrawTexture(rightNodeRect, pantheon.Icon);


            Rect attributesRect = new Rect(0f, pantheonHeaderRect.yMax + 4f, innerRect.width * 0.33f, innerRect.height - pantheonHeaderRect.yMax - 8f);
            DrawAttributes(attributesRect, pantheon);

            Rect godsRect = new Rect(attributesRect.xMax + 4f, attributesRect.y, innerRect.width - attributesRect.xMax - 4f, attributesRect.height);
            DrawGods(pantheon, godsRect);
            GUI.EndGroup();
        }

        private static void DrawAttributes(Rect inRect, PantheonDef pantheon)
        {
            Widgets.DrawMenuSection(inRect);
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            Rect titleRect = new Rect(0f, 0f, inRect.width, 48f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "PantheonAttributes".Translate());
            Text.Font = GameFont.Small;

            float curY = titleRect.yMax + 4f;

            foreach (var attribute in pantheon.pantheonAttributes)
            {
                Rect attributeRect = new Rect(0f, curY, inRect.width, 64f);
                Rect attributeIconRect = new Rect(0f, curY, 64f, 64f);
                GUI.DrawTexture(attributeIconRect, attribute.Icon);
                Rect labelRect = new Rect(attributeIconRect.xMax + 4f, curY, attributeRect.width - 64f, 64f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, attribute.label.CapitalizeFirst());
                if (Mouse.IsOver(attributeRect))
                {
                    GUI.DrawTexture(attributeRect, TexUI.HighlightSelectedTex);
                }
                TooltipHandler.TipRegion(attributeRect, new TipSignal(attribute.description));

                Text.Anchor = TextAnchor.UpperLeft;

                curY = attributeRect.yMax + 4f;
            }

            GUI.EndGroup();
        }


        private static Vector2 godScrollPos = Vector2.zero;
        private static void DrawGods(PantheonDef pantheon, Rect godsRect)
        {
            Widgets.DrawMenuSection(godsRect);
            godsRect = godsRect.ContractedBy(17f);
            GUI.BeginGroup(godsRect);
            Text.Font = GameFont.Medium;
            Rect godsDescrRect = new Rect(0f, 0f, godsRect.width, 32f);
            Widgets.Label(godsDescrRect, "PantheonMembers".Translate());
            Text.Font = GameFont.Small;
            Rect memberRect = new Rect(0f, godsDescrRect.yMax + 4f, godsRect.width, godsRect.height - godsDescrRect.yMax - 4f);
            Rect viewRect = new Rect(0f, 0f, godsRect.width - 24f, pantheon.members.Count * 226f);
            Widgets.BeginScrollView(memberRect, ref godScrollPos, viewRect);

            float curY = 0f;
            foreach (var member in pantheon.members)
            {
                Widgets.DrawLineHorizontal(0f, curY, godsRect.width);
                Rect godLabelRect = new Rect(0f, curY + 8f, godsRect.width, 32f);
                GUI.color = member.god.mainColor;
                Text.Font = GameFont.Medium;
                Widgets.Label(godLabelRect, member.god.label);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Rect descriptionRect = new Rect(godLabelRect);
                descriptionRect.height = Text.LineHeight * 3f;
                descriptionRect.y += godLabelRect.height + 2f;
                Widgets.Label(descriptionRect, member.god.description);

                float godsFavour = GlobalWorshipTracker.Current.GetFavourProgressFor(member.god).Favour;
                Rect favourLabelRect = new Rect(0f, descriptionRect.yMax + 4f, descriptionRect.width, Text.LineHeight);
                Widgets.Label(favourLabelRect, String.Concat("GodsFavour".Translate(), ": ", ((int)godsFavour).ToString()));

                Rect borderRect = new Rect(48f, favourLabelRect.yMax + 6f, godsRect.width - 96f, 32f);
                GUI.DrawTexture(borderRect, BackgroundTile);
                Rect progressRect = new Rect(borderRect).ContractedBy(4f);
                Widgets.FillableBar(progressRect, GlobalWorshipTracker.Current.GetFavourProgressFor(member.god).FavourPercentage, member.god.WorshipBarFillTexture, null, false);

                //TooltipHandler.TipRegion(progressRect, new TipSignal(string.Concat(godsFavour, " / ", FavourProgress.ProgressRange.max)));

                Rect nodesRect = new Rect(borderRect.x, borderRect.y - 8f, progressRect.width, 48f);
                DrawGodWonderNodes(nodesRect, member.god, godsFavour);
                curY = nodesRect.yMax + 4f;
            }
            Widgets.EndScrollView();

            GUI.EndGroup();
        }

        private static void DrawGodWonderNodes(Rect nodesRect, GodDef god, float godsFavour)
        {
            var tracker = GlobalWorshipTracker.Current;
            float totalWidth = nodesRect.width;
            foreach (var wonder in cachedWonders.Where(x => x.associatedGods.Contains(god)))
            {
                bool available = godsFavour >= wonder.favourCost;
                float xOffset = nodesRect.x + (wonder.favourCost / FavourProgress.ProgressRange.max * totalWidth) - 24f;
                Rect nodeRect = new Rect(xOffset, nodesRect.y, 48f, 48f);
                GUI.DrawTexture(nodeRect, wonder.wonderIcon);

                bool canActivate = tracker.GlobalWonderCooldownTicks[wonder] == 0;
                if (!canActivate)
                {
                    float num = 1f - (tracker.GlobalWonderCooldownTicks[wonder] / (float)wonder.CooldownTicks);
                    Widgets.FillableBar(nodeRect, Mathf.Clamp01(num), cooldownBarTex, null, doBorder: false);

                    Text.Font = GameFont.Tiny;
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(nodeRect, num.ToStringPercent("F0"));
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                if (Mouse.IsOver(nodeRect))
                {
                    GUI.DrawTexture(nodeRect, TexUI.HighlightSelectedTex);
                    TooltipHandler.TipRegion(nodeRect, new TipSignal(wonder.ToolTip, wonder.description.GetHashCode() * 124758));
                }
                if (canActivate && Widgets.ButtonInvisible(nodeRect, true))
                {
                    if (!available)
                    {

                        Messages.Message("MessageWonderPointsMissing".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else if (wonder.pointsScalable && wonder.pointsCurve != null)
                    {
                        Func<int, string> textGetter;
                        textGetter = ((int x) => "SetWorshipPoints".Translate());
                        Dialog_Slider window = new Dialog_Slider(textGetter, wonder.favourCost, (int)godsFavour, delegate (int value)
                        {
                            TriggerWonder(god, wonder);
                        }, wonder.favourCost);
                        Find.WindowStack.Add(window);
                    }
                    else
                    {
                        TriggerWonder(god, wonder);
                    }
                }
            }
        }

        private static void TriggerWonder(GodDef god, WonderDef wonder)
        {
            if (wonder.Worker.TryExecuteWonder(god, wonder.favourCost))
            {
                GlobalWorshipTracker.Current.ConsumeFavourFor(wonder.favourCost, god);
            }
        }

        private static Vector2 followersScrollPos = Vector2.zero;
        private static Vector2 altarsScrollPos = Vector2.zero;

        internal static void DrawWorshippers(Rect inRect, List<Pawn> worshippers, List<BuildingAltar> altars, float contributionRecordMaximum)
        {
            inRect = inRect.ContractedBy(17f);
            GUI.BeginGroup(inRect);
            Rect pawnsRect = new Rect(0f, 0f, 256f, inRect.height);
            Widgets.DrawMenuSection(pawnsRect);
            pawnsRect = pawnsRect.ContractedBy(5f);
            GUI.BeginGroup(pawnsRect);
            Text.Font = GameFont.Medium;
            Rect labelRect = new Rect(0f, 0f, pawnsRect.width, Text.LineHeight);
            Widgets.Label(labelRect, "Followers".Translate());
            Text.Font = GameFont.Small;
            Rect outerRect = new Rect(0f, labelRect.yMax + 4f, pawnsRect.width, pawnsRect.height - labelRect.yMax - 32f);
            Rect viewRect = new Rect(0f, 0f, outerRect.width - 16f, worshippers.Count * 64f + 4f);
            Widgets.BeginScrollView(outerRect, ref followersScrollPos, viewRect);
            float curY = 0f;
            foreach (var pawn in worshippers)
            {
                Rect iconRect = new Rect(0f, curY, 64f, 64f);
                GUI.DrawTexture(iconRect, PortraitsCache.Get(pawn, iconRect.size, Rot4.South));

                Rect nameRect = new Rect(68f, curY, viewRect.width - 68f, Text.LineHeight);
                Widgets.Label(nameRect, pawn.NameShortColored);

                Rect contributionRect = new Rect(nameRect);

                contributionRect.y += Text.LineHeight + 4f;
                Widgets.Label(contributionRect, "WorshipEfforts".Translate());
                contributionRect.y += Text.LineHeight;
                contributionRect.height = 12f;
                contributionRect.width -= 8f;
                float pawnContribution = pawn.records.GetValue(WorshipRecordDefOf.GlobalFavourContributed);
                Widgets.FillableBar(contributionRect, Mathf.Clamp(pawnContribution / contributionRecordMaximum, 0f, 1f), GlobalWorshipTracker.Current.PlayerPantheon.FillTex);
                TooltipHandler.TipRegion(contributionRect, new TipSignal("WorshipEffortsDesc".Translate(), pawn.GetHashCode() * 144758));

                Rect totalRect = new Rect(0f, curY, viewRect.width, 68f);
                if (Mouse.IsOver(totalRect))
                {
                    Widgets.DrawHighlight(totalRect);
                }
                curY = totalRect.yMax + 4f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();

            Rect altarsRect = new Rect(pawnsRect.xMax + 8f, 0f, inRect.width - pawnsRect.xMax - 8f, pawnsRect.height);
            Widgets.DrawMenuSection(altarsRect);
            altarsRect = altarsRect.ContractedBy(17f);
            GUI.BeginGroup(altarsRect);
            altarsRect = new Rect(0f, 0f, altarsRect.width, altarsRect.height);
            Text.Font = GameFont.Medium;
            Rect altarTitleRect = new Rect(4f, 0f, altarsRect.width, Text.LineHeight);
            Widgets.Label(altarTitleRect, "PlacesOfWorship".Translate());
            Text.Font = GameFont.Small;
            Rect outerAltarRect = new Rect(0f, altarTitleRect.yMax + 4f, altarsRect.width - 8f, altarsRect.height - altarTitleRect.yMax - 4f);
            Rect altarViewRect = new Rect(0f, 0f, altarsRect.width - 16f, altars.Count * 128f);
            Widgets.BeginScrollView(outerAltarRect, ref altarsScrollPos, altarViewRect);
            curY = 0f;
            foreach (var altar in altars)
            {
                Rect altarRect = new Rect(0f, curY, altarViewRect.width - 8f, Text.LineHeight * 8f);
                Widgets.DrawWindowBackground(altarRect);

                Widgets.DrawLineHorizontal(4f, curY, altarViewRect.width - 8f);
                Rect iconRect = new Rect(4f, curY + 2f, 64f, 64f);
                Widgets.ThingIcon(iconRect, altar);
                Rect titleRect = new Rect(4f, curY + 2f, altarViewRect.width, Text.LineHeight);
                Widgets.Label(titleRect, altar.RoomName);

                Rect rect3 = new Rect(68f, titleRect.yMax + 2f, 200f, 25f);

                Rect statRect = new Rect(rect3.xMax + 4f, titleRect.yMax + 2f, altarViewRect.width - 38f, Text.LineHeight * 6);
                GUI.BeginGroup(statRect);

                var room = altar.GetRoom();
                Widgets.ListSeparator(ref curY, viewRect.width, "AltarStats".Translate());
                curY += 4f;
                float recordY = curY;
                if (room != null)
                {
                    WorshipUIUtility.DrawRoomRecord(ref recordY, room, RoomStatDefOf.Impressiveness, statRect.width - 32f, altar);
                    WorshipUIUtility.DrawRoomRecord(ref recordY, room, RoomStatDefOf.Wealth, statRect.width - 32f, altar);
                }

                WorshipUIUtility.DrawAltarRecord(ref recordY, statRect.width - 32f, altar, WorshipRecordDefOf.SermonsHeldAltar);
                WorshipUIUtility.DrawAltarRecord(ref recordY, statRect.width - 32f, altar, WorshipRecordDefOf.SermonAttendees);

                GUI.EndGroup();
                curY = altarRect.yMax + 8f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();


            GUI.EndGroup();
        }

        private static void DrawRoomRecord(ref float curY, Room room, RoomStatDef stat, float width, BuildingAltar altar)
        {
            float num = width * 0.45f;
            string text = room.GetStat(stat).ToString();
            Rect rect = new Rect(8f, curY, width -8f, Text.CalcHeight(text, num));
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(new Rect(rect.x, rect.y, width * 0.8f, rect.height));
            }
            Rect rect2 = rect;
            rect2.width -= num;
            Widgets.Label(rect2, stat.LabelCap);
            Rect rect3 = rect;
            rect3.x = rect2.xMax;
            rect3.width = num;
            Widgets.Label(rect3, text);
            if (stat.description != null && Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, new TipSignal(() => stat.description ?? "", stat.GetHashCode()));
            }
            curY += rect.height;
        }

        private static void DrawAltarRecord(ref float curY, float width, BuildingAltar altar, RecordDef record)
        {
            float num = width * 0.45f;
            string text = (record.type != 0) ? altar.records.GetValue(record).ToString("0.##") : altar.records.GetAsInt(record).ToStringTicksToPeriod();
            Rect rect = new Rect(8f, curY, width - 8f, Text.CalcHeight(text, num));
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(new Rect(rect.x, rect.y, width * 0.8f, rect.height));
            }
            Rect rect2 = rect;
            rect2.width -= num;
            Widgets.Label(rect2, record.LabelCap);
            Rect rect3 = rect;
            rect3.x = rect2.xMax;
            rect3.width = num;
            Widgets.Label(rect3, text);
            if (Mouse.IsOver(rect))
            {
                TooltipHandler.TipRegion(rect, new TipSignal(() => record.description, record.GetHashCode()));
            }
            curY += rect.height;
        }
    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption.Worship
{
    public class Dialog_AssignPreacher : Window
    {
        protected SermonTemplate sermon;

        private BuildingAltar altar;

        private Vector2 scrollPosition;

        private const float EntryHeight = 35f;

        private const float LineSpacing = 8f;

        public override Vector2 InitialSize => new Vector2(620f, 500f);

        protected virtual Pawn TargetedPawn => this.sermon.Preacher;

        public Dialog_AssignPreacher(BuildingAltar altar, SermonTemplate sermon)
        {
            this.sermon = sermon;
            this.altar = altar;
            doCloseButton = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        private IEnumerable<Pawn> Candidates
        {
            get
            {
                if (!altar.Spawned)
                {
                    return Enumerable.Empty<Pawn>();
                }
                return altar.Map.mapPawns.FreeColonists.OrderByDescending(x => x.skills.GetSkill(SkillDefOf.Social).Level);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect outRect = new Rect(inRect);
            outRect.yMin += 20f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)Candidates.Count() * 35f + 100f);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            try
            {
                float num = 0f;
                bool flag = false;
                if (TargetedPawn != null)
                {
                    flag = true;
                    Rect rect = new Rect(0f, num, viewRect.width * 0.7f, 32f);
                    Widgets.Label(rect, TargetedPawn.LabelCap);
                    rect.x = rect.xMax;
                    rect.width = viewRect.width * 0.3f;
                    if (Widgets.ButtonText(rect, "BuildingUnassign".Translate()))
                    {
                        UnassignPawn(TargetedPawn);
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        return;
                    }
                    num += 35f;
                }
                if (flag)
                {
                    num += 15f;
                }
                foreach (Pawn assigningCandidate in Candidates)
                {
                    if (TargetedPawn != assigningCandidate)
                    {
                        AcceptanceReport acceptanceReport = AcceptanceReport.WasAccepted;
                        bool accepted = acceptanceReport.Accepted;
                        string text = assigningCandidate.LabelCap + (accepted ? "" : (" (" + acceptanceReport.Reason.StripTags() + ")")) + AptitudeText(assigningCandidate);
                        float width = viewRect.width * 0.7f;
                        float num2 = Text.CalcHeight(text, width);
                        float num3 = (35f > num2) ? 35f : num2;
                        Rect rect2 = new Rect(0f, num, width, num3);
                        if (!accepted)
                        {
                            GUI.color = Color.gray;
                        }
                        Widgets.Label(rect2, text);
                        rect2.x = rect2.xMax;
                        rect2.width = viewRect.width * 0.3f;
                        rect2.height = 35f;
                        TaggedString taggedString = "BuildingAssign".Translate();
                        if (Widgets.ButtonText(rect2, taggedString, drawBackground: true, doMouseoverSound: true, accepted))
                        {
                            AssignPawn(assigningCandidate);
                            SoundDefOf.Click.PlayOneShotOnCamera();
                            Close();
                            break;
                        }
                        GUI.color = Color.white;
                        num += num3;
                    }
                }
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        protected virtual string AptitudeText(Pawn assigningCandidate)
        {
            return string.Concat("(", "PreacherAptitude".Translate(assigningCandidate.skills.GetSkill(SkillDefOf.Social).Level.ToString()), ")");
        }

        protected virtual void AssignPawn(Pawn assigningCandidate)
        {
            sermon.Preacher = assigningCandidate;
        }

        protected virtual void UnassignPawn(Pawn pawn)
        {
            sermon.Preacher = null;
        }
    }

    public class Dialog_AssignAssistant : Dialog_AssignPreacher
    {
        public Dialog_AssignAssistant(BuildingAltar altar, SermonTemplate sermon) : base(altar, sermon)
        {
        }

        protected override string AptitudeText(Pawn assigningCandidate)
        {
            return "";
        }

        protected override void AssignPawn(Pawn assigningCandidate)
        {
            this.sermon.Assistant = assigningCandidate;
        }

        protected override void UnassignPawn(Pawn pawn)
        {
            this.sermon.Assistant = null;
        }
    }
}

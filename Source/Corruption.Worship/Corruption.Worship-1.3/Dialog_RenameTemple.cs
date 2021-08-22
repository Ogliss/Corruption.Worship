using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class Dialog_RenameTemple : Window
    {
        BuildingAltar Altar;

        private string curName;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 200f);
            }
        }

        public Dialog_RenameTemple(BuildingAltar altar)
        {
            this.Altar = altar;
            this.forcePause = true;
            this.closeOnClickedOutside = false;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "RenameTemple".Translate());
            this.curName = Widgets.TextField(new Rect(0f, rect.height - 35f, rect.width / 2f - 20f, 35f), this.curName);
            if (Widgets.ButtonText(new Rect(rect.width / 2f + 20f, rect.height - 35f, rect.width / 2f - 20f, 35f), "Accept".Translate(), true, false, true) || flag)
            {
                if (this.IsValidRoomName(this.curName))
                {
                    this.Altar.RoomName = this.curName;
                    Find.WindowStack.TryRemove(this, true);
                }
                else
                {
                    Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.NegativeEvent);
                }
                Event.current.Use();
            }
        }

        private bool IsValidRoomName(string s)
        {
            return s.Length != 0 && GenText.IsValidFilename(s);
        }
    }
}

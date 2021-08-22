using Corruption.Core.Gods;
using Corruption.Core.Soul;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class Dialog_FormAndSendPilgrims : Dialog_FormCaravan
    {
        private GodDef god;
        public Dialog_FormAndSendPilgrims(Map map, GodDef god) : base(map)
        {
            this.god = god;
        }
        public override void PostClose()
        {
            base.PostClose();
            foreach (Pawn p in TransferableUtility.GetPawnsFromTransferables(this.transferables))
            {
                CompSoul soul = p.Soul();

                if (soul != null)
                {
                    soul.OnPilgrimageFor = this.god;
                }
            }
        }
    }
}

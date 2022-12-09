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
    public class ITab_Altar : ITab
    {
        protected BuildingAltar SelAltar
        {
            get
            {
                return (BuildingAltar)base.SelThing;
            }
        }

        public ITab_Altar()
        {
            this.size = TempleCardUtility.TempleCardSize;
            this.labelKey = "TabAltar";
        }

        public override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(5f);
            TempleCardUtility.DrawTempleCard(rect, SelAltar);
        }
    }
}

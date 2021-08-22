using Corruption.Core;
using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class Dialog_SetReligion : Dialog_SetPantheon
    {
        public Dialog_SetReligion(PantheonDef currentDef) : base(currentDef)
        {

        }

        protected override void SelectionChanged(PantheonDef selectedDef)
        {
            GlobalWorshipTracker.Current.PlayerPantheon = selectedDef ?? GlobalWorshipTracker.Current.PlayerPantheon;
        }
    }
}

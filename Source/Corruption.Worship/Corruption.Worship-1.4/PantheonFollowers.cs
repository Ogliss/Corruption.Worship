using Corruption.Core.Gods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Corruption.Worship
{
    public class GlobalPantheonFollowers
    {
        public struct FollowersPerTile
        {
            public int Tile;

            public List<Pawn> Followers;

            public FollowersPerTile(int tile)
            {
                this.Tile = tile;
                this.Followers = new List<Pawn>();
            }
        }

        public PantheonDef Pantheon;

        public List<FollowersPerTile> GlobalPawns = new List<FollowersPerTile>();

        public List<Pawn> AllPawns => this.GlobalPawns.SelectMany(x => x.Followers).ToList();

        public GlobalPantheonFollowers() { }
        public GlobalPantheonFollowers(PantheonDef pantheon)
        {
            Pantheon = pantheon;
        }
    }
}

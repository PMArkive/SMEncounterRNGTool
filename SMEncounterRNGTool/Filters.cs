using System.Linq;

namespace SMEncounterRNGTool
{
    class Filters
    {
        public bool[] Nature;
        public bool[] HPType;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, Stats;
        public byte PerfectIVs;
        public bool Skip;
        public int Lv;
        public bool[] Slot;

        public bool CheckIVs(RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > result.IVs[i] || result.IVs[i] > IVup[i])
                    return false;
            if (result.IVs.Count(e => e == 31) < PerfectIVs)
                return false;
            return true;
        }

        public bool CheckStats(RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (Stats[i] != 0 && Stats[i] != result.Stats[i])
                    return false;
            return true;
        }

        public bool CheckNature(int resultnature)
        {
            if (Nature.All(n => !n)) return true;
            return Nature[resultnature];
        }

        public bool CheckHiddenPower(int[] IV)
        {
            if (HPType.All(n => !n)) return true;
            var val = 15 * ((IV[0] & 1) | (IV[1] & 1) << 1 | (IV[2] & 1) << 2 | (IV[5] & 1) << 3 | (IV[3] & 1) << 4 | (IV[4] & 1) << 5) / 63;
            return HPType[val];
        }

        public bool CheckSlot(int slot)
        {
            if (Slot.All(n => !n)) return true;
            return Slot[slot];
        }
    }
}

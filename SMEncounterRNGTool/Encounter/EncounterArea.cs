using System;
using System.Linq;

namespace SMEncounterRNGTool
{
    public class EncounterArea
    {
        public byte Location, idx;
        public int Locationidx => Location + (idx << 8);
        public byte NPC, Correction = 1;
        private String _mark;
        public String mark
        {
            get
            {
                string tmp = _mark ?? (idx > 0 ? idx.ToString() : "");
                return tmp == "" ? "" : $" ({tmp})";
            }
            set { _mark = value; }
        }
        public byte LevelMin;
        private byte _LevelMax;
        public byte LevelMax { get { return _LevelMax > 0 ? _LevelMax : (byte)(LevelMin + 3); } set { _LevelMax = value; } }

        public int[] Species = new int[1];
        private int[] _MoonSpecies;
        public int[] MoonSpecies { get { return _MoonSpecies ?? Species; } set { _MoonSpecies = value; } }

        private readonly static int[] DayList = new[] { 734, 735, 165, 166, 046, 751, 752, 425, 174 };
        private readonly static int[] NightList = new[] { 019, 020, 167, 168, 755, 283, 284, 200, 731 };
        private readonly static int[] SunList = new[] { 546, 766 };
        private readonly static int[] MoonList = new[] { 548, 765 };
        
        public bool DayNightDifference => Species.Any(i => DayList.Contains(i));
        public bool SunMoonDifference => Species.Any(i => SunList.Contains(i)) || _MoonSpecies != null;

        public int[] getSpecies(bool IsMoon, bool IsNight)
        {
            int[] table = (int[])(IsMoon ? MoonSpecies : Species).Clone();
            if (IsNight && DayNightDifference) // Replace species
                for (int i = 1; i < table.Length; i++)
                {
                    int idx = Array.IndexOf(DayList, table[i]);
                    if (idx > -1)
                        table[i] = NightList[idx];
                }
            if (IsMoon && SunMoonDifference)
                for (int i = 1; i < table.Length; i++)
                {
                    int idx = Array.IndexOf(SunList, table[i]);
                    if (idx > -1)
                        table[i] = MoonList[idx];
                }
            return table;
        }

        public readonly static byte[][] SlotType = new byte[][]
        {
            new byte[]{1,2,1,2,3,3,4,4,4,4}, //0
            new byte[]{1,2,1,3,4,5,6,6,7,7}, //1
            new byte[]{1,2,1,3,4,5,6,7,3,3}, //2
            new byte[]{1,2,1,3,4,3,5,6,6,7}, //3
            new byte[]{1,2,3,1,4,5,6,5,7,7}, //4
            new byte[]{1,2,1,2,3,3,4,5,5,6}, //5
            new byte[]{1,2,3,4,5,6,2,6,6,6}, //6
            new byte[]{1,2,1,2,1,2,3,3,3,3}, //7
            new byte[]{1,2,1,2,3,3,4,4,3,5}, //8
            new byte[]{1,2,3,4,5,6,6,7,8,8}, //9
            new byte[]{1,2,3,3,3,3,4,5,6,6}, //10
            new byte[]{1,2,1,3,3,3,4,5,6,6}, //11
            new byte[]{1,2,3,4,5,5,6,6,6,6}, //12
            new byte[]{1,2,1,3,3,3,4,4,4,4}, //13
            new byte[]{1,2,1,3,4,4,5,5,5,5}, //14
            new byte[]{1,1,2,2,3,3,3,3,3,3}, //15
            new byte[]{1,2,3,2,4,4,5,5,5,5}, //16
            new byte[]{1,2,3,4,5,4,4,4,4,4}, //17
            new byte[]{1,2,3,4,5,5,2,2,2,2}, //18
            new byte[]{1,2,3,2,4,4,2,2,2,5}, //19
            new byte[]{1,2,3,3,4,5,6,3,7,7}, //20
            new byte[]{1,2,1,2,2,3,4,4,4,4}, //21
            new byte[]{1,2,1,1,1,1,1,1,1,1}, //22
            new byte[]{1,2,1,3,4,5,6,5,7,7}, //23
            new byte[]{1,2,1,1,2,2,2,3,4,4}, //24
            new byte[]{1,2,3,3,4,4,3,4,4,4}, //25
            new byte[]{1,2,3,3,1,2,4,4,4,4}, //26
            new byte[]{1,2,1,2,3,3,4,4,5,5}, //27
        };
    }
}

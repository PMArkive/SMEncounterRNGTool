using System;
using System.Linq;
namespace SMEncounterRNGTool
{
    public class EncounterArea
    {
        public byte Location, NPC, Correction = 1;
        public byte idx;
        public int Locationidx => Location + (idx << 8);
        public byte LevelMin, LevelMax;
        public EncounterType EnctrType = EncounterType.Grass;
        public bool DayNightDifference = false;
        public int[] MoonSpecies { get { return _MoonSpecies ?? Species; } set { _MoonSpecies = value; } }
        public int[] Species;
        private int[] _MoonSpecies;

        public int[] getSpecies(bool IsMoon, bool IsNight)
        {
            int[] table = (int[])(IsMoon ? MoonSpecies : Species).Clone();
            if (IsNight && DayNightDifference)
            {
                for (int i = 1; i < table.Length; i++)
                {
                    int idx = Array.IndexOf(DayList, table[i]);
                    if (idx > -1)
                        table[i] = NightList[idx];
                }
            }
            return table;
        }

        public static int[][] SlotType = new int[][]
        {
            new []{1,2,1,2,3,3,4,4,4,4},
            new []{1,2,1,3,4,5,6,6,7,7},
            new []{1,2,1,3,4,5,6,7,3,3},
            new []{1,2,1,2,2,2,3,3,3,3},
            new []{1,2,1,2,3,3,2,2,2,2},
        };

        public static int[] DayList = new[] { 734, 165, };
        public static int[] NightList = new[] { 019, 167, };
    }

    class LocationTable
    {
        public readonly static EncounterArea[] Table =
        {
            new EncounterArea
            {
                Location = 006, NPC = 00, Correction = 15,
                idx = 1, LevelMin = 02, LevelMax = 03,
                Species = new[] {0,734,731,010,165},
                DayNightDifference = true,
            },
            new EncounterArea
            {
                Location = 006, NPC = 00, Correction = 15,
                idx = 3, LevelMin = 03, LevelMax = 05,
                Species = new[] {1,734,731,736,010,011,165,172},
                DayNightDifference = true,
            },
            new EncounterArea
            {
                Location = 006, NPC = 00, Correction = 15,
                idx = 4, LevelMin = 10, LevelMax = 13,
                Species = new[] {2,734,731,438,010,011,165,446},
                DayNightDifference = true,
            },
            new EncounterArea
            {
                Location = 007, NPC = 00, Correction = 15,
                idx = 0, LevelMin = 05, LevelMax = 07,
                Species = new[] {3,734,278,079},
                DayNightDifference = true,
            },
            new EncounterArea
            {
                Location = 008, NPC = 00, Correction = 15,
                idx = 6, LevelMin = 06, LevelMax = 08,
                Species = new[] {4,052,081,088},
            },
        };
        public readonly static int[] SMLocationList = Table.Select(t => t.Locationidx).ToArray();
    }

    public enum EncounterType
    {
        Any, Grass, Surf,
        Old_Rod, Good_Rod, Super_Rod,
        Rock_Smash, Special,
    }
}

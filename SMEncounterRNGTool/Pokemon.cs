using System.Linq;
using PKHeX.Core;

namespace SMEncounterRNGTool
{
    class Pokemon
    {
        public short Species;
        public bool Template; // If it's a category
        public byte Form;
        public bool Wild;
        public bool Gift;
        public short Delay;
        public byte NPC;
        public bool UB;
        public int[] UBLocation;
        public byte[] UBRate;
        public bool ShinyLocked;
        public bool Syncable = true;
        public byte Level;
        public bool? SunOnly;
        
        public short SpecForm => (short)(Species + (Form << 11));
        public bool InSun => SunOnly ?? true;
        public bool InMoon => !SunOnly ?? true;
        public bool IsSolgaleo => Species == 791;
        public bool IsLunala => Species == 792;
        public bool IsEvent => Species == 151;
        public bool IsCrabrawler => Species == 739;
        public bool IsBlank => Species == 0;
        public bool AlwaysSync => Gift;
        public PersonalInfo info => PersonalTable.SM.getFormeEntry(Species, Form);
        public override string ToString()
        {
            switch (Species)
            {
                case 0: return "-";
                case 151: return StringItem.eventstr;
                case 408: return StringItem.fossilstr;
                case 722: return StringItem.starterstr;
                case 133: return $"{StringItem.species[133]} ({StringItem.species[0]})";
                case 718: return StringItem.species[718] + (Form == 2 ? "-10%" : "-50%");
                default: return StringItem.species[Species];
            }
        }

        public static Pokemon[] getVersionList(bool IsMoon) => SpeciesList.Where(pm => IsMoon ? pm.InMoon : pm.InSun).ToArray();

        public readonly static Pokemon[] SpeciesList =
        {
            new Pokemon { Species = 000, Level = 00, Template = true, Wild = true,},    // Blank
            new Pokemon { Species = 151, Level =100, Template = true, NPC = 4, Syncable = false, },        // Event
            new Pokemon { Species = 785, Level = 60, ShinyLocked = true },              // Tapu Koko
            new Pokemon { Species = 786, Level = 60, ShinyLocked = true },              // Tapu Lele
            new Pokemon { Species = 787, Level = 60, ShinyLocked = true },              // Tapu Bulu
            new Pokemon { Species = 788, Level = 60, ShinyLocked = true, NPC = 1, },    // Tapu Fini
            new Pokemon { Species = 791, Level = 55, ShinyLocked = true, NPC = 6, Delay = 288, SunOnly = true},    // Solgaleo
            new Pokemon { Species = 792, Level = 55, ShinyLocked = true, NPC = 6, Delay = 282, SunOnly = false},   // Lunala
            new Pokemon { Species = 789, Level = 05, ShinyLocked = true, NPC = 3, Delay = 34, Gift = true},    // Cosmog
            new Pokemon { Species = 772, Level = 40, NPC = 8, Delay = 34, Gift = true,},    // Type:Null
            new Pokemon { Species = 801, Level = 50, ShinyLocked = true, NPC = 6, Delay = 34, Gift = true,},    // Magearna
            new Pokemon { Species = 718, Level = 50, ShinyLocked = true, NPC = 3, Delay = 32, Gift = true, Form = 2,},    // Zygarde-10%
            new Pokemon { Species = 718, Level = 50, ShinyLocked = true, NPC = 3, Delay = 32, Gift = true, Form = 3,},    // Zygarde-50%
            new Pokemon { Species = 408, Level = 15, NPC = 1, Delay = 40, Gift = true, Template = true,},    // Fossil
            new Pokemon { Species = 793, Level = 55, UB = true, UBLocation = new []{100,082}, UBRate = new byte[]{80,30},},    // Nihilego
            new Pokemon { Species = 794, Level = 65, UB = true, UBLocation = new []{040}, UBRate = new byte[]{30}, SunOnly = true,},    // Buzzwole
            new Pokemon { Species = 795, Level = 60, UB = true, UBLocation = new []{046}, UBRate = new byte[]{50}, SunOnly = false,},   // Pheromosa
            new Pokemon { Species = 796, Level = 65, UB = true, UBLocation = new []{346,076}, UBRate = new byte[]{15,30},},    // Xurkitree
            new Pokemon { Species = 797, Level = 65, UB = true, UBLocation = new []{134,124}, UBRate = new byte[]{30,30}, SunOnly = false,},    // Celesteela
            new Pokemon { Species = 798, Level = 60, UB = true, UBLocation = new []{134,376,632}, UBRate = new byte[]{30,30,30}, SunOnly = true,},    // Kartana
            new Pokemon { Species = 799, Level = 70, UB = true, UBLocation = new []{694}, UBRate = new byte[]{80},},    // Guzzlord
            new Pokemon { Species = 800, Level = 75, UB = true, UBLocation = new []{548},UBRate = new byte[]{05},},    // Necrozma
            new Pokemon { Species = 722, Level = 05, NPC = 5, Delay = 40, Gift = true, Syncable = false, Template = true},    // Starters
            new Pokemon { Species = 142, Level = 40, NPC = 3, Delay = 34, Gift = true,},    // Aerodactyl
            new Pokemon { Species = 137, Level = 30, NPC = 4, Delay = 34, Gift = true,},    // Porygon
            new Pokemon { Species = 739, Level = 18, NPC = 1, Delay = 04, Wild = true,},    // Crabrawler
            new Pokemon { Species = 133, Level = 01, NPC = 5, Delay = 04, Gift = true, Syncable = false},    // Gift Eevee Egg
        };

        public readonly static byte[] Reorder1 = { 1, 2, 5, 3, 4 };    // In-game index to Normal index
        public readonly static byte[] Reorder2 = { 0, 1, 2, 4, 5, 3 }; // Normal index to In-Game index

        public static void NatureAdjustment(int[] stats, int nature)
        {
            byte inc = Reorder1[nature / 5];
            byte dec = Reorder1[nature % 5];
            if (inc == dec)
                return;
            stats[inc] = (int)(1.1 * stats[inc]);
            stats[dec] = (int)(0.9 * stats[dec]);
        }

        public static int getHiddenPowerValue(int[] IVs)
        {
            return 15 * IVs.Select((iv, i) => (iv & 1) << Reorder2[i]).Sum() / 63;
        }
    }
}

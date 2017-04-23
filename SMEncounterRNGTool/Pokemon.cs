using System.Linq;

namespace SMEncounterRNGTool
{
    class Pokemon
    {
        #region Property
        public short Species;
        public bool Template; // If it's a category
        public byte Form;
        public bool Wild;
        public bool Gift;
        public short Delay;
        public byte NPC;
        public bool UB;
        public int[] UBLocation;
        public byte Location;
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
        public bool IslandScan => IslandScanSpecies.Any(t => t.Species == Species);
        public bool AlwaysSync => Gift;
        public override string ToString()
        {
            switch (Species)
            {
                case 0: return "-";
                case 151: return StringItem.eventstr;
                case 155: return "<" + StringItem.islandscanstr + ">";
                case 408: return StringItem.fossilstr;
                case 722: return StringItem.starterstr;
                case 133: return $"{StringItem.species[133]} ({StringItem.species[0]})";
                case 718: return StringItem.species[718] + (Form == 2 ? "-10%" : "-50%");
                default: return StringItem.species[Species];
            }
        }
        #endregion

        public static Pokemon[] getVersionList(bool IsMoon) => SpeciesList.Where(pm => IsMoon ? pm.InMoon : pm.InSun).ToArray();

        #region table
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
            new Pokemon { Species = 800, Level = 75, UB = true, UBLocation = new []{548}, UBRate = new byte[]{05},},    // Necrozma
            new Pokemon { Species = 155, Location = 010, Template = true, Wild = true},    // Island Scan
            new Pokemon { Species = 722, Level = 05, NPC = 5, Delay = 40, Gift = true, Syncable = false, Template = true},    // Starters
            new Pokemon { Species = 142, Level = 40, NPC = 3, Delay = 34, Gift = true,},    // Aerodactyl
            new Pokemon { Species = 137, Level = 30, NPC = 4, Delay = 34, Gift = true,},    // Porygon
            new Pokemon { Species = 739, Level = 18, NPC = 1, Delay = 04, Wild = true,},    // Crabrawler
            new Pokemon { Species = 133, Level = 01, NPC = 5, Delay = 04, Gift = true, Syncable = false},    // Gift Eevee Egg
        };

        public readonly static Pokemon[] IslandScanSpecies =
        {
            // QR Scan: Su/M/Tu/W/Th/F/Sa
            // Melemele Island
            new Pokemon { Species = 155, Level = 12, Location = 010, }, // Cyndaquil @ Route 3
            new Pokemon { Species = 158, Level = 12, Location = 042, }, // Totodile @ Seaward Cave
            new Pokemon { Species = 633, Level = 13, Location = 034, }, // Deino @ Ten Carat Hill
            new Pokemon { Species = 116, Level = 18, Location = 014, }, // Horsea @ Kala'e Bay
            new Pokemon { Species = 599, Level = 08, Location = 021, }, // Klink @ Hau'oli City
            new Pokemon { Species = 152, Level = 10, Location = 012, }, // Chikorita @ Route 2
            new Pokemon { Species = 607, Level = 10, Location = 038, }, // Litwick @ Hau'oli Cemetery
                                                                                                                       
            // Akala Island                                                                                            
            new Pokemon { Species = 574, Level = 17, Location = 054, }, // Gothita @ Route 6
            new Pokemon { Species = 363, Level = 19, Location = 056, }, // Spheal @ Route 7
            new Pokemon { Species = 404, Level = 20, Location = 058, }, // Luxio @ Route 8
            new Pokemon { Species = 679, Level = 23, Location = 094, }, // Honedge @ Akala Outskirts
            new Pokemon { Species = 543, Level = 14, Location = 050, }, // Venipede @ Route 4
            new Pokemon { Species = 069, Level = 16, Location = 052, }, // Bellsprout @ Route 5
            new Pokemon { Species = 183, Level = 17, Location = 086, }, // Marill @ Brooklet Hill
                                                                                                                       
            // Ula'ula Island                                                                                          
            new Pokemon { Species = 111, Level = 30, Location = 138, }, // Rhyhorn @ Blush Mountain
            new Pokemon { Species = 220, Level = 31, Location = 114, }, // Swinub @ Tapu Village
            new Pokemon { Species = 578, Level = 33, Location = 118, }, // Duosion @ Route 16
            new Pokemon { Species = 315, Level = 34, Location = 128, }, // Roselia @ Ula'ula Meadow
            new Pokemon { Species = 397, Level = 27, Location = 106, }, // Staravia @ Route 10
            new Pokemon { Species = 288, Level = 27, Location = 108, }, // Vigoroth @ Route 11
            new Pokemon { Species = 610, Level = 28, Location = 136, }, // Axew @ Mount Hokulani
                                                                                                                       
            // Poni Island                                                                                             
            new Pokemon { Species = 604, Level = 55, Location = 164, }, // Eelektross @ Poni Grove
            new Pokemon { Species = 534, Level = 57, Location = 166, }, // Conkeldurr @ Poni Plains
            new Pokemon { Species = 468, Level = 59, Location = 170, }, // Togekiss @ Poni Gauntlet
            new Pokemon { Species = 542, Level = 57, Location = 156, }, // Leavanny @ Poni Meadow
            new Pokemon { Species = 497, Level = 43, Location = 184, }, // Serperior @ Exeggutor Island
            new Pokemon { Species = 503, Level = 43, Location = 158, }, // Samurott @ Poni Wilds
            new Pokemon { Species = 500, Level = 43, Location = 160, }, // Emboar @ Ancient Poni Path
        };
        #endregion

        #region formula
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

        public static int[] getStats(int[] IVs, int Nature, int Lv, int[] BS)
        {
            int[] Stats = new int[6];
            Stats[0] = (((BS[0] * 2 + IVs[0]) * Lv) / 100) + Lv + 10;
            for (int i = 1; i < 6; i++)
                Stats[i] = (((BS[i] * 2 + IVs[i]) * Lv) / 100) + 5;
            NatureAdjustment(Stats, Nature);
            return Stats;
        }
        #endregion
    }
}

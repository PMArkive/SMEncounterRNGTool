namespace SMEncounterRNGTool
{
    class Pokemon
    {
        public readonly static int[] SpecForm =
        {
            151,//Event
            785,//Tapu Koko
            786,//Tapu Lele
            787,//Tapu Bulu
            788,//Tapu Fini
            791,//Solgaleo
            792,//Lunala
            789,//Cosmog
            772,//Type:Null
            801,//Magearna
            718 + (2 << 11),//Zygarde-10%
            718 + (3 << 11),//Zygarde-50%
            142,//Aerodactyl
            137,//Porygon
            142,//Fossil 
            739,//Crabrawler
            793,//Nihilego
            794,//Buzzwole
            795,//Pheromosa
            796,//Xurkitree
            797,//Celesteela
            798,//Kartana
            799,//Guzzlord
            800,//Necrozma
        };

        public const int Solgaleo_index = 6;
        public const int Lunala_index = 7;
        public const int TypeNull_index = 9;
        public const int Zygarde_index = 11;
        public const int Fossil_index = 15;
        public const int UB_StartIndex = 17;
        public const int AlwaysSync_Index = 8;
        public static bool ShinyLocked(int index)
        {
            if (index == 0)
                return false;
            if (index < TypeNull_index)
                return true;
            if (index > TypeNull_index && index < Fossil_index - 2)
                return true;
            return false;
        }

        public readonly static byte[] PokeLevel =
        {
            100,
            60,60,60,60,
            55,55,5,
            40,50,50,50,
            40,30,15, //Stationary
            0,
            55,65,60,65,65,60,70,75        //UB
        };

        public readonly static int[] NPC =
        {
            4,
            0,0,0,1,//Tapus
            6,6,3,
            8,6,3,3,
            3,4,1, //Stationary
            1,
        };

        public readonly static int[] timedelay =
        {
            0,
            0,0,0,0,
            288,282,34,
            34,34,32,32,
            34,34,40,
            4,
        };

        public readonly static int[][] UB_rate =
        {
            new []{80,30},
            new []{30},
            new []{50},
            new []{15,30}, //todo
            new []{30,30}, //todo
            new []{30},
            new []{80},
            new []{5},
        };

        public readonly static int[][] UBLocation =
        {
            new []{100,082},
            new []{040},
            new []{046},
            new []{346,076},
            new []{134,124},
            new []{134},
            new []{694},
            new []{548},
        };

        public readonly static byte[] Reorder = { 1, 2, 5, 3, 4 };

        public static void NatureAdjustment(int[] stats, int nature)
        {
            int inc = Reorder[nature / 5];
            int dec = Reorder[nature % 5];
            if (inc == dec)
                return;
            stats[inc] = (int)(1.1 * stats[inc]);
            stats[dec] = (int)(0.9 * stats[dec]);
        }
    }
}

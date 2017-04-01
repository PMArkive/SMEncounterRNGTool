namespace SMEncounterRNGTool
{

    class SearchSetting
    {
        #region pokedex
        public readonly static int[,] pokedex =
        {
            { 151, 0 },//Event
            { 785, 0 },//Tapu Koko
            { 786, 0 },//Tapu Lele
            { 787, 0 },//Tapu Bulu
            { 788, 0 },//Tapu Fini
            { 791, 0 },//Solgaleo
            { 792, 0 },//Lunala
            { 789, 0 },//Cosmog
            { 772, 0 },//Type:Null
            { 801, 0 },//Magearna
            { 718, 2 },//Zygarde-10%
            { 718, 3 },//Zygarde-50%
            { 142, 0 },//Aerodactyl
            { 137, 0 },//Porygon
            { 142, 0 },//Fossil 
            { 739, 0 },//Crabrawler
            { 793, 0 },//Nihilego
            { 794, 0 },//Buzzwole
            { 795, 0 },//Pheromosa
            { 796, 0 },//Xurkitree
            { 797, 0 },//Celesteela
            { 798, 0 },//Kartana
            { 799, 0 },//Guzzlord
            { 800, 0 },//Necrozma
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
            0,0,0,0,0,0,0,0        //UB
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

        public readonly static int[] UB_rate =
        {
            80,30,50,15,30,30,80,5
        };

        public readonly static int[] honeycorrection =
        {
            13,5,5,7,26,26,1,1,
        };

        public readonly static byte[][] UBLocation =
        {
            new byte[]{100 ,082},
            new byte[]{040},
            new byte[]{046},
            new byte[]{090,076},
            new byte[]{134,124},
            new byte[]{134,120},
            new byte[]{182},
            new byte[]{036},
        };

        #endregion
        #region calc_data
        public readonly static double[,] natures_mag =
        {
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1.1, 0.9, 1, 1, 1 },
            { 1, 1.1, 1, 1, 1, 0.9 },
            { 1, 1.1, 1, 0.9, 1, 1 },
            { 1, 1.1, 1, 1, 0.9, 1 },
            { 1, 0.9, 1.1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1.1, 1, 1, 0.9 },
            { 1, 1, 1.1, 0.9, 1, 1 },
            { 1, 1, 1.1, 1, 0.9, 1 },
            { 1, 0.9, 1,1, 1, 1.1 },
            { 1, 1, 0.9, 1,1, 1.1 },
            { 1, 1,1, 1, 1, 1 },
            { 1, 1,1, 0.9, 1, 1.1 },
            { 1, 1,1, 1, 0.9, 1.1 },
            { 1, 0.9, 1, 1.1, 1,1 },
            { 1, 1, 0.9, 1.1, 1, 1 },
            { 1, 1, 1, 1.1, 1, 0.9 },
            { 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1.1, 0.9, 1 },
            { 1, 0.9, 1,1, 1.1, 1 },
            { 1, 1, 0.9, 1, 1.1, 1},
            { 1, 1, 1, 1, 1.1, 0.9 },
            { 1, 1, 1, 0.9, 1.1, 1 },
            { 1, 1, 1, 1, 1, 1}
        };
        #endregion
        #region String Setting
        public static string[] naturestr =
        {
            "勤奋", "怕寂寞", "勇敢", "固执",
            "顽皮", "大胆", "坦率", "悠闲", "淘气",
            "乐天", "胆小", "急躁", "认真", "爽朗",
            "天真", "内敛", "慢吞吞", "冷静", "害羞",
            "马虎", "温和", "温顺",
            "自大", "慎重", "浮躁"
            };
        public static string[] hpstr =
        {
            "一般",
            "格斗", "飞行", "毒", "地面", "岩石",
            "虫", "幽灵", "钢", "火", "水",
            "草", "电", "超能力", "冰", "龙",
            "恶","妖精"
            };

        public static string[] genderstr = { "-", "♂", "♀" };
        public static string[] abilitystr = { "-", "1", "2", "H" };
        #endregion

        public int Nature = -1;
        public int HPType = -1;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, BS, Stats;
        public bool Skip;
        public int Lv;
        public bool[] Slot;


        public bool CheckIVs(RNGSearch.RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > result.IVs[i] || result.IVs[i] > IVup[i])
                    return false;
            return true;
        }

        public bool CheckStats(RNGSearch.RNGResult result)
        {
            for (int i = 0; i < 6; i++)
                if (Stats[i] != 0 && Stats[i] != result.Stats[i])
                    return false;
            return true;
        }

        public void getStats(RNGSearch.RNGResult result)
        {
            int[] IV = new int[6];
            result.Stats = new int[6];
            for (int i = 0; i < 6; i++)
                IV[i] = result.IVs[i];

            result.Stats[0] = (((BS[0] * 2 + IV[0]) * Lv) / 100) + Lv + 10;
            for (int i = 1; i < 6; i++)
                result.Stats[i] = (int)(((((BS[i] * 2 + IV[i]) * Lv) / 100) + 5) * natures_mag[result.Nature, i]);
        }

        public bool CheckHiddenPower(int[] IV)
        {
            if (HPType == -1)
                return true;
            var val = 15 * ((IV[0] & 1) + 2 * (IV[1] & 1) + 4 * (IV[2] & 1) + 8 * (IV[5] & 1) + 16 * (IV[3] & 1) + 32 * (IV[4] & 1)) / 63;
            return val == HPType;
        }

        public static bool[] TranslateSlot(string slottext)
        {
            bool[] SlotArray = new bool[11];
            if (slottext == "")
                return SlotArray;
            try
            {
                string[] slotstrarray = slottext.Split(' ', ',');
                uint tmp;
                for (int i = 0; i < slotstrarray.Length; i++)
                {
                    uint.TryParse(slotstrarray[i], out tmp);
                    if ((tmp < 11) && (tmp > 0))
                        SlotArray[tmp] = true;
                }
                SlotArray[0] = true; //Text is valid
            }
            catch
            { }
            return SlotArray;
        }
    }
}

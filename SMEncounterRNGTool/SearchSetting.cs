namespace SMEncounterRNGTool
{

    class SearchSetting
    {
        #region pokedex
        public static int[,] pokedex =
        {
            { 785, 70, 115, 85, 95, 75, 130},   //Tapu Koko
            { 786, 70, 85, 75, 130, 115, 95},   //Tapu Lele
            { 787, 70, 130, 115, 85, 95, 75},   //Tapu Bulu
            { 788, 70, 75, 115, 95, 130, 85},   //Tapu Fini
            { 791, 137, 137, 107, 113, 89, 97}, //Solgaleo
            { 792, 137, 113, 89, 137, 107, 97}, //Lunala
            { 789, 43, 29, 31, 29, 31, 37},     //Cosmog
            { 772, 95, 95, 95, 95, 95, 59},     //Type:Null
            { 801, 80, 95, 115, 130, 115, 65},  //Magearna
            { 718, 54, 100, 71, 61, 85, 115},   //Zygarde-10%
            { 718, 108, 100, 121, 81, 95, 95},  //Zygarde-50%
            { 142, 80, 105, 65, 60, 75, 130},   //Aerodactyl
            { 137, 65, 60, 70, 85, 75, 40},     //Porygon
            { 793, 109, 53, 47, 127, 131, 103}, //Nihilego
            { 794, 107, 139, 139, 53, 53, 79 }, //Buzzwole
            { 795, 71, 137, 37, 137, 37, 151 }, //Pheromosa
            { 796, 83, 89, 71, 173, 71, 83 },   //Xurkitree
            { 797, 97, 101, 103, 107, 101, 61 },//Celesteela
            { 798, 59, 181, 131, 59, 31, 109 }, //Kartana
            { 799, 223, 101, 53, 97, 53, 43 },  //Guzzlord
            { 800, 97, 107, 101, 127, 89, 79 }, //Necrozma
        };

        public static int[] PokeLevel =
        {
            60,60,60,60,
            55,55,5,
            40,50,50,50,
            40,30, //Stationary
            55,65,60,65,65,60,70,75        //UB
        };

        public static int[] NPC =
        {
            0,0,0,1,//Tapus
            2,6,3,
            8,6,3,3,
            3,4, //Stationary
            0,0,0,0,0,0,0,0        //UB
        };

        public static int[] honeycorrection =
        {
            13,5,5,7,26,26,1,1,
        };

        #endregion
        #region calc_data
        public double[,] natures_mag =
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
        #endregion

        public int Nature = -1;
        public int HPType = -1;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, BS, Status, p_Status;
        public bool Skip;
        public int Lv;
        public bool[] Slot;


        public bool validIVs(int[] IV)
        {
            for (int i = 0; i < 6; i++)
                if (IVlow[i] > IV[i] || IV[i] > IVup[i])
                    return false;
            return true;
        }

        public bool validStatus(RNGSearch.RNGResult result, SearchSetting setting)
        {
            for (int i = 0; i < 6; i++)
                if (setting.Status[i] != 0 && setting.Status[i] != p_Status[i])
                    return false;
            return true;
        }

        public void getStatus(RNGSearch.RNGResult result, SearchSetting setting)
        {
            setting.p_Status = new int[6];
            int[] IV = new int[6];

            for (int i = 0; i < 6; i++)
                IV[i] = result.IVs[i];

            p_Status[0] = (((BS[0] * 2 + IV[0]) * Lv) / 100) + Lv + 10;
            for (int i = 1; i < 6; i++)
                p_Status[i] = (int)(((((BS[i] * 2 + IV[i]) * Lv) / 100) + 5) * natures_mag[result.Nature, i]);

            result.p_Status = setting.p_Status;

            return;
        }

        public bool mezapa_check(int[] IV)
        {
            if (HPType == -1)
                return true;
            var val = 15 * ((IV[0] & 1) + 2 * (IV[1] & 1) + 4 * (IV[2] & 1) + 8 * (IV[5] & 1) + 16 * (IV[3] & 1) + 32 * (IV[4] & 1)) / 63;
            return val == HPType;
        }

        public static bool[] TranslateSlot(string slottext)
        {
            bool[] SlotArray = new bool[11];
            if (slottext=="")
                return SlotArray;
            try
            {
                string[] slotstrarray=slottext.Split(' ',',');
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

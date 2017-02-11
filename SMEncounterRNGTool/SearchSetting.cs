using System;

namespace SMEncounterRNGTool
{

    class SearchSetting
    {
        #region pokedex
        public static string[,] pokedex =
        {
            { "卡璞・鸣鸣", "70", "115", "85", "95", "75", "130"},
            { "卡璞・蝶蝶", "70", "85", "75", "130", "115", "95"},
            { "卡璞・哞哞", "70", "130", "115", "85", "95", "75"},
            { "卡璞・鳍鳍", "70", "75", "115", "95", "130", "85"},
            { "索尔迦雷欧", "137", "137", "107", "113", "89", "97"},
            { "露奈雅拉", "137", "113", "89", "137", "107", "97"},
            { "属性：空", "95", "95", "95", "95", "95", "59"},
            { "玛机雅娜", "80", "95", "115", "130", "115", "65"},
            { "基格尔德-10%", "54", "100", "71", "61", "85", "115"},
            { "基格尔德-50%", "108", "100", "121", "81", "95", "95"},
            { "虚吾伊德", "109", "53", "47", "127", "131", "103"},
            { "爆肌蚊", "107", "139", "139", "53", "53", "79" },
            { "费洛美螂", "71", "137", "37", "137", "37", "151" },
            { "电束木", "83", "89", "71", "173", "71", "83" },
            { "铁火辉夜", "97", "101", "103", "107", "101", "61" },
            { "纸御剑", "59", "181", "131", "59", "31", "109" },
            { "恶食大王", "223", "101", "53", "97", "53", "43" },
            { "奈克洛兹玛", "97", "107", "101", "127", "89", "79" },
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
            "-",
            "格斗", "飞行", "毒", "地面", "岩石",
            "虫", "幽灵", "钢", "火", "水",
            "草", "电", "超能力", "冰", "龙",
            "恶",
            };

        public static string[] genderstr = { "-", "♂", "♀" };
        #endregion

        public int Nature = -1;
        public int HPType = -1;
        public int Ability = -1;
        public int Gender = -1;
        public int[] IVup, IVlow, BS,Status, p_Status;
        public bool Skip;
        public int Lv;
        public int Slot;


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
            {
                IV[i] = result.IVs[i];
            }

            p_Status[0] = (int)(((BS[0] * 2 + IV[0]) * Lv) / 100) + Lv + 10;
            for (int i = 1; i < 6; i++)
                 p_Status[i] = (int)(((int)(((BS[i] * 2 + IV[i]) * Lv) / 100) + 5) * natures_mag[result.Nature, i]);

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
    }
}

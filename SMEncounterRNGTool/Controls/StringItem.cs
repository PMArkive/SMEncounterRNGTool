using System.Linq;
using SMEncounterRNGTool.Controls;

namespace SMEncounterRNGTool
{
    class StringItem
    {
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
        public static string[] location, species;

        public static string getlocationstr(int locationidx)
        {
            int loc = locationidx & 0xFF;
            int idx = locationidx >> 8;
            string append = LocationTable.Table.FirstOrDefault(t => t.Location == loc && t.idx == idx).mark;
            return location[loc] + append;
        }

        public static ComboItem[] NatureList()
        {
            return naturestr.Select((str, i) => new ComboItem(str, i)).ToArray();
        }
        public static ComboItem[] HiddenPowerList()
        {
            return hpstr.Skip(1).Take(16).Select((str, i) => new ComboItem(str, i)).ToArray();
        }
    }
}
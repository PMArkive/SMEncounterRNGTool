using System.Linq;
using SMEncounterRNGTool.Controls;

namespace SMEncounterRNGTool
{
    class StringItem
    {
        public static string[] naturestr = new bool[25].Select(i => "").ToArray();
        public static string[] hpstr = new bool[18].Select(i => "").ToArray();

        public static string[] genderstr = { "-", "♂", "♀" };
        public static string[] abilitystr = { "-", "1", "2", "H" };
        public static string[] eventabilitystr = { "1/2", "1/2/H" };
        public static string[] location, species;
        public static string eventstr, fossilstr, starterstr, islandscanstr;

        public static string getlocationstr(int locationidx)
            => location[locationidx & 0xFF] + LocationTable.Table.FirstOrDefault(t => t.Locationidx == locationidx).mark;

        public static ComboItem[] NatureList
            => naturestr.Select((str, i) => new ComboItem(str, i)).ToArray();

        public static ComboItem[] HiddenPowerList
            => hpstr.Skip(1).Take(16).Select((str, i) => new ComboItem(str, i)).ToArray();

        public static ComboItem[] GenderRatioList =
        {
            new ComboItem("Genderless", 255),
            new ComboItem("♂1：♀1", 127),
            new ComboItem("♂7：♀1", 031),
            new ComboItem("♂3：♀1", 063),
            new ComboItem("♂1：♀3", 191),
            new ComboItem("♂ Only", 000),
            new ComboItem("♀ Only", 254),
        };
    }
}
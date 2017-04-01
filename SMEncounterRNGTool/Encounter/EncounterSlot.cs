namespace PKHeX.Core
{
    public class EncounterSlot
    {
        public int Species { get; set; }
        public int Form;
        public byte LevelMin;
        public byte LevelMax;
        public SlotType Type = SlotType.Any;
        public int SlotNumber;
        public EncounterSlot() { }
        public virtual EncounterSlot Clone()
        {
            return new EncounterSlot
            {
                Species = Species,
                LevelMax = LevelMax,
                LevelMin = LevelMin,
                Type = Type,
                SlotNumber = SlotNumber,
            };
        }
    }
}

namespace SMEncounterRNGTool
{
    class LocationTable
    {
        public class DataPoint
        {
            public byte Location, NPC, Correction = 1;
        }

        public readonly static DataPoint[] Table =
        {
            new DataPoint { Location = 006, NPC = 00, Correction = 15 },
            new DataPoint { Location = 008, NPC = 00, Correction = 23 },
        };
    }
}

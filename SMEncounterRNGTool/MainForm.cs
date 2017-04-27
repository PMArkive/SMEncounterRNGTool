using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static PKHeX.Util;

namespace SMEncounterRNGTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        #region Controls Grouping
        private int[] IVup
        {
            get { return new[] { (int)ivmax0.Value, (int)ivmax1.Value, (int)ivmax2.Value, (int)ivmax3.Value, (int)ivmax4.Value, (int)ivmax5.Value, }; }
            set
            {
                if (value.Length < 6) return;
                ivmax0.Value = value[0]; ivmax1.Value = value[1]; ivmax2.Value = value[2];
                ivmax3.Value = value[3]; ivmax4.Value = value[4]; ivmax5.Value = value[5];
            }
        }
        private int[] IVlow
        {
            get { return new[] { (int)ivmin0.Value, (int)ivmin1.Value, (int)ivmin2.Value, (int)ivmin3.Value, (int)ivmin4.Value, (int)ivmin5.Value, }; }
            set
            {
                if (value.Length < 6) return;
                ivmin0.Value = value[0]; ivmin1.Value = value[1]; ivmin2.Value = value[2];
                ivmin3.Value = value[3]; ivmin4.Value = value[4]; ivmin5.Value = value[5];
            }
        }
        private int[] BS
        {
            get { return new[] { (int)BS_0.Value, (int)BS_1.Value, (int)BS_2.Value, (int)BS_3.Value, (int)BS_4.Value, (int)BS_5.Value, }; }
            set
            {
                if (value.Length < 6) return;
                BS_0.Value = value[0]; BS_1.Value = value[1]; BS_2.Value = value[2];
                BS_3.Value = value[3]; BS_4.Value = value[4]; BS_5.Value = value[5];
            }
        }
        private int[] Stats
        {
            get { return new[] { (int)Stat0.Value, (int)Stat1.Value, (int)Stat2.Value, (int)Stat3.Value, (int)Stat4.Value, (int)Stat5.Value, }; }
            set
            {
                if (value.Length < 6) return;
                Stat0.Value = value[0]; Stat1.Value = value[1]; Stat2.Value = value[2];
                Stat3.Value = value[3]; Stat4.Value = value[4]; Stat5.Value = value[5];
            }
        }
        private List<Controls.ComboItem> Locationlist = new List<Controls.ComboItem>();
        private List<DataGridViewRow> dgvrowlist = new List<DataGridViewRow>();
        private NumericUpDown[] EventIV { get { return new[] { EventIV0, EventIV1, EventIV2, EventIV3, EventIV4, EventIV5, }; } }
        private CheckBox[] EventIVLocked { get { return new[] { Event_IV_Fix0, Event_IV_Fix1, Event_IV_Fix2, Event_IV_Fix3, Event_IV_Fix4, Event_IV_Fix5, }; } }
        #endregion

        #region Global Variable
        private string version = "1.1.1";

        private EventRule e = new EventRule();
        private RNGSetting rng = new RNGSetting();
        private Filters setting = new Filters();
        private ModelStatus status = new ModelStatus();
        private static byte[] blinkflaglist;
        private static int[] locationlist;
        private EncounterArea ea = new EncounterArea();
        private bool IsMoon => GameVersion.SelectedIndex > 0;
        private bool IsNight => Night.Checked;
        private bool IsEvent => PK.IsEvent;
        private int[] slotspecies => ea.getSpecies(IsMoon, IsNight);
        private Pokemon[] PMDropdownlist;
        private Pokemon PK;

        private byte ModelNumber => (byte)(NPC.Value + 1);
        private bool ShowDelay => ConsiderDelay.Checked && !ShowResultsAfterDelay.Checked;
        #endregion

        #region Translation
        private string curlanguage;
        private static readonly string[] langlist = { "en", "cn" };
        private static readonly string[] ANY_STR = { "Any", "任意" };
        private static readonly string[] NONE_STR = { "None", "无" };
        private static readonly string[] NORESULT_STR = { "Not Found", "未找到" };
        private static readonly string[] NOSELECTION_STR = { "Please Select", "请选择" };
        private static readonly string[] SETTINGERROR_STR = { "Error at ", "出错啦0.0 发生在" };
        private static readonly string[] WAIT_STR = { "Please Wait...", "请稍后..." };
        private static readonly string[] EVENT_STR = { "Event", "配信" };
        private static readonly string[] FOSSIL_STR = { "Fossil", "化石" };
        private static readonly string[] STARTER_STR = { "Starters", "御三家" };
        private static readonly string[] ISLAND_STR = { "Island Scan", "岛屿搜索" };
        private static readonly string[] FILEERRORSTR = { "Invalid file!", "文件格式不正确" };
        private static readonly string[,] PIDTYPE_STR =
        {
            { "Random", "Nonshiny", "Shiny","Specified"},
            { "随机", "必不闪", "必闪","特定"}
        };
        private static readonly string[,] GAMEVERSION_STR =
        {
            { "Sun", "Moon" },
            { "太阳", "月亮" }
        };

        private int lindex { get { return Lang.SelectedIndex; } set { Lang.SelectedIndex = value; } }

        private void ChangeLanguage(object sender, EventArgs e)
        {
            string lang = langlist[lindex];

            if (lang == curlanguage)
                return;

            curlanguage = lang;
            TranslateInterface(this, curlanguage); // Translate the UI to language.
            Properties.Settings.Default.Language = curlanguage;
            Properties.Settings.Default.Save();
            TranslateInterface(this, lang);
            Text = Text + $" v{version} @wwwwwwzx";

            StringItem.naturestr = getStringList("Natures", curlanguage);
            StringItem.hpstr = getStringList("Types", curlanguage);
            StringItem.species = getStringList("Species", curlanguage);
            StringItem.location = getStringList("Location", curlanguage);
            StringItem.genderratio = getStringList("Genderratio",curlanguage);
            StringItem.eventstr = EVENT_STR[lindex];
            StringItem.fossilstr = FOSSIL_STR[lindex];
            StringItem.starterstr = STARTER_STR[lindex];
            StringItem.islandscanstr = ISLAND_STR[lindex];

            Nature.Items.Clear();
            Nature.BlankText = ANY_STR[lindex];
            Nature.Items.AddRange(StringItem.NatureList);

            HiddenPower.Items.Clear();
            HiddenPower.BlankText = ANY_STR[lindex];
            HiddenPower.Items.AddRange(StringItem.HiddenPowerList);
            
            GenderRatio.DisplayMember = "Text";
            GenderRatio.ValueMember = "Value";
            GenderRatio.DataSource = new BindingSource(StringItem.GenderRatioList, null);

            for (int i = 0; i < 2; i++)
                GameVersion.Items[i] = GAMEVERSION_STR[lindex, i];

            for (int i = 0; i < 4; i++)
                Event_PIDType.Items[i] = PIDTYPE_STR[lindex, i];

            SyncNature.Items[0] = NONE_STR[lindex];
            for (int i = 0; i < StringItem.naturestr.Length; i++)
                Event_Nature.Items[i + 1] = SyncNature.Items[i + 1] = StringItem.naturestr[i];

            RefreshPokemonDropdownlist();
            RefreshIslandPokemon();
            RefreshLocation();

            // display something upon loading
            Nature.CheckBoxItems[0].Checked = true;
            Nature.CheckBoxItems[0].Checked = false;
            HiddenPower.CheckBoxItems[0].Checked = true;
            HiddenPower.CheckBoxItems[0].Checked = false;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            DGV.Columns["dgv_Rand"].DefaultCellStyle.Font = new Font("Consolas", 9);
            DGV.Columns["dgv_PID"].DefaultCellStyle.Font = new Font("Consolas", 9);
            DGV.Columns["dgv_EC"].DefaultCellStyle.Font = new Font("Consolas", 9);
            Type dgvtype = typeof(DataGridView);
            System.Reflection.PropertyInfo dgvPropertyInfo = dgvtype.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            dgvPropertyInfo.SetValue(DGV, true, null);

            int LastSelectedPKM = Properties.Settings.Default.PKM;

            SyncNature.Items.Add("-");
            Event_Nature.Items.Add("-");
            SyncNature.Items.AddRange(StringItem.naturestr);
            Event_Nature.Items.AddRange(StringItem.naturestr);

            Gender.Items.AddRange(StringItem.genderstr);
            Event_Gender.Items.AddRange(StringItem.genderstr);

            for (int i = 0; i < 6; i++)
                EventIV[i].Enabled = false;

            string l = Properties.Settings.Default.Language;
            int lang = Array.IndexOf(langlist, l);
            if (lang < 0) lang = Array.IndexOf(langlist, "en");

            lindex = lang;
            ChangeLanguage(null, null);

            string TimeResultInstructText = "";
            switch (lang)
            {
                case 0: TimeResultInstructText = "Format: Time in Frame (Time in Second) <Target Frame Lifetime>"; break;
                case 1: TimeResultInstructText = "格式：帧数 (秒数) <持续时间>"; break;
            }
            TimeResult.Items.Add(TimeResultInstructText);

            SyncNature.SelectedIndex = 0;
            Event_Nature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            GenderRatio.SelectedIndex = 0;
            Event_Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Event_Ability.SelectedIndex = 0;
            Event_PIDType.SelectedIndex = 0;

            Slot.CheckBoxItems[0].Checked = true;
            Slot.CheckBoxItems[0].Checked = false;
            Seed.Value = Properties.Settings.Default.Seed;
            ShinyCharm.Checked = Properties.Settings.Default.ShinyCharm;
            TSV.Value = Properties.Settings.Default.TSV;
            Advanced.Checked = Properties.Settings.Default.Advance;
            GameVersion.SelectedIndex = Properties.Settings.Default.IsSun ? 0 : 1;
            Poke.SelectedValue = LastSelectedPKM;
            (Properties.Settings.Default.ClockInput ? StartClockInput : EndClockInput).Checked = true;

            ByIVs.Checked = true;
            BySaveScreen.Checked = true;

            SearchMethod_CheckedChanged(null, null);
            NPC_ValueChanged(null, null);
            CreateTimeline_CheckedChanged(null, null);
        }

        #region DataEntry
        private void RefreshLocation()
        {
            if (PK.IsBlank)
                locationlist = LocationTable.SMLocationList;
            else if (PK.UB)
                locationlist = PK.UBLocation;
            else if (PK.QR)
                locationlist = PK.Template ? LocationTable.QRLocationList : LocationTable.Table.Where(t => t.Location == PK.Location).Select(t => t.Locationidx).ToArray();
            else
                return;

            Locationlist = locationlist.Select(loc => new Controls.ComboItem(StringItem.getlocationstr(loc), loc)).ToList();
            MetLocation.DisplayMember = "Text";
            MetLocation.ValueMember = "Value";
            MetLocation.DataSource = new BindingSource(Locationlist, null);

            LoadSpecies();
        }

        private void RefreshPokemonDropdownlist()
        {
            int tmp = Poke.SelectedIndex;
            PMDropdownlist = Pokemon.getVersionList(IsMoon);
            var List = PMDropdownlist.Select(s => new Controls.ComboItem(s.ToString(), s.SpecForm));
            Poke.DisplayMember = "Text";
            Poke.ValueMember = "Value";
            Poke.DataSource = new BindingSource(List, null);
            Poke.SelectedIndex = 0 <= tmp && tmp < Poke.Items.Count ? tmp : 0;
        }

        private void RefreshIslandPokemon()
        {
            int tmp = Island_Poke.SelectedIndex;
            var List = Pokemon.QRScanSpecies.Select(s => new Controls.ComboItem(s.ToString(), s.SpecForm));
            Island_Poke.DisplayMember = "Text";
            Island_Poke.ValueMember = "Value";
            Island_Poke.DataSource = new BindingSource(List, null);
            Island_Poke.SelectedIndex = 0 <= tmp && tmp < Island_Poke.Items.Count ? tmp : 0;
        }

        private void LoadSpecies()
        {
            int tmp = SlotSpecies.SelectedIndex;
            var List = slotspecies.Skip(1).Distinct().Select(SpecForm => new Controls.ComboItem(StringItem.species[SpecForm & 0x7FF], SpecForm));
            List = new[] { new Controls.ComboItem("-", 0) }.Concat(List);
            SlotSpecies.DisplayMember = "Text";
            SlotSpecies.ValueMember = "Value";
            SlotSpecies.DataSource = new BindingSource(List, null);
            if (0 <= tmp && tmp < SlotSpecies.Items.Count)
                SlotSpecies.SelectedIndex = tmp;
        }

        private void LoadSlotSpeciesInfo()
        {
            int SpecForm = (int)SlotSpecies.SelectedValue;
            byte[] Slottype = EncounterArea.SlotType[slotspecies[0]];
            List<int> Slotidx = new List<int>();
            for (int i = Array.IndexOf(slotspecies, SpecForm); i > -1; i = Array.IndexOf(slotspecies, SpecForm, i + 1))
                Slotidx.Add(i);
            for (int i = 0; i < 10; i++)
                Slot.CheckBoxItems[i + 1].Checked = Slotidx.Contains(Slottype[i]);

            SetPersonalInfo(SpecForm);
        }

        private void SetPersonalInfo(int Species, int Form)
        {
            if (Species == 0)
                return;
            var t = PersonalTable.SM.getFormeEntry(Species, Form);
            BS = new[] { t.HP, t.ATK, t.DEF, t.SPA, t.SPD, t.SPE };
            if (UBOnly.Checked && !PK.QR)
                return;
            GenderRatio.SelectedValue = t.Gender;
            if (FuncUtil.IsRandomGender(t.Gender))
                Gender.Visible = L_gender.Visible = true;
            if (t.Abilities[0] != t.Abilities[1])
                Ability.Visible = L_Ability.Visible = true;
            Fix3v.Checked = t.EggGroups[0] == 0x0F; //Undiscovered Group
        }
        private void SetPersonalInfo(int SpecForm) => SetPersonalInfo(SpecForm & 0x7FF, SpecForm >> 11);

        private void Island_Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            PK = Pokemon.QRScanSpecies.FirstOrDefault(pk => pk.Species == (int)Island_Poke.SelectedValue);
            RefreshLocation();
            Filter_Lv.Value = PK.Level;
            SetPersonalInfo(PK.SpecForm);
        }

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            PK = PMDropdownlist.FirstOrDefault(pm => pm.SpecForm == (int)Poke.SelectedValue) ?? Pokemon.SpeciesList[0];
            //General
            Properties.Settings.Default.PKM = (int)Poke.SelectedValue;
            Properties.Settings.Default.Save();

            Stationary.Checked = !(Wild.Checked = PK.Wild || PK.UB);
            UB.Visible = UB.Checked = PK.UB || PK.QR;
            Method_CheckedChanged(null, null);
            RefreshLocation();
            //Enable
            SOS.Visible = Fishing.Visible = Stationary.Enabled = Wild.Enabled = PK.IsBlank;
            GenderRatio.Enabled = Fix3v.Enabled = PK.IsBlank || PK.IsEvent || PK.UB || PK.QR;
            L_EventInstruction.Visible = PK.IsEvent;
            IslandScanSetting.Visible = PK.QR;
            Honey.Enabled = Encounter_th.Enabled = !PK.IsCrabrawler;
            SyncNature.Enabled = PK.Syncable;
            AlwaysSynced.Checked = PK.AlwaysSync;
            if (!PK.Syncable)
                SyncNature.SelectedIndex = 0;
            ConsiderDelay.Checked = true;
            AlwaysSynced.Enabled = false;
            if (PK.IsBlank)
                return;
            MainRNGEgg.Visible = MainRNGEgg.Checked = false;
            if (PK.IsEvent)
            {
                L_Ability.Visible = L_gender.Visible = Gender.Visible = Ability.Visible = true;
                Event_CheckedChanged(null, null);
                return;
            }
            Filter_Lv.Value = PK.Level;
            SetPersonalInfo(PK.SpecForm);
            if (PK.UB || PK.QR)
            {
                //Island Scan and UB Text
                UB.Text = PK.QR ? StringItem.islandscanstr : "UB";
                dgv_ubvalue.HeaderText = PK.QR ? dgv_ubvalue.HeaderText.Replace("UB", "QR") : dgv_ubvalue.HeaderText.Replace("QR", "UB");
                UBOnly.Text = PK.QR ? UBOnly.Text.Replace("UB", "QR") : UBOnly.Text.Replace("QR", "UB");
                return;
            }
            if (PK.IsCrabrawler)
            {
                Honey.Checked = false;
                WildEncounterSetting.Visible = false;
                Encounter_th.Value = 101;
            }
            Timedelay.Value = PK.Delay;
            NPC.Value = PK.NPC;
        }
        #endregion

        #region UILogic
        private void TSV_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.TSV = (short)TSV.Value;
            Properties.Settings.Default.Save();
        }

        private void ShinyCharm_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShinyCharm = ShinyCharm.Checked;
            Properties.Settings.Default.Save();
        }

        private void Advanced_CheckedChanged(object sender, EventArgs e)
        {
            dgv_research.Visible = Advanced.Checked;
            Properties.Settings.Default.Advance = Advanced.Checked;
            Properties.Settings.Default.Save();
        }

        private void Seed_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Seed = Seed.Value;
            Properties.Settings.Default.Save();
        }

        private void GameVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPokemonDropdownlist();
            Properties.Settings.Default.IsSun = !IsMoon;
            Properties.Settings.Default.Save();
        }

        private void DayNight_CheckedChanged(object sender, EventArgs e)
        {
            if (ea.DayNightDifference)
                LoadSpecies();
        }

        private void Location_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PK.UB || PK.QR)
                Special_th.Value = PK.UB ? PK.UBRate[MetLocation.SelectedIndex] : 50;
            ea = LocationTable.Table.FirstOrDefault(t => t.Locationidx == (int)MetLocation.SelectedValue);
            NPC.Value = ea.NPC;
            Correction.Value = ea.Correction;
            RNGSetting.slottype = (byte)(ea.Locationidx == 1190 ? 1 : 0); // Poni Plains (4)

            Lv_min.Value = ea.SunMoonDifference && IsMoon ? ea.LevelMinMoon : ea.LevelMin;
            Lv_max.Value = ea.SunMoonDifference && IsMoon ? ea.LevelMaxMoon : ea.LevelMax;
            LoadSpecies();
        }

        private void SlotSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSlotSpeciesInfo();
            if (SlotSpecies.SelectedIndex > 0 && (Filter_Lv.Value > Lv_max.Value || Filter_Lv.Value < Lv_min.Value))
                Filter_Lv.Value = 0;
            if (PK.UB || PK.QR)
            {
                if (SlotSpecies.SelectedIndex == 0) Filter_Lv.Value = PK.Level;
                UBOnly.Checked = SlotSpecies.SelectedIndex == 0;
            }
        }

        private void Method_CheckedChanged(object sender, EventArgs e)
        {
            Honey.Checked = Wild.Checked;
            if (Stationary.Checked)
                SOS.Checked = Fishing.Checked = UB.Checked = false;
            else
                EncounteredOnly.Checked = true;

            if (PK.IsBlank)
            {
                AlwaysSynced.Enabled = AlwaysSynced.Checked = Stationary.Checked;
                SOS.Visible = Fishing.Visible = Wild.Checked;
                MainRNGEgg.Visible = Stationary.Checked;
                MainRNGEgg.Checked = false;
            }

            UB_CheckedChanged(null, null);
            ConsiderDelay_CheckedChanged(null, null);
            Honey_CheckedChanged(null, null);
            Reset_Click(null, null);
            WildEncounterSetting.Visible = Honey.Visible = Wild.Checked;
            dgv_slot.Visible = dgv_item.Visible = dgv_lv.Visible = Wild.Checked;
            label9.Visible = L_Lv.Visible = L_gender.Visible = L_Ability.Visible = L_Slot.Visible = Wild.Checked;
            Lv_min.Visible = Lv_max.Visible = Slot.Visible = Gender.Visible = Ability.Visible = Wild.Checked;
        }

        private void IVLocked_CheckedChanged(object sender, EventArgs e)
        {
            string str = ((CheckBox)sender).Name;
            int i = int.Parse(str.Remove(0, str.IndexOf("Fix") + 3));
            EventIV[i].Enabled = ((CheckBox)sender).Checked;
        }

        private void UB_CheckedChanged(object sender, EventArgs e)
        {
            dgv_ubvalue.Visible = UBOnly.Visible = UBOnly.Checked = L_UB_th.Visible = Special_th.Visible = UB.Checked;
        }

        private void Honey_CheckedChanged(object sender, EventArgs e)
        {
            dgv_encounter.Visible = L_Encounter_th.Visible = Encounter_th.Visible = EncounteredOnly.Visible = !Honey.Checked && Wild.Checked;
            if (Honey.Checked)
            {
                Timedelay.Value = 186;
                ConsiderDelay.Checked = true;
                SOS.Checked = Fishing.Checked = false;
                GenderRatio.SelectedIndex = 1; Fix3v.Checked = false;
            }
            else
            {
                EnterBagTime.Checked = false;
                Correction.Value = 1;
                if (Wild.Checked) Timedelay.Value = 0;
            }
            label10.Text = Honey.Checked ? "F" : "+4F   1F=1/60s";
            EnterBagTime.Visible = L_Correction.Visible = Correction.Visible = Honey.Checked;
            Timedelay.Enabled = ConsiderDelay.Enabled = !Honey.Checked;
        }

        private void Fishing_CheckedChanged(object sender, EventArgs e)
        {
            if (!Fishing.Checked) return;
            SOS.Checked = Honey.Checked = false;
            Encounter_th.Value = 100;
        }

        private void SOS_CheckedChanged(object sender, EventArgs e)
        {
            if (SOS.Checked)
                Fishing.Checked = Honey.Checked = false;
            SOSSetting.Visible = SOS.Checked;
        }

        private void ConsiderDelay_CheckedChanged(object sender, EventArgs e)
        {
            ShowResultsAfterDelay.Checked = ConsiderDelay.Checked;
            Timedelay.Enabled = ShowResultsAfterDelay.Enabled = HighLightFrameAfter.Enabled = ConsiderDelay.Checked;
            dgv_delay.Visible = ConsiderDelay.Checked;
        }

        private void UBOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (UBOnly.Checked)
                EncounteredOnly.Checked = true;
        }

        private void SyncNature_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AlwaysSynced.Checked)
            {
                Nature.ClearSelection();
                if (SyncNature.SelectedIndex > 0)
                    Nature.CheckBoxItems[SyncNature.SelectedIndex].Checked = true;
            }
        }

        private void SearchMethod_CheckedChanged(object sender, EventArgs e)
        {
            IVPanel.Visible = ByIVs.Checked;
            StatPanel.Visible = ByStats.Checked;
            ShowStats.Enabled = ShowStats.Checked = ByStats.Checked;
        }

        private void NPC_ValueChanged(object sender, EventArgs e)
        {
            var ControlON = NPC.Value == 0 ? BlinkOnly : SafeFOnly;
            var ControlOFF = NPC.Value == 0 ? SafeFOnly : BlinkOnly;
            ControlON.Visible = true;
            ControlOFF.Visible = ControlOFF.Checked = false;
            Refinement.Visible = Refinement.Checked = NPC.Value != 0;
        }

        private void CreateTimeline_CheckedChanged(object sender, EventArgs e)
        {
            dgv_time.Visible = CreateTimeline.Checked || Refinement.Checked;
            TimeSpan.Enabled = CreateTimeline.Checked;
            Refinement.Enabled = AroundTarget.Enabled = BlinkOnly.Enabled = SafeFOnly.Enabled = !CreateTimeline.Checked;
            ShowResultsAfterDelay.Enabled = !CreateTimeline.Checked && ConsiderDelay.Checked;
            Frame_max.Visible = label7.Visible = !CreateTimeline.Checked;
            L_StartingPoint.Visible = CreateTimeline.Checked;
            if (CreateTimeline.Checked)
                Refinement.Checked = AroundTarget.Checked = BlinkOnly.Checked = SafeFOnly.Checked = false;
            ShowResultsAfterDelay.Checked = ConsiderDelay.Checked;
        }

        private void Modification_CheckedChanged(object sender, EventArgs e)
        {
            dgv_time.Visible = CreateTimeline.Checked || Refinement.Checked;
            if (Refinement.Checked)
                CreateTimeline.Checked = false;
        }

        private void Fix3v_CheckedChanged(object sender, EventArgs e)
        {
            Event_CheckedChanged(null, null);
            PerfectIVs.Value = Fix3v.Checked ? 3 : 0;
        }

        private void MainRNGEgg_CheckedChanged(object sender, EventArgs e)
        {
            dgv_H.Visible = dgv_A.Visible = dgv_B.Visible = dgv_C.Visible = dgv_D.Visible = dgv_S.Visible = !MainRNGEgg.Checked;
            dgv_ability.Visible = dgv_EC.Visible = dgv_gender.Visible = dgv_hiddenpower.Visible = dgv_nature.Visible = dgv_synced.Visible = !MainRNGEgg.Checked;
            if (MainRNGEgg.Checked)
            {
                NPC.Value = 4;
                Timedelay.Value = 38;
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            if (!AlwaysSynced.Checked) Nature.ClearSelection();
            HiddenPower.ClearSelection();
            Slot.ClearSelection();
            Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;

            if (ByIVs.Checked && Wild.Checked && (!UB.Checked || Filter_Lv.Value <= Lv_max.Value) || PK.IsBlank)
                Filter_Lv.Value = 0;

            Stats = new int[6];
            IVlow = new int[6];
            IVup = new[] { 31, 31, 31, 31, 31, 31 };
        }
        #endregion
    }
}

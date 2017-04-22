using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.IO;
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
        private List<NumericUpDown> EventIV = new List<NumericUpDown>();
        private List<CheckBox> EventIVLocked = new List<CheckBox>();
        private List<Controls.ComboItem> Locationlist = new List<Controls.ComboItem>();
        private List<DataGridViewRow> dgvrowlist = new List<DataGridViewRow>();
        #endregion

        #region Global Variable
        private string version = "1.1.1";

        private EventRule e = new EventRule();
        private RNGSetting rng = new RNGSetting();
        private Filters setting = new Filters();
        private ModelStatus status = new ModelStatus();
        private static byte[] blinkflaglist;
        private static int[] locationlist = LocationTable.SMLocationList;
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
        private static readonly string[] EVENT_STR = { "<Event>", "<配信>" };
        private static readonly string[] FOSSIL_STR = { "<Fossil>", "<化石>" };
        private static readonly string[] STARTER_STR = { "<Starter>", "<御三家>" };
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
            StringItem.eventstr = EVENT_STR[lindex];
            StringItem.fossilstr = FOSSIL_STR[lindex];
            StringItem.starterstr = STARTER_STR[lindex];

            Nature.Items.Clear();
            Nature.BlankText = ANY_STR[lindex];
            Nature.Items.AddRange(StringItem.NatureList);

            HiddenPower.Items.Clear();
            HiddenPower.BlankText = ANY_STR[lindex];
            HiddenPower.Items.AddRange(StringItem.HiddenPowerList);

            for (int i = 0; i < 2; i++)
                GameVersion.Items[i] = GAMEVERSION_STR[lindex, i];

            for (int i = 0; i < 4; i++)
                Event_PIDType.Items[i] = PIDTYPE_STR[lindex, i];

            SyncNature.Items[0] = NONE_STR[lindex];
            for (int i = 0; i < StringItem.naturestr.Length; i++)
                Event_Nature.Items[i + 1] = SyncNature.Items[i + 1] = StringItem.naturestr[i];

            RefeshPokemonDropdownlist();

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

            EventIV.Add(EventIV0);
            EventIV.Add(EventIV1);
            EventIV.Add(EventIV2);
            EventIV.Add(EventIV3);
            EventIV.Add(EventIV4);
            EventIV.Add(EventIV5);

            EventIVLocked.Add(Event_IV_Fix0);
            EventIVLocked.Add(Event_IV_Fix1);
            EventIVLocked.Add(Event_IV_Fix2);
            EventIVLocked.Add(Event_IV_Fix3);
            EventIVLocked.Add(Event_IV_Fix4);
            EventIVLocked.Add(Event_IV_Fix5);

            SyncNature.Items.Add("-");
            Event_Nature.Items.Add("-");
            SyncNature.Items.AddRange(StringItem.naturestr);
            Event_Nature.Items.AddRange(StringItem.naturestr);

            GenderRatio.DisplayMember = "Text";
            GenderRatio.ValueMember = "Value";
            GenderRatio.DataSource = new BindingSource(StringItem.GenderRatioList, null);

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
            Poke.SelectedValue = Properties.Settings.Default.PKM;
            GameVersion.SelectedIndex = Properties.Settings.Default.IsSun ? 0 : 1;
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
            if (!PK.UB && !PK.IsBlank)
                return;
            locationlist = PK.IsBlank ? LocationTable.SMLocationList : PK.UBLocation;

            Locationlist = locationlist.Select(loc => new Controls.ComboItem(StringItem.getlocationstr(loc), loc)).ToList();
            MetLocation.DisplayMember = "Text";
            MetLocation.ValueMember = "Value";
            MetLocation.DataSource = new BindingSource(Locationlist, null);

            LoadSpecies();
        }

        private void RefeshPokemonDropdownlist()
        {
            int tmp = Poke.SelectedIndex;
            PMDropdownlist = Pokemon.getVersionList(IsMoon);
            var List = PMDropdownlist.Select(s => new Controls.ComboItem(s.ToString(), s.SpecForm));
            Poke.DisplayMember = "Text";
            Poke.ValueMember = "Value";
            Poke.DataSource = new BindingSource(List, null);
            Poke.SelectedIndex = 0 <= tmp && tmp < Poke.Items.Count ? tmp : 0;
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
            if (UBOnly.Checked)
                return;
            GenderRatio.SelectedValue = t.Gender;
            if (FuncUtil.IsRandomGender(t.Gender))
                Gender.Visible = L_gender.Visible = true;
            if (t.Abilities[0] != t.Abilities[1])
                Ability.Visible = L_Ability.Visible = true;
            Fix3v.Checked = t.EggGroups[0] == 0x0F; //Undiscovered Group
        }
        private void SetPersonalInfo(int SpecForm) => SetPersonalInfo(SpecForm & 0x7FF, SpecForm >> 11);
        #endregion

        #region SearchSeedfunction
        private void Clear_Click(object sender, EventArgs e)
        {
            ((QRInput.Checked) ? QRList : Clock_List).Text = "";
        }

        private void Back_Click(object sender, EventArgs e)
        {
            TextBox tmp = QRInput.Checked ? QRList : Clock_List;
            string str = tmp.Text;
            if (tmp.Text != "")
            {
                if (str.LastIndexOf(",") != -1)
                    str = str.Remove(str.LastIndexOf(","));
                else
                    str = "";
            }
            tmp.Text = str;
        }

        private void Get_Clock_Number(object sender, EventArgs e)
        {
            TextBox tmp = QRInput.Checked ? QRList : Clock_List;
            string str = ((Button)sender).Name;
            string n = str.Remove(0, str.IndexOf("button") + 6);

            if (tmp.Text != "") tmp.Text += ",";
            tmp.Text += !EndClockInput.Checked || QRInput.Checked ? n : ((Convert.ToInt32(n) + 13) % 17).ToString();

            if (QRInput.Checked)
            {
                if (QRList.Text.Count(c => c == ',') < 3)
                    return;
                QRSearch_Click(null, null);
            }
            else
                SearchforSeed(null, null);
        }

        private void SearchforSeed(object sender, EventArgs e)
        {
            if (Clock_List.Text.Count(c => c == ',') < 7)
            {
                SeedResults.Text = "";
                return;
            }
            var text = "";
            try
            {
                SeedResults.Text = WAIT_STR[lindex];
                var results = SFMTSeedAPI.request(Clock_List.Text);
                if (results == null || results.Count() == 0)
                    text = NORESULT_STR[lindex];
                else
                {
                    text = string.Join(" ", results.Select(r => r.seed));
                    if (results.Count() == 1)
                    {
                        Time_min.Value = 418 + Clock_List.Text.Count(c => c == ',');
                        uint s0;
                        if (uint.TryParse(text, NumberStyles.HexNumber, null, out s0))
                            Seed.Value = s0;
                    }
                }
            }
            catch (Exception exc)
            {
                text = exc.Message;
            }
            finally
            {
                SeedResults.Text = text;
            }
        }

        private void QRSearch_Click(object sender, EventArgs e)
        {
            uint InitialSeed = (uint)Seed.Value;
            int min = (int)Frame_min.Value;
            int max = (int)Frame_max.Value;
            if (QRList.Text == "")
                return;
            string[] str = QRList.Text.Split(',');
            try
            {
                int[] Clock_List = str.Select(s => int.Parse(s)).ToArray();
                int[] temp_List = new int[Clock_List.Length];

                SFMT sfmt = new SFMT(InitialSeed);
                SFMT seed = new SFMT();
                bool flag = false;

                QRResult.Items.Clear();

                for (int i = 0; i < min; i++)
                    sfmt.NextUInt64();

                int cnt = 0;
                int tmp = 0;
                for (int i = min; i <= max; i++, sfmt.NextUInt64())
                {
                    seed = (SFMT)sfmt.DeepCopy();

                    for (int j = 0; j < Clock_List.Length; j++)
                        temp_List[j] = (int)(seed.NextUInt64() % 17);

                    if (temp_List.SequenceEqual(Clock_List))
                        flag = true;

                    if (flag)
                    {
                        flag = false;
                        switch (lindex)
                        {
                            case 0: QRResult.Items.Add($"The last clock is at {i + Clock_List.Length - 1}F, you're at {i + Clock_List.Length + 1}F after quiting QR"); break;
                            case 1: QRResult.Items.Add($"最后的指针在 {i + Clock_List.Length - 1} 帧，退出QR后在 {i + Clock_List.Length + 1} 帧"); break;
                        }
                        cnt++;
                        tmp = i + Clock_List.Length + 1;
                    }
                }

                if (cnt == 1)
                    Time_min.Value = tmp;
            }
            catch
            {
                Error(SETTINGERROR_STR[lindex] + L_QRList.Text);
            }
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

        private void ClockInput_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ClockInput = StartClockInput.Checked;
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
            RefeshPokemonDropdownlist();
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
            if (PK.UB && MetLocation.SelectedIndex >= 0)
                UB_th.Value = PK.UBRate[MetLocation.SelectedIndex];
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
            if (SlotSpecies.SelectedIndex == 0 && PK.UB)
                Filter_Lv.Value = PK.Level;
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
            dgv_ubvalue.Visible = UBOnly.Visible = UBOnly.Checked =  L_UB_th.Visible = UB_th.Visible = UB.Checked;
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

        private void Event_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent)
            {
                Timedelay.Value = YourID.Checked && !IsEgg.Checked ? 62 : 0;
                IVsCount.Value = Fix3v.Checked ? 3 : 0;
            }
        }

        private void Fix3v_CheckedChanged(object sender, EventArgs e)
        {
            Event_CheckedChanged(null, null);
            PerfectIVs.Value = Fix3v.Checked ? 3 : 0;
        }

        private void OtherInfo_CheckedChanged(object sender, EventArgs e)
        {
            L_Event_TSV.Visible = L_Event_G7TID.Visible = Event_EC.Enabled = Event_PID.Enabled = Event_TID.Enabled = Event_SID.Enabled = OtherInfo.Checked;
        }

        private void Event_PIDType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!OtherInfo.Checked)
                Event_EC.Value = (Event_PIDType.SelectedIndex == 3) ? 0x12 : 0;
            L_EC.Visible = Event_EC.Visible = L_PID.Visible = Event_PID.Visible = Event_PIDType.SelectedIndex == 3;
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
        #endregion

        #region TimerCalculateFunction

        private int[] CalcFrame(int min, int max)
        {
            uint InitialSeed = (uint)Seed.Value;
            SFMT sfmt = new SFMT(InitialSeed);

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            //total_frame[0] Start; total_frame[1] Duration
            int[] total_frame = new int[2];
            int n_count = 0;
            int timer = 0;
            status = new ModelStatus(ModelNumber, sfmt);

            while (min + n_count <= max)
            {
                n_count += status.NextState();
                total_frame[timer]++;
                if (min + n_count == max)
                    timer = 1;
            }
            return total_frame;
        }

        private void CalcTime_Output(int min, int max)
        {
            int[] totaltime = CalcFrame(min, max);
            double realtime = totaltime[0] / 30.0;
            string str = $" {totaltime[0] * 2}F ({realtime.ToString("F")}s) <{totaltime[1] * 2}F>. ";
            switch (lindex)
            {
                case 0: str = "Set Eontimer for" + str + (ShowDelay ? $" Hit frame {max}" : ""); break;
                case 1: str = "计时器设置为" + str + (ShowDelay ? $" 在 {max} 帧按A" : ""); break;
            }
            TimeResult.Items.Add(str);
        }

        private void CalcTime_Click(object sender, EventArgs e)
        {
            TimeResult.Items.Clear();
            int min = (int)Time_min.Value;
            int max = (int)Time_max.Value;
            if (!ShowDelay)
            {
                CalcTime_Output(min, max);
                return;
            }
            int[] tmptimer = new int[2];
            int delaytime = (int)Timedelay.Value / 2;
            for (int tmp = max - ModelNumber * delaytime; tmp <= max; tmp++)
            {
                tmptimer = CalcFrame(tmp, max);
                if (tmptimer[0] + tmptimer[1] > delaytime && tmptimer[0] <= delaytime)
                    CalcTime_Output(min, tmp - (int)Correction.Value);
                if (tmptimer[0] == delaytime && tmptimer[1] == 0)
                    CalcTime_Output(min, tmp - (int)Correction.Value);
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

        #region Search

        private void CalcList_Click(object sender, EventArgs e)
        {
            if (IsEvent && NatureLocked.Checked && Event_Nature.SelectedIndex == 0)
                Event_Nature.SelectedIndex = 1;
            if (ivmin0.Value > ivmax0.Value)
                Error(SETTINGERROR_STR[lindex] + L_H.Text);
            else if (ivmin1.Value > ivmax1.Value)
                Error(SETTINGERROR_STR[lindex] + L_A.Text);
            else if (ivmin2.Value > ivmax2.Value)
                Error(SETTINGERROR_STR[lindex] + L_B.Text);
            else if (ivmin3.Value > ivmax3.Value)
                Error(SETTINGERROR_STR[lindex] + L_C.Text);
            else if (ivmin4.Value > ivmax4.Value)
                Error(SETTINGERROR_STR[lindex] + L_D.Text);
            else if (ivmin5.Value > ivmax5.Value)
                Error(SETTINGERROR_STR[lindex] + L_S.Text);
            else if (CreateTimeline.Checked)
                createtimeline();
            else if (Frame_min.Value > Frame_max.Value)
                Error(SETTINGERROR_STR[lindex] + L_frame.Text);
            else if (Refinement.Checked)
                ConsiderHistoryStationarySearch();
            else
                StationarySearch();
        }

        private void getblinkflaglist(int min, int max, SFMT sfmt)
        {
            blinkflaglist = new byte[max - min + 2];
            SFMT st = (SFMT)sfmt.DeepCopy();
            if (ModelNumber == 1)
                MarkNoNPCFlag(st, min, max);
            else
                MarkMultipleNPCFlag(st, min, max);
        }

        private void MarkNoNPCFlag(SFMT st, int min, int max)
        {
            int blink_flag = 0;
            ulong rand;
            for (int i = 0; i < min - 2; i++)
                st.NextUInt64();
            if ((int)(st.NextUInt64() & 0x7F) == 0)
                blinkflaglist[0] = (byte)((int)(st.NextUInt64() % 3) == 0 ? 36 : 30);
            else if ((int)(st.NextUInt64() & 0x7F) == 0)
                blink_flag = 1;
            for (int i = min; i <= max; i++)
            {
                rand = st.NextUInt64();
                if (blink_flag == 1)
                {
                    blinkflaglist[i - min] = 5;
                    blinkflaglist[++i - min] = (byte)((int)(rand % 3) == 0 ? 36 : 30);
                    blink_flag = 0; st.NextUInt64(); // Reset and advance
                }
                if ((int)(rand & 0x7F) == 0)
                    blink_flag = blinkflaglist[i - min] = 1;
            }
        }

        private void MarkMultipleNPCFlag(SFMT st, int min, int max)
        {
            int Model_n = ModelNumber;
            int blink_flag = 0;
            int[] Unsaferange = new[] { 35 * (Model_n - 1), 41 * (Model_n - 1) };
            List<ulong> Randlist = new List<ulong>();
            int Min = Math.Max(min - Unsaferange[1], 418);
            for (int i = 0; i < Min; i++)
                st.NextUInt64();
            for (int i = 0; i <= (Model_n - 1) * 5 + 1; i++) // Create Buffer for checkafter
                Randlist.Add(st.NextUInt64());
            for (int i = Min; i <= max; i++, Randlist.RemoveAt(0), Randlist.Add(st.NextUInt64()))
            {
                if ((Randlist[0] & 0x7F) == 0)
                {
                    blink_flag = Unsaferange[blink_flag == 0 ? Checkafter(Randlist) : 1];
                    if (i >= min) blinkflaglist[i - min] = (byte)(blink_flag == 0 ? 1 : 3);
                    continue;
                }
                if (blink_flag > 0)
                {
                    blink_flag--;
                    if (i >= min) blinkflaglist[i - min] = 2;
                }
            }
        }

        private byte Checkafter(List<ulong> Randlist)
        {
            if (Randlist.Skip(1).Take(Randlist.Count - 2).Any(r => (r & 0x7F) == 0))
                return 1;
            if (Randlist.Last() % 3 == 0) return 1;
            return 0;
        }

        private void Prepare(SFMT sfmt)
        {
            dgvrowlist.Clear();
            DGV.Rows.Clear();
            status = new ModelStatus(ModelNumber, sfmt);
            if (ea.Location == 120) // Route 17
                status.route17 = true;
            setting = FilterSettings;
            rng = new RNGSetting();
            e = IsEvent ? geteventsetting() : null;
            RefreshRNGSettings(sfmt);
        }

        private Filters FilterSettings => new Filters
        {
            Nature = Nature.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            HPType = HiddenPower.CheckBoxItems.Skip(1).Select(e => e.Checked).ToArray(),
            Gender = Gender.SelectedIndex,
            Ability = Ability.SelectedIndex,
            Slot = Slot.CheckBoxItems.Select(e => e.Checked).ToArray(),
            IVlow = IVlow,
            IVup = IVup,
            BS = BS,
            Stats = Stats,
            Skip = DisableFilters.Checked,
            Lv = (byte)Filter_Lv.Value,
            PerfectIVs = (byte)PerfectIVs.Value,
        };

        private void RefreshRNGSettings(SFMT sfmt)
        {
            RNGSetting.Considerhistory = CreateTimeline.Checked || Refinement.Checked;
            RNGSetting.Considerdelay = ShowResultsAfterDelay.Checked;
            RNGSetting.PreDelayCorrection = (int)Correction.Value;
            RNGSetting.delaytime = (int)Timedelay.Value / 2;
            RNGSetting.modelnumber = ModelNumber;
            RNGSetting.ConsiderBagEnteringTime = EnterBagTime.Checked;
            RNGSetting.IsSolgaleo = PK.IsSolgaleo;
            RNGSetting.IsLunala = PK.IsLunala;
            RNGSetting.SolLunaReset = (RNGSetting.IsSolgaleo || RNGSetting.IsLunala) && ModelNumber == 7;
            RNGSetting.Synchro_Stat = (byte)(SyncNature.SelectedIndex - 1);
            RNGSetting.TSV = (int)TSV.Value;
            RNGSetting.AlwaysSynchro = AlwaysSynced.Checked;
            RNGSetting.Honey = Honey.Checked;
            RNGSetting.UB = UB.Checked;
            RNGSetting.ShinyCharm = ShinyCharm.Checked;
            RNGSetting.Wild = Wild.Checked;
            RNGSetting.Fix3v = Fix3v.Checked;
            RNGSetting.randomgender = FuncUtil.IsRandomGender((int)GenderRatio.SelectedValue);
            RNGSetting.gender = FuncUtil.getGenderRatio((int)GenderRatio.SelectedValue);
            RNGSetting.PokeLv = PK.Level;
            RNGSetting.Lv_min = (byte)Lv_min.Value;
            RNGSetting.Lv_max = (byte)Lv_max.Value;
            RNGSetting.UB_th = (byte)UB_th.Value;
            RNGSetting.Encounter_th = (byte)Encounter_th.Value;
            RNGSetting.ShinyLocked = PK.ShinyLocked;

            RNGSetting.fishing = Fishing.Checked;
            RNGSetting.SOS = SOS.Checked;
            RNGSetting.ChainLength = (byte)ChainLength.Value;

            RNGSetting.route17 = ea.Location == 120;
            RNGSetting.IsMainRNGEgg = MainRNGEgg.Checked;
            RNGSetting.IsMinior = PK.IsBlank && Wild.Checked && (int)SlotSpecies.SelectedValue == 774;

            RNGSetting.ResetModelStatus();
            RNGSetting.CreateBuffer(sfmt, ConsiderDelay.Checked);
        }

        private void MarkResults(RNGResult result, int blinkidx = -1, int realtime = -1)
        {
            // Mark realtime
            if (realtime > -1)
                result.realtime = realtime;
            // Mark Blink
            if (0 <= blinkidx && blinkidx < blinkflaglist.Length)
                result.Blink = blinkflaglist[blinkidx];
        }

        private bool frameMatch(RNGResult result, Filters setting)
        {
            if (setting.Skip)
                return true;
            if (ShinyOnly.Checked && !result.Shiny)
                return false;
            if (BlinkOnly.Checked && result.Blink < 5)
                return false;
            if (SafeFOnly.Checked && result.Blink > 1)
                return false;
            if (MainRNGEgg.Checked)
                return true;
            if (ByIVs.Checked ? !setting.CheckIVs(result) : !setting.CheckStats(result))
                return false;
            if (!setting.CheckHiddenPower(result))
                return false;
            if (!setting.CheckNature(result.Nature))
                return false;
            if (setting.Gender != 0 && setting.Gender != result.Gender)
                return false;
            if (setting.Ability != 0 && setting.Ability != result.Ability)
                return false;

            if (Wild.Checked)
            {
                if (setting.Lv != 0 && setting.Lv != result.Lv)
                    return false;
                if (EncounteredOnly.Checked && result.Encounter >= Encounter_th.Value)
                    return false;
                if (UBOnly.Checked && result.UbValue >= UB_th.Value)
                    return false;
                if (!setting.CheckSlot(result.Slot))
                    return false;
            }

            return true;
        }

        private static readonly string[] blinkmarks = { "-", "★", "?", "? ★" };

        private DataGridViewRow getRow(int i, RNGResult result)
        {
            int d = i - (int)Time_max.Value;
            string true_nature = StringItem.naturestr[result.Nature];
            string SynchronizeFlag = result.Synchronize ? "O" : "X";
            if (result.UbValue < UB_th.Value && ShowDelay)
                result.Blink = 2;
            string BlinkFlag = result.Blink < 4 ? blinkmarks[result.Blink] : result.Blink.ToString();
            string PSV = result.PSV.ToString("D4");
            string Encounter = result.Encounter == -1 ? "-" : result.Encounter.ToString();
            string Slot = result.Slot == 0 ? "-" : result.Slot.ToString();
            string Lv = result.Lv == 0 ? "-" : result.Lv.ToString();
            string Item = result.Item == 100 ? "-" : result.Item.ToString();
            string UbValue = result.UbValue == 100 ? "-" : result.UbValue.ToString();
            string randstr = result.row_r.ToString("X16");
            string PID = result.PID.ToString("X8");
            string EC = result.EC.ToString("X8");
            string time = CreateTimeline.Checked || Refinement.Checked ? (2 * result.realtime).ToString() + "F" : "-";

            if (!Advanced.Checked)
            {
                if (Encounter != "-")
                    Encounter = result.Encounter < Encounter_th.Value ? "O" : "X";
                if (UbValue != "-")
                    UbValue = result.UbValue < UB_th.Value ? "O" : "X";
                if (UbValue == "O")
                    Slot = "UB";
                if (result.Item < 50)
                    Item = "50%";
                else if (result.Item < 55)
                    Item = "5%";
                else
                    Item = "-";
                time = CreateTimeline.Checked || Refinement.Checked ? (result.realtime / 30.0).ToString("F") + " s" : "-";
            }

            if (IsEvent && !OtherInfo.Checked && e.PIDType > 1)
                PID = PSV = "-";

            string frameadvance = result.frameshift.ToString("+#;-#;0");
            int[] Status = ShowStats.Checked ? result.Stats : result.IVs;

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(DGV);

            string research = (result.row_r % 6).ToString() + " " + (result.row_r % 32).ToString("D2") + " " + (result.row_r % 100).ToString("D2") + " " + (result.row_r % 252).ToString("D3");

            row.SetValues(
                i, d.ToString("+#;-#;0"), BlinkFlag,
                Status[0], Status[1], Status[2], Status[3], Status[4], Status[5],
                true_nature, SynchronizeFlag, StringItem.hpstr[result.hiddenpower + 1], result.Clock, PSV, frameadvance, UbValue, Slot, Lv, StringItem.genderstr[result.Gender], StringItem.abilitystr[result.Ability], Item, Encounter,
                randstr, PID, EC, time, research
                );

            if (result.Shiny)
                row.DefaultCellStyle.BackColor = Color.LightCyan;

            row.Cells[24].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            Font BoldFont = new Font("Microsoft Sans Serif", 8, FontStyle.Bold);
            for (int k = 0; k < 6; k++)
            {
                if (result.IVs[k] < 1)
                {
                    row.Cells[3 + k].Style.Font = BoldFont;
                    row.Cells[3 + k].Style.ForeColor = Color.OrangeRed;
                }
                else if (result.IVs[k] > 29)
                {
                    row.Cells[3 + k].Style.Font = BoldFont;
                    row.Cells[3 + k].Style.ForeColor = Color.MediumSeaGreen;
                }
            }
            return row;
        }

        private void StationarySearch()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100; max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value; max = (int)Frame_max.Value;
            }
            getblinkflaglist(min, max, sfmt);
            // Advance
            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            bool HasStageFrame = ModelNumber == 1;
            // Start
            for (int i = min; i <= max; i++, RNGSetting.Rand.RemoveAt(0), RNGSetting.Rand.Add(sfmt.NextUInt64()))
            {
                if (HasStageFrame)
                    RNGSetting.StageFrame = blinkflaglist[i - min] >= 5 ? blinkflaglist[i - min] : (byte)0;
                RNGResult result = rng.Generate(e);
                MarkResults(result, i - min);
                if (!frameMatch(result, setting))
                    continue;
                dgvrowlist.Add(getRow(i, result));
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }

        private void ConsiderHistoryStationarySearch()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100; max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value; max = (int)Frame_max.Value;
            }
            int StartFrame = (int)Frame_min.Value;
            getblinkflaglist(min, max, sfmt);
            // Advance
            for (int i = 0; i < StartFrame; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            int frameadvance;
            int[] remain_frame;
            int realtime = 0;
            bool phase;
            // Start
            for (int i = StartFrame; i <= max;)
            {
                remain_frame = (int[])status.remain_frame.Clone();
                phase = status.phase;
                frameadvance = status.NextState();

                while (frameadvance > 0)
                {
                    RNGSetting.remain_frame = (int[])remain_frame.Clone();
                    RNGSetting.phase = phase;
                    RNGResult result = rng.Generate(e);
                    RNGSetting.Rand.RemoveAt(0);
                    RNGSetting.Rand.Add(sfmt.NextUInt64());
                    frameadvance--;
                    i++;
                    if (i <= min || i > max)
                        continue;
                    MarkResults(result, i - min - 1, realtime);
                    if (!frameMatch(result, setting))
                        continue;
                    dgvrowlist.Add(getRow(i - 1, result));
                }
                realtime++;
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }

        private void createtimeline()
        {
            // Blinkflag
            SFMT sfmt = new SFMT((uint)Seed.Value);
            int start_frame = (int)Frame_min.Value;
            getblinkflaglist(start_frame, start_frame, sfmt);
            // Advance
            for (int i = 0; i < start_frame; i++)
                sfmt.NextUInt64();
            // Prepare
            Prepare(sfmt);
            int totaltime = (int)TimeSpan.Value * 30;
            int frame = (int)Frame_min.Value;
            int frameadvance, Currentframe;
            // Start
            for (int i = 0; i <= totaltime; i++)
            {
                Currentframe = frame;
                RNGSetting.remain_frame = (int[])status.remain_frame.Clone();
                RNGSetting.phase = status.phase;

                RNGResult result = rng.Generate(e);
                MarkResults(result, i, i);

                frameadvance = status.NextState();
                frame += frameadvance;
                for (int j = 0; j < frameadvance; j++)
                {
                    RNGSetting.Rand.RemoveAt(0);
                    RNGSetting.Rand.Add(sfmt.NextUInt64());
                }

                if (!frameMatch(result, setting))
                    continue;
                dgvrowlist.Add(getRow(Currentframe, result));
                if (dgvrowlist.Count > 100000) break;
            }
            DGV.Rows.AddRange(dgvrowlist.ToArray());
            DGV.CurrentCell = null;
        }
        #endregion

        #region Misc Function

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            PK = PMDropdownlist.FirstOrDefault(pm => pm.SpecForm == (int)Poke.SelectedValue) ?? Pokemon.SpeciesList[0];
            //General
            Properties.Settings.Default.PKM = (int)Poke.SelectedValue;
            Properties.Settings.Default.Save();
            
            Stationary.Checked = !(Wild.Checked = PK.Wild || PK.UB);
            UB.Visible = UB.Checked = PK.UB;
            Method_CheckedChanged(null, null);
            RefreshLocation();
            //Enable
            SOS.Visible = Fishing.Visible = Stationary.Enabled = Wild.Enabled = PK.IsBlank;
            GenderRatio.Enabled = Fix3v.Enabled = PK.IsBlank || PK.IsEvent || PK.UB;
            L_EventInstruction.Visible = PK.IsEvent;
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
            if (PK.UB)
                return;
            if (PK.IsCrabrawler)
            {
                Honey.Checked = false;
                WildEncounterSetting.Visible = false;
                Encounter_th.Value = 101;
            }
            Timedelay.Value = PK.Delay;
            NPC.Value = PK.NPC;
        }

        private EventRule geteventsetting()
        {
            int[] IVs = new[] { -1, -1, -1, -1, -1, -1 };
            for (int i = 0; i < 6; i++)
                if (EventIVLocked[i].Checked)
                    IVs[i] = (int)EventIV[i].Value;
            if (IVsCount.Value > 0 && IVs.Count(iv => iv >= 0) + IVsCount.Value > 5)
            {
                Error(SETTINGERROR_STR[lindex] + L_IVsCount.Text);
                IVs = new[] { -1, -1, -1, -1, -1, -1 };
            }
            EventRule e = new EventRule
            {
                IVs = (int[])IVs.Clone(),
                IVsCount = (byte)IVsCount.Value,
                YourID = YourID.Checked,
                PIDType = (byte)Event_PIDType.SelectedIndex,
                AbilityLocked = AbilityLocked.Checked,
                NatureLocked = NatureLocked.Checked,
                GenderLocked = GenderLocked.Checked,
                OtherInfo = OtherInfo.Checked,
                EC = (uint)Event_EC.Value,
                Ability = (byte)Event_Ability.SelectedIndex,
                Nature = (byte)(Event_Nature.SelectedIndex - 1),
                Gender = (byte)Event_Gender.SelectedIndex,
                IsEgg = IsEgg.Checked
            };
            if (e.YourID)
                e.TSV = (uint)TSV.Value;
            else
            {
                e.TID = (int)Event_TID.Value;
                e.SID = (int)Event_SID.Value;
                e.TSV = (uint)(e.TID ^ e.SID) >> 4;
                e.PID = (uint)Event_PID.Value;
            }
            return e;
        }

        private void SetTargetFrame_Click(object sender, EventArgs e)
        {
            try
            {
                Time_max.Value = Convert.ToDecimal(DGV.CurrentRow.Cells[0].Value);
            }
            catch (NullReferenceException)
            {
                Error(NOSELECTION_STR[lindex]);
            }
        }

        private void SetStartFrame_Click(object sender, EventArgs e)
        {
            try
            {
                Frame_min.Value = Convert.ToDecimal(DGV.CurrentRow.Cells[0].Value);
            }
            catch (NullReferenceException)
            {
                Error(NOSELECTION_STR[lindex]);
            }
        }

        private void HideControlPanel(object sender, EventArgs e)
        {
            if (ControlPanel.Visible)
            {
                ControlPanel.Visible = false;
                DGV.Height += ControlPanel.Height;
                DGV.Location = new Point(DGV.Location.X, DGV.Location.Y - ControlPanel.Height);
            }
            else
            {
                ControlPanel.Visible = true;
                DGV.Height -= ControlPanel.Height;
                DGV.Location = new Point(DGV.Location.X, DGV.Location.Y + ControlPanel.Height);
            }
        }

        private void HighLightFrameAfter_Click(object sender, EventArgs e)
        {
            try
            {
                int FrameAfter = Convert.ToInt32(DGV.CurrentRow.Cells["dgv_Frame"].Value) + Convert.ToInt32(DGV.CurrentRow.Cells["dgv_delay"].Value);
                int currentrowindex = DGV.CurrentRow.Index;
                for (int i = Convert.ToInt32(DGV.CurrentRow.Cells["dgv_delay"].Value); i > 0; i--)
                {
                    if (FrameAfter == Convert.ToInt32(DGV.Rows[currentrowindex + i].Cells[0].Value))
                    {
                        DGV.Rows[currentrowindex + i].DefaultCellStyle.BackColor = Color.Yellow;
                        return;
                    }
                }
            }
            catch
            {
            }
        }

        private void L_IVRange_Click(object sender, EventArgs e)
        {
            IVlow = IVup;
        }
        #endregion

        #region WC7 Import
        private void B_Open_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Gen7 Wonder Card Files|*.wc7";
            openFileDialog1.Title = "Select a Wonder Card File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                if (!ReadWc7(openFileDialog1.FileName))
                    Error(FILEERRORSTR[lindex]);
        }

        private bool ReadWc7(string filename)
        {
            BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open));
            try
            {
                byte[] Data = br.ReadBytes(0x108);
                byte CardType = Data[0x51];
                if (CardType != 0) return false;
                byte[] PIDType_Order = new byte[] { 3, 0, 2, 1 };
                byte[] Stats_index = new byte[] { 0xAF, 0xB0, 0xB1, 0xB3, 0xB4, 0xB2 };
                ushort sp = BitConverter.ToUInt16(Data, 0x82);
                L_EventSpecies.Text = L_Species.Text + ": " + StringItem.species[sp];
                L_EventSpecies.Visible = true;
                Poke.SelectedIndex = 1; // Switch to <Event>, set to Mew
                byte form = Data[0x84];
                SetPersonalInfo(sp, form); // Set pkm personal rule before wc7 rule
                AbilityLocked.Checked = Data[0xA2] < 3;
                Event_Ability.SelectedIndex = AbilityLocked.Checked ? Data[0xA2] + 1 : Data[0xA2] - 3;
                NatureLocked.Checked = Data[0xA0] != 0xFF;
                Event_Nature.SelectedIndex = NatureLocked.Checked ? Data[0xA0] + 1 : 0;
                GenderLocked.Checked = Data[0xA1] != 3;
                Event_Gender.SelectedIndex = GenderLocked.Checked ? (Data[0xA1] + 1) % 3 : 0;
                if (Data[0xA1] == 2) GenderRatio.SelectedIndex = 0;
                Fix3v.Checked = Data[Stats_index[0]] == 0xFE;
                switch (Data[Stats_index[0]])
                {
                    case 0xFE: IVsCount.Value = 3; break;
                    case 0xFD: IVsCount.Value = 2; break;
                    // Maybe more rules here
                    default: IVsCount.Value = 0; break;
                }
                for (int i = 0; i < 6; i++)
                {
                    if (Data[Stats_index[i]] < 0xFD)
                    {
                        EventIV[i].Value = Data[Stats_index[i]];
                        EventIVLocked[i].Checked = true;
                    }
                    else
                    {
                        EventIV[i].Value = 0;
                        EventIVLocked[i].Checked = false;
                    }
                }
                Event_TID.Value = BitConverter.ToUInt16(Data, 0x68);
                Event_SID.Value = BitConverter.ToUInt16(Data, 0x6A);
                Event_PIDType.SelectedIndex = PIDType_Order[Data[0xA3]];
                if (Event_PIDType.SelectedIndex == 3)
                    Event_PID.Value = BitConverter.ToUInt32(Data, 0xD4);
                Event_EC.Value = BitConverter.ToUInt32(Data, 0x70);
                if (Event_EC.Value > 0) Event_EC.Visible = L_EC.Visible = true;
                IsEgg.Checked = Data[0xD1] == 1;
                YourID.Checked = Data[0xB5] == 3;
                OtherInfo.Checked = true;
                Filter_Lv.Value = Data[0xD0];
                br.Close();
            }
            catch
            {
                br.Close();
                return false;
            }
            return true;
        }

        private void IDChanged(object sender, EventArgs e)
        {
            L_Event_G7TID.Text = "G7TID:  "; L_Event_TSV.Text = "TSV:   ";
            uint G7TID = ((uint)Event_TID.Value + ((uint)Event_SID.Value << 16)) % 1000000;
            uint TSV = ((uint)Event_TID.Value ^ (uint)Event_SID.Value) >> 4;
            L_Event_G7TID.Text += G7TID.ToString("D6");
            L_Event_TSV.Text += TSV.ToString("D4");
        }

        private void DropEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void DragDropWC(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && !ReadWc7(files[0]))
                Error(FILEERRORSTR[lindex]);
        }

        private void NatureLocked_CheckedChanged(object sender, EventArgs e)
        {
            Event_Nature.Enabled = NatureLocked.Checked;
            if (!NatureLocked.Checked) Event_Nature.SelectedIndex = 0;
        }

        private void GenderLocked_CheckedChanged(object sender, EventArgs e)
        {
            Event_Gender.Enabled = GenderLocked.Checked;
            if (!GenderLocked.Checked) Event_Gender.SelectedIndex = 0;
            if (IsEvent) GenderRatio.Enabled = !GenderLocked.Checked;
        }

        private void AbilityLocked_CheckedChanged(object sender, EventArgs e)
        {
            Event_Ability.Items.Clear();
            Event_Ability.Items.AddRange(AbilityLocked.Checked ? StringItem.abilitystr : StringItem.eventabilitystr);
            Event_Ability.SelectedIndex = 0;
        }
        #endregion
    }
}

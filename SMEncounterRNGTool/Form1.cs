using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using static PKHeX.Util;

namespace SMEncounterRNGTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region HexNumericFunction
        private void NumericUpDown_Enter(object sender, EventArgs e)
        {
            NumericUpDown NumericUpDown = sender as NumericUpDown;
            NumericUpDown.Select(0, NumericUpDown.Text.Length);
        }

        private void NumericUpDown_Check(object sender, CancelEventArgs e)
        {
            NumericUpDown NumericUpDown = sender as NumericUpDown;
            Control ctrl = NumericUpDown;
            if (ctrl == null)
                return;
            if (!string.IsNullOrEmpty(NumericUpDown.Text))
                return;
            foreach (var box in ((NumericUpDown)ctrl).Controls.OfType<TextBox>())
            {
                box.Undo();
                break;
            }
        }
        #endregion

        List<NumericUpDown> IVlow = new List<NumericUpDown>();
        List<NumericUpDown> IVup = new List<NumericUpDown>();
        List<NumericUpDown> Stat = new List<NumericUpDown>();
        List<NumericUpDown> EventIV = new List<NumericUpDown>();
        List<CheckBox> EventIVLocked = new List<CheckBox>();
        RNGSearch.EventRule e = new RNGSearch.EventRule();
        EncounterArea ea = new EncounterArea();
        List<Controls.ComboItem> Locationlist = new List<Controls.ComboItem>();
        byte[] locationlist = EncounterArea.SMLocationList;

        private string version = "1.1.0";

        #region Translation
        private string curlanguage;
        private static readonly string[] langlist = { "en", "cn" };
        private static readonly string[] NORESULT_STR = { "Not Found", "未找到" };
        private static readonly string[] NOSELECTION_STR = { "Please Select", "请选择" };
        private static readonly string[] SETTINGERROR_STR = { "Error at ", "出错啦0.0 发生在" };
        private static readonly string[] WAIT_STR = { "Please Wait...", "请稍后..." };
        private static readonly string[] EVENT_STR = { "<Event>", "<配信>" };
        private static readonly string[] FOSSIL_STR = { "<Fossil>", "<化石>" };
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
        private static string[] location, species;

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

            SearchSetting.naturestr = getStringList("Natures", curlanguage);
            SearchSetting.hpstr = getStringList("Types", curlanguage);
            species = getStringList("Species", curlanguage);
            location = getStringList("Location", curlanguage);

            for (int i = 0; i < 2; i++)
                GameVersion.Items[i] = GAMEVERSION_STR[lindex, i];

            for (int i = 0; i < 4; i++)
                Event_PIDType.Items[i] = PIDTYPE_STR[lindex, i];

            for (int i = 1; i < SearchSetting.hpstr.Length - 1; i++)
                HiddenPower.Items[i] = SearchSetting.hpstr[i];

            for (int i = 0; i < SearchSetting.naturestr.Length; i++)
                Event_Nature.Items[i + 1] = Nature.Items[i + 1] = SyncNature.Items[i + 1] = SearchSetting.naturestr[i];

            Poke.Items[1] = EVENT_STR[lindex];
            for (int i = 1; i < SearchSetting.pokedex.GetLength(0); i++)
                Poke.Items[i + 1] = species[SearchSetting.pokedex[i, 0]];

            RefreshLocation();

            Poke.Items[SearchSetting.Zygarde_index] += "-10%";
            Poke.Items[SearchSetting.Zygarde_index + 1] += "-50%";
            Poke.Items[SearchSetting.Fossil_index] = FOSSIL_STR[lindex];
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

            IVlow.Add(ivmin0);
            IVlow.Add(ivmin1);
            IVlow.Add(ivmin2);
            IVlow.Add(ivmin3);
            IVlow.Add(ivmin4);
            IVlow.Add(ivmin5);

            IVup.Add(ivmax0);
            IVup.Add(ivmax1);
            IVup.Add(ivmax2);
            IVup.Add(ivmax3);
            IVup.Add(ivmax4);
            IVup.Add(ivmax5);

            Stat.Add(Stat0);
            Stat.Add(Stat1);
            Stat.Add(Stat2);
            Stat.Add(Stat3);
            Stat.Add(Stat4);
            Stat.Add(Stat5);

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

            Nature.Items.Add("-");
            SyncNature.Items.Add("-");
            Event_Nature.Items.Add("-");
            HiddenPower.Items.Add("-");
            Poke.Items.Add("-");
            foreach (string t in SearchSetting.naturestr)
            {
                Nature.Items.Add("");
                SyncNature.Items.Add("");
                Event_Nature.Items.Add("");
            }
            for (int i = 0; i < SearchSetting.pokedex.GetLength(0); i++)
                Poke.Items.Add("");
            for (int i = 1; i < SearchSetting.hpstr.Length - 1; i++)
                HiddenPower.Items.Add("");

            foreach (string t in SearchSetting.genderstr)
            {
                Gender.Items.Add(t);
                Event_Gender.Items.Add(t);
            }
            
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

            HiddenPower.SelectedIndex = 0;
            Nature.SelectedIndex = 0;
            SyncNature.SelectedIndex = 0;
            Event_Nature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            Event_Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Event_Ability.SelectedIndex = 0;
            Event_PIDType.SelectedIndex = 0;

            Seed.Value = Properties.Settings.Default.Seed;
            ShinyCharm.Checked = Properties.Settings.Default.ShinyCharm;
            TSV.Value = Properties.Settings.Default.TSV;
            Advanced.Checked = Properties.Settings.Default.Advance;
            Poke.SelectedIndex = Properties.Settings.Default.Pokemon;
            GameVersion.SelectedIndex = Properties.Settings.Default.IsSun ? 0 : 1;

            if (Properties.Settings.Default.ClockInput)
                StartClockInput.Checked = true;
            else
                EndClockInput.Checked = true;

            ByIVs.Checked = true;
            BySaveScreen.Checked = true;

            SearchMethod_CheckedChanged(null, null);
            NPC_ValueChanged(null, null);
            CreateTimeline_CheckedChanged(null, null);
        }


        private int UBIndex => SearchSetting.UB_StartIndex;

        private void RefreshLocation()
        {
            Locationlist.Clear();
            if (Poke.SelectedIndex == 0)
                locationlist = EncounterArea.SMLocationList;
            else if (Poke.SelectedIndex >= UBIndex)
                locationlist = SearchSetting.UBLocation[Poke.SelectedIndex - UBIndex];
            else
                return;

            for (byte i = 0; i < locationlist.Length; i++)
                Locationlist.Add(new Controls.ComboItem { Text = location[locationlist[i]], Value = locationlist[i] });
            Location.DisplayMember = "Text";
            Location.ValueMember = "Value";
            Location.DataSource = new BindingSource(Locationlist, null);

            if (Location.SelectedValue == null || Location.SelectedIndex < 0 ) Location.SelectedIndex = 0;

            LoadSpecies();
        }

        private void LoadSpecies()
        {
            try
            {
                ea = (GameVersion.SelectedIndex == 0 ? EncounterArea.EncounterSun : EncounterArea.EncounterMoon).FirstOrDefault(e => e.Location == (int)Location.SelectedValue);
                var List = ea.Slots.Select(slot => new Controls.ComboItem { Text = species[slot.Species], Value = slot.Species, });

                SlotSpecies.DisplayMember = "Text";
                SlotSpecies.ValueMember = "Value";
                SlotSpecies.DataSource = new BindingSource(List, null);
            }
            catch { }
        }

        private void LoadPersonalInfo()
        {
            if (SlotSpecies.SelectedValue == null) return;
            var slot = ea.Slots.FirstOrDefault(e => e.Species == (int)SlotSpecies.SelectedValue);
            Lv_min.Value = slot.LevelMin;
            Lv_max.Value = slot.LevelMax;
            setBS(slot.Species,slot.Form);
        }

        private void setBS(int Species, int Form)
        {
            var t = PersonalTable.SM.getFormeEntry(Species,Form);
            BS_0.Value = t.HP;
            BS_1.Value = t.ATK;
            BS_2.Value = t.DEF;
            BS_3.Value = t.SPA;
            BS_4.Value = t.SPD;
            BS_5.Value = t.SPE;
        }


        #region SearchSeedfunction
        private void Clear_Click(object sender, EventArgs e)
        {
            ((QRInput.Checked) ? QRList : Clock_List).Text = "";
        }

        private void Back_Click(object sender, EventArgs e)
        {
            TextBox tmp = (QRInput.Checked) ? QRList : Clock_List;
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
            TextBox tmp = (QRInput.Checked) ? QRList : Clock_List;
            string str = ((Button)sender).Name;
            string number = str.Remove(0, str.IndexOf("button") + 6);

            if (tmp.Text == "")
                tmp.Text += Convert_Clock(number);
            else
                tmp.Text += "," + Convert_Clock(number);

            if (QRInput.Checked)
            {
                if (QRList.Text.Where(c => c == ',').Count() < 3)
                    return;
                QRSearch_Click(null, null);
            }
            else
                SearchforSeed(null, null);
        }

        private string Convert_Clock(string n)
        {
            int tmp = Convert.ToInt32(n);
            if (EndClockInput.Checked && !QRInput.Checked)
            {
                if (tmp >= 4)
                    tmp -= 4;
                else
                    tmp += 13;
                n = tmp.ToString();
            }
            return n;
        }

        private void SearchforSeed(object sender, EventArgs e)
        {
            var needle = Clock_List.Text.Split(',');
            if (Clock_List.Text.Where(c => c == ',').Count() >= 7)
            {
                var text = "";
                try
                {
                    SeedResults.Text = WAIT_STR[lindex];
                    var results = SFMTSeedAPI.request(Clock_List.Text);
                    if (results == null || results.Count() == 0)
                    {
                        text = NORESULT_STR[lindex];
                    }
                    else
                    {
                        text = string.Join(" ", results.Select(r => r.seed));
                        if (results.Count() == 1)
                        {
                            Time_min.Value = 418 + Clock_List.Text.Where(c => c == ',').Count();
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
            else
                SeedResults.Text = "";
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
                SFMT seed = new SFMT(InitialSeed);
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

        #region validcheck
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
            LoadSpecies();
            Properties.Settings.Default.IsSun = GameVersion.SelectedIndex == 0;
            Properties.Settings.Default.Save();
        }

        private void Location_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSpecies();
        }

        private void SlotSpecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPersonalInfo();
        }

        private void Method_CheckedChanged(object sender, EventArgs e)
        {
            Honey.Checked = Wild.Checked;
            if (Stationary.Checked)
                SOS.Checked = Fishing.Checked = UB.Checked = false;
            else
                EncounteredOnly.Checked = true;

            if (Poke.SelectedIndex == 0)
            {
                AlwaysSynced.Enabled = AlwaysSynced.Checked = Stationary.Checked;
                SOS.Visible = Fishing.Visible = Wild.Checked;
            }

            GenderRatio.SelectedIndex = Stationary.Checked ? 0 : 1;

            UB_CheckedChanged(null, null);
            ConsiderDelay_CheckedChanged(null, null);
            Honey_CheckedChanged(null, null);
            Reset_Click(null, null);
            WildEncounterSetting.Visible = GenderRatio.Visible = Honey.Visible = Wild.Checked;
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
            dgv_ubvalue.Visible = UBOnly.Visible = L_UB_th.Visible = UB_th.Visible = UB.Checked;
            if (!UB.Checked)
                UBOnly.Checked = false;
        }

        private void Honey_CheckedChanged(object sender, EventArgs e)
        {
            dgv_encounter.Visible = L_Encounter_th.Visible = Encounter_th.Visible = EncounteredOnly.Visible = !Honey.Checked && Wild.Checked;
            if (Honey.Checked)
            {
                Timedelay.Value = 186;
                ConsiderDelay.Checked = true;
                SOS.Checked = Fishing.Checked = false;
            }
            else
            {
                Correction.Value = 1;
                if (Wild.Checked) Timedelay.Value = 0;
            }
            label10.Text = Honey.Checked ? "F" : "+4F   1F=1/60s";
            L_Correction.Visible = Correction.Visible = Honey.Checked;
            Modification.Visible = Timedelay.Enabled = ConsiderDelay.Enabled = !Honey.Checked;
        }


        private void Fishing_CheckedChanged(object sender, EventArgs e)
        {
            if (Fishing.Checked)
            {
                SOS.Checked = Honey.Checked = false;
                Encounter_th.Value = 100;
            }
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
                Nature.SelectedIndex = SyncNature.SelectedIndex;
        }

        private void SearchMethod_CheckedChanged(object sender, EventArgs e)
        {
            IVPanel.Visible = ByIVs.Checked;
            StatPanel.Visible = ByStats.Checked;
            ShowStats.Enabled = ShowStats.Checked = ByStats.Checked;
        }

        private void NPC_ValueChanged(object sender, EventArgs e)
        {
            if (NPC.Value == 0)
            {
                BlinkOnly.Visible = true;
                SafeFOnly.Visible = SafeFOnly.Checked = false;
            }
            else
            {
                SafeFOnly.Visible = true;
                BlinkOnly.Visible = BlinkOnly.Checked = false;
            }
        }

        private void AlwaysSynced_CheckedChanged(object sender, EventArgs e)
        {
            if (Poke.SelectedIndex == 0)
                ConsiderBlink.Enabled = !AlwaysSynced.Checked;
            if (AlwaysSynced.Checked)
                ConsiderBlink.Checked = ConsiderBlink.Enabled = false;
        }

        private void CreateTimeline_CheckedChanged(object sender, EventArgs e)
        {
            dgv_time.Visible = CreateTimeline.Checked || Modification.Checked;
            TimeSpan.Enabled = CreateTimeline.Checked;
            AroundTarget.Enabled = BlinkOnly.Enabled = SafeFOnly.Enabled = !CreateTimeline.Checked;
            ShowResultsAfterDelay.Enabled = !CreateTimeline.Checked && ConsiderDelay.Checked;
            Frame_max.Visible = label7.Visible = !CreateTimeline.Checked;
            L_StartingPoint.Visible = CreateTimeline.Checked;
            if (CreateTimeline.Checked)
                Modification.Checked = AroundTarget.Checked = BlinkOnly.Checked = SafeFOnly.Checked = false;
            ShowResultsAfterDelay.Checked = ConsiderDelay.Checked;
        }

        private void Modification_CheckedChanged(object sender, EventArgs e)
        {
            dgv_time.Visible = CreateTimeline.Checked || Modification.Checked;
            if (Modification.Checked)
                CreateTimeline.Checked = false;
        }

        private void YourID_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent && !IsEgg.Checked)
                Timedelay.Value = YourID.Checked ? 62 : 0;
        }

        private void IsEgg_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent && YourID.Checked)
                Timedelay.Value = IsEgg.Checked ? 0 : 62;
        }

        private void Fix3v_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent)
                IVsCount.Value = Fix3v.Checked ? 3 : 0;
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
        #endregion

        #region TimerCalculateFunction
        private int[] CalcFrame(int min, int max)
        {
            uint InitialSeed = (uint)Seed.Value;
            SFMT sfmt = new SFMT(InitialSeed);

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            int n_count = 0;

            int[] remain_frame = new int[ModelNumber];
            //total_frame[0] Start; total_frame[1] Duration
            int[] total_frame = new int[2];
            bool[] blink_flag = new bool[ModelNumber];

            int timer = 0;

            while (min + n_count <= max)
            {
                //NPC Loop
                for (int i = 0; i < ModelNumber; i++)
                {
                    if (remain_frame[i] > 0)
                        remain_frame[i]--;

                    if (remain_frame[i] == 0)
                    {
                        //Blinking
                        if (blink_flag[i])
                        {
                            remain_frame[i] = (int)(sfmt.NextUInt64() % 3) == 0 ? 36 : 30;
                            n_count++;
                            blink_flag[i] = false;
                        }
                        //Not Blinking
                        else
                        {
                            if ((int)(sfmt.NextUInt64() & 0x7F) == 0)
                            {
                                remain_frame[i] = 5;
                                blink_flag[i] = true;
                            }
                            n_count++;
                        }
                    }
                }
                total_frame[timer]++;
                if (min + n_count == max)
                    timer = 1;
            }
            return total_frame;
        }

        private bool showdelay { get { return ConsiderDelay.Checked && !ShowResultsAfterDelay.Checked; } }

        private void CalcTime_Click(object sender, EventArgs e)
        {
            TimeResult.Items.Clear();
            int min = (int)Time_min.Value;
            int max = (int)Time_max.Value;
            int delaytime = RNGSearch.delaytime;
            int[] tmptimer = new int[2];
            if (showdelay)
            {
                for (int tmp = max - ModelNumber * delaytime; tmp <= max; tmp++)
                {
                    tmptimer = CalcFrame(tmp, max);
                    if ((tmptimer[0] + tmptimer[1] > delaytime) && (tmptimer[0] <= delaytime))
                        CalcTime_Output(min, tmp - (int)Correction.Value);
                    if ((tmptimer[0] == delaytime) && (tmptimer[1] == 0))
                        CalcTime_Output(min, tmp - (int)Correction.Value);
                }
            }
            else
                CalcTime_Output(min, max);
        }

        private void CalcTime_Output(int min, int max)
        {
            int[] totaltime = CalcFrame(min, max);
            float realtime = (float)totaltime[0] / 30;
            string str = $" {totaltime[0] * 2}F ({realtime.ToString("F")}s) <{totaltime[1] * 2}F>. ";
            switch (lindex)
            {
                case 0: str = "Set Eontimer for" + str + (showdelay ? $" Hit frame {max}" : ""); break;
                case 1: str = "计时器设置为" + str + (showdelay ? $" 在 {max} 帧按A" : ""); break;
            }
            TimeResult.Items.Add(str);
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            HiddenPower.SelectedIndex = 0;
            if (!AlwaysSynced.Checked) Nature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Slot.Text = "";

            if (ByIVs.Checked && Wild.Checked && (!UB.Checked || Lv_Search.Value <= Lv_max.Value))
                Lv_Search.Value = 0;
            for (int i = 0; i < 6; i++)
            {
                IVlow[i].Value = 0;
                IVup[i].Value = 31;
                Stat[i].Value = 0;
            }
        }
        #endregion

        #region Search
        private int ModelNumber => (int)NPC.Value + 1; 
        private bool IsEvent => Poke.SelectedIndex == 1;

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
            else if (Modification.Checked)
                ConsiderHistoryStationarySearch();
            else
                StationarySearch();
        }

        private byte[] getblinkflaglist(int min, int max, SFMT sfmt)
        {
            byte[] blinkflaglist = new byte[max - min + 2];
            int Model_n = ModelNumber;
            SFMT st = (SFMT)sfmt.DeepCopy();
            int blink_flag = 0;

            if (Model_n == 1)
            {
                ulong rand;
                for (int i = 0; i < min - 2; i++)
                    st.NextUInt64();
                if ((int)(st.NextUInt64() & 0x7F) == 0)
                    blinkflaglist[0] = (int)(st.NextUInt64() % 3) == 0 ? (byte)36 : (byte)30;
                else if ((int)(st.NextUInt64() & 0x7F) == 0)
                    blink_flag = 1;
                for (int i = min; i <= max; i++)
                {
                    rand = st.NextUInt64();
                    if (blink_flag == 1)
                    {
                        blinkflaglist[i - min] = 5;
                        blinkflaglist[++i - min] = (int)(rand % 3) == 0 ? (byte)36 : (byte)30;
                        blink_flag = 0; rand = st.NextUInt64();
                    }
                    if ((int)(rand & 0x7F) == 0)
                        blink_flag = blinkflaglist[i - min] = 1;
                }
            }
            else
            {
                int[] Unsaferange = new int[] { 35 * (Model_n - 1), 41 * (Model_n - 1) };
                List<ulong> Randlist = new List<ulong>();
                int Min = Math.Max(min - Unsaferange[1], 418);
                for (int i = 0; i < Min; i++)
                    st.NextUInt64();
                for (int i = 0; i <= (ModelNumber - 1) * 5 + 1; i++)
                    Randlist.Add(st.NextUInt64());
                for (int i = Min; i <= max; i++, Randlist.RemoveAt(0), Randlist.Add(st.NextUInt64()))
                {
                    if ((Randlist[0] & 0x7F) == 0)
                    {
                        if (blink_flag == 0)
                        {
                            blink_flag = Unsaferange[Checkafter(Randlist)];
                            if (i >= min) blinkflaglist[i - min] = 1;
                        }
                        else
                        {
                            blink_flag = Unsaferange[1];
                            if (i >= min) blinkflaglist[i - min] = 3;
                        }
                    }
                    else if (blink_flag > 0)
                    {
                        blink_flag--;
                        if (i >= min) blinkflaglist[i - min] = 2;
                    }
                }
            }
            return blinkflaglist;
        }

        private byte Checkafter(List<ulong> Randlist)
        {
            for (int i = 1; i < Randlist.Count - 1; i++)
                if ((Randlist[i] & 0x7F) == 0) return 1;
            if (Randlist[Randlist.Count - 1] % 3 == 0) return 1;
            return 0;
        }

        private void StationarySearch()
        {
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100; max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value; max = (int)Frame_max.Value;
            }

            SFMT sfmt = new SFMT((uint)Seed.Value);
            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            var setting = getSettings();
            var rng = getRNGSettings();
            if (IsEvent) e = geteventsetting();

            byte[] Blinkflaglist = getblinkflaglist(min, max, sfmt);

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            RNGSearch.CreateBuffer(sfmt, ConsiderDelay.Checked);

            for (int i = min; i <= max; i++, RNGSearch.Rand.RemoveAt(0), RNGSearch.Rand.Add(sfmt.NextUInt64()))
            {
                RNGSearch.RNGResult result = IsEvent ? rng.GenerateEvent(e) : rng.Generate();
                result.Blink = Blinkflaglist[i - min];

                if ((RNGSearch.IsSolgaleo || RNGSearch.IsLunala) && ModelNumber == 7) RNGSearch.modelnumber = 7;

                if (!frameMatch(result, setting))
                    continue;

                list.Add(getRow_Sta(i, rng, result, DGV));

                if (list.Count > 100000) break;
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }

        private void ConsiderHistoryStationarySearch()
        {
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

            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            byte[] Blinkflaglist = getblinkflaglist(min, max, sfmt);

            for (int i = 0; i < StartFrame; i++)
                sfmt.NextUInt64();
            var st = CreateNPCStatus(sfmt);
            var setting = getSettings();
            var rng = getRNGSettings();
            RNGSearch.ResetModelStatus();
            RNGSearch.CreateBuffer(sfmt, ConsiderDelay.Checked);
            if (IsEvent) e = geteventsetting();

            int frameadvance;
            int[] remain_frame;
            bool[] blink_flag;

            int realtime = 0;
            for (int i = StartFrame; i <= max;)
            {
                remain_frame = (int[])st.remain_frame.Clone();
                blink_flag = (bool[])st.blink_flag.Clone();
                frameadvance = st.NextState();

                while (frameadvance > 0)
                {
                    RNGSearch.remain_frame = (int[])remain_frame.Clone();
                    RNGSearch.blink_flag = (bool[])blink_flag.Clone();
                    RNGSearch.RNGResult result = IsEvent ? rng.GenerateEvent(e) : rng.Generate();
                    result.realtime = realtime;
                    if ((RNGSearch.IsSolgaleo || RNGSearch.IsLunala) && ModelNumber == 7) RNGSearch.modelnumber = 7;
                    RNGSearch.Rand.RemoveAt(0);
                    RNGSearch.Rand.Add(sfmt.NextUInt64());
                    frameadvance--;
                    i++;
                    if (i <= min || i > max || !frameMatch(result, setting))
                        continue;
                    result.Blink = Blinkflaglist[i - min - 1];
                    list.Add(getRow_Sta(i - 1, rng, result, DGV));
                }
                realtime++;
                if (list.Count > 100000) break;
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }


        private void createtimeline()
        {
            SFMT sfmt = new SFMT((uint)Seed.Value);

            int start_frame = (int)Frame_min.Value;
            byte start_flag = getblinkflaglist(start_frame, start_frame, sfmt)[0];

            for (int i = 0; i < start_frame; i++)
                sfmt.NextUInt64();

            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            var st = CreateNPCStatus(sfmt);
            var setting = getSettings();
            var rng = getRNGSettings();
            RNGSearch.ResetModelStatus();
            RNGSearch.CreateBuffer(sfmt, ConsiderDelay.Checked);
            if (IsEvent) e = geteventsetting();

            int totaltime = (int)TimeSpan.Value * 30;
            int frame = (int)Frame_min.Value;
            int frameadvance, Currentframe;

            for (int i = 0; i <= totaltime; i++)
            {
                Currentframe = frame;
                RNGSearch.remain_frame = (int[])st.remain_frame.Clone();
                RNGSearch.blink_flag = (bool[])st.blink_flag.Clone();

                RNGSearch.RNGResult result = IsEvent ? rng.GenerateEvent(e) : rng.Generate();

                if (i == 0) result.Blink = start_flag;
                if ((RNGSearch.IsSolgaleo || RNGSearch.IsLunala) && ModelNumber == 7) RNGSearch.modelnumber = 7;

                result.realtime = i;
                frameadvance = st.NextState();
                frame += frameadvance;

                for (int j = 0; j < frameadvance; j++)
                {
                    RNGSearch.Rand.RemoveAt(0);
                    RNGSearch.Rand.Add(sfmt.NextUInt64());
                }

                if (!frameMatch(result, setting))
                    continue;

                list.Add(getRow_Sta(Currentframe, rng, result, DGV));

                if (list.Count > 100000) break;
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }

        private ModelStatus CreateNPCStatus(SFMT sfmt)
        {
            ModelStatus.Modelnumber = ModelNumber;
            ModelStatus.smft = (SFMT)sfmt.DeepCopy();
            return new ModelStatus();
        }

        private SearchSetting getSettings()
        {
            int[] IVup = { (int)ivmax0.Value, (int)ivmax1.Value, (int)ivmax2.Value, (int)ivmax3.Value, (int)ivmax4.Value, (int)ivmax5.Value, };
            int[] IVlow = { (int)ivmin0.Value, (int)ivmin1.Value, (int)ivmin2.Value, (int)ivmin3.Value, (int)ivmin4.Value, (int)ivmin5.Value, };
            int[] BS = { (int)BS_0.Value, (int)BS_1.Value, (int)BS_2.Value, (int)BS_3.Value, (int)BS_4.Value, (int)BS_5.Value, };
            int[] Stats = { (int)Stat0.Value, (int)Stat1.Value, (int)Stat2.Value, (int)Stat3.Value, (int)Stat4.Value, (int)Stat5.Value, };

            return new SearchSetting
            {
                Nature = Nature.SelectedIndex - 1,
                HPType = HiddenPower.SelectedIndex - 1,
                Gender = Gender.SelectedIndex,
                Ability = Ability.SelectedIndex,
                Slot = SearchSetting.TranslateSlot(Slot.Text),
                IVlow = IVlow,
                IVup = IVup,
                BS = BS,
                Stats = Stats,
                Skip = DisableFilters.Checked,
                Lv = (int)Lv_Search.Value,
            };
        }

        private RNGSearch getRNGSettings()
        {
            byte gender_threshold = 0;
            switch (GenderRatio.SelectedIndex)
            {
                case 1: gender_threshold = 126; break;
                case 2: gender_threshold = 30; break;
                case 3: gender_threshold = 63; break;
                case 4: gender_threshold = 189; break;
            }

            RNGSearch.Considerhistory = CreateTimeline.Checked || Modification.Checked;
            RNGSearch.Considerdelay = ShowResultsAfterDelay.Checked;
            RNGSearch.PreDelayCorrection = (int)Correction.Value;
            RNGSearch.delaytime = (int)Timedelay.Value / 2;
            RNGSearch.ConsiderBlink = ConsiderBlink.Checked;
            RNGSearch.modelnumber = ModelNumber;
            RNGSearch.IsSolgaleo = Poke.SelectedIndex == SearchSetting.Solgaleo_index;
            RNGSearch.IsLunala = Poke.SelectedIndex == SearchSetting.Lunala_index;

            var rng = new RNGSearch
            {
                Synchro_Stat = (byte)(SyncNature.SelectedIndex - 1),
                TSV = (int)TSV.Value,
                AlwaysSynchro = AlwaysSynced.Checked,
                Honey = Honey.Checked,
                UB = UB.Checked,
                ShinyCharm = ShinyCharm.Checked,
                Wild = Wild.Checked,
                Fix3v = Fix3v.Checked,
                gender_ratio = gender_threshold,
                nogender = GenderRatio.SelectedIndex == 0,
                PokeLv = (Wild.Checked && Poke.SelectedIndex > 0) ? SearchSetting.PokeLevel[Poke.SelectedIndex - 1] : (byte)Lv_Search.Value,
                Lv_min = (byte)Lv_min.Value,
                Lv_max = (byte)Lv_max.Value,
                UB_th = (byte)UB_th.Value,
                Encounter_th = (byte)Encounter_th.Value,
                ShinyLocked = SearchSetting.ShinyLocked(Poke.SelectedIndex),
                fishing = Fishing.Checked,
                SOS = SOS.Checked,
                ChainLength = (byte)ChainLength.Value,
            };
            return rng;
        }

        private bool frameMatch(RNGSearch.RNGResult result, SearchSetting setting)
        {
            setting.getStats(result);

            if (setting.Skip)
                return true;
            if (ShinyOnly.Checked && !result.Shiny)
                return false;
            if (BlinkOnly.Checked && result.Blink < 5)
                return false;
            if (SafeFOnly.Checked && result.Blink > 1)
                return false;
            if (ByIVs.Checked && !setting.CheckIVs(result))
                return false;
            if (ByStats.Checked && !setting.CheckStats(result))
                return false;
            if (!setting.CheckHiddenPower(result.IVs))
                return false;
            if (setting.Nature != -1 && setting.Nature != result.Nature)
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
                if (setting.Slot[0] && (result.Slot < 0 || !setting.Slot[result.Slot]))
                    return false;
            }

            return true;
        }

        private static readonly string[] blinkmarks = new string[] { "-", "★", "?", "? ★" };

        private DataGridViewRow getRow_Sta(int i, RNGSearch rng, RNGSearch.RNGResult result, DataGridView dgv)
        {
            int d = i - (int)Time_max.Value;
            string true_nature = SearchSetting.naturestr[result.Nature];
            string SynchronizeFlag = (result.Synchronize ? "O" : "X");
            if ((result.UbValue < UB_th.Value) && (ConsiderDelay.Checked) && (!ShowResultsAfterDelay.Checked))
                result.Blink = 2;
            string BlinkFlag = result.Blink < 4 ? blinkmarks[result.Blink] : result.Blink.ToString();
            string PSV = result.PSV.ToString("D4");
            string Encounter = (result.Encounter == -1) ? "-" : result.Encounter.ToString();
            string Slot = (result.Slot == 0) ? "-" : result.Slot.ToString();
            string Lv = (result.Lv == 0) ? "-" : result.Lv.ToString();
            string Item = (result.Item == 100) ? "-" : result.Item.ToString();
            string UbValue = (result.UbValue == 100) ? "-" : result.UbValue.ToString();
            string randstr = result.row_r.ToString("X16");
            string PID = result.PID.ToString("X8");
            string EC = result.EC.ToString("X8");
            string time = (CreateTimeline.Checked || Modification.Checked) ? (2 * result.realtime).ToString() + "F" : "-";

            if (!Advanced.Checked)
            {
                if (Encounter != "-")
                    Encounter = (result.Encounter < Encounter_th.Value) ? "O" : "X";
                if (UbValue != "-")
                    UbValue = (result.UbValue < UB_th.Value) ? "O" : "X";
                if (UbValue == "O") Slot = "UB";
                if (result.Item < 50)
                    Item = "50%";
                else if (result.Item < 55)
                    Item = "5%";
                else
                    Item = "-";
                time = (CreateTimeline.Checked || Modification.Checked) ? ((float)result.realtime / 30).ToString("F") + " s" : "-";
            }

            if (IsEvent && !OtherInfo.Checked && e.PIDType > 1) { PID = "-"; PSV = "-"; }

            string frameadvance = result.frameshift.ToString("+#;-#;0");
            int[] Status = new int[6] { 0, 0, 0, 0, 0, 0 };
            if (ShowStats.Checked)
                Status = result.Stats;
            else
                Status = result.IVs;

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dgv);

            string research = (result.row_r % 6).ToString() + " " + (result.row_r % 32).ToString("D2") + " " + (result.row_r % 100).ToString("D2");

            row.SetValues(
                i, d.ToString("+#;-#;0"), BlinkFlag,
                Status[0], Status[1], Status[2], Status[3], Status[4], Status[5],
                true_nature, SynchronizeFlag, result.Clock, PSV, frameadvance, UbValue, Slot, Lv, SearchSetting.genderstr[result.Gender], SearchSetting.abilitystr[result.Ability], Item, Encounter,
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

        #endregion

        #region Misc Function

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            const int AlwaysSync_Index = SearchSetting.AlwaysSync_Index;
            const int Fossil_index = SearchSetting.Fossil_index;
            //General
            Properties.Settings.Default.Pokemon = (byte)Poke.SelectedIndex;
            Properties.Settings.Default.Save();

            UB.Visible = UB.Checked = Wild.Checked = Poke.SelectedIndex >= UBIndex;
            Stationary.Checked = Poke.SelectedIndex < UBIndex - 1;
            if (Poke.SelectedIndex == 0 || Poke.SelectedIndex == UBIndex - 1) Wild.Checked = true;
            Method_CheckedChanged(null, null);
            RefreshLocation();
            //Enable
            SOS.Visible = Fishing.Visible = Stationary.Enabled = Wild.Enabled = Poke.SelectedIndex == 0;
            GenderRatio.Enabled = Fix3v.Enabled = Poke.SelectedIndex < 2 || Poke.SelectedIndex >= UBIndex;
            Honey.Enabled = Encounter_th.Enabled = Poke.SelectedIndex != UBIndex - 1;
            //Event
            L_EventInstruction.Visible = IsEvent;
            //
            if (Poke.SelectedIndex == Fossil_index + 1) Honey.Checked = false;

            if (Poke.SelectedIndex == 0) return;
            AlwaysSynced.Checked = (Poke.SelectedIndex >= AlwaysSync_Index && Poke.SelectedIndex < UBIndex - 1);
            ConsiderBlink.Checked = !AlwaysSynced.Checked;
            Fix3v.Checked = (Poke.SelectedIndex > 1 && Poke.SelectedIndex < Fossil_index - 2);
            ConsiderDelay.Checked = true;
            ConsiderBlink.Enabled = AlwaysSynced.Enabled = false;

            setBS(SearchSetting.pokedex[Poke.SelectedIndex - 1, 0], SearchSetting.pokedex[Poke.SelectedIndex - 1, 1]);
            Lv_Search.Value = SearchSetting.PokeLevel[Poke.SelectedIndex - 1];
            NPC.Value = SearchSetting.NPC[Poke.SelectedIndex - 1];

            if (Poke.SelectedIndex >= UBIndex)
            {
                Correction.Value = SearchSetting.honeycorrection[Poke.SelectedIndex - UBIndex];
                UB_th.Value = SearchSetting.UB_rate[Poke.SelectedIndex - UBIndex];
            }
            else
            {
                Timedelay.Value = SearchSetting.timedelay[Poke.SelectedIndex - 1];
            }
                

            switch (Poke.SelectedIndex)
            {
                case 1:
                    GenderRatio.SelectedIndex = 1;
                    L_Ability.Visible = L_gender.Visible = Gender.Visible = Ability.Visible = true;
                    ConsiderBlink.Checked = false; GenderRatio.Visible = true;
                    Timedelay.Value = (YourID.Checked && !IsEgg.Checked) ? 62 : 0;
                    break;
                case Fossil_index - 2:
                case Fossil_index:
                    GenderRatio.SelectedIndex = 2;
                    GenderRatio.Visible = L_gender.Visible = Gender.Visible = true; break;
                case Fossil_index - 1:
                    L_Ability.Visible = Ability.Visible = true; break;
                case Fossil_index + 1:
                    Encounter_th.Value = 101; ConsiderBlink.Checked = false; break;
            }
        }

        private RNGSearch.EventRule geteventsetting()
        {
            int[] IVs = new int[6] { -1, -1, -1, -1, -1, -1 };
            int cnt = 0;
            for (int i = 0; i < 6; i++)
            {
                if (EventIVLocked[i].Checked)
                {
                    cnt++;
                    IVs[i] = (int)EventIV[i].Value;
                }
            }
            if (IVsCount.Value > 0 && cnt + IVsCount.Value > 5)
            {
                Error(SETTINGERROR_STR[lindex] + L_IVsCount.Text);
                IVs = new int[6] { -1, -1, -1, -1, -1, -1 };
            }
            RNGSearch.EventRule e = new RNGSearch.EventRule
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
            for (int i = 0; i < 6; i++)
                IVlow[i].Value = IVup[i].Value;
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
                AbilityLocked.Checked = Data[0xA2] < 3;
                Event_Ability.SelectedIndex = AbilityLocked.Checked ? Data[0xA2] + 1 : Data[0xA2] - 3;
                NatureLocked.Checked = Data[0xA0] != 0xFF;
                Event_Nature.SelectedIndex = NatureLocked.Checked ? Data[0xA0] + 1 : 0;
                GenderLocked.Checked = Data[0xA1] != 3;
                Event_Gender.SelectedIndex = GenderLocked.Checked ? (Data[0xA1] + 1) % 3 : 0;
                Poke.SelectedIndex = 1;
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
                Lv_Search.Value = Data[0xD0];
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
            if (files.Length == 1)
                if (!ReadWc7(files[0]))
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
            if (AbilityLocked.Checked)
            {
                Event_Ability.Items.Add("-");
                Event_Ability.Items.Add("1");
                Event_Ability.Items.Add("2");
                Event_Ability.Items.Add("H");
            }
            else
            {
                Event_Ability.Items.Add("1/2");
                Event_Ability.Items.Add("1/2/H");
            }
            Event_Ability.SelectedIndex = 0;
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
                // クリップボードへコピー
                box.Undo();
                break;
            }
        }
        #endregion

        List<NumericUpDown> IVlow = new List<NumericUpDown>();
        List<NumericUpDown> IVup = new List<NumericUpDown>();
        List<NumericUpDown> BS = new List<NumericUpDown>();
        List<NumericUpDown> Stat = new List<NumericUpDown>();
        List<NumericUpDown> EventIV = new List<NumericUpDown>();
        List<CheckBox> EventIVLocked = new List<CheckBox>();
        RNGSearch.EventRule e = new RNGSearch.EventRule();
        private string version = "1.03";

        #region Translation
        private string curlanguage;
        private static readonly string[] langlist = { "en", "cn" };
        private static readonly string[] NORESULT_STR = { "Not Found", "未找到" };
        private static readonly string[] NOSELECTION_STR = { "Please Select", "请选择" };
        private static readonly string[] SETTINGERROR_STR = { "Error at ", "出错啦0.0 发生在" };
        private static readonly string[] WAIT_STR = { "Please Wait...", "请稍后..." };
        private static readonly string[] EVENT_STR = { "<Event>", "<配信>" };
        private static readonly string[,] PIDTYPE_STR =
        {
            { "Random PID", "Random Nonshiny", "Random Shiny","Specified"},
            { "随机PID", "随机不闪", "随机闪","固定"}
        };

        private int lindex { get { return Lang.SelectedIndex; } set { Lang.SelectedIndex = value; } }
        private bool IsEvent { get { return Poke.SelectedIndex == 1; } }

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
            string[] species = getStringList("Species", curlanguage);

            for (int i = 0; i < 4; i++)
                Event_PID.Items[i] = PIDTYPE_STR[lindex, i];

            for (int i = 1; i < SearchSetting.hpstr.Length - 1; i++)
                HiddenPower.Items[i] = SearchSetting.hpstr[i];

            for (int i = 0; i < SearchSetting.naturestr.Length; i++)
                Nature.Items[i + 1] = SyncNature.Items[i + 1] = SearchSetting.naturestr[i];

            Poke.Items[1] = EVENT_STR[lindex];
            for (int i = 1; i < SearchSetting.pokedex.GetLength(0); i++)
                Poke.Items[i + 1] = species[SearchSetting.pokedex[i, 0]];
            Poke.Items[SearchSetting.Zygarde_index] += "-10%";
            Poke.Items[SearchSetting.Zygarde_index + 1] += "-50%";
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

            BS.Add(BS_0);
            BS.Add(BS_1);
            BS.Add(BS_2);
            BS.Add(BS_3);
            BS.Add(BS_4);
            BS.Add(BS_5);

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
            HiddenPower.Items.Add("-");
            Poke.Items.Add("-");
            foreach (string t in SearchSetting.naturestr)
            {
                Nature.Items.Add("");
                SyncNature.Items.Add("");
            }
            for (int i = 0; i < SearchSetting.pokedex.GetLength(0); i++)
                Poke.Items.Add("");
            for (int i = 1; i < SearchSetting.hpstr.Length - 1; i++)
                HiddenPower.Items.Add("");

            foreach (string t in SearchSetting.genderstr)
                Gender.Items.Add(t);

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
                case 0: TimeResultInstructText = "Format: Time in Frame (Time in Second) <Frame Lifetime>"; break;
                case 1: TimeResultInstructText = "格式：帧数 (秒数) <持续时间>"; break;
            }
            TimeResult.Items.Add(TimeResultInstructText);

            HiddenPower.SelectedIndex = 0;
            Nature.SelectedIndex = 0;
            SyncNature.SelectedIndex = 0;
            Gender.SelectedIndex = 0;
            Ability.SelectedIndex = 0;
            Event_PID.SelectedIndex = 0;

            Seed.Value = Properties.Settings.Default.Seed;
            ShinyCharm.Checked = Properties.Settings.Default.ShinyCharm;
            TSV.Value = Properties.Settings.Default.TSV;
            Advanced.Checked = Properties.Settings.Default.Advance;
            Poke.SelectedIndex = Properties.Settings.Default.Pokemon;

            if (Properties.Settings.Default.ClockInput)
                StartClockInput.Checked = true;
            else
                EndClockInput.Checked = true;

            ByIVs.Checked = true;
            BySaveScreen.Checked = true;

            Advanced_CheckedChanged(null, null);
            SearchMethod_CheckedChanged(null, null);
            NPC_ValueChanged(null, null);
            CreateTimeline_CheckedChanged(null, null);
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
            Properties.Settings.Default.Advance = Advanced.Checked;
            Properties.Settings.Default.Save();
            SearchByRand.Visible = Advanced.Checked;
        }

        private void Seed_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Seed = Seed.Value;
            Properties.Settings.Default.Save();
        }

        private void Method_CheckedChanged(object sender, EventArgs e)
        {
            Honey.Checked = Wild.Checked;
            if (Stationary.Checked)
                UB.Checked = false;
            else
                EncounteredOnly.Checked = true;

            Fix3v.Checked = Stationary.Checked;
            GenderRatio.SelectedIndex = Stationary.Checked ? 0 : 1;
            label10.Text = Wild.Checked ? "F" : "+4F   1F=1/60s";

            UB_CheckedChanged(null, null);
            ConsiderDelay_CheckedChanged(null, null);
            Honey_CheckedChanged(null, null);

            GenderRatio.Visible = UB.Visible = Honey.Visible = Wild.Checked;
            label9.Visible = L_Lv.Visible = L_gender.Visible = L_Ability.Visible = L_Slot.Visible = Wild.Checked;
            Lv_min.Visible = Lv_max.Visible = Slot.Visible = Gender.Visible = Ability.Visible = Wild.Checked;
        }

        private void IVLocked_CheckedChanged(object sender, EventArgs e)
        {
            string str = ((CheckBox)sender).Name;
            int i = Int32.Parse(str.Remove(0, str.IndexOf("Fix") + 3));
            EventIV[i].Enabled = ((CheckBox)sender).Checked;
        }

        private void UB_CheckedChanged(object sender, EventArgs e)
        {
            UBOnly.Visible = L_UB_th.Visible = UB_th.Visible = UB.Checked;
            if (!UB.Checked)
                UBOnly.Checked = false;
        }

        private void Honey_CheckedChanged(object sender, EventArgs e)
        {
            L_Encounter_th.Visible = Encounter_th.Visible = EncounteredOnly.Visible = !Honey.Checked && Wild.Checked;
            if (Honey.Checked)
            {
                Timedelay.Value = 186;
                ConsiderDelay.Checked = true;
            }
            else
            {
                Correction.Value = 0;
                if (Wild.Checked) Timedelay.Value = 0;
            }

            L_Correction.Visible = Correction.Visible = Honey.Checked;
            Timedelay.Enabled = ConsiderDelay.Enabled = !Honey.Checked;
        }

        private void ConsiderDelay_CheckedChanged(object sender, EventArgs e)
        {
            ShowResultsAfterDelay.Checked = ConsiderDelay.Checked;
            Timedelay.Enabled = ShowResultsAfterDelay.Enabled = HighLightFrameAfter.Enabled = ConsiderDelay.Checked;
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
            if (AlwaysSynced.Checked)
                ConsiderBlink.Checked = false;
        }


        private void CreateTimeline_CheckedChanged(object sender, EventArgs e)
        {
            TimeSpan.Enabled = CreateTimeline.Checked;
            BlinkOnly.Enabled = SafeFOnly.Enabled = !CreateTimeline.Checked;
            Frame_max.Visible = label7.Visible = !CreateTimeline.Checked;
            L_StartingPoint.Visible = CreateTimeline.Checked;
            if (CreateTimeline.Checked)
                BlinkOnly.Checked = SafeFOnly.Checked = false;
        }


        private void YourID_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent)
                Timedelay.Value = (YourID.Checked) ? 62 : 0;
        }

        private void Fix3v_CheckedChanged(object sender, EventArgs e)
        {
            if (IsEvent)
            {
                IVsCount.Value = Fix3v.Checked ? 3 : 0;
            }
        }
        #endregion

        #region TimerCalculateFunction
        private int[] CalcFrame(int min, int max)
        {
            uint InitialSeed = (uint)Seed.Value;
            int NPC_n = (int)NPC.Value + 1;
            SFMT sfmt = new SFMT(InitialSeed);

            for (int i = 0; i < min; i++)
                sfmt.NextUInt64();

            int n_count = 0;

            int[] remain_frame = new int[NPC_n];
            //total_frame[0] Start; total_frame[1] Duration
            int[] total_frame = new int[2];
            bool[] blink_flag = new bool[NPC_n];

            int timer = 0;

            while (min + n_count <= max)
            {
                //NPC Loop
                for (int i = 0; i < NPC_n; i++)
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
                for (int tmp = max - (int)(NPC.Value + 1) * delaytime; tmp <= max; tmp++)
                {
                    tmptimer = CalcFrame(tmp, max);
                    if ((tmptimer[0] + tmptimer[1] > delaytime) && (tmptimer[0] <= delaytime))
                        CalcTime_Output(min, tmp - (int)Correction.Value);
                    if ((tmptimer[0] == delaytime) && (tmptimer[1] == 0))
                        CalcTime_Output(min, tmp - (int)Correction.Value);
                }
            }
            else
            {
                CalcTime_Output(min, max);
            }
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

            if (ByIVs.Checked)
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

        private void CalcList_Click(object sender, EventArgs e)
        {
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
            else
                StationarySearch();
        }

        private void StationarySearch()
        {
            int max, min;
            if (AroundTarget.Checked)
            {
                min = (int)Time_max.Value - 100;
                max = (int)Time_max.Value + 100;
            }
            else
            {
                min = (int)Frame_min.Value;
                max = (int)Frame_max.Value;
            }

            SFMT sfmt = new SFMT((uint)Seed.Value);
            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            var setting = getSettings();
            var rng = getRNGSettings();

            //Blink flag history
            int blink_flag = 0;
            int UnsafeRange = 41 * (int)NPC.Value;
            if (UnsafeRange == 0)
            {
                for (int i = 0; i < min - 2; i++)
                    sfmt.NextUInt64();
                for (int i = 0; i < 2; i++)
                {
                    switch (blink_flag)
                    {
                        case 0:
                            if ((sfmt.NextUInt64() & 0x7F) == 0) blink_flag = 1; break;
                        case 1:
                            blink_flag = (sfmt.NextUInt64() % 3) == 0 ? 36 : 30; break;
                    }
                }
            }
            else if (!Honey.Checked)
            {
                for (int i = 0; i < Math.Max(418, min - UnsafeRange); i++)
                    sfmt.NextUInt64();
                for (int i = Math.Max(418, min - UnsafeRange); i < min; i++)
                {
                    if ((sfmt.NextUInt64() & 0x7F) == 0)
                        blink_flag = UnsafeRange;
                    else
                    if (blink_flag > 0) blink_flag--;
                }
            }
            else
            {
                for (int i = 0; i < min; i++)
                    sfmt.NextUInt64();
            }

            RNGSearch.CreateBuffer(sfmt);

            if (IsEvent)
                e = geteventsetting();

            for (int i = min; i <= max; i++, RNGSearch.Rand.RemoveAt(0), RNGSearch.Rand.Add(sfmt.NextUInt64()))
            {
                RNGSearch.RNGResult result = IsEvent ? rng.GenerateEvent(e) : rng.Generate();

                if (NPC.Value == 0)
                {
                    switch (blink_flag)
                    {
                        case 0:
                            if (result.Blink == 1) blink_flag = 1; break;
                        case 1:
                            blink_flag = (result.row_r % 3) == 0 ? 36 : 30; result.Blink = 5; break;
                        default:
                            result.Blink = blink_flag; blink_flag = 0; break;
                    }
                }
                else if (!Honey.Checked)
                {
                    if (result.Blink == 1)
                    {
                        if (blink_flag > 0) result.Blink = -2;
                        blink_flag = UnsafeRange;
                    }
                    else if (blink_flag > 0)
                    {
                        blink_flag--;
                        result.Blink = -1;
                    }
                }

                if (!frameMatch(result, setting))
                    continue;

                list.Add(getRow_Sta(i, rng, result, DGV));

                if (list.Count > 100000) break;
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }

        private void createtimeline()
        {
            SFMT sfmt = new SFMT((uint)Seed.Value);

            for (int i = 0; i < (int)Frame_min.Value; i++)
                sfmt.NextUInt64();

            List<DataGridViewRow> list = new List<DataGridViewRow>();
            DGV.Rows.Clear();

            var st = CreateNPCStatus(sfmt);
            var setting = getSettings();
            var rng = getRNGSettings();

            RNGSearch.ResetNPCStatus();
            RNGSearch.CreateBuffer(sfmt);

            if (IsEvent)
                e = geteventsetting();

            int totaltime = (int)TimeSpan.Value * 30;
            int frame = (int)Frame_min.Value;
            int frameadvance = 0;

            for (int i = 0; i <= totaltime; i++)
            {
                RNGSearch.remain_frame = (int[])st.remain_frame.Clone();
                RNGSearch.blink_flag = (bool[])st.blink_flag.Clone();

                RNGSearch.RNGResult result = IsEvent ? rng.GenerateEvent(e) : rng.Generate();

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

                list.Add(getRow_Sta(frame - frameadvance, rng, result, DGV));

                if (list.Count > 100000) break;
            }
            DGV.Rows.AddRange(list.ToArray());
            DGV.CurrentCell = null;
        }

        private NPCStatus CreateNPCStatus(SFMT sfmt)
        {
            NPCStatus.npcnumber = (int)NPC.Value + 1;
            NPCStatus.smft = (SFMT)sfmt.DeepCopy();
            return new NPCStatus();
        }

        private SearchSetting getSettings()
        {
            int[] IVup = { (int)ivmax0.Value, (int)ivmax1.Value, (int)ivmax2.Value, (int)ivmax3.Value, (int)ivmax4.Value, (int)ivmax5.Value, };
            int[] IVlow = { (int)ivmin0.Value, (int)ivmin1.Value, (int)ivmin2.Value, (int)ivmin3.Value, (int)ivmin4.Value, (int)ivmin5.Value, };
            int[] BS = { (int)BS_0.Value, (int)BS_1.Value, (int)BS_2.Value, (int)BS_3.Value, (int)BS_4.Value, (int)BS_5.Value, };
            int[] Status = { (int)Stat0.Value, (int)Stat1.Value, (int)Stat2.Value, (int)Stat3.Value, (int)Stat4.Value, (int)Stat5.Value, };

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
                Status = Status,
                Skip = DisableFilters.Checked,
                Lv = (int)Lv_Search.Value,
            };
        }

        private RNGSearch getRNGSettings()
        {
            int gender_threshold = 0;
            switch (GenderRatio.SelectedIndex)
            {
                case 1: gender_threshold = 126; break;
                case 2: gender_threshold = 30; break;
                case 3: gender_threshold = 63; break;
                case 4: gender_threshold = 189; break;
            }

            RNGSearch.createtimeline = CreateTimeline.Checked;
            RNGSearch.CalcDelay = ConsiderDelay.Checked;
            RNGSearch.Considerdelay = ShowResultsAfterDelay.Checked;
            RNGSearch.PreDelayCorrection = (int)Correction.Value;
            RNGSearch.delaytime = (int)Timedelay.Value / 2;
            RNGSearch.ConsiderBlink = ConsiderBlink.Checked;
            RNGSearch.npcnumber = (int)NPC.Value + 1;
            RNGSearch.IsSolgaleo = Poke.SelectedIndex == SearchSetting.Solgaleo_index;
            RNGSearch.IsLunala = Poke.SelectedIndex == SearchSetting.Lunala_index;

            var rng = new RNGSearch
            {
                Synchro_Stat = SyncNature.SelectedIndex - 1,
                TSV = (int)TSV.Value,
                AlwaysSynchro = AlwaysSynced.Checked,
                Honey = Honey.Checked,
                UB = UB.Checked,
                ShinyCharm = ShinyCharm.Checked,
                Wild = Wild.Checked,
                Fix3v = Fix3v.Checked,
                gender_ratio = gender_threshold,
                nogender = GenderRatio.SelectedIndex == 0,
                PokeLv = (Poke.SelectedIndex == 0) ? -1 : SearchSetting.PokeLevel[Poke.SelectedIndex - 1],
                Lv_min = (int)Lv_min.Value,
                Lv_max = (int)Lv_max.Value,
                UB_th = (int)UB_th.Value,
                ShinyLocked = SearchSetting.ShinyLocked(Poke.SelectedIndex),
            };
            return rng;
        }

        private bool frameMatch(RNGSearch.RNGResult result, SearchSetting setting)
        {
            setting.getStatus(result, setting);

            if (setting.Skip)
                return true;

            if (ShinyOnly.Checked && !result.Shiny)
                return false;

            if (BlinkOnly.Checked && result.Blink < 5)
                return false;

            if (SafeFOnly.Checked && result.Blink < 0)
                return false;

            if (ByIVs.Checked && !setting.validIVs(result.IVs))
                return false;

            if (ByStats.Checked && !setting.validStatus(result, setting))
                return false;

            if (!setting.mezapa_check(result.IVs))
                return false;

            if (setting.Nature != -1 && setting.Nature != result.Nature)
                return false;

            if (Wild.Checked)
            {
                if (setting.Lv != 0 && setting.Lv != result.Lv)
                    return false;

                if (EncounteredOnly.Checked && result.Encounter >= Encounter_th.Value)
                    return false;

                if (UBOnly.Checked && result.UbValue >= UB_th.Value)
                    return false;

                if (!UB.Checked || result.UbValue >= UB_th.Value)
                {
                    if (setting.Slot[0] && !setting.Slot[result.Slot])
                        return false;

                    if (setting.Gender != 0 && setting.Gender != result.Gender)
                        return false;

                    if (setting.Ability != 0 && setting.Ability != result.Ability)
                        return false;
                }
            }

            return true;
        }

        private DataGridViewRow getRow_Sta(int i, RNGSearch rng, RNGSearch.RNGResult result, DataGridView dgv)
        {
            int d = i - (int)Time_max.Value;
            string true_nature = SearchSetting.naturestr[result.Nature];
            string SynchronizeFlag = (result.Synchronize ? "O" : "X");
            if ((result.UbValue < UB_th.Value) && (ConsiderDelay.Checked) && (!ShowResultsAfterDelay.Checked))
                result.Blink = -1;
            string BlinkFlag = "";
            switch (result.Blink)
            {
                case -2: BlinkFlag = "★+?"; break;
                case -1: BlinkFlag = "?"; break;
                case 0: BlinkFlag = "-"; break;
                case 1: BlinkFlag = "★"; break;
                default: BlinkFlag = result.Blink.ToString(); break;
            }
            string PSV = result.PSV.ToString("D4");
            string Ability = result.Ability.ToString();
            string Encounter = (result.Encounter == -1) ? "-" : result.Encounter.ToString();
            string Slot = (result.Slot == -1) ? "-" : result.Slot.ToString();
            string Lv = (result.Lv == -1) ? "-" : result.Lv.ToString();
            string Item = (result.Item == -1) ? "-" : result.Item.ToString();
            string UbValue = (result.UbValue == 100) ? "-" : result.UbValue.ToString();
            string randstr = result.row_r.ToString("X16");
            string PID = result.PID.ToString("X8");
            string EC = result.EC.ToString("X8");
            string time = (CreateTimeline.Checked) ? (2 * result.realtime).ToString() + "F" : "-";

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
                time = (CreateTimeline.Checked) ? ((float)result.realtime / 30).ToString("F") + "s" : "-";
            }

            if (IsEvent)
            {
                if (e.AbilityLocked) Ability = "-";
                if (e.NatureLocked) true_nature = "-";
                if (e.PIDType > 1) { PID = "-"; PSV = "-"; EC = "-"; }
            }

            string frameadvance = result.frameshift.ToString("+#;-#;0");
            if (ConsiderDelay.Checked && !ShowResultsAfterDelay.Checked)
            {
                RNGSearch.Resetindex(); RNGSearch.ResetNPCStatus();
                frameadvance = (IsEvent) ? rng.getframeshift(e).ToString("+#;-#;0") : rng.getframeshift().ToString("+#;-#;0");
            }

            int[] Status = new int[6] { 0, 0, 0, 0, 0, 0 };
            if (ShowStats.Checked)
                Status = result.p_Status;
            else
                Status = result.IVs;

            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dgv);

            row.SetValues(
                i, d.ToString("+#;-#;0"), BlinkFlag,
                Status[0], Status[1], Status[2], Status[3], Status[4], Status[5],
                true_nature, SynchronizeFlag, result.Clock, PSV, frameadvance, UbValue, Slot, Lv, SearchSetting.genderstr[result.Gender], Ability, Item, Encounter,
                randstr, PID, EC, time
                );

            if (result.Shiny)
                row.DefaultCellStyle.BackColor = Color.LightCyan;

            return row;
        }

        #endregion

        #region Misc Function

        private void Poke_SelectedIndexChanged(object sender, EventArgs e)
        {
            const int UB_StartIndex = SearchSetting.UB_StartIndex;
            const int AlwaysSync_Index = SearchSetting.AlwaysSync_Index;
            //General
            Properties.Settings.Default.Pokemon = (byte)Poke.SelectedIndex;
            Properties.Settings.Default.Save();

            UB.Checked = Wild.Checked = Poke.SelectedIndex >= UB_StartIndex;
            Stationary.Checked = Poke.SelectedIndex < UB_StartIndex;
            Method_CheckedChanged(null, null);
            AlwaysSynced.Checked = (Poke.SelectedIndex >= AlwaysSync_Index) && (Poke.SelectedIndex < UB_StartIndex);
            ConsiderBlink.Checked = !AlwaysSynced.Checked;
            //Enable
            ConsiderBlink.Enabled = Stationary.Enabled = Wild.Enabled = AlwaysSynced.Enabled = Poke.SelectedIndex == 0;
            Fix3v.Enabled = (Poke.SelectedIndex < 2) || (Poke.SelectedIndex >= UB_StartIndex);
            //1
            L_EventInstruction.Visible = IsEvent;

            if (Poke.SelectedIndex == 0) return;
            ConsiderDelay.Checked = true;
            for (int i = 0; i < 6; i++)
                BS[i].Value = SearchSetting.pokedex[Poke.SelectedIndex - 1, i + 1];
            Lv_Search.Value = SearchSetting.PokeLevel[Poke.SelectedIndex - 1];
            NPC.Value = SearchSetting.NPC[Poke.SelectedIndex - 1];
            if (Poke.SelectedIndex >= UB_StartIndex)
            {
                Correction.Value = SearchSetting.honeycorrection[Poke.SelectedIndex - UB_StartIndex];
                UB_th.Value = SearchSetting.UB_rate[Poke.SelectedIndex - UB_StartIndex];
            }
            else
                Timedelay.Value = SearchSetting.timedelay[Poke.SelectedIndex - 1];

            switch (Poke.SelectedIndex)
            {
                case 1:
                    ConsiderBlink.Checked = false; GenderRatio.Visible = true;
                    L_Ability.Visible = L_gender.Visible = Gender.Visible = Ability.Visible = true;
                    Fix3v.Checked = false; Timedelay.Value = YourID.Checked ? 62 : 0;
                    break;
                case UB_StartIndex - 2: Fix3v.Checked = false; GenderRatio.SelectedIndex = 2; break;
                case UB_StartIndex - 1: Fix3v.Checked = false; break;
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
            return new RNGSearch.EventRule
            {
                IVs = (int[])IVs.Clone(),
                IVsCount = (int)IVsCount.Value,
                YourID = YourID.Checked,
                PIDType = Event_PID.SelectedIndex,
                AbilityLocked = AbilityLocked.Checked,
                NatureLocked = NatureLocked.Checked,
                GenderLocked = GenderLocked.Checked,
            };
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

        private void SearchByCurrSeed_Click(object sender, EventArgs e)
        {
            SFMT sfmt = new SFMT((uint)Seed.Value);
            for (int i = 0; i < Frame_min.Value; i++)
                sfmt.NextUInt64();
            if (CurrSeed.Text == "")
                return;
            for (int i = (int)Frame_min.Value; i < Frame_max.Value; i++)
            {
                string tmp = sfmt.NextInt64().ToString("X16");
                if (tmp.Contains(CurrSeed.Text))
                {
                    Result_Text.Text = $"{i} F";
                    Frame_min.Value = i;
                    Reset_Click(null, null);
                    CalcList_Click(null, null);
                    return;
                }
            }
            Result_Text.Text = NORESULT_STR[lindex];
        }

        #endregion
    }
}

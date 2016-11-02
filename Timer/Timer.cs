using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Newtonsoft.Json;

namespace Timer
{
    public partial class Timer : Form
    {
        private int Cooldown;

        public Timer()
        {
            InitializeComponent();
            updateTimerLabels();

            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json"))
                readData();
        }

        #region Numerics
        private void numericSeconds_ValueChanged(object sender, EventArgs e)
        {
            if (numericSeconds?.Value != null)
                Seconds.Text = numericSeconds.Value.ToString();
        }
        private void numericMinutes_ValueChanged(object sender, EventArgs e)
        {
            if (numericMinutes?.Value != null)
                Minutes.Text = numericMinutes.Value.ToString();
        }
        private void numericHours_ValueChanged(object sender, EventArgs e)
        {
            if (numericHours?.Value != null)
                Hours.Text = numericHours.Value.ToString();
        }
        #endregion

        #region Buttons
        private void buttonStart_Click(object sender, EventArgs e)
        {
            int count = Convert.ToInt32((numericHours.Value * 3600) + (numericMinutes.Value * 60) + (numericSeconds.Value));
            Cooldown = count;

            if (ouputText.Text == String.Empty)
            {
                MessageBox.Show("Output path can't be null!");
                return;
            }

            writeData();

            CooldownTimer.Start();
            CooldownTimer.Enabled = true;

            buttonStart.Enabled = false;
            buttonPause.Enabled = true;
            buttonStop.Enabled = true;
        }
        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (buttonPause.Text == "Pause")
            {
                CooldownTimer.Stop();
                CooldownTimer.Enabled = false;
                buttonPause.Text = "Resume";
            }
            else if (buttonPause.Text == "Resume")
            {
                CooldownTimer.Start();
                CooldownTimer.Enabled = true;
                buttonPause.Text = "Pause";
            }
        }
        private void buttonStop_Click(object sender, EventArgs e)
        {
            CooldownTimer.Stop();
            CooldownTimer.Enabled = false;

            Cooldown = 0;
            updateTimerLabels();

            buttonStart.Enabled = true;
            buttonPause.Enabled = false;
            buttonStop.Enabled = false;
        }
        private void buttonSearch_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Text file|*.txt";
            op.Title = "Select output text file";
            op.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString();

            if (op.ShowDialog() == DialogResult.OK)
                ouputText.Text = op.FileName;
        }
        #endregion

        #region Timer
        private void CooldownTimer_Tick(object sender, EventArgs e)
        {
            if (Cooldown > 0)
            {
                Cooldown--;
                updateTimerLabels();
                updateOutput(ouputText.Text);
            }
            else
            {
                CooldownTimer.Stop();
                CooldownTimer.Enabled = false;

                buttonStart.Enabled = true;
                buttonPause.Enabled = false;
                buttonStop.Enabled = false;
            }
        }
        private void updateOutput(string path)
        {
            if (path == String.Empty)
                return;

            TimeSpan t = TimeSpan.FromSeconds(Cooldown);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path))
            {
                file.Write(String.Format(Format.Text, t.ToString()));
            }
        }
        #endregion

        #region Helper
        private void updateTimerLabels()
        {
            if (Cooldown > 0)
            {
                TimeSpan t = TimeSpan.FromSeconds(Cooldown);
                string[] timeSplit = t.ToString().Split(':');
                Hours.Text = timeSplit[0];
                Minutes.Text = timeSplit[1];
                Seconds.Text = timeSplit[2];
            }
            else
            {
                if (numericSeconds?.Value != null)
                    Seconds.Text = numericSeconds.Value.ToString();
                if (numericMinutes?.Value != null)
                    Minutes.Text = numericMinutes.Value.ToString();
                if (numericHours?.Value != null)
                    Hours.Text = numericHours.Value.ToString();
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("{0} = Time will be print here.\n^ This must be included in format! Other ways timer will not been shown!\n\nHere is one example format what you can try out:\nStream start soon, {0}!");
        }
        #endregion

        #region JSON
        private void writeData()
        {
            Storing _storing = new Storing();

            _storing.hours = Convert.ToInt32(numericHours.Value);
            _storing.minutes = Convert.ToInt32(numericMinutes.Value);
            _storing.seconds = Convert.ToInt32(numericSeconds.Value);

            _storing.path = ouputText.Text;
            _storing.format = Format.Text;

            Debug.WriteLine(JsonConvert.SerializeObject(_storing));

            using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _storing);
            }
        }
        private void readData()
        {
            Storing _storing = JsonConvert.DeserializeObject<Storing>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json"));

            numericHours.Value = _storing.hours;
            numericMinutes.Value = _storing.minutes;
            numericSeconds.Value = _storing.seconds;

            ouputText.Text = _storing.path;
            Format.Text = _storing.format;
        }
        #endregion
    }
}

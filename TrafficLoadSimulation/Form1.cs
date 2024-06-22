using Microsoft.VisualBasic.ApplicationServices;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Unicode;
using System.Timers;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.Mime.MediaTypeNames;

namespace TrafficLoadSimulation
{
    public partial class Form1 : Form
    {
        System.Timers.Timer timer;
        int m, s;
        RequestUnderLoad requestTest;
        Requests requests;
        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(["GET", "POST", "DELETE","PUT"]);
            comboBox1.SelectedIndex = 0;
        }



        private async void button1_Click(object sender, EventArgs e)
        {
            if (tabPage1.Focused) { 
            //График avg response time
                chart1.ChartAreas[0].AxisX.Maximum = DateTime.Now.AddMinutes((int)numericUpDown2.Value).ToOADate();
                chart1.ChartAreas[0].AxisX.Minimum = DateTime.Now.ToOADate();

                chart1.ChartAreas[0].AxisY.Maximum = 100.0d;
                chart1.ChartAreas[0].AxisY.Minimum = 0.0d;

                chart1.ChartAreas[0].AxisX.LabelStyle.Format = "mm:ss";

                chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Seconds;
                chart1.ChartAreas[0].AxisX.Interval = 5;

                chart1.Series[0].XValueType = ChartValueType.DateTime;
                chart1.Series[0].YValueType = ChartValueType.Double;


                requestTest = new RequestUnderLoad { MethodHttp = comboBox1.SelectedText, Url = textBox1.Text, VirtualUser = (int)numericUpDown1.Value, TestDuration = (int)numericUpDown2.Value };
                //Таймер + запрос
                s = 0;
                m = (int)numericUpDown2.Value;
                timer.Start();
                textBox2.Text = (await requestTest.startTest()).ToString();
                timer.Stop();
                label4.Text = "Time: 00:00";
            }
            else
            {
                requests = new Requests(textBox1.Text,comboBox1.SelectedItem.ToString(),textBox5.Text,textBox4.Text);
                textBox3.Text =  (await requests.start()).ToString();
            }
    }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                var avgResponseTime = (double)requestTest.sumTime.Sum() / (requestTest.countRequest == 0 ? 1 : requestTest.countRequest);
                var avgErrorRate = (double)requestTest.errorRate;
                chart1.Series[1].Points.AddXY(DateTime.Now, avgErrorRate);
                chart1.Series[0].Points.AddXY(DateTime.Now, avgResponseTime);
                if (avgResponseTime > chart1.ChartAreas[0].AxisY.Maximum)
                {
                    chart1.ChartAreas[0].AxisY.Maximum = avgResponseTime + 10;
                }
                s = s - 1;
                if (s <= 0)
                {
                    m = m - 1;
                    s = 59;
                }
                label4.Text = $"Time: {m:D2}:{s:D2}";
            }));
        }

    }
}

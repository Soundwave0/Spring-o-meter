

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;



namespace Spring_o_meter
{
    
    public partial class Form1 : Form
    { 
        //initializing values for calibration
        private double signal_zero_state_calibration = 0;
        private double signal_test_weight_calibration = 0;
        private double to_force_map = 0;
        Boolean calibrated = false;
       

      //initalizing values for reading  
      //idea of using structs for this purpose
        public struct  data_trial
        {
            
            public data_trial(double pos, double force)
            {
                Pos = pos;
                Force = force;
            }
            public double Pos { get; set; }
            public double Force { get; set; }
        }
        data_trial current_data = new data_trial(0, 0);
        data_trial prev_data = new data_trial(0, 0);
        Stack<data_trial> data_trial_stack = new Stack<data_trial>();// will store the data in stack format
        private double spring_k = 0;
        private double mean_k = 0;
        private double sum = 0;
        // for position component
        private const double GEAR_RATIO = 3;//netweem driver and gear
        private const double DISTANCE_PER_ROTATION = 2; // distance in mm in distance per rotation
        private  const double IMPULSES_PER_ROTATION = 30;// impulses in the encoder
        //for the graph
        private const int k_stack_reading_depth = 5;// stack reading depth for graphing trials
        private Stack<double> stack_k = new Stack<double>(); // stack that stores the spring constants
        
        private int sample_count = 0;
        //for the port
        private string port = "NULL";
        Boolean com_connected = false;
        private Boolean kickpoint = false;
        private Boolean kickpoint_found = false;
        private double kickpoint_value = 0.15;
        private double sigweight;

        /* PROTCOL GUIDE STM(System Transfer Mechanism)
         * ONN = turn on calibration, led turns on 
         * OFF = turn off calibration, led turns off 
         * WGH = get the force signal 
         * POS = get the position 
         * IMP = get the Impulse counter 
         * COM = tests COMport connections should return OK
         * RST = resets the position should return SUC
         * TRE = tares the scale should return SUC
         * CAL = should calibrate the weight
         * TREF = tares fast during measurement
         * ERR = value received when request is an error
         * CALVAL = change the calibration value in the esp program
         * APUL = application pulls from microcontroller eeprom
         * MPUL = Microcontroller gets value from application
        */


        private double Impulse_to_position(double encoder_count)
        {
            return (1 / IMPULSES_PER_ROTATION) * (DISTANCE_PER_ROTATION / GEAR_RATIO) * encoder_count;
        }
        private void Force_Calibration()
        {
            if (calibration_Toggle.Text == "ON" && com_connected)
            {

                serialPort1.WriteLine("WGH");
                signal_test_weight_calibration = Convert.ToDouble(serialPort1.ReadLine());
                double sigdif = signal_test_weight_calibration - signal_zero_state_calibration;
                if (double.TryParse(textBox1.Text, out double testweight))//checks if is a double
                {
                    double sigweight = testweight / (sigdif);
                    to_force_map = sigweight * 9.806 / 1000;//now in newtons, gives newtons per sig unit
                    enter_Weight_Button.Text = "Calibrated";
                }
                else
                {
                    to_force_map = -1;
                    MessageBox.Show("please enter a valid value", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            
        }
        void Graph_K(Chart chartx, String series_name,Stack<data_trial> stack)
        {
            data_trial[] data_trial_array = stack.ToArray();
            SortedDictionary<double,double> keyValuePairs = new SortedDictionary<double,double>();
            

            for (int i =0; i < data_trial_array.Length;i++)
            {
                double key = Math.Round(Math.Abs(data_trial_array[i].Pos),2);
                double value = Math.Round(Math.Abs(data_trial_array[i].Force),2);
                keyValuePairs.Add(key, value);

            }
            foreach(var j in  keyValuePairs)
            {
                chartx.Series[series_name].Points.AddXY(j.Key, j.Value);
            }
            
        }
        private double Signal_To_Force(double Signal)
        {
            return Signal*to_force_map;
        }
        private String Get_STM(String code)
        {
            if (com_connected)
            {
                serialPort1.WriteLine(code);
                String stm_output = serialPort1.ReadLine();
                return stm_output;
            }
            else
            {
                MessageBox.Show("COM is not activated", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "ERR";
            }
            
        }
        
        private double Get_Position()
        {
            String imp = (Get_STM("IMP"));
            int current_impulse_count = Int32.Parse(imp);
            double current_position = (Impulse_to_position(current_impulse_count));
            return Math.Round(current_position,5);
        }
        private double Get_Force()
        {
           
            String output = Get_STM("WGH"); 
           double cur_sig =  Convert.ToDouble(output);
            double current_force = Math.Round(Signal_To_Force(cur_sig), 5);
            return current_force;
        }
        private void start_COM(String port_method)
        {
            serialPort1.PortName = port_method; //make sure to get the right COM
            serialPort1.Open();//opening the serial port
        }
        private Boolean Check_COM()
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            try
            {
                start_COM(port);
                serialPort1.Write("COM");
                //System.Threading.Thread.Sleep(200);
                string com_status = serialPort1.ReadLine();

                if (com_status == "OK")
                {
                    MessageBox.Show("Serial Connected successfully", "PORT NOTIFICATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    com_connected = true;
                    return true;
                }
                else
                {
                    MessageBox.Show("Correct the COM#", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Correct the COM# big boy error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }


        }
        private String COM_dialog()
        {
            Form2 port_dialog = new Form2();
            port_dialog.ShowDialog();
            while (port_dialog.finished == false)
            {
                
            }
            port = port_dialog.port;
            return port;
        }
        private void Graph_Stack(Chart chartx,int stack_reading_depth, Stack<double> stack, String series_name) //  fix this function
        {
            
            Double[] stack_array = stack.ToArray();
            double max = -1;
            if (chart1.Series.Count != 0)
            {
                chart1.Series.Clear();
                chart1.Series.Add(series_name);

            }

            for (int i = 0; i < stack_reading_depth; i++)
            {
                if (i<stack_array.Length)
                {
                    double sample_spring_constant = stack_array[i];
                    
                    if (max < sample_spring_constant) max = sample_spring_constant;
                    
                    chart1.Series[series_name].Points.AddXY(i, sample_spring_constant);
                    chart1.ChartAreas[0].AxisY.Maximum = max + 0.1 * max;
                    chart1.ChartAreas[0].AxisY.Minimum = 0;
                }

                else
                {
                    chart1.Series[series_name].Points.AddXY(i, 0);
                }
            }
           
        }
        static double standardDeviation(IEnumerable<double> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                double average = sequence.Average();
                double sum = sequence.Sum(d => Math.Pow(d - average, 2));
                result = Math.Sqrt((sum) / sequence.Count());
            }
            return result;
        }
        static double Mean(IEnumerable<double> sequence)
        {
            double result = 0;

            if (sequence.Any())
            {
                double average = sequence.Average();
                
            }
            return result;
        }

        public Form1()
        {
            InitializeComponent();
            timer1.Start();
        }

        private void Form1_Load(object sender, EventArgs e)//upon loading the form
        {
            
        }
        private void button1_Click(object sender, EventArgs e)// toggling the 
        {
            
            try
            {
                if (calibration_Toggle.Text == "OFF")
                {
                    calibration_Toggle.Text = "ON";
                    calibration_Toggle.BackColor = Color.Green;
                    serialPort1.Write("ONN\n");//turns led on stm on
                    serialPort1.WriteLine("TRE");
                    if (serialPort1.ReadLine() == "SUC")
                    {

                    }
                    else
                    {
                        MessageBox.Show("STMERROR", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }
                else
                {
                    serialPort1.WriteLine("TRE");
                    calibration_Toggle.Text = "OFF";
                    calibration_Toggle.BackColor = Color.Red;
                    serialPort1.Write("OFF\n");
                }
            }
            catch
            {
                MessageBox.Show("PORT is not configured", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)//complete calibration
        {
            if (calibration_Toggle.Text == "ON" )
            {
                Force_Calibration();
                calibrated = true;
                String response = Get_STM("MPUL");
                if (response == "MAPREQ")
                {
                    String sigweight_string = sigweight.ToString();
                    serialPort1.WriteLine(sigweight_string);

                }


            }
            else if(calibration_Toggle.Text == "OFF")
            {
                MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
           

        }

      


        private void button2_Click(object sender, EventArgs e)//calibrating the self_weight;//pokelpokel
        {
           
            if (calibration_Toggle.Text == "ON" && !calibrated)
            {
                serialPort1.WriteLine("CAL");   
                String output = Get_STM("WGH");
                signal_zero_state_calibration =  Convert.ToDouble(serialPort1.ReadLine());
                button2.Text = "Calibrated";
                label23.Text = "LOAD TEST";
                label23.BackColor = Color.Green;
               
            }
            else if(calibrated)
            {
                MessageBox.Show("Restart to calibrate", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


        }

        private void button1_Click_2(object sender, EventArgs e)//sampling button add to chart also
        {
            if (button4.BackColor != Color.Green)
            {
                MessageBox.Show("calibrate", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            prev_data = current_data;
            sample_count++;
            
            current_data = new data_trial(Get_Position(), Get_Force());
            data_trial_stack.Push(current_data);
            label12.Text = current_data.Pos.ToString();
            label13.Text = current_data.Force.ToString();
            label14.Text = prev_data.Pos.ToString();
            label15.Text = prev_data.Force.ToString();
            if (prev_data.Pos == current_data.Pos)
            {
                MessageBox.Show("change position to attain measurement", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (sample_count >= 2)
            {
                spring_k = Math.Round(Math.Abs((current_data.Force - prev_data.Force) / (current_data.Pos - prev_data.Pos)), 3);
                sum += spring_k;
                mean_k = Mean(stack_k);
                label17.Text = spring_k.ToString();
                stack_k.Push(spring_k);
                try
                {
                    Graph_Stack(chart1, k_stack_reading_depth, stack_k, "K");// to graph the whole thing by displaying
                }
                catch
                {
                    stack_k.Pop();
                    stack_k.Push(0);
                    MessageBox.Show("Graph error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            //last 5 measurements from the stack in the form of a line graph


        }
        private void button3_Click(object sender, EventArgs e)
        {
            String port_output  = COM_dialog();
            label4.Text = port_output;
        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            if(Check_COM())
            {
                button4.BackColor = Color.Green;
               

            }
            else button4.BackColor = Color.Red;

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
           if (com_connected&&calibrated)
             {
                double force = Get_Force();
                double position = Get_Position();
                label20.Text = Math.Round(position,5).ToString()+" mm";
                label22.Text = Math.Round(force, 5).ToString()+" N";
                if(!kickpoint_found && kickpoint)
                {
                    if(Math.Abs(force) >=kickpoint_value)
                    {
                        kickpoint_found = true;
                        label30.Text = Math.Round(position, 5).ToString() + " mm";
                    }

                }

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("TRE");
            if(serialPort1.ReadLine()=="SUC")
            {
               
            }
            else
            {
                MessageBox.Show("STMERROR", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(Get_STM("RST")=="SUC")
            {

            }
            else
            {
                MessageBox.Show("STMERROR", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Graph_K(chart2,"KC", data_trial_stack);
            label25.Text = Math.Abs(Math.Round(sum/sample_count,3)).ToString();
            double stand_div = standardDeviation(stack_k.ToArray());
            label27.Text= Math.Round(stand_div,5).ToString();



        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            if(button8.Text == "OFF")
            {
                button8.Text = "ON";
                button8.BackColor = Color.Green;
                kickpoint = true;
            }
            else
            {
                button8.Text = "OFF";
                button8.BackColor = Color.Red;
                kickpoint = false;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            stack_k.Clear();
            sample_count = 0;
            label12.Text = "0"; 
            label13.Text = "0";
            label14.Text = "0";
            label15.Text = "0";
            label25.Text = "NULL";
            label30.Text = "NULL";
            chart1.Series.Clear();
            chart1.Series.Add("K");
            chart2.Series.Clear();
            chart2.Series.Add("KC");

        }

        private void button10_Click(object sender, EventArgs e)
        {
            calibrated = true;
            String output = Get_STM("APUL");
            to_force_map = Convert.ToDouble(output) * 9.806 / 1000;

            
        }
    }
}



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
        private double force;
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
        private const double GEAR_RATIO = 3;
        private const double DISTANCE_PER_ROTATION = 2;
        private  const double IMPULSES_PER_ROTATION = 30;
        //for the graph
        private const int k_stack_reading_depth = 5;
        private Stack<double> stack_k = new Stack<double>();
        
        private int sample_count = 0;
        //for the port
        private string port = "NULL";
        Boolean com_connected = false;
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
         * CALVAL = change the calibration value in the program
         * 
        */

        // #TODO get only clicker position from the STM calculate position here where it is much more accurate




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
                if (double.TryParse(textBox1.Text, out double testweight))//chekcs if is a double
                {
                    double a = testweight / (sigdif);
                    to_force_map = a * 9.806 / 1000;//now in newtons, gives newtons per sig unit
                    enter_Weight_Button.Text = "Calibrated";
                }
                else
                {
                    to_force_map = -1;
                    MessageBox.Show("please enter a valid value", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            
        }
        private double Signal_To_Force(double Signal)
        {
            return Signal*to_force_map;
        }
        private String Get_STM(String code)//reading output from stm // this requires some fixing
        {
            if (com_connected)
            {
                serialPort1.WriteLine(code);
                2String stm_output = serialPort1.ReadLine();
                return stm_output;
            }
            else
            {
                MessageBox.Show("COM is not activated", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return "ERR";
            }
            // below code is for parsing and checking values , create an alternate function for this purpose 
           /* if (double.TryParse(serialPort1.ReadLine(), out double stm_output))//chekcs if is a double
            {
                return stm_output;
            }
            else
            {

                MessageBox.Show("serial port error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }
            */
        }
        
        private double Get_Position()
        {
            String imp = (Get_STM("IMP"));
            int current_impulse_count = Int32.Parse(imp);
            double current_position = (Impulse_to_position(current_impulse_count));
            return current_position;
        }
        private double Get_Force()
        {
           
            String output = Get_STM("WGH"); 
           double cur_sig =  Convert.ToDouble(output);
            double current_force = Signal_To_Force(cur_sig);
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
            Stack<Double> stack_copy = stack;
            
            if (chart1.Series.Count != 0)
            {
                chart1.Series.Clear();
                chart1.Series.Add(series_name);

            }


            for (int i = 0; i < stack_reading_depth; i++)
            {
                if (stack_copy.Count > 0)
                {
                    double sample_spring_constant = stack_copy.Pop();
                    sum += sample_spring_constant;
                    chart1.Series[series_name].Points.AddXY("T" + i, sample_spring_constant);

                }

                else
                {
                    chart1.Series[series_name].Points.AddXY("TNULL", 0);
                }
            }
            
           

            //calculates the mean of the different reading values
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
      
            }
            else
            {
                MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

      


        private void button2_Click(object sender, EventArgs e)//calibrating the self_weight;
        {
           
            if (calibration_Toggle.Text == "ON")
            {
                
                
                serialPort1.WriteLine("CAL");//calibrates based on constant provided
                
                String output = Get_STM("WGH");
                signal_zero_state_calibration =  Convert.ToDouble(serialPort1.ReadLine());
                button2.Text = "Calibrated";
                label23.Text = "LOAD TEST";
                label23.BackColor = Color.Green;
               
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
            current_data = new data_trial(Get_Position(),Get_Force());
            data_trial_stack.Push(current_data);
            label12.Text = current_data.Pos.ToString();
            label13.Text = current_data.Force.ToString();
            label14.Text = prev_data.Pos.ToString();
            label15.Text = prev_data.Force.ToString();
            spring_k = Math.Abs((current_data.Force - prev_data.Force)/(current_data.Pos - prev_data.Pos));
            label17.Text = spring_k.ToString();
            stack_k.Push(spring_k);
            
            Graph_Stack(chart1,k_stack_reading_depth,stack_k,"K");// to graph the whole thing by displaying
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
                
                label20.Text = Get_Position().ToString();
                label22.Text = Get_Force().ToString();

                //implement a stack that will continously read position data and plot it against time
                //implement a force stack that will do the same but with force
                //implement a spring constant stack that will do the same but with series measurements

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
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Spring_o_meter
{
    
    public partial class Form1 : Form
    { //initializing values for calibration
        private double signal_zero_state_calibration = 0;
        private double signal_test_weight_calibration = 0;
        private double to_force_map = 0;
      //initalizing values for reading  
        private double signal_current_force = 0;
        private double signal_current_position = 0;
        private double signal_prev_force = 0;
        private double signal_prev_position = 0;
        private double newts_current_force = 0;
        private double mm_current_position = 0;
        private double newts_prev_force = 0;
        private double mm_prev_position = 0;
        private double spring_k = 0;
        private double mean_k = 0;
        private double sum = 0;
        //for the graph
        private const int stack_reading_depth = 5;
        private Stack<double> stack_k = new Stack<double>();
        private int sample_count = 0;
        //for the port
        private string port = "NULL";//change into variable port
        /*
         * ONN = turn on calibration, led turns on
         * OFF = turn off calibration, led turns off
         * XRC = force signal
         * POS = get the position
         * FRC = get the force
         * XOS = position signal
         
         */

        
        private void Force_Calibration()
        {
            if (calibration_Toggle.Text == "ON")
            {
                double sigdif = signal_test_weight_calibration - signal_zero_state_calibration;
                if (int.TryParse(textBox1.Text, out int testweight))//chekcs if is a double
                {
                    double a = sigdif / (testweight);
                    to_force_map = a * 9.806 / 1000;//now in newtons
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
            return Signal *to_force_map;
        }
        private double Get_STM(String code)//reading output from stm
        {
            serialPort1.Write(code+"\n");// signal_zero_state_calibration = read force from stm
            System.Threading.Thread.Sleep(200);
            if (double.TryParse(serialPort1.ReadLine(), out double stm_output))//chekcs if is a double
            {
                return stm_output;
            }
            else
            {

                MessageBox.Show("serial port error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }
     
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
                System.Threading.Thread.Sleep(200);
                string com_status = serialPort1.ReadLine();

                if (com_status == "OK")
                {
                    MessageBox.Show("Serial Connected successfully", "PORT NOTIFICATION", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show("Correct the COM#", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //this.Close();
                    return false;
                }
            }
            catch
            {
                MessageBox.Show("Correct the COM# big boy error", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //this.Close();
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
       

        public Form1()
        {
            InitializeComponent();
           
            
        }

        private void Form1_Load(object sender, EventArgs e)//upon loading the form
        {
            

        }

        private void label1_Click(object sender, EventArgs e)
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


                }
                else
                {
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
      
            }
            else
            {
                MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)//calibrating the self_weight;
        {
           
            if (calibration_Toggle.Text == "ON")
            {
                button2.Text = "Calibrated";
                //signal_zero_state_calibration = Get_STM("XRC");
            }
            else
            {
                MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


        }

        private void button1_Click_2(object sender, EventArgs e)//sampling button add to chart also
        {
            if(button4.BackColor!=Color.Green) MessageBox.Show("Turn On calibration", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning); ;
            sample_count++;
            //calculate the spring constant by pinging the stm with FRC and POS and
            //subtracting the current and previous measurement and displacements dividng and doing absolute value
            // this value will 
            //push current calculated spring constant onto the stack
            Stack<Double> stack_k_copy = stack_k;
            if (chart1.Series.Count != 0)
            {
                chart1.Series.Clear();
                chart1.Series.Add("K");

            }

            
            for (int i =0; i < stack_reading_depth;i++)
            {
                if (stack_k_copy.Count != 0)
                {
                    double sample_spring_constant = stack_k_copy.Pop();
                    sum += sample_spring_constant;
                    chart1.Series["K"].Points.AddXY("T"+sample_count, sample_spring_constant);
                    

                    //add line to plot the specified point to the chart
                }

                else
                {
                    chart1.Series["K"].Points.AddXY("TNULL", 0);
                }
            }
            mean_k = sum / sample_count;//calculates the mean of the different reading values

        }

        private void chart1_Click(object sender, EventArgs e)
        {
            
        }

      
        private void label4_Click_1(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String port_output  = COM_dialog();
            label4.Text = port_output;
        }

        private void label19_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if(Check_COM())
            {
                button4.BackColor = Color.Green;
            }
            else button4.BackColor = Color.Red;

        }
    }
}

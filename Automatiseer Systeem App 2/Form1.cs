using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Configuration;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using System.Globalization;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Controls;
using System.Windows.Media;
using Label = System.Windows.Forms.Label;

namespace Automatiseer_Systeem_App_1
{
    public partial class Form1 : Form
    {
        //-------------------------Variabelen Cyclus Programma-------------------------------------------------------

        System.Timers.Timer ProgrammaUitvoerTimer;
        System.Timers.Timer ReadSerialDataTimer;
        System.Timers.Timer millisecTimer;
        int s;
        int[] homeposition = { 20, 30, 10 };//angle in degrees
        int[] zeroarray = { 0, 0, 0 };
        int StappenLijstIndex = 0;
        static bool progamOn = false;
        static bool moveready = false;
        string statustext = "";
        List<Stap> StappenLijst = new List<Stap>();
        int loops_done;
        string GcodePath = @"C:\Users\rikve\Desktop\Programmeer\BaseMapCodeer\Projecten Codeer\Automatiseer Systeem App 2\Resources\Gcode path";
        bool wait_untill_ready = false;
        private Label[] OutputPinLabels = new Label[5];
        private Label[] InputPinLabels = new Label[5];
        private PictureBox[] OutputPinImages = new PictureBox[5];
        private PictureBox[] InputPinImages = new PictureBox[5];

        char[] InputPinsStatus;
        char[] OutputPinsStatus;
        //------------------------------Cyclus Programma--------------------------------------------------------------------------------

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProgrammaUitvoerTimer = new System.Timers.Timer();
            ProgrammaUitvoerTimer.Interval = 1000;
            ProgrammaUitvoerTimer.Elapsed += OnTimeEventProgramma;
            ReadSerialDataTimer = new System.Timers.Timer();
            ReadSerialDataTimer.Interval = 500;
            ReadSerialDataTimer.Elapsed += OnTimeEventSerial; 
            millisecTimer = new System.Timers.Timer();
            millisecTimer.Interval = 1;
            MakePinsUI();
            //millisecTimer.Elapsed += ;

        }//beschrijft timer

        private void MakePinsUI()
        {
            int i;
            for (i = 0; i < 5; i++)
            {
                OutputPinLabels[i] = new Label();
                OutputPinLabels[i].Location = new System.Drawing.Point(10, (330 + (i * 15))); // Adjust the location as needed
                OutputPinLabels[i].Text = "O " + (i + 1) + ":";
                OutputPinLabels[i].Size = new System.Drawing.Size(40, 15);
                this.Controls.Add(OutputPinLabels[i]); // Adding to the Form's Controls collection
                OutputPinImages[i] = new PictureBox();
                OutputPinImages[i].Location = new System.Drawing.Point(50, (331 + (i * 15))); // Adjust the location as needed
                OutputPinImages[i].Size = new System.Drawing.Size(13, 13); // Adjust the size as needed
                OutputPinImages[i].Image = Properties.Resources.GREEN;
                this.Controls.Add(OutputPinImages[i]); // Adding to the Form's Controls collection
            }
            for (int j = 0; j < 5; j++)
            {
                InputPinLabels[j] = new Label();
                InputPinLabels[j].Location = new System.Drawing.Point(10, (330 + ((i + j) * 15))); // Adjust the location as needed
                InputPinLabels[j].Text = "I " + (j + 1) + ":";
                InputPinLabels[j].Size = new System.Drawing.Size(40, 15);
                this.Controls.Add(InputPinLabels[j]); // Adding to the Form's Controls collection
                InputPinImages[j] = new PictureBox();
                InputPinImages[j].Location = new System.Drawing.Point(50, (331 + ((i + j) * 15))); // Adjust the location as needed
                InputPinImages[j].Size = new System.Drawing.Size(13, 13); // Adjust the size as needed
                InputPinImages[j].Image = Properties.Resources.GREEN;
                this.Controls.Add(InputPinImages[j]); // Adding to the Form's Controls collection
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ReadSerialDataTimer.Stop();
        }

        private static void DataReceivedHandler(
                       object sender,
                       SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            Console.WriteLine("Data Received:");
            Console.Write(indata);
        }
        public void OnTimeEventProgramma(object sender, ElapsedEventArgs e)
        {
           Invoke(new Action(() =>
            {
                s += 1;
                int sec = s;
                ProgramTimerLabel.Text = s.ToString();
                // SerialOutputValueLabel.Text = serialtext;
                // StatusWaardeLabel.Text = statustext;
            }));
            if (StappenLijstIndex == StappenLijst.Count() - 1)
            {
                loops_done += 1;
                Invoke(new Action(() =>
                {
                    CyclussenKlaarValueLabel.Text = loops_done.ToString();
                }));
                if (loops_done < program_info.total_loops_goal)
                    Loop_Re_Start();
                else
                    StopProgramButton_Click(null,null);
            }
            //  moveready = true;
            if (progamOn && moveready)// StappenLijstIndex < StappenLijst.Count()
            { 
                RunProgram();
            }
        }//Voert uit per 1000ms wanneer timer.Start()
        public void OnTimeEventSerial(object sender, ElapsedEventArgs e)
        {

            Invoke(new Action(() =>
            {
                SerialInputTextBox.Text = currentReceived;
                //checkSensors();
                // StatusWaardeLabel.Text = statustext;
            }));
            //  moveready = true;

        }//Voert uit per 500ms wanneer timer.Start()

        /*private void checkSensors()
        {
            if(currentReceived == "Z")
            {
                ProgrammaUitvoerTimer.Stop();
            }
            if(currentReceived == "A" && ProgrammaUitvoerTimer.Enabled == false)
            {
                ProgrammaUitvoerTimer.Start();
            }
            
        }*/
        
        private void OpenProgramButton2_Click(object sender, EventArgs e)
        {
            Stream st;
            int i;
            OpenFileDialog d1 = new OpenFileDialog();
            loops_done = 0;
            // using (OpenFileDialog openFileDialog = new OpenFileDialog())
            d1.InitialDirectory = GcodePath;
            String str ="";
            if (d1.ShowDialog() == DialogResult.OK)
            {
                if ((st = d1.OpenFile()) != null)
                {
                    string file = d1.FileName;
                    str = File.ReadAllText(file);
                    ProgramShowTextBox.Text = str;
                }
                //"1 5 152 122 231\r\1 10 152 122 231"
                st.Close();
                string[] separator = {"\r\n", "For"};
                string[] lijnen = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                i = 0;
                new program_info(lijnen[i]);
                CyclusDoelValueLabel.Text = program_info.total_loops_goal.ToString();
                i++;
                try
                {
                  while(!string.IsNullOrWhiteSpace(lijnen[i]))
                    { 
                        StappenLijst.Add(new Stap(lijnen[i]));//parses, places values into variables. Does inversekin, makes Rcode(steps)
                        i++;
                    }
                }
                catch (Exception ex)
                {                   
                }     
            }
        }
        private void StartProgramButton_Click(object sender, EventArgs e)
        {
            progamOn = true;
            moveready = true;
            ProgrammaUitvoerTimer.Start();
            StappenLijstIndex = 0;
            wait_untill_ready = false;
            // serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
        }//ProgramOn = true en timer start

        private void Loop_Re_Start()
        {
            s = 0;
            StappenLijstIndex = 0;
        }

        private void StopProgramButton_Click(object sender, EventArgs e)
        {
            progamOn = false;
            ProgrammaUitvoerTimer.Stop();
            s = 0;
            ProgramTimerLabel.Text = "0";
            SerialOutputValueLabel.Text = "---";
            StatusWaardeLabel.Text = "----";
        }

        private void PauzePlayButton_Click(object sender, EventArgs e)
        {
            ProgrammaUitvoerTimer.Stop();
        }


        public void RunProgram()
        {
           // bool a = false;//deze switch moet nog weg waarshijnlijk
            Invoke(new Action(() =>
            {
                switch (wait_untill_ready)
                {
                    case true:
                        statustext = "Status: voorwaardes ontbreken";
                        ProgrammaUitvoerTimer.Stop();
                        break;
  
                    default:
                        if (s == StappenLijst[StappenLijstIndex].Starttijd)
                        {
                            StatusWaardeLabel.Text = "Status: geen voorwaardes ontbreken";
                            CodeInputLabel.Text = $"J1:{StappenLijst[StappenLijstIndex].J1pos} J2: {StappenLijst[StappenLijstIndex].J2pos} J3: {StappenLijst[StappenLijstIndex].J3pos}";
                            SerialOutputValueLabel.Text = StappenLijst[StappenLijstIndex].Rcode;
                            if(serialPort1.IsOpen)
                                serialPort1.WriteLine(StappenLijst[StappenLijstIndex].Rcode);
                            else
                                StatusWaardeLabel.Text = "nothing send, serial not open";
                            StappenLijstIndex++;
                        }
                        break;
                }
            }
               ));
           }//Functie bij StartProgramButton, wanneer starttijd stap waar is: status en stap weergeven, waardes van commandos per stap versturen via serial in juiste format
        //stuurt command, als niet ok is en niet uitvoert. dan wachten met volgende stap én volgende cyclus(indien laatste)

        //--------------------------------COM PORTS-----------------------------------------------------------
        string currentReceived = string.Empty;
        string receivedStr = string.Empty;
        char[] strarray = new char[128];
        string SerialFlag = "";
        string[] ParseString = new string[128];
        char[] Digital_Pin_States = new char[128];
        
        private void OpenPortsButton_Click(object sender, EventArgs e)
        {
            
            try
            {
                serialPort1.PortName = COMselectSwitchButton.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.Open();
                ReadSerialDataTimer.Start();
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

                J1AngleTextbox.Enabled = true;
                J2AngleTextbox.Enabled = true;
                Z1PositionTextBox.Enabled = true;
                J1StuurButton.Enabled = true;
                J2StuurButton.Enabled = true;
                Z1StuurButton.Enabled = true;
                StatusWaardeLabel.Text = COMselectSwitchButton.Text +" gekozen";
                OpenPortsButton.Enabled = false;
            }
            catch (System.IO.IOException)
            {
                StatusWaardeLabel.Text = COMselectSwitchButton.Text + " poort niet open";
            }
        }

        void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            currentReceived = serialPort1.ReadLine();
            //receivedStr = receivedStr + currentReceived;
            if(currentReceived != null)
            {
                strarray = currentReceived.ToCharArray();
                Invoke(new Action(() =>
                {
                    // checkSensors();
                    if (currentReceived.Length > 2)
                        parseSerial();
                    SerialInputTextBox.Text = currentReceived;
                    // StatusWaardeLabel.Text = statustext;
                }));
              /*  if (strarray[0] == 'A' && ProgrammaUitvoerTimer.Enabled == false)
                {
                    ProgrammaUitvoerTimer.Start();
                }*/
            }
        }

        void CompareInputsAndConditions()
        {
            //in condities: 12-0 10-0 6-1
            //in statusstring: 0110101011
            //while()
            //StappenLijst[StappenLijstIndex].Conditi

        }



        //serial in (not ok)   : not ok 12-0 10-0 6-1
        //serial in (ok)   :    ok
        public void parseSerial()
        {
            string not_ok = "not ok";
            string ok_after_not_ok = "ok";
            string pinnbr;
            char on_or_off;
            //StappenLijst[StappenLijstIndex].Condition01
            
            //Console.WriteLine(currentReceived);
            //split into parts in pinorder with name 
            if (strarray.Equals(not_ok))
            {

                
                ProgrammaUitvoerTimer.Stop();
                statustext = "Wachten.. voorwaardes ontbreken";
            }
            if (strarray.Equals(ok_after_not_ok))
            {
                ProgrammaUitvoerTimer.Start();
                statustext = "voorwaardes weer ok";
            }

            if (strarray[1] == 'I')//stuurt deze elke keer er iets verandert en wanneer de controller opstart
                UpdatePinStatus(currentReceived, true);
            if (strarray[1] == 'O')
                UpdatePinStatus(currentReceived, false);
        }

        private void UpdatePinStatus(string statusData, bool InOrOut)
        {
            int i = 0;
            while (i < statusData.Length)
            {
                if (statusData[i] == '1' && i >= 2)
                {
                    SetPinStatus(i - 2, true, InOrOut);
                }
                else if (statusData[i] == '0' && i >= 2)
                {
                    SetPinStatus(i - 2, false, InOrOut);
                }
                i++;
            }
        }
        private void SetPinStatus(int pinIndex, bool status, bool InIsOne)
        {
         
            try
            {
                if(InIsOne)
                {
                    if (status)
                        InputPinImages[pinIndex].Image = Properties.Resources.GREEN;
                    else
                        InputPinImages[pinIndex].Image = Properties.Resources.RED;
                    if (status)
                        InputPinsStatus[pinIndex] = '1';
                    else
                        InputPinsStatus[pinIndex] = '0';
                }
                else
                {
                    if (status)
                        OutputPinImages[pinIndex].Image = Properties.Resources.GREEN;
                    else
                        OutputPinImages[pinIndex].Image = Properties.Resources.RED;
                    if (status)
                        OutputPinsStatus[pinIndex] = '1';
                    else
                        OutputPinsStatus[pinIndex] = '0';
                }
            }
            catch(Exception)
            {

            }
        }

        private void DisconnectSerialsButton_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            ReadSerialDataTimer.Close();
            J1AngleTextbox.Enabled = true;
            J2AngleTextbox.Enabled = true;
            Z1PositionTextBox.Enabled = true;
            J1StuurButton.Enabled = true;
            J2StuurButton.Enabled = true;
            Z1StuurButton.Enabled = true;
            OpenPortsButton.Enabled = true;
        }
        private void COMselectSwitchButton_Click(object sender, EventArgs e)
        {

            if (COMselectSwitchButton.Text == "COM3")
            {
                COMselectSwitchButton.Text = "COM6";
            }
            else if (COMselectSwitchButton.Text == "COM6")
            {
                COMselectSwitchButton.Text = "COM7";
            }
            else if (COMselectSwitchButton.Text == "COM7")
            {
                COMselectSwitchButton.Text = "COM8";
            }
            else if (COMselectSwitchButton.Text == "COM8")
            {
                COMselectSwitchButton.Text = "COM9";
            }
            else if (COMselectSwitchButton.Text == "COM9")
            {
                COMselectSwitchButton.Text = "COM3";
            }
        }

        //-----------------Remote--------------------------------------------------------------------------
        private void J1StuurButton_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("Ja" + J1AngleTextbox.Text);
            J1AngleTextbox.Text = "";
        }
        private void J2StuurButton_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("Jb" + J2AngleTextbox.Text);
            J2AngleTextbox.Text = "";
        }
        private void Z1StuurButton_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine("Z1" + Z1PositionTextBox.Text);
            Z1PositionTextBox.Text = "";
        }
        //-----------------Manual programma invoer---------------------------------------------------------
        //-----------------Variabelen-------
        List<newstep_makeline> UploadStappenlijst = new List<newstep_makeline>();
        List<Stap> StepListForValidation = new List<Stap>();
        int IndexVali = 0;
        //---------------------------------

        public void NieuwProgrammaButton_Click(object sender, EventArgs e)
        {
            SaveNewProgramButton.Enabled = true;
            StapStarttijdTextbox.Enabled = true;
            StapPosXTextbox.Enabled = true;
            StapPosYTextbox.Enabled = true;
            StapPosZTextbox.Enabled = true;
            SaveStapButton.Enabled = true;
            NewFileNameTextBox.Enabled = true;
            
        }

        private void SaveNewProgramButton_Click(object sender, EventArgs e)
        {
            string fileName = NewFileNameTextBox.Text;
            string path = GcodePath + fileName + ".txt";
            string totaal = "";

            string temp;
            int time_one = 0;
            int time_two = 0;
            int test = 0;
            int nestedloopmax = UploadStappenlijst.Count() - 1;

            for (int i = 0; i < UploadStappenlijst.Count() - 1; i++)
            {
                for (int j = 0; j < nestedloopmax; j++)
                {
                    for (int y = 0; UploadStappenlijst[j].lijn[y] != ' '; y++)
                        time_one = (time_one * 10) + UploadStappenlijst[j].lijn[y] - 48;//parse time
                    for (int y = 0; UploadStappenlijst[j + 1].lijn[y] != ' '; y++)
                        time_two = (time_two * 10) + UploadStappenlijst[j + 1].lijn[y] - 48;
                    if (time_one > time_two)                                            //Swap Steplines base on time
                    {
                        temp = UploadStappenlijst[j].lijn;                              //j into temp
                        UploadStappenlijst[j].lijn = UploadStappenlijst[j + 1].lijn;    //swap next into j.
                        UploadStappenlijst[j + 1].lijn = temp;                          //swap temp into next.
                    }
                    time_one = 0;
                    time_two = 0;
                }
                nestedloopmax--;
            }//sorteer op starttijd
            totaal = TotaalCyclussenInputValueLabel.Text + " " + "\r\n";
            for (int i = 0; i < UploadStappenlijst.Count(); i++)
            {
                totaal = totaal + UploadStappenlijst[i].lijn;
            }
            TestSaveStapWaardeTextBox.Text = totaal;
            if (!File.Exists(path))
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(totaal);
                }
            }
            SaveNewProgramButton.Enabled = false;
            StapStarttijdTextbox.Enabled = false;
            StapPosZTextbox.Enabled = false;
            SaveStapButton.Enabled = false;
            NewFileNameTextBox.Enabled = false;
            IndexVali = 0;
        }

        private void SaveStapButton_Click(object sender, EventArgs e)
        {
            string vaccuumBool = "x";
            string Duratie = "x";
            string PreConditions = "";
            CheckedListBox.CheckedIndexCollection selectedIndices = PreCondiCheckedListBox.CheckedIndices;
            int i = 0;

            foreach (int index in selectedIndices)
            {
                PreConditions = PreConditions + index.ToString() + " ";
                i++;
            }
            while(i < 3)
            {
                PreConditions = PreConditions + "x ";
                i++;
            }
            if (VaccuumZuigButton.Checked)
            {
                Duratie = VaccuumDuratieTextbox.Text;
                vaccuumBool = "V";
            }
            if (validatiecheck(StapPosXTextbox.Text, StapPosYTextbox.Text, StapPosZTextbox.Text))
            {   if(IndexVali == 0)
                {
                    StepListForValidation.Add(new Stap("0", homeposition[0].ToString(), homeposition[1].ToString(), homeposition[2].ToString(), zeroarray));
                    IndexVali++;
                }
                StepListForValidation.Add(new Stap(StapStarttijdTextbox.Text, StapPosXTextbox.Text, StapPosYTextbox.Text, StapPosZTextbox.Text, StepListForValidation[IndexVali - 1].rcode_string_array));

                //parses, places values into variables. Does inversekin, makes Rcode(steps)
                //compares previous Rcode with new Rcode and gives Duration.

               if (IndexVali > 0 && StepListForValidation[IndexVali].next_possible == StepListForValidation[IndexVali - 1].Starttijd)
                    UploadStappenlijst.Add(new newstep_makeline(StapStarttijdTextbox.Text, "x", "x", "x", vaccuumBool, Duratie, PreConditions));
                else
                    UploadStappenlijst.Add(new newstep_makeline(StapStarttijdTextbox.Text, StapPosXTextbox.Text, StapPosYTextbox.Text, StapPosZTextbox.Text, vaccuumBool, Duratie, PreConditions));
                //saves Gcode, xyz and conditions
                TestSaveStapWaardeTextBox.Text = TestSaveStapWaardeTextBox.Text + UploadStappenlijst.Last().lijn;
                DurationMoveValueLabel.Text = StepListForValidation[IndexVali].duration_string;
                NextTimeValueLabel.Text = StepListForValidation[IndexVali].next_possible.ToString();
                AAATest1.Text = "from:\t" + StepListForValidation[IndexVali - 1].Rcode;
                AAATest2.Text = "to:\t" + StepListForValidation[IndexVali].Rcode;
                IndexVali++;
                
            }
            else
                TestSaveStapWaardeTextBox.Text = "out of bounds\n";
        }

        bool validatiecheck(string x, string y, string z)
        {
            float bereik_max = 300;
            float bereik_min = 50;
            float blindspot_angle_min = 60;
            float blindspot_angle_max = 70;
            float Z_max = 230;
            float Z_min = -50;
            float bereik_len = (float)Math.Sqrt(Math.Pow(double.Parse(x), 2) + Math.Pow(double.Parse(y), 2));
            if(bereik_len < bereik_max && bereik_len > bereik_min && float.Parse(z) < Z_max && float.Parse(z) > Z_min)
                return (true);
            return (false);
        }
        

        private void OpenProgramButton_Click(object sender, EventArgs e)
        {
            FileName2TextBox.Enabled = false;
            ProgramShowTextBox.Enabled = false;

            Stream st;

            OpenFileDialog d1 = new OpenFileDialog();

            // using (OpenFileDialog openFileDialog = new OpenFileDialog())
            d1.InitialDirectory = GcodePath;

            if (d1.ShowDialog() == DialogResult.OK)
            {
                if ((st = d1.OpenFile()) != null)
                {
                    string file = d1.FileName;
                    String str = File.ReadAllText(file);
                    ProgramShowTextBox.Text = str;
                }
                string path = d1.InitialDirectory;
                string result;

                result = Path.GetFileNameWithoutExtension(d1.FileName);
                FileName2TextBox.Text = result;
                st.Close();
            }
        }

        private void CloseProgramButton_Click(object sender, EventArgs e)
        {
            ProgramShowTextBox.Enabled = false;
            ProgramShowTextBox.ReadOnly = true;
            FileName2TextBox.Enabled = false;
        }

        private void EditProgramButton_Click(object sender, EventArgs e)
        {
            ProgramShowTextBox.Enabled = true;
            ProgramShowTextBox.ReadOnly = false;
            FileName2TextBox.Enabled = true;
            FileName2TextBox.ReadOnly = false;
        }

        private void ReWriteProgramButton_Click(object sender, EventArgs e)
        {
            string Code = Convert.ToString(ProgramShowTextBox.Text);
            using (StreamWriter A = new StreamWriter(GcodePath + FileName2TextBox.Text + ".txt"))
                A.WriteLine(Code);
        }

        private void NewProgramButton_Click(object sender, EventArgs e)
        {
            FileName2TextBox.ReadOnly = false;
            ProgramShowTextBox.Enabled = true;
            ProgramShowTextBox.ReadOnly = false;
            FileName2TextBox.Enabled = true;
        }

        //-----------------Inverse Position Calcu---------------------------------------------------------

        private void NewCalcuButton_Click(object sender, EventArgs e)
        {
            InverXTextBox.Enabled = true;
            InverYTextBox.Enabled = true;
            InverZTextBox.Enabled = true;
            InverXTextBox.Text = "";
            InverYTextBox.Text = "";
            InverZTextBox.Text = "";
            J1WaardeLabel.Text = "";
            J2WaardeLabel.Text = "";
            ZWaardeLabel.Text = "";
        }

        private void CalculateButton_Click(object sender, EventArgs e)
        {
            float X = 1;
            float Y = 1;
            float Z = 1;
            try
            {
                Z = float.Parse(InverZTextBox.Text);
                Y = float.Parse(InverYTextBox.Text); //Desired Yposition of end-effector in mm
                X = float.Parse(InverXTextBox.Text); //Desired Y position of end-effector in mm
            }
            catch (FormatException)
            {
                StatusWaardeLabel.Text = "Format X,Y of Z inverse niet juist";
            }
            float r1 = 0;
            float phi1 = 0;
            float phi2 = 0;
            float phi3 = 0;
            float a1 = 100;
            float a2 = 200;
            float a3 = 100;
            float a4 = 200;
            float T1 = 0; // theta 1 in !radians
            float T2 = 0;
            //functie Theta1 neemt hoek in graden
            float bereik1 = (float)Math.Pow(X, 2) + (float)Math.Pow(Y, 2);

            //((X > 100 || X < -100) && (Y > 100 || Y < -100)
            // && (X < 400 || X > -400) && (Y < 400 || Y > -400))
            if (bereik1 < (float)Math.Pow(400, 2) && (bereik1 > (float)Math.Pow(100, 2)) && (Z > 0 && Z < 100))
            {
                r1 = (float)Math.Sqrt(X * X + Y * Y); //eq.1c

                phi1 = (float)Math.Acos(((a4 * a4) - (a2 * a2) - (r1 * r1)) / (-2 * a2 * r1)); //eq.2
                phi2 = (float)Math.Atan(Y / X); //eq.3
                T1 = phi2 - phi1; //eq.4
                phi3 = (float)Math.Acos(((r1 * r1) - (a2 * a2) - (a4 * a4)) / (-2 * a2 * a4)); //eq.5
                T2 = (float)3.141459 - phi3; //eq.6(180gr in radiants)
                float J1 = Reken.RadtoDegr(T1);
                float J2 = Reken.RadtoDegr(T2);

                string Joint1 = J1.ToString();
                string Joint2 = J2.ToString();
                string Zas = Z.ToString();

                J1WaardeLabel.Text = Joint1;
                J2WaardeLabel.Text = Joint2;
                ZWaardeLabel.Text = Zas;
            }
            if(Z > 100 || Z < 0)
            {
                ZWaardeLabel.Text = "OutRange";
            }
            if (bereik1 > (float)Math.Pow(400, 2) || (bereik1 < (float)Math.Pow(100, 2)))
            {
                J1WaardeLabel.Text = "OutRange";
                J2WaardeLabel.Text = "OutRange";
            }

            // else if( (bereik1 < (float)Math.Pow(400, 2)) && (bereik1 > (float)Math.Pow(100, 2)) )
            // {
            //     J1WaardeLabel.Text = "OutRange";
            //     J2WaardeLabel.Text = "OutRange";
            // }
            // else if (Z < 0 || Z > 100)
            // {
            //     ZWaardeLabel.Text = "OutRange";
            // }


        }
        private void SendPositionButton_Click(object sender, EventArgs e)
        {
           string a = "J1"+J1WaardeLabel.Text;
           string b = "J2"+J2WaardeLabel.Text;
           string c = "Z"+ZWaardeLabel.Text;


            try
            {
                serialPort1.WriteLine(a);
                serialPort1.WriteLine(b);
                serialPort1.WriteLine(c);
            }
            catch (System.InvalidOperationException)
            {
                StatusWaardeLabel.Text = "COM3 poort niet open";
            }
        }




        //--------------------------------Einde Inverse Pos Calcu---------------------------------------
    }
    public class MyClass
    {
        public int stapnaam;
        public string stapnaam1;
        public string positieXtext;
        public string positieYtext;
        public string positieZtext;
        public string starttijdtext;

        public string stap(string stap, string Xtext, string Ytext, string positieZ, string Starttijd)
        {
            stapnaam = Int32.Parse(stap);
            positieXtext = Xtext;
            positieYtext = Ytext;
            positieZtext = positieZ;

            starttijdtext = Starttijd;
            string text = "";
            if (stapnaam > 0)
            {
                stapnaam1 = stapnaam.ToString();
                text = stapnaam + " " + starttijdtext + " " + positieXtext + " " + positieYtext + " " + positieZtext + "\r\n";
                
            }
            return text;
        }

    }
    public class Reken
    {
        static public float RadtoDegr(float Rad)
        {
            float Degr = (Rad / (float)3.1414599) * (float)180;          
            return Degr;
        }

        static public string BounceValue(string a)
        {
            string b = a;
            return  b;
        }

    }
    
    public class Programma
    {
        float j1 = 1;
        float j2 = 1;
        float z = 1;
       
        //if()
        public float StatusOven(int Celsius, bool OvenLadeOpen)
        {
            return 0;
        }
        public float StatusBlower(int Celsius, bool BlowerLadeOpen)
        {
            return 0;
        }

        public float stap1Arm(string tijd, float J1, float J2, float AZ)
        {
            // if(tijd == "00:00:05:00")
            // {
            // }
            j1 = J1;
            string joint1 = j1.ToString();
            

            return 0;
        }
        static public float stap1Oven(string tijd, string status1, string status2)
        {
            // if(tijd == "00:00:05:00")
            // {
            // }




            return 0;
        }
        static public float stap1Blower(string tijd, string status1, string status2)
        {
            // if(tijd == "00:00:05:00")
            // {
            // }

            return 0;
        }
    }
}

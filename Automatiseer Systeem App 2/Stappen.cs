using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Automatiseer_Systeem_App_1
{
    class Stap
    {
        public string stapnaam;
        public int Starttijd;
        public float Xpos;
        public float Ypos;
        public float J1pos;
        public float J2pos;
        public float J3pos;
        public string Status;
        public float Zpos;

        public string Rcode = "";
        public string RcodePrevious = "";
        public int[] steps = { 0, 0, 0 };
        public float[] duration = { 0, 0, 0 };
        public string duration_string = "0";
        public float next_possible = 0;
        public int[] prev_rcode_string_array = { 0, 0, 0 };
        public int[] rcode_string_array = {0, 0, 0};
        int StappenJ1;
        int StappenJ2;
        public char[][] ConditieArray;
        public int Condition02;
        public int Condition03;
        int StappenJ3;
        int steps_per_rev = 200;
        int steps_per_sec = 1000;
        public Stap(string lijn)
        {
            //aa = lijn;
            //neemt huidige waardes van subdelen[aray] en wijst posities gevraagde posities toe
            if (!string.IsNullOrEmpty(lijn))
            {
                string[] subdelen = lijn.Split(' ');
                try
                {
                    int i = 0;
                    stapnaam = subdelen[0];
                    Starttijd = Int32.Parse(subdelen[0]);
                    Xpos = float.Parse(subdelen[1]);
                    Ypos = float.Parse(subdelen[2]);
                    Zpos = float.Parse(subdelen[3]);
                    while (i < 3)
                    { 
                        if (!string.IsNullOrEmpty(subdelen[4 + i]))
                            ConditieArray[i] = subdelen[4 + i].ToCharArray();
                        i++;
                    }
                    //InverseKinamaticsInClass();
                    InverseKinamaticsv2();
                    ToArduinoRcodeFunction();
                    //voert functie inverse uit met bovenstaande waardes, rekent nodige hoekstanden
                }
                catch (FormatException)
                {
                    Status = "Parsing error";
                }
            }
        }
        public Stap(string start, string x, string y, string z, int[] Input_Rcode_Ints)
        {
            Starttijd = Int32.Parse(start);
            prev_rcode_string_array = Input_Rcode_Ints;//references the same memory location now
            Starttijd = Int32.Parse(start);
            Xpos = float.Parse(x);
            Ypos = float.Parse(y);
            Zpos = float.Parse(z);

            InverseKinamaticsv2();
            ToArduinoRcodeFunction();
            duration_of_move();
        }
        public void duration_of_move()
        {
            for (int i = 0; i < 3; i++)
            {                                                                       // new  prev
                if (rcode_string_array[i] > prev_rcode_string_array[i])             // 2000 1000    2000 - 1000    = 1000
                    steps[i] = rcode_string_array[i] - prev_rcode_string_array[i];
                if (rcode_string_array[i] < prev_rcode_string_array[i])             // 1000 2000    2000 - 1000    = 1000
                    steps[i] = prev_rcode_string_array[i] - rcode_string_array[i];  // 1000 -2000   1000 - (-2000) = 3000
                if (rcode_string_array[i] == prev_rcode_string_array[i])
                    steps[i] = 0;
                duration[i] = (float)steps[i] / (float)steps_per_sec;
                duration_string = duration_string + duration[i].ToString() + " ";
            }
          /*  duration_string = duration_string + "\n Previous:";
            for (int i = 0; i < 3; i++)
                duration_string = duration_string + prev_rcode_string_array[i].ToString() + " ";
            duration_string = duration_string + "\n Current:";
            for (int i = 0; i < 3; i++)
                duration_string = duration_string + code_string_array[i].ToString() + " ";*/
            for(int i = 0; i < 3; i++)
            {
                if (duration[i] > next_possible)
                    next_possible = duration[i];
            }
            next_possible = next_possible + Starttijd;
            for (int i = 0; i < 3; i++)
                RcodePrevious = RcodePrevious + " " + rcode_string_array[i];

        }
        public void ToArduinoRcodeFunction()
        {
            StappenJ1 = DegreesToSteps(J1pos);
            StappenJ2 = DegreesToSteps(J2pos);
            StappenJ3 = DegreesToSteps(J3pos);

            rcode_string_array[0] = StappenJ1;
            rcode_string_array[1] = StappenJ2;
            rcode_string_array[2] = StappenJ3;//for some reason

            //if it is a robot motion command
            Rcode = "M " + StappenJ1 + " " + StappenJ2 + " " + StappenJ3;
            //if it is a vaccuum command
            //Rcode = "V " + StappenJ1 + " " + StappenJ2 + " " + StappenJ3;
            //if it is a blower command

            //if it is a oven command

            //if it is a station 1 command
            //if it is a station 2 command

            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(new string(ConditieArray[i])))
                    Rcode = Rcode + " " + ConditieArray[i];
            }
            Rcode = Rcode + "\r\n";
        }
        
        int DegreesToSteps(float Angle)
        {
            int Compare;
            float min_angle = 0;
            float max_angle = 360;
            float gbratio = 40;
            float StepsPerRev = 400;
            float max_comp = StepsPerRev * gbratio;//motorsteps per rev * reductie
            float min_comp = 0;
            Compare = Convert.ToInt32(((max_comp - min_comp) / (max_angle - min_angle)) * (Angle - min_angle) + min_comp);
            return Compare;
        }
        
        public void InverseKinamaticsv2()
        {
            double Gc = Math.Sqrt(Math.Pow(Xpos, 2) + Math.Pow(Ypos, 2));
            double T1 = Math.Atan2(Xpos, Ypos);
            double a1 = 350;
            double a2 = 350;
            double D = (Math.Pow(Gc, 2) + Math.Pow(Zpos, 2) - Math.Pow(a1, 2) - Math.Pow(a2, 2)) / (2 * a1 * a2);
            // Adjusted calculation for T3 using the cosine law
            double T3 = Math.Atan2(-Math.Sqrt(1 - Math.Pow(D, 2)), D);
            double a2c2 = a2 * Math.Cos(T3);
            double a2s2 = a2 * Math.Sin(T3);
            double T2 = Math.Atan2(Gc, Zpos) - Math.Atan2(a1 + a2c2, a2s2);
            T2 *= -1;
            T3 += T2;

            J1pos = (float)((180 / Math.PI) * T1);
            J2pos = (float)((180 / Math.PI) * T2);
            J3pos = (float)((180 / Math.PI) * T3);
        }
            
    /*        public void InverseKinamaticsInClass()
        {
            
            
            float r1 = 0;
            float phi1 = 0;
            float phi2 = 0;
            float phi3 = 0;
            //float a1 = 100;
            float a2 = 350;
            //float a3 = 100;
            float a4 = 350;
            float T1 = 0; // theta 1 in !radians
            float T2 = 0;
            //functie Theta1 neemt hoek in graden
            //  float bereik1 = (float)Math.Pow(Xpos, 2) + (float)Math.Pow(Ypos, 2);

            //((X > 100 || X < -100) && (Y > 100 || Y < -100)
            // && (X < 400 || X > -400) && (Y < 400 || Y > -400))
            

            float L = (float)Math.Sqrt((Xpos * Xpos) + (Ypos * Ypos));//eq.1c pythagoras lengte extensie
            J1pos = Reken.RadtoDegr((float)Math.Atan(Xpos / Ypos));//base angle
            
            string Joint1 = J1pos.ToString();


            float h = (float)(Math.Sqrt(L * L + Zpos * Zpos));

            float phi = (float)(Math.Atan(Zpos / L) * (180 / 3.1415));

            float theta = (float)(Math.Acos((h / 2) / a2) * (180 / 3.1415));

            J2pos = phi + theta; // angle for first part of the arm
            J3pos = phi - theta; // angle for second part of the arm

*/

            /*
            if (bereik1 < (float)Math.Pow(400, 2) && (bereik1 > (float)Math.Pow(100, 2)) && (Zpos > 0 && Zpos < 100))
            {
                r1 = (float)Math.Sqrt(Xpos * Xpos + Ypos * Ypos); //eq.1c

                phi1 = (float)Math.Acos(((a4 * a4) - (a2 * a2) - (r1 * r1)) / (-2 * a2 * r1)); //eq.2
                phi2 = (float)Math.Atan(Ypos / Xpos); //eq.3
                T1 = phi2 - phi1; //eq.4
                phi3 = (float)Math.Acos(((r1 * r1) - (a2 * a2) - (a4 * a4)) / (-2 * a2 * a4)); //eq.5
                T2 = (float)3.141459 - phi3; //eq.6(180gr in radiants)
                float J1 = Reken.RadtoDegr(T1);
                float J2 = Reken.RadtoDegr(T2);

                string Joint1 = J1.ToString();
                string Joint2 = J2.ToString();
                string Zas = Zpos.ToString();
                J1pos = float.Parse(Joint1);
                J2pos = float.Parse(Joint2);

            }
            if (Zpos > 100 || Zpos < 0)
            {
                Status = "Z=OutRange";
            }
            if (bereik1 > (float)Math.Pow(400, 2) || (bereik1 < (float)Math.Pow(100, 2)))
            {
                Status = "XY OutRange";
                Status = "XY OutRange";
            }
            void GetAngleFromXYZ()
            {
                float lengte = L;
                float r1 = 0;
                float phi1 = 0;
                float phi2 = 0;
                float phi3 = 0;
                float a1 = 340; //linklengte 1
                float a2 = 360; //linklengte 2
                float T1 = 0; // theta 1 in !radians hoek joint 1
                float T2 = 0; // hoek joint2
                              //functie Theta1 neemt hoek in graden
                              //float bereik1 = (float)Math.Pow(Len, 2) + (float)Math.Pow(Z, 2);
                float bereik2 = 280;
                //((X > 100 || X < -100) && (Y > 100 || Y < -100)
                // && (X < 400 || X > -400) && (Y < 400 || Y > -400))
                if (bereik2 <= 300)
                {
                    //eq.1c pythagoras


                    r1 = (float)Math.Sqrt(alen * alen + Z * Z);// lange zijde uitreken //eq.1
                    phi2 = (float)Math.Atan(Z / L); //eq.3           
                    phi3 = (float)Math.Acos(((a1 * a1) - (a2 * a2) - (r1 * r1)) / (-2 * a2 * r1));  //eq.5
                    phi1 = (float)Math.Acos(((a2 * a2) - (a1 * a1) - (r1 * r1)) / (-2 * a1 * r1));   //eq.2                                                                         //T2 = 180 - phi3, T + phi3 = 180
                    T2 = (float)3.141459 - phi3; //eq.4 T2 = 180gr - phi3
                    T1 = phi2 - phi1;//eq.4

                    float J2 = Reken.RadtoDegr(T1);
                    float J3 = Reken.RadtoDegr(T2);

                    Joint2 = J2.ToString();
                    Joint3 = J3.ToString();
                    Zpos = Z.ToString();


                }
                if (bereik2 > 300)
                {

                }
            }




            // = var J1
            // = var J2
            // J1pos = var J1
            // J2pos = var J2
            */
        //}
    }
    
}

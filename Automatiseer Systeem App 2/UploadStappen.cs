using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Automatiseer_Systeem_App_1
{
    class newstep_makeline
    {
        public string lijn = " ";

        public newstep_makeline(string Starttijd, string Xpos, string Ypos, string Zpos , string VaccuumBool, string Duratie, string PreCondition)
        { 
            lijn = Starttijd + " " + Xpos + " " + Ypos + " " + Zpos + " " + VaccuumBool + " " + Duratie + " " + PreCondition + "\r\n";
        }
    }
}

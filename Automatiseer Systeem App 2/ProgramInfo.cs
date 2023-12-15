using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Automatiseer_Systeem_App_1
{
    public class program_info
    {
        public static int total_loops_goal;

        public program_info(string lijn)
        {
            if (!string.IsNullOrEmpty(lijn))
            {
                string[] subdelen = lijn.Split(' ');
                try
                {
                    total_loops_goal = Int32.Parse(subdelen[0]);
                }
                catch (FormatException)
                {
                }
            }
        }
    }
}

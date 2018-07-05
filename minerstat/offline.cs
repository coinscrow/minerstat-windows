using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace minerstat
{
    class offline
    {

        async public static void protect(object sender, ElapsedEventArgs exw)
        {
            // HOOK
            if (modules.checkNet(false) == false) { }
            //if (modules.IsReach().Equals(false)) { }

            // DEBUG
            Console.WriteLine("Offline Events: N#>" + Program.connectionError.ToString() + "/ L#> " + Program.prevConnectionError.ToString());

            // IF NO ERROR
            if (Program.connectionError.ToString().Equals("False"))
            {
                
                if (Program.prevConnectionError.ToString().Equals("True")) {
                    // ONLY RUN THIS IS THE PREV STATUS WAS != OK
                    Program.watchDogs.Start();
                    Program.syncLoop.Start();
                    Program.NewMessage("NODE => Connection has come back!", "");
                }

                Program.prevConnectionError = Program.connectionError;

            } else
            {

                if (Program.prevConnectionError.ToString().Equals("False"))
                {
                    // ONLY RUN THIS IF THE PREV STATUS WAS OK
                    Program.NewMessage("ERROR => Connection problems detected", "");
                    Program.watchDogs.Stop();
                    Program.syncLoop.Stop();
                }

                Program.prevConnectionError = Program.connectionError;
            }

        }


    }
}

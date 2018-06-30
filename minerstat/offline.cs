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
            // DEBUG
            Console.WriteLine("Offline Events: N#>" + Program.connectionError.ToString() + "/ L#> " + Program.prevConnectionError.ToString());

            // IF NO ERROR
            if (Program.connectionError.ToString().Equals("False"))
            {
                
                if (Program.prevConnectionError.ToString().Equals("True")) {
                    // ONLY RUN THIS IS THE PREV STATUS WAS != OK
                    Program.watchDogs.Stop();
                    Program.syncLoop.Stop();
                    Program.crashLoop.Stop();
                    await Task.Delay(200);
                    Application.Restart();
                }

                Program.prevConnectionError = Program.connectionError;

            } else
            {

                if (Program.prevConnectionError.ToString().Equals("False"))
                {
                    // ONLY RUN THIS IF THE PREV STATUS WAS OK
                    Program.watchDogs.Stop();
                    Program.syncLoop.Stop();
                    Program.crashLoop.Stop();
                    await Task.Delay(200);
                    Application.Restart();
                }

                Program.prevConnectionError = Program.connectionError;
            }

        }


    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace minerstat
{
    class buffer
    {

        async public static void Protection(object sender, ElapsedEventArgs exw)
        {

            Program.NewMessage("BUFFER => RESTARTING THE APP.." , "");

            await Task.Delay(2000);

            File.WriteAllText(@Program.minerstatDir + "/buffer.txt", "Temporary file will be removed on next start.");

            await Task.Delay(2000);

            if (!File.Exists(Program.minerstatDir + "/buffer.txt"))
            {
                File.WriteAllText(@Program.minerstatDir + "/buffer.txt", "Temporary file will be removed on next start.");
            }

            await Task.Delay(500);

            Program.watchDogs.Stop();
            Program.syncLoop.Stop();

            await Task.Delay(100);

            Application.Restart();

        }

    }
}

using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace minerstat {
 static class Program {

  // minerstat sync
  public static string lastupdate;
  public static int totalByte;
  public static int totalTraffic;
  public static string suffix;
  public static int watchDogFailover;
  public static int watchDogFailoverCpu;

  // Open hardware monitor
  public static int monitorport;

  // minerstat Auth
  public static string loginjson;
  public static string token;
  public static string worker;

  // minerstat Direcories
  public static string currentDir;
  public static string tempDir;
  public static string minerstatDir;

  // minerstat Display Console
  public static List < string > Message = new List < string > ();
  public static void update(string last) {
   lastupdate = last;
  }

  // Internet Speed
  public static System.Net.WebClient wcc;
  public static DateTime dt1c;
  public static byte[] datac;
  public static DateTime dt2c;
  public static double connectionspeed;

  // Timers
  public static System.Timers.Timer watchDogs;
  public static System.Timers.Timer syncLoop;
  public static Boolean SyncStatus;

  // Resources
  static string lib, browser, locales, res;

  [STAThread]
  static void Main(string[] args) {

            if (args.Length == 0)
            {
                MessageBox.Show("ERROR => Please, Start with minerstat.exe");
                Application.Exit();
            }
            else {

                // Assigning file paths to varialbles
                lib = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\libcef.dll");
                browser = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\CefSharp.BrowserSubprocess.exe");
                locales = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\locales\");
                res = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\");

                var libraryLoader = new CefLibraryHandle(lib);
                bool isValid = !libraryLoader.IsInvalid;
                libraryLoader.Dispose();

                var settings = new CefSettings();
                settings.BrowserSubprocessPath = browser;
                settings.LocalesDirPath = locales;
                settings.ResourcesDirPath = res;

                Cef.Initialize(settings);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // SET Global Varibles
                //currentDir = System.Environment.CurrentDirectory;
                currentDir = AppDomain.CurrentDomain.BaseDirectory;
                tempDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                minerstatDir = tempDir + "/minerstat";
                suffix = "byte";
                totalTraffic = 0;

                // Open hardware monitor
                Random random = new Random();
                monitorport = random.Next(8600, 8700);

                // Initalize Watchdog
                watchDogs = new System.Timers.Timer(TimeSpan.FromSeconds(3).TotalMilliseconds); // set the time (5 min in this case)
                watchDogs.AutoReset = true;
                watchDogs.Elapsed += new System.Timers.ElapsedEventHandler(watchDog.health);
                watchDogFailover = 0;

                // Initalize Syncing
                syncLoop = new System.Timers.Timer(TimeSpan.FromSeconds(30).TotalMilliseconds); // set the time (5 min in this case)
                syncLoop.AutoReset = true;
                syncLoop.Elapsed += new System.Timers.ElapsedEventHandler(sync.loop);

                // Check update folder
                if (Directory.Exists(currentDir + "/update/"))
                {
                    if (File.Exists(currentDir + "/update/minerstat.exe"))
                    {
                        try
                        {

                            File.Delete("minerstat.exe");
                            File.Copy("update/minerstat.exe", "minerstat.exe");

                            Directory.Delete(currentDir + "/update/", true);

                        } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                    }
                }

                // RUN UX
                SyncStatus = false;
                Application.Run(new Form1());
                }

  }

  public static void NewMessage(string text, string type) {
   try {


    if (Message.Count >= 15) {
     Message.RemoveAt(0);
    }

    String hourMinute;

    hourMinute = DateTime.Now.ToString("HH:mm:ss");
    
                if (type.Equals("WARNING")) {
                    Message.Add(("<span>[" + hourMinute + "] ") + text + "</span> <br>");
                } else {
                    Message.Add(("[" + hourMinute + "] ") + text + " <br>");
                }

   } catch (Exception) {

   }
  }

 }
}
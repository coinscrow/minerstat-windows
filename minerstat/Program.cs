using System;
using System.Collections.Generic;
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


  [STAThread]
  static void Main() {
   Application.EnableVisualStyles();
   Application.SetCompatibleTextRenderingDefault(false);

   // SET Global Varibles
   currentDir = System.Environment.CurrentDirectory;
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

   // RUN UX
   SyncStatus = false;
   Application.Run(new Form1());


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
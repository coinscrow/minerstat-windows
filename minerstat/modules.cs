using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using OpenHardwareMonitor.Hardware;


namespace minerstat
{
    class modules
    {

        public static DriveInfo[] allDrives = DriveInfo.GetDrives();
        public static long free;
        public PerformanceCounter ramCounter;

        public static double CheckInternetSpeed()
        {
            Program.datac = Encoding.ASCII.GetBytes("dump");

            try
            {
                // Create Object Of WebClient
                Program.wcc = new System.Net.WebClient();

                //DateTime Variable To Store Download Start Time.
                Program.dt1c = DateTime.Now;

                //Number Of Bytes Downloaded Are Stored In ‘data’
                Program.datac = Program.wcc.DownloadData("http://google.com");

                //DateTime Variable To Store Download End Time.
                Program.dt2c = DateTime.Now;

                //To Calculate Speed in Kb Divide Value Of data by 1024 And Then by End Time Subtract Start Time To Know Download Per Second.

            }
            catch (Exception)
            { }

            return Math.Round(((Program.datac.Length / 1024) / (Program.dt2c - Program.dt1c).TotalSeconds) * 8, 2);

        }

        public static string FreeRam(PerformanceCounter ramCounter)
        {
            try
            {
                ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);
                return ramCounter.ToString(); ;
            } catch(Exception ex)
            {
                return "0";
            }

        }

        public static void updateTraffic(int package)
        {

            Program.totalByte = Program.totalByte + package;

            if (Program.totalByte < 1000)
            {
                Program.suffix = "byte";
                Program.totalTraffic = Program.totalByte;
            }

            if (Program.totalByte >= 1000)
            {
                Program.suffix = "KB";
                Program.totalTraffic = Program.totalByte / 1000;
            }

            if (Program.totalByte > 1000000)
            {
                Program.totalTraffic = Program.totalByte / 1000000;
                Program.suffix = "MB";
            }

        }

        public async static void StartServer(Computer computer)
        {

            try
            {

                var visitor = new hardwareMonitor.Visitor();

                // Start a HTTP server on port 8087.
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + Program.monitorport + "/");
                listener.Start();

                // While the server is still running...
                while (listener.IsListening)
                {
                    // Context is an incoming HTTP request.
                    var context = await listener.GetContextAsync();
                    // Update the sensors.
                    computer.Accept(visitor);
                    // Serialize the data in computer.Hardware into a JSON string, ignoring circular references.
                    // NOTE: The circular reference is from each node having a reference to its parent.
                    // TODO: The data can be cleaned up some before serializing, it contains a bunch of extraneous data.
                    // The serializer in OpenHardwareMonitor's GUI library used by its own JSON server simply collects
                    // the values to serialize. It can serve as a starting point.
                    var settings = new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        ContractResolver = new hardwareMonitor.CustomContractResolver()
                    };
                    var data = JsonConvert.SerializeObject(computer.Hardware, settings);



                    var buffer = Encoding.UTF8.GetBytes(data);
                    var response = context.Response;

                    try
                    {
                        response.AddHeader("Access-Control-Allow-Origin", "*");
                        response.AddHeader("Content-Type", "application/json");
                        response.OutputStream.Write(buffer, 0, buffer.Length);
                    }
                    catch (HttpListenerException)
                    {
                        // Don't do anything, this is most likely caused by the client ending the TCP connection, and
                        // therefore writing to the closed TCP connection will throw an exception.
                    }

                    response.OutputStream.Close();
                }

            }
            catch (Exception)
            { }
        }

        public static string GetLocalIPAddress()
        {

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable().Equals(true))
            {

                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "0.0.0.0";
                //throw new Exception("No network adapters with an IPv4 address in the system!");

            }
            else
            {
                return "0.0.0.0";
            }
        }

        public static long GetTotalFreeSpace(string driveName)
        {
            free = 0;
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    free = free + d.TotalFreeSpace;
                }
            }
            return free;
        }

        public static string GetUserIP()
        {
            try
            {
                string ipList = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.GetValue(0).ToString();
                return ipList;
            } catch(Exception ex)
            {
                return "0.0.0.0";
            }
        }

        public static Boolean checkNet(Boolean est)
        {
            try
            {
                Ping myPing = new Ping();
                String host = "minerstat.farm";
                byte[] buffer = new byte[32];
                int timeout = 2000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {                   
                    if (est.Equals(true))
                    {
                        Program.connectionspeed = CheckInternetSpeed() / 1000;
                        Program.NewMessage("NODE => Estimated Internet Speed: " + Program.connectionspeed + " MB/s", "INFO");
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsReach()
        {
            try
            {
                modules.getData checkServer = new modules.getData("http://minerstat.farm/server.php", "POST", "");

                string res;
                res = checkServer.GetResponse().ToString();

                if (res.Contains("ok"))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }
        }


        internal static string cctcp(string cmd)
        {
            string result = null;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 3333);
                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes(cmd);
                stream.Write(bytes, 0, bytes.Length);
                result = streamReader.ReadLine();
                stream.Close();
            }
            catch (Exception value)
            {
                Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");
            }
            return result;
        }


        internal static bool getStat_sgminer(string cmd = "summary+pools+devs")
        {

            bool result = false;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 4028);

                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes(cmd);
                stream.Write(bytes, 0, bytes.Length);
                string text = streamReader.ReadLine();
                sync.apiResponse = text;
                Console.WriteLine(text);
                Console.ReadLine();
                stream.Close();
                result = true;
            }
            catch (Exception value)
            {
                Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");
            }
            return result;
        }

        internal static string getStat_cpu(string cmd = "summary")
        {
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 4048);

                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes(cmd);
                stream.Write(bytes, 0, bytes.Length);
                string text = streamReader.ReadLine();
                sync.apiCpu = text;
                Console.WriteLine(text);
                Console.ReadLine();
                stream.Close();
                return "text";
            }
            catch (Exception value)
            {
                Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");
            }
            return "error";
        }

        internal static bool getStat_ewbf()
        {

            bool result = false;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 42000);

                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes("{\"id\":2, \"method\":\"getstat\"}\n");
                stream.Write(bytes, 0, bytes.Length);
                string res = streamReader.ReadLine();
                sync.apiResponse = res;
                stream.Close();
                result = true;
            }
            catch (Exception value)
            {

                Program.NewMessage("WATCHDOG: Ewbf 0Sol/s or not started ?!", "ERROR");           
                Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");

            }
            return result;
        }


        internal static bool getStat_claymore()
        {
            bool result = false;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", Int32.Parse("3333"));

                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes("{\"id\":0, \"jsonrpc\":\"2.0\", \"method\":\"miner_getstat2\"}\n");
                stream.Write(bytes, 0, bytes.Length);
                string res = streamReader.ReadLine();
                sync.apiResponse = res;
                stream.Close();
                result = true;
            }
            catch (Exception value)
            {
                Program.NewMessage("WATCHDOG: Claymore not started ?!", "ERROR");
                Program.NewMessage(value.ToString().Substring(0, 42) + "...", "ERROR");
            }
            return result;
        }



        internal static bool getStat_zm()
        {
            bool result = false;
            try
            {
                TcpClient tcpClient = new TcpClient("127.0.0.1", 2222);

                Stream stream = tcpClient.GetStream();
                StreamReader streamReader = new StreamReader(stream);
                byte[] bytes = Encoding.ASCII.GetBytes("{\"id\":1, \"method\":\"getstat\"}\n");
                stream.Write(bytes, 0, bytes.Length);
                string res = streamReader.ReadLine();
                sync.apiResponse = res;
                stream.Close();
                result = true;
            }
            catch (Exception value)
            {  }
            return result;
        }

        internal static bool getStat()
        {
            bool result = false;
            try
            {
                string text = cctcp("summary");
                bool flag = text != null;
                if (flag)
                {
                    sync.apiResponse = text;
                    result = true;
                }
                else
                {
                    if (!result)
                    {
                        Program.NewMessage("ccminer Socked Closed..", "ERROR");
                    }
                }
            }
            catch (Exception) { result = false; }
            return result;
        }

        public class getData
        {
            private WebRequest request;
            private Stream dataStream;
            private string status;

            public String Status
            {
                get
                {
                    return status;
                }
                set
                {
                    status = value;
                }
            }

            public getData(string url)
            {
                // Create a request using a URL that can receive a post.

                request = WebRequest.Create(url);
            }

            public getData(string url, string method) : this(url)
            {

                if (method.Equals("GET") || method.Equals("POST"))
                {
                    // Set the Method property of the request to POST.
                    request.Method = method;
                }
                else
                {
                    throw new Exception("Invalid Method Type");
                }
            }

            public getData(string url, string method, string data) : this(url, method)
            {
                try
                {

                    // Create POST data and convert it to a byte array.
                    string postData = data;
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    // Set the ContentType property of the WebRequest.
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Set the ContentLength property of the WebRequest.
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.
                    dataStream = request.GetRequestStream();

                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    // Close the Stream object.
                    dataStream.Close();

                }
                catch (Exception)
                {   }

            }

            public static string responseFromServer;

            public string GetResponse()
            {
                // Get the original response.

                try
                {

                    WebResponse response = request.GetResponse();
                    this.Status = ((HttpWebResponse)response).StatusDescription;

                    // Get the stream containing all content returned by the requested server.
                    dataStream = response.GetResponseStream();

                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    // Read the content fully up to the end.
                    responseFromServer = reader.ReadToEnd();

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();


                }
                catch (Exception)
                {  }

                return responseFromServer;


            }


        }

    }
}
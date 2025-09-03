using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Renci.SshNet;

namespace GUI_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionInfo conn;
        SshClient sshClient;
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();

            StartToGraph();
        }

        public async void StartToGraph() 
        {
            string ipAddress = "191.252.222.248";
            string keyFilePath = ".\\rufo";
            string keyfileText = string.Empty;
            try
            {
                // Open the text file using a stream reader.
                using StreamReader reader = new(keyFilePath);

                // Read the stream as a string.
                string text = reader.ReadToEnd();
                keyfileText = reader.ReadToEnd();
            }
            catch (IOException e)
            {
                Console.WriteLine("HMI Monitor - .\\rufo The file could not be read:");
                Console.WriteLine(e.Message);
            }

            try
            {
                conn = new ConnectionInfo(ipAddress, 22, "root", new AuthenticationMethod[]{
                    new PrivateKeyAuthenticationMethod("root", new PrivateKeyFile[]
                    { new PrivateKeyFile(keyFilePath) }),
                });

                Console.WriteLine("HMI Monitor - Connection OK");

                Console.WriteLine("\nPress any key to stop the loop...");

                using (sshClient = new SshClient(conn))
                {
                    sshClient.Connect();
                    var dateTime = System.DateTime.Now;
                    string outputFileName = dateTime.Year + "_" + dateTime.Month + "_" + dateTime.Day + "At" + dateTime.Hour + "_" + dateTime.Minute + "_" + dateTime.Second;
                    var dateTimeValue = System.DateTime.Now.ToString().Replace("/", "_").Replace(":", "_");

                    var filePath = $"./{outputFileName}.csv";
                    //var filePath = $"./LOBO";
                    string headerContent = "DATETIME, CPU, RAM, HD\n";

                    Console.WriteLine(headerContent);
                    File.AppendAllText(filePath, headerContent);

                    List<double> dates = new List<double>();
                    List<double> ram = new List<double>();

                    while (true)
                    {
                        var dateTimeX = System.DateTime.Now;
                        var result = sshClient.RunCommand("echo \"$(top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/' | awk '{print 100 - $1}')%, $(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }'), $(df -h / | awk '/\\// {print $(NF-1)}')\"");
                        var result2 = sshClient.RunCommand("echo \"$(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }')\"");
                        string outputLine = dateTimeX + ", " + result2.Result;

                        Console.Write(outputLine);
                        File.AppendAllText(filePath, outputLine);

                        double y = Convert.ToDouble(result2.Result.Remove(result2.Result.Length - 2))/10;

                        dates.Add(dateTimeX.ToOADate());
                        ram.Add(y);

                        WpfPlot1.Plot.Clear();
                        WpfPlot1.Plot.Add.Scatter(dates.ToArray(), ram.ToArray());
                        WpfPlot1.Plot.Axes.DateTimeTicksBottom();

                        WpfPlot1.Refresh();
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"HMI Monitor - Error trying to connect: {e}", e);
            }

        }
    }
}
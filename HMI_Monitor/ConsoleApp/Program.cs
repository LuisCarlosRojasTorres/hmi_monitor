// See https://aka.ms/new-console-template for more information
using Renci.SshNet;

Console.WriteLine("HMI Monitor - Console Version!");
Console.WriteLine("------------------------------\n");

ConnectionInfo conn;
SshClient sshClient;

string ipAddress = "191.252.222.248";
string keyFilePath = "C:\\Users\\redto\\OneDrive\\Documentos\\repositories\\Engineering\\pc\\hmi_monitor\\HMI_Monitor\\ConsoleApp\\bin\\Debug\\net8.0\\id_ed25519";

try
{
    // Open the text file using a stream reader.
    using StreamReader reader = new(keyFilePath);

    // Read the stream as a string.
    string text = reader.ReadToEnd();
}
catch (IOException e)
{
    Console.WriteLine("HMI Monitor - The file could not be read:");
    Console.WriteLine(e.Message);
}

try
{
    conn = new ConnectionInfo(ipAddress, 22, "root", new AuthenticationMethod[]{
                    new PrivateKeyAuthenticationMethod("root", new PrivateKeyFile[]
                    { new PrivateKeyFile(keyFilePath) }),
                });

    Console.WriteLine("HMI Monitor - Connection OK");

    using (sshClient = new SshClient(conn))
    {
        sshClient.Connect();
        var dateTime = System.DateTime.Now;
        string outputFileName = dateTime.Year+"_"+dateTime.Month+"_"+dateTime.Day+"At"+dateTime.Hour+"_"+dateTime.Minute+"_"+dateTime.Second;
        var dateTimeValue = System.DateTime.Now.ToString().Replace("/","_").Replace(":","_");

        var filePath = $"./{outputFileName}.csv";
        //var filePath = $"./LOBO";
        string content = "DATETIME, CPU, RAM, HD";

        File.AppendAllText(filePath, content + Environment.NewLine);

        

        Console.WriteLine("Press any key to stop the loop...");
        while (!Console.KeyAvailable)
        {
            var result = sshClient.RunCommand("echo \"$(top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/' | awk '{print 100 - $1}')%, $(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }'), $(df -h / | awk '/\\// {print $(NF-1)}')\"");
            var dateTimeX = System.DateTime.Now.ToString();
            Console.WriteLine(result.Result);
            File.AppendAllText(filePath, dateTimeX + ", " + result.Result + Environment.NewLine);
            await Task.Delay(1000);            
        }        
    }
}
catch(Exception e) {
    Console.WriteLine($"HMI Monitor - Error trying to connect: {e}",e);
}


// See https://aka.ms/new-console-template for more information
using Renci.SshNet;

Console.WriteLine("HMI Monitor - Console Version!");
Console.WriteLine("------------------------------\n");

ConnectionInfo conn;
SshClient sshClient;

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
        string outputFileName = dateTime.Year+"_"+dateTime.Month+"_"+dateTime.Day+"At"+dateTime.Hour+"_"+dateTime.Minute+"_"+dateTime.Second;
        var dateTimeValue = System.DateTime.Now.ToString().Replace("/","_").Replace(":","_");

        var filePath = $"./{outputFileName}.csv";
        //var filePath = $"./LOBO";
        string headerContent = "DATETIME, CPU, RAM, HD\n";

        Console.WriteLine(headerContent);
        File.AppendAllText(filePath, headerContent);        

        while (!Console.KeyAvailable)
        {
            var dateTimeX = System.DateTime.Now.ToString();
            var result = sshClient.RunCommand("echo \"$(top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/' | awk '{print 100 - $1}')%, $(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }'), $(df -h / | awk '/\\// {print $(NF-1)}')\"");
            
            string outputLine = dateTimeX + ", " + result.Result;
            
            Console.Write(outputLine);
            File.AppendAllText(filePath, outputLine);
            await Task.Delay(1000);            
        }        
    }
}
catch(Exception e) {
    Console.WriteLine($"HMI Monitor - Error trying to connect: {e}",e);
}


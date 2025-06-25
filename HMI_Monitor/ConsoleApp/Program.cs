// See https://aka.ms/new-console-template for more information
using Renci.SshNet;

Console.WriteLine("HMI Monitor - Console Version!");
Console.WriteLine("------------------------------\n");

ConnectionInfo conn;
SshClient sshClient;

string ipAddress = "191.252.222.248";
string keyFilePath = "C:\\Users\\redto\\OneDrive\\Documentos\\repositories\\Engineering\\pc\\hmi_monitor\\HMI_Monitor\\ConsoleApp\\bin\\Debug\\net8.0\\id_ed25519";

string cpuConsumptionResult = string.Empty;
string ramConsumptionResult = string.Empty;
string hdConsumptionResult = string.Empty;

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
        
        var result = sshClient.RunCommand("echo \"CPU $(top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/' | awk '{print 100 - $1}')% RAM $(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }') HDD $(df -h / | awk '/\\// {print $(NF-1)}')\"");
        cpuConsumptionResult = sshClient.RunCommand("echo \"$(top -bn1 | grep 'Cpu(s)' | sed 's/.*, *\\([0-9.]*\\)%* id.*/\\1/' | awk '{print 100 - $1}')%\"").Result;
        ramConsumptionResult = sshClient.RunCommand("echo \"RAM $(free -m | awk '/Mem:/ { printf(\"%3.1f%%\", $3/$2*100) }')\"").Result;
        hdConsumptionResult = sshClient.RunCommand("echo \"HDD $(df -h / | awk '/\\// {print $(NF-1)}')\"").Result;

        while (result.Result == string.Empty)
        {
            await Task.Delay(1000);
            Console.WriteLine(" - Waiting");
        }

        Console.WriteLine("Command Output:");
        Console.WriteLine(result.Result);
        Console.WriteLine(cpuConsumptionResult);
        Console.WriteLine(ramConsumptionResult);
        Console.WriteLine(hdConsumptionResult);

    }
}
catch(Exception e) {
    Console.WriteLine($"HMI Monitor - Error trying to connect: {e}",e);
}


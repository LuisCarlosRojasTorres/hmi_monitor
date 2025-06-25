// See https://aka.ms/new-console-template for more information
using Renci.SshNet;

Console.WriteLine("HMI Monitor - Console Version!");
Console.WriteLine("------------------------------");
Console.WriteLine("\n\n\n");

ConnectionInfo conn;
SshClient sshClient;

string ipAddress = "191.252.222.248";
string keyFile = string.Empty;
string keyFilePath = "C:\\Users\\redto\\OneDrive\\Documentos\\repositories\\Engineering\\pc\\hmi_monitor\\HMI_Monitor\\ConsoleApp\\bin\\Debug\\net8.0\\id_ed25519";



try
{
    // Open the text file using a stream reader.
    using StreamReader reader = new(keyFilePath);

    // Read the stream as a string.
    keyFile = reader.ReadToEnd();
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
}
catch(Exception e) {
    Console.WriteLine($"HMI Monitor - Error trying to connect: {e}",e);
}
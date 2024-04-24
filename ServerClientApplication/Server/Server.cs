using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

static class Server
{
    private const int Port = 12345;
    private const string ServerFilesDirectory = "ServerFiles";

    static void Main(string[] args)
    {
        Console.WriteLine("Сервер запущен. Ожидание подключений...");

        if (!Directory.Exists(ServerFilesDirectory))
        {
            Directory.CreateDirectory(ServerFilesDirectory);
        }

        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Клиент подключен.");
            HandleClient(client);
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8)
        {
            AutoFlush = true
        };
        try
        {
            string request = reader.ReadLine();
            string[] parts = request.Split('|');
            string command = parts[0];
            switch (command)
            {
                case "UPLOAD":
                    HandleUploadRequest(parts, reader, writer);
                    break;
                case "LIST_FILES":
                    HandleListFilesRequest(writer);
                    break;
                case "DOWNLOAD":
                    HandleDownloadRequest(parts, writer);
                    break;
                default:
                    writer.WriteLine("RESPONSE|false|Invalid command");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
        }
        finally
        {
            reader.Close();
            writer.Close();
            stream.Close();
            client.Close();
        }
    }

    static void HandleListFilesRequest(StreamWriter writer)
    {
        try
        {
            string[] files = Directory.GetFiles(ServerFilesDirectory);
            writer.Write("FILE_LIST|");
            writer.WriteLine(string.Join("|", files.Select(Path.GetFileName)));
            Console.WriteLine("Список файлов отправлен клиенту.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка файлов:{ex.Message} ");
            writer.WriteLine("RESPONSE|false|Failed to list files");
        }
    }

    static void HandleDownloadRequest(string[] parts, StreamWriter writer)
    {
        try
        {
            string fileName = parts[1];
            string filePath = Path.Combine(ServerFilesDirectory, fileName);
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);

                writer.WriteLine($"FILE_DATA|{Convert.ToBase64String(fileData)}");
                Console.WriteLine($"Файл '{fileName}' отправлен клиенту.");
            }
            else
            {
                writer.WriteLine("RESPONSE|false|File not found");
                Console.WriteLine($"Файл '{fileName}' не найден на сервере.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке файла клиенту:{ex.Message}");
            writer.WriteLine("RESPONSE|false|Error occurred while sending file");
        }
    }

    static void HandleUploadRequest(string[] parts, StreamReader reader,
        StreamWriter writer)
    {
        try
        {
            string fileName = parts[1];
            int fileSize = int.Parse(parts[2]);
            byte[] fileData = new byte[fileSize];
            int bytesRead = reader.BaseStream.Read(fileData, 0, fileSize);
            if (bytesRead == fileSize)
            {
                File.WriteAllBytes(Path.Combine(ServerFilesDirectory, fileName),
                    fileData);
                writer.WriteLine("RESPONSE|true|File uploaded successfully");
                Console.WriteLine($"Файл '{fileName}' успешно загружен на сервер.");
            }
            else
            {
                writer.WriteLine("RESPONSE|false|Incomplete file data");
                Console.WriteLine($"Ошибка при загрузке файла '{fileName}':неполные данные файла.");
            }
        }
        catch (Exception ex)
        {
            writer.WriteLine($"RESPONSE|false|{ex.Message}");
            Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
        }
    }
}
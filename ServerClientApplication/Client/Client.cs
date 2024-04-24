using System.Net.Sockets;
using System.Text;

static class Client
{
    private const string ServerAddress = "localhost";
    private const int Port = 12345;

    static void Main(string[] args)
    {
        try
        {
            bool exitRequested = false;
            while (!exitRequested)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Получить список файлов на сервере");
                Console.WriteLine("2. Скачать файл с сервера");
                Console.WriteLine("3. Загрузить файл на сервер");
                Console.WriteLine("4. Выйти");
                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Некорректный ввод. Пожалуйста, выберите номер действия.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        GetFileListFromServer();
                        break;
                    case 2:
                        DownloadFileFromServer();
                        break;
                    case 3:
                        UploadFileToServer();
                        break;
                    case 4:
                        exitRequested = true;
                        Console.WriteLine("До свидания!");
                        break;
                    default:
                        Console.WriteLine("Некорректный выбор. Пожалуйста, выберите номер действия.");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void GetFileListFromServer()
    {
        try
        {
            using TcpClient client = new TcpClient(ServerAddress, Port);
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;
            writer.WriteLine("LIST_FILES");
            string response = reader.ReadLine();
            string[] parts = response.Split('|');
            if (parts[0] == "FILE_LIST")
            {
                Console.WriteLine("Список файлов на сервере:");
                for (int i = 1; i < parts.Length; i++)
                {
                    Console.WriteLine(parts[i]);
                }
            }
            else
            {
                Console.WriteLine("Ошибка при получении списка файлов.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка файлов:{ex.Message}");
        }
    }

    static void DownloadFileFromServer()
    {
        try
        {
            using TcpClient client = new TcpClient(ServerAddress, Port);
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;
            Console.WriteLine("Введите имя файла для скачивания:");
            string fileName = Console.ReadLine();
            writer.WriteLine($"DOWNLOAD|{fileName}");
            string response = reader.ReadLine();
            string[] parts = response.Split('|');
            if (parts[0] == "FILE_DATA")
            {
                string base64Data = parts[1];
                byte[] fileData = Convert.FromBase64String(base64Data);
                File.WriteAllBytes(fileName, fileData);
                Console.WriteLine($"Файл '{fileName}' успешно скачан с сервера.");
            }
            else
            {
                Console.WriteLine($"Ошибка при скачивании файла: {parts[2]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при скачивании файла: {ex.Message}");
        }
    }

    static void UploadFileToServer()
    {
        try
        {
            using TcpClient client = new TcpClient(ServerAddress, Port);
            using NetworkStream stream = client.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;
            Console.WriteLine("Введите путь к файлу для загрузки на сервер:");
            string filePath = Console.ReadLine();
            string fileName = Path.GetFileName(filePath);
            byte[] fileData = File.ReadAllBytes(filePath);
            writer.WriteLine($"UPLOAD|{fileName}|{fileData.Length}");
            writer.BaseStream.Write(fileData, 0, fileData.Length);
            string response = reader.ReadLine();
            string[] parts = response.Split('|');
            if (parts[0] == "RESPONSE" && parts[1] == "true")
            {
                Console.WriteLine($"Файл '{fileName}' успешно загружен на сервер.");
            }
            else
            {
                Console.WriteLine($"Ошибка при загрузке файла: {parts[2]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке файла: {ex.Message}");
        }
    }
}
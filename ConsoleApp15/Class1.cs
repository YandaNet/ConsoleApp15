using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    static TcpClient client;

    static NetworkStream stream;

    static void Main()
    {
        client =
            new TcpClient("127.0.0.1", 5000);

        stream = client.GetStream();

        Console.Write("Логін: ");

        string login = Console.ReadLine();

        byte[] loginData =
            Encoding.UTF8.GetBytes(login);

        stream.Write(loginData, 0, loginData.Length);

        Thread thread =
            new Thread(GetMessages);

        thread.Start();

        Console.WriteLine("КОМАНДИ:");
        Console.WriteLine("/add ім'я");
        Console.WriteLine("/remove ім'я");
        Console.WriteLine("/rename старе_ім'я нове_ім'я");
        Console.WriteLine("/contacts");
        Console.WriteLine("/exit");

        while (true)
        {
            string text = Console.ReadLine();

            if (text == "/exit")
                break;

            string message;

            if (text.StartsWith("/"))
            {
                message = text;
            }
            else
            {
                message = login + ": " + text;
            }

            byte[] data =
                Encoding.UTF8.GetBytes(message);

            stream.Write(data, 0, data.Length);
        }

        stream.Close();

        client.Close();
    }

    static void GetMessages()
    {
        while (true)
        {
            try
            {
                byte[] buffer = new byte[1024];

                int bytes =
                    stream.Read(buffer, 0, buffer.Length);

                string message =
                    Encoding.UTF8.GetString(buffer, 0, bytes);

                Console.WriteLine("\n" + message);
            }
            catch
            {
                break;
            }
        }
    }
}

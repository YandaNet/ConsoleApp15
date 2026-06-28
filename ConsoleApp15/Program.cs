using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    static List<TcpClient> clients = new List<TcpClient>();

    static Dictionary<TcpClient, string> users =
        new Dictionary<TcpClient, string>();

    static Dictionary<string, List<string>> contacts =
        new Dictionary<string, List<string>>();

    static void Main()
    {
        TcpListener server =
            new TcpListener(IPAddress.Any, 5000);

        server.Start();

        Console.WriteLine("Сервер запущений...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();

            clients.Add(client);

            Thread thread =
                new Thread(() => HandleClient(client));

            thread.Start();
        }
    }

    static void HandleClient(TcpClient client)
    {
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];

        int bytes = stream.Read(buffer, 0, buffer.Length);

        string login =
            Encoding.UTF8.GetString(buffer, 0, bytes);

        users[client] = login;

        if (!contacts.ContainsKey(login))
        {
            contacts[login] = new List<string>();
        }

        Console.WriteLine(login + " підключився");

        SendAll(login + " увійшов у чат");

        while (true)
        {
            try
            {
                buffer = new byte[1024];

                bytes = stream.Read(buffer, 0, buffer.Length);

                if (bytes == 0)
                    break;

                string message =
                    Encoding.UTF8.GetString(buffer, 0, bytes);

                if (message.StartsWith("/add "))
                {
                    string contact =
                        message.Substring(5);

                    contacts[login].Add(contact);

                    SendOne(client,
                        "Контакт додано: " + contact);
                }

                else if (message.StartsWith("/remove "))
                {
                    string contact =
                        message.Substring(8);

                    contacts[login].Remove(contact);

                    SendOne(client,
                        "Контакт видалено: " + contact);
                }

                else if (message.StartsWith("/rename "))
                {
                    string[] parts =
                        message.Split(' ');

                    if (parts.Length >= 3)
                    {
                        string oldName = parts[1];
                        string newName = parts[2];

                        int index =
                            contacts[login].IndexOf(oldName);

                        if (index != -1)
                        {
                            contacts[login][index] = newName;

                            SendOne(client,
                                "Контакт перейменовано");
                        }
                        else
                        {
                            SendOne(client,
                                "Контакт не знайдено");
                        }
                    }
                }

                else if (message == "/contacts")
                {
                    string list = "Контакти:";

                    foreach (string c in contacts[login])
                    {
                        list += c + "\n";
                    }

                    SendOne(client, list);
                }

                else
                {
                    Console.WriteLine(message);

                    SendAll(message);
                }
            }
            catch
            {
                break;
            }
        }

        Console.WriteLine(login + " вийшов");

        clients.Remove(client);

        users.Remove(client);

        SendAll(login + " покинув чат");

        stream.Close();
        client.Close();
    }

    static void SendAll(string message)
    {
        byte[] data =
            Encoding.UTF8.GetBytes(message);

        foreach (TcpClient client in clients)
        {
            try
            {
                NetworkStream stream =
                    client.GetStream();

                stream.Write(data, 0, data.Length);
            }
            catch
            {

            }
        }
    }

    static void SendOne(TcpClient client, string message)
    {
        try
        {
            byte[] data =
                Encoding.UTF8.GetBytes(message);

            NetworkStream stream =
                client.GetStream();

            stream.Write(data, 0, data.Length);
        }
        catch
        {

        }
    }
}

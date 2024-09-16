using Microsoft.VisualBasic.FileIO;
using System;
using System.Net.Sockets;
using System.Text;

namespace Cliente
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TcpClient client = GenerateNewClient();
                NetworkStream stream = client.GetStream();
                
                Console.WriteLine("Conectado a sala!");

                string result = GetMessage(stream);
                Console.WriteLine(result);
                if (result == "1")
                {
                    Console.WriteLine("Aguardando outro jogador!");
                    result = GetMessage(stream);
                    Console.WriteLine(result);
                }

                HandleBoard(stream);


                stream.Close();
                client.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }

            Console.WriteLine("\nPressione Enter para sair");
            Console.Read();
        }

        static TcpClient GenerateNewClient()
        {
            string server = "127.0.0.1";
            int port = 13000;

            return new TcpClient(server, port);
        }

        static void HandleBoard(NetworkStream stream)
        {

            string board = GetMessage(stream);
            //if (board.EndsWith("1") || board.EndsWith("2") || board.EndsWith("3")) return;

            Console.WriteLine(board);
            board = board.Insert(3, "\n");
            board = board.Insert(7, "\n");
            Console.WriteLine(board);
            //Console.WriteLine(GetMessage(stream));

            while (true)
            {
                Console.WriteLine("Digite uma posição (1 - 9): ");
                SendMessage(stream, GetInput());

                string response = GetMessage(stream);
                Console.WriteLine(GetMessage(stream));
                if (response == "1")
                {
                    break;
                }
            }

            HandleBoard(stream);
        }

        static string GetInput() => Console.ReadLine() ?? string.Empty;

        static string GetMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[512];
            int bytesRead;
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        static void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}

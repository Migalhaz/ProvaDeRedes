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
                string server = "127.0.0.1";
                int port = 13000;
                Client newCLient = new(server, port);
                Board board = Board.StartGame(newCLient);
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
    }

    class Board
    {
        Client m_player;

        Board(Client player)
        {
            m_player = player;
        }
        public static Board StartGame(Client player)
        {
            Board board = new Board(player);
            board.MakeConnection();
            return board;
        }

        void MakeConnection()
        {
            string messageFromPlayer = m_player.GetMessage();
            switch (messageFromPlayer)
            {
                case "1":
                    Console.WriteLine("Aguardando outro jogador!");
                    MakeConnection();
                    return;
                case "0":
                    Console.WriteLine("Jogador conectado! A partida será iniciada!");
                    HandleBoard();
                    return;
                default:
                    Console.WriteLine("Mensagem inválida! tentando novamente.");
                    MakeConnection();
                    return;
            }
        }

        void HandleBoard()
        {
            string msg1 = m_player.GetMessage();
            Console.WriteLine(msg1);

            if (msg1.EndsWith("X") || msg1.EndsWith("O"))
            {
                PlayerMove();
                return;
            }
            else if (msg1.EndsWith("1") || msg1.EndsWith("2") || msg1.EndsWith("3"))
            {
                CloseGame($"{msg1[msg1.Length - 1]}");
                return;
            }
            else
            {
                NotAPlayerMove();
                return;
            }
        }

        void PlayerMove()
        {
            Console.WriteLine("Insira sua jogada: ");
            string input = Console.ReadLine() ?? string.Empty; ;
            m_player.SendMessage(input);
            string result = m_player.GetMessage();
            if (result == "-1")
            {
                Console.WriteLine("Jogada inválida! Por favor, tente novamente.");
                PlayerMove();
                return;
            }

            HandleBoard();
        }

        void NotAPlayerMove()
        {
            string board = m_player.GetMessage();
            Console.Write(board);
            HandleBoard();
        }

        void CloseGame(string gameResult)
        {
            if (gameResult == "1" || gameResult == "2")
            {
                Console.WriteLine($"O jogador número {gameResult} ganhou!");
            }
            else if (gameResult == "3")
            {
                Console.WriteLine("O jogo deu velha! >:P");
            }
            else
            {
                Console.WriteLine("Houve um erro inesperado!");
            }
            
            Console.WriteLine("O jogo encerrou! Obrigado por jogar :D");
            m_player.Disconnect();
        }
    }

    class Client
    {
        TcpClient m_tcpClient;
        NetworkStream m_stream;
        public Client(string serverIp, int port)
        {
            m_tcpClient = GenerateNewClient(serverIp, port);
            m_stream = m_tcpClient.GetStream();
        }

        TcpClient GenerateNewClient(string serverIp, int port) => new TcpClient(serverIp, port);

        public string GetMessage()
        {
            Console.WriteLine("ESPERANDO MENSAGEM!");
            return GetMessage(m_stream);
        }
        public void SendMessage(string message) => SendMessage(m_stream, message);

        string GetMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[512];
            int bytesRead;
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        void SendMessage(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            m_stream.Close();
            m_tcpClient.Close();
        }
    }
}

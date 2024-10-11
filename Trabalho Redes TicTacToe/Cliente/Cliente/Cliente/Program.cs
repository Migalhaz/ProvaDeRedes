using System;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace Cliente
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string serverIp = "127.0.0.1";
                int port = 13000;
                Client client = new(serverIp, port);
                Board gameBoard = new Board(client);
                gameBoard.StartGame();
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
        bool m_gameOver;

        public Board(Client client)
        {
            m_player = client;
            m_gameOver = false;
        }
        
        public void StartGame()
        {
            ConnectToServer();
        }

        void ConnectToServer()
        {
            Console.WriteLine("Conectando ao servidor...");
            while (true)
            {
                string serverMessage = m_player.ReceiveMessage();
                Console.WriteLine($"Mensagem do servidor: {serverMessage}");


                switch (serverMessage)
                {
                    case "1":
                        Console.WriteLine("Aguardando outro jogador...");
                        break;
                    case "0":
                        Console.WriteLine("Outro jogador conectado! Iniciando o jogo.");
                        GameLoop();
                        return;
                    default:
                        if (serverMessage.StartsWith("0"))
                        {
                            string boardMessage = serverMessage.Substring(1);
                            Console.WriteLine($"Tabuleiro atual: {boardMessage}");
                            if (boardMessage.EndsWith("X") || boardMessage.EndsWith("O"))
                            {
                                PlayerTurn();
                            }
                            else if (boardMessage.EndsWith("1") || boardMessage.EndsWith("2") || boardMessage.EndsWith("3"))
                            {
                                m_gameOver = true;
                                EndGame(boardMessage);
                            }
                            else
                            {
                                Console.WriteLine("Aguardando o movimento do adversário...");
                            }
                            GameLoop();
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Erro: resposta inesperada do servidor. Tentando novamente.");
                        }
                        break;
                }
            }
        }

        void GameLoop()
        {
            while (!m_gameOver)
            {
                string boardMessage = m_player.ReceiveMessage();
                //DrawBoard(boardMessage);
                Console.WriteLine($"Tabuleiro atual: {boardMessage}");

                if (boardMessage.EndsWith("X") || boardMessage.EndsWith("O"))
                {
                    PlayerTurn();
                }
                else if (boardMessage.EndsWith("1") || boardMessage.EndsWith("2") || boardMessage.EndsWith("3"))
                {
                    m_gameOver = true;
                    EndGame(boardMessage);
                }
                else
                {
                    Console.WriteLine("Aguardando o movimento do adversário...");
                }
            }
        }

        void PlayerTurn()
        {
            Console.WriteLine("Sua vez! Insira sua jogada (1-9): ");
            string? move = Console.ReadLine();
            move = string.IsNullOrWhiteSpace(move) ? "-1" : move;

            m_player.SendMessage(move);
            string response = m_player.ReceiveMessage();
            if (response == "-1")
            {
                Console.WriteLine("Jogada inválida! Por favor, tente novamente.");
                PlayerTurn();
            }
        }

        void EndGame(string result)
        {
            if (result.EndsWith("1"))
            {
                Console.WriteLine("O jogador 1 venceu!");
            }
            else if (result.EndsWith("2"))
            {
                Console.WriteLine("O jogador 2 venceu!");
            }
            else if (result.EndsWith("3"))
            {
                Console.WriteLine("O jogo terminou em empate!");
            }
            else
            {
                Console.WriteLine("Erro inesperado ao finalizar o jogo.");
            }

            Console.WriteLine("Obrigado por jogar! O jogo terminou.");
            m_player.Disconnect();
        }
    }

    class Client
    {
        private TcpClient m_tcpClient;
        private NetworkStream m_stream;

        public Client(string serverIp, int port)
        {
            m_tcpClient = new TcpClient(serverIp, port);
            m_stream = m_tcpClient.GetStream();
        }

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[256];
            int bytesRead = m_stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            m_stream.Write(data, 0, data.Length);
        }

        public void Disconnect()
        {
            m_stream.Close();
            m_tcpClient.Close();
        }
    }
}

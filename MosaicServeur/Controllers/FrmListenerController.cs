﻿using Serveur.Controllers.Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Serveur.Controllers
{
    public class FrmListenerController
    {
        private Socket _serverSocket;

        private const int BUFFER_SIZE = 2048;
        //private const int PORT = 4444;
        private int _port;
        private static readonly byte[] _buffer = new byte[BUFFER_SIZE];
        public static bool LISTENING;

        public void listen(int port, bool IPv6)
        {
            _port = port;

            try
            {
                if (!LISTENING)
                {
                    if(Socket.OSSupportsIPv6 && IPv6)
                    {
                        _serverSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                        // fix for mono compatibility, SocketOptionName.IPv6Only
                        SocketOptionName ipv6only = (SocketOptionName)27;
                        _serverSocket.SetSocketOption(SocketOptionLevel.IPv6, ipv6only, 0);
                        _serverSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
                    }
                    else
                    {
                        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
                    }

                    _serverSocket.Listen(1000);
                    Views.FrmMain.instance.setListeningStatus("Port : " + port + ' ' + "Listening.");
                    LISTENING = true;
                    _serverSocket.BeginAccept(new AsyncCallback(acceptClient), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void stopListening()
        {
            if(_serverSocket != null)
            {
                _serverSocket.Dispose();
                _serverSocket.Close();
                _serverSocket = null;
                LISTENING = false;
                Views.FrmMain.instance.setListeningStatus("Not listening.");
            }
        }

        private void acceptClient(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (Exception)
            {
                return;
            }

            authentication(new ClientMosaic(socket));
            _serverSocket.BeginAccept(acceptClient, null);
        }

        private void authentication(ClientMosaic client)
        {
            new Packets.ServerPackets.GetAuthentication().Execute(client); // begin handshake                   
        }
    }
}

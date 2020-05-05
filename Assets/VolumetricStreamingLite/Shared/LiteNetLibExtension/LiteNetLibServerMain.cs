// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace LiteNetLibExtension
{
    public class LiteNetLibServerMain : MonoBehaviour, INetEventListener
    {
        [SerializeField] int _port = 11010;
        [SerializeField] string _key = "LiteNetLibExample";

        NetManager _serverNetManager;
        Dictionary<int, NetPeer> _connectedClientDictionary;

        public delegate void OnPeerConnectedDelegate(NetPeer peer);
        public OnPeerConnectedDelegate OnPeerConnectedHandler;

        public delegate void OnPeerDisconnectedDelegate(NetPeer peer);
        public OnPeerDisconnectedDelegate OnPeerDisconnectedHandler;

        public delegate void OnNetworkReceiveDelegate(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);
        public OnNetworkReceiveDelegate OnNetworkReceived;

        void Awake()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "--port" :
                        _port = int.Parse(args[i + 1]);
                        break;
                    case "--key" :
                        _key = args[i + 1];
                        break;
                }
            }

            StartServer(_port);
        }

        void OnApplicationQuit()
        {
            StopServer();
        }

        void StartServer(int port)
        {
            _serverNetManager = new NetManager(this);
            _connectedClientDictionary = new Dictionary<int, NetPeer>();

            if (_serverNetManager.Start(port))
            {
                Console.WriteLine("LiteNetLib server started listening on port " + port);
            }
            else
            {
                Console.WriteLine("LiteNetLib server could not start!");
            }
        }

        void StopServer()
        {
            if (_serverNetManager != null && _serverNetManager.IsRunning)
            {
                _serverNetManager.Stop();
                Console.WriteLine("LiteNetLib server stopped.");
            }
        }

        void FixedUpdate()
        {
            if (_serverNetManager.IsRunning)
            {
                _serverNetManager.PollEvents();
            }
        }

        public void SendData(int clientId, NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            if (_connectedClientDictionary.ContainsKey(clientId))
            {
                _connectedClientDictionary[clientId].Send(dataWriter, deliveryMethod);
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("OnPeerConnected : " + peer.EndPoint.Address + " : " + peer.EndPoint.Port);

            if (!_connectedClientDictionary.ContainsKey(peer.Id))
            {
                _connectedClientDictionary.Add(peer.Id, peer);
            }

            NetDataWriter dataWriter = new NetDataWriter();
            dataWriter.Put((int)NetworkDataType.ReceiveOwnCliendId);
            dataWriter.Put(peer.Id);
            peer.Send(dataWriter, DeliveryMethod.ReliableOrdered);

            OnPeerConnectedHandler?.Invoke(peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine("OnPeerDisconnected : " + peer.EndPoint.Address + " : " + peer.EndPoint.Port + " Reason : " + disconnectInfo.Reason.ToString());

            if (_connectedClientDictionary.ContainsKey(peer.Id))
            {
                _connectedClientDictionary.Remove(peer.Id);
            }

            OnPeerDisconnectedHandler?.Invoke(peer);
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Console.WriteLine("OnNetworkError : " + socketError);
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            OnNetworkReceived?.Invoke(peer, reader, deliveryMethod);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Console.WriteLine("OnNetworkReceiveUnconnected");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey(_key);
        }
    }
}

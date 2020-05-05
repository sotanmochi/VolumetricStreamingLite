// Copyright (c) 2020 Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace LiteNetLibExtension
{
    public class LiteNetLibClientMain : MonoBehaviour, INetEventListener
    {
        [SerializeField] string _address = "localhost";
        [SerializeField] int _port = 11010;
        [SerializeField] string _key = "LiteNetLibExample";

        NetManager _clientNetManager;
        NetPeer _serverPeer;

        public delegate void OnNetworkReceiveDelegate(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod);
        public OnNetworkReceiveDelegate OnNetworkReceived;

        void OnApplicationQuit()
        {
            StopClient();
        }

        void FixedUpdate()
        {
            if (_clientNetManager != null && _clientNetManager.IsRunning)
            {
                _clientNetManager.PollEvents();
            }
        }

        public bool StartClient()
        {
            return StartClient(_address, _port);
        }

        public bool StartClient(string address, int port)
        {
            _clientNetManager = new NetManager(this);
            if (_clientNetManager.Start())
            {
                Debug.Log("LiteNetLib client started!");
                _clientNetManager.Connect(address, port, _key);
                return true;
            }
            else
            {
                Debug.LogError("Could not start LiteNetLib client!");
                return false;
            }
        }

        public void StopClient()
        {
            if (_clientNetManager != null && _clientNetManager.IsRunning)
            {
                _clientNetManager.Stop();
                Debug.Log("LiteNetLib client stopped.");
            }
        }

        public void SendData(NetDataWriter dataWriter, DeliveryMethod deliveryMethod)
        {
            if (_serverPeer != null)
            {
                _serverPeer.Send(dataWriter, deliveryMethod);
            }
            else
            {
                Debug.LogError("Could not send data! Server peer is null!");
            }
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            _serverPeer = peer;
            Debug.Log("OnPeerConnected : " + peer.EndPoint.Address + " : " + peer.EndPoint.Port);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _serverPeer = null;
            Debug.Log("OnPeerDisconnected : " + peer.EndPoint.Address + " : " + peer.EndPoint.Port + " Reason : " + disconnectInfo.Reason.ToString());
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Debug.LogError("OnNetworkError : " + socketError);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            OnNetworkReceived?.Invoke(peer, reader, deliveryMethod);
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Debug.Log("OnNetworkReceiveUnconnected");
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
        }
    }
}

﻿using UnityEngine.Networking;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using System.Text;
using System.Collections.Generic;

namespace UNETSteamworks
{
    public class SteamNetworkConnection : NetworkConnection
    {
        public static int nextId = -1;

        public CSteamID steamId;
        HostTopology m_HostTopology;

        public SteamNetworkConnection() : base()
        {

        }

        public SteamNetworkConnection(CSteamID steamId, HostTopology hostTopology)
        {
            this.steamId = steamId;
            m_HostTopology = hostTopology;
        }

        public void Initialize()
        {
            Initialize(string.Empty, 0, ++nextId, m_HostTopology);
        }

        public override void Initialize(string address, int hostId, int connectionId, HostTopology hostTopology)
        {
            m_HostTopology = hostTopology;
            base.Initialize(address, hostId, connectionId, hostTopology);
        }


        public void Update()
        {
            FlushChannels();
        }

        public override bool TransportSend(byte[] bytes, int numBytes, int channelId, out byte error)
        {
            Debug.LogError("TransportSend");

            if (steamId.m_SteamID == SteamUser.GetSteamID().m_SteamID)
            {
                // sending to self. short circuit
                TransportReceive(bytes, numBytes, channelId);
                error = 0;
                return true;
            }

            EP2PSend eP2PSendType = EP2PSend.k_EP2PSendReliable;

            QosType qos = m_HostTopology.DefaultConfig.Channels[channelId].QOS;
            if (qos == QosType.Unreliable || qos == QosType.UnreliableFragmented || qos == QosType.UnreliableSequenced)
            {
                eP2PSendType = EP2PSend.k_EP2PSendUnreliable;
            }

            if (SteamNetworking.SendP2PPacket(steamId, bytes, (uint)numBytes, eP2PSendType, channelId))
            {
                error = 0;
                return true;
            }
            else
            {
                error = 1;
                return false;
            }
        }
     
    }

}

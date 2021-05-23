using Ceras;
using Ceras.Formatters;
using Intersect.Client.Framework.Network;
using Intersect.Crypto;
using Intersect.Crypto.Formats;
using Intersect.Logging;
using Intersect.Network;
using Intersect.Network.Packets;
using Intersect.Plugins.Helpers;
using Intersect.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Ceras.Resolvers.StandardFormatterResolver;

namespace Intersect.Client.UnityGame.Network
{

    public class UnitySocket : GameSocket
    {

        public static ClientNetwork ClientLidgrenNetwork;

        public static ConcurrentQueue<KeyValuePair<IConnection, IPacket>> PacketQueue = new ConcurrentQueue<KeyValuePair<IConnection, IPacket>>();

        public override void Connect(string host, int port, byte[] rsaBytes, NetworkHelper networkHelper)
        {
            if (ClientLidgrenNetwork != null)
            {
                ClientLidgrenNetwork.Close();
                ClientLidgrenNetwork = null;
            }

            NetworkConfiguration config = new NetworkConfiguration(host, (ushort)port);

            using (Stream stream = new MemoryStream(rsaBytes))
            {
                RsaKey rsaKey = EncryptionKey.FromStream<RsaKey>(stream);
                Debug.Assert(rsaKey != null, "rsaKey != null");
                ClientLidgrenNetwork = new ClientNetwork(networkHelper, config, rsaKey.Parameters);
            }

            if (ClientLidgrenNetwork == null)
            {
                return;
            }
            ClientLidgrenNetwork.Handler = AddPacketToQueue;
            ClientLidgrenNetwork.OnConnected += OnConnected;
            ClientLidgrenNetwork.OnDisconnected += OnDisconnected;
            ClientLidgrenNetwork.OnConnectionDenied += (sender, connectionEventArgs) => OnConnectionFailed(sender, connectionEventArgs, true);
            if (!ClientLidgrenNetwork.Connect())
            {
                Log.Error("An error occurred while attempting to connect.");
            }
        }

        public override void SendPacket(object packet)
        {
            if (packet is IntersectPacket intersectPacket && ClientLidgrenNetwork != null)
            {
                ClientLidgrenNetwork.Send(intersectPacket);
            }
        }

        public static bool AddPacketToQueue(IConnection connection, IPacket packet)
        {
            if (packet is AbstractTimedPacket timedPacket)
            {
                Timing.Global.Synchronize(timedPacket.UTC, timedPacket.Offset);
            }

            PacketQueue.Enqueue(new KeyValuePair<IConnection, IPacket>(connection, packet));

            return true;
        }

        public override void Update()
        {
            while (PacketQueue.Count > 0)
            {
                if (PacketQueue.TryDequeue(out KeyValuePair<IConnection, IPacket> dequeued))
                {
                    OnDataReceived(dequeued.Value);
                }
                else
                {
                    break;
                }
            }
        }

        public override void Disconnect(string reason)
        {
            ClientLidgrenNetwork?.Disconnect(reason);
        }

        public override void Dispose()
        {
            ClientLidgrenNetwork?.Close();
            ClientLidgrenNetwork?.Dispose();
            ClientLidgrenNetwork = null;
        }

        public override bool IsConnected()
        {
            return ClientLidgrenNetwork?.IsConnected ?? false;
        }

        public override int Ping
        => ClientLidgrenNetwork?.Ping ?? -1;


        //private static TypeConfig<Guid> typeguid;
        //private static TypeConfig<Guid?> typeNull;
        //private static TypeConfig<DateTime> typeDT;
        //private static TypeConfig<Enums.Gender> typeGender;
        //private static ReinterpretArrayFormatter<int> rearrayInt;
        //private static CollectionFormatter<Dictionary<Guid, string>, KeyValuePair<Guid, string>> collGuidString;
        //private static CollectionFormatter<Dictionary<string, string>, KeyValuePair<string, string>> collStringString;
        //private static TypeConfig<Enums.Access> typeE1;
        //private static TypeConfig<Enums.AdminActions> typeE2;
        //private static TypeConfig<Enums.ChatboxChannel> typeE3;
        //private static TypeConfig<Enums.ChatBoxTabs> typeE4;
        //private static TypeConfig<Enums.ChatMessageType> typeE5;
        //private static TypeConfig<Enums.ClientPackets> typeE6;
        //private static TypeConfig<Enums.CommonEventTrigger> typeE7;
        //private static TypeConfig<Enums.ConsumableType> typeE8;
        //private static TypeConfig<Enums.CustomSpriteLayers> typeE9;
        //private static TypeConfig<Enums.DamageType> typeE10;
        //private static TypeConfig<Enums.Directions> typeE11;
        //private static TypeConfig<Enums.DisplayLevelStyles> typeE12;
        //private static TypeConfig<Enums.EffectType> typeE13;
        //private static TypeConfig<Enums.EntityTypes> typeE14;
        //private static TypeConfig<Enums.EventGraphicType> typeE15;
        //private static TypeConfig<Enums.EventMovementFrequency> typeE16;
        //private static TypeConfig<Enums.EventMovementSpeed> typeE17;
        //private static TypeConfig<Enums.EventMovementType> typeE18;
        //private static TypeConfig<Enums.EventRenderLayer> typeE19;
        //private static TypeConfig<Enums.EventTrigger> type20;
        //private static TypeConfig<Enums.GameObjectType> type21;
        //private static TypeConfig<Enums.Gender> type22;
        //private static TypeConfig<Enums.GuildMemberUpdateActions> type23;
        //private static TypeConfig<Enums.GuildRanks> type24;
        //private static TypeConfig<Enums.ItemHandling> type25;
        //private static TypeConfig<Enums.ItemTypes> type26;
        //private static TypeConfig<Enums.MapAttributes> type27;
        //private static TypeConfig<Enums.MapListUpdates> type28;
        //private static TypeConfig<Enums.MapZones> type29;
        //private static TypeConfig<Enums.NpcBehavior> type30;
        //private static TypeConfig<Enums.NpcMovement> type31;
        //private static TypeConfig<Enums.NpcSpawnDirection> type32;
        //private static TypeConfig<Enums.QuestObjective> type33;
        //private static TypeConfig<Enums.ServerPackets> type34;
        //private static TypeConfig<Enums.SpellTargetTypes> type35;
        //private static TypeConfig<Enums.SpellTypes> type36;
        //private static TypeConfig<Enums.Stats> type37;
        //private static TypeConfig<Enums.StatusTypes> type38;
        //private static TypeConfig<Enums.StringVariableComparators> type39;
        //private static TypeConfig<Enums.TargetTypes> type40;
        //private static TypeConfig<Enums.VariableComparators> type41;
        //private static TypeConfig<Enums.VariableDataTypes> type42;
        //private static TypeConfig<Enums.VariableMods> type43;
        //private static TypeConfig<Enums.VariableTypes> type44;
        //private static TypeConfig<Enums.Vitals> type45;
        //private static TypeConfig<Enums.WarpDirection> type46;
        //private static TypeConfig<KeyValuePair<Guid, string>> t49;
        //private static TypeConfig<KeyValuePair<string, string>> tc50;
        //private static NullableFormatter<Guid> type48;
        //private static EnumFormatter<Enums.Access> typeE1f;
        //private static EnumFormatter<Enums.AdminActions> typeE2f;
        //private static EnumFormatter<Enums.ChatboxChannel> typeE3f;
        //private static EnumFormatter<Enums.ChatBoxTabs> typeE4f;
        //private static EnumFormatter<Enums.ChatMessageType> typeE5f;
        //private static EnumFormatter<Enums.ClientPackets> typeE6f;
        //private static EnumFormatter<Enums.CommonEventTrigger> typeE7f;
        //private static EnumFormatter<Enums.ConsumableType> typeE8f;
        //private static EnumFormatter<Enums.CustomSpriteLayers> typeE9f;
        //private static EnumFormatter<Enums.DamageType> typeE10f;
        //private static EnumFormatter<Enums.Directions> typeE11f;
        //private static EnumFormatter<Enums.DisplayLevelStyles> typeE12f;
        //private static EnumFormatter<Enums.EffectType> typeE13f;
        //private static EnumFormatter<Enums.EntityTypes> typeE14f;
        //private static EnumFormatter<Enums.EventGraphicType> typeE15f;
        //private static EnumFormatter<Enums.EventMovementFrequency> typeE16f;
        //private static EnumFormatter<Enums.EventMovementSpeed> typeE17f;
        //private static EnumFormatter<Enums.EventMovementType> typeE18f;
        //private static EnumFormatter<Enums.EventRenderLayer> typeE19f;
        //private static EnumFormatter<Enums.EventTrigger> type20f;
        //private static EnumFormatter<Enums.GameObjectType> type21f;
        //private static EnumFormatter<Enums.Gender> type22f;
        //private static EnumFormatter<Enums.GuildMemberUpdateActions> type23f;
        //private static EnumFormatter<Enums.GuildRanks> type24f;
        //private static EnumFormatter<Enums.ItemHandling> type25f;
        //private static EnumFormatter<Enums.ItemTypes> type26f;
        //private static EnumFormatter<Enums.MapAttributes> type27f;
        //private static EnumFormatter<Enums.MapListUpdates> type28f;
        //private static EnumFormatter<Enums.MapZones> type29f;
        //private static EnumFormatter<Enums.NpcBehavior> type30f;
        //private static EnumFormatter<Enums.NpcMovement> type31f;
        //private static EnumFormatter<Enums.NpcSpawnDirection> type32f;
        //private static EnumFormatter<Enums.QuestObjective> type33f;
        //private static EnumFormatter<Enums.ServerPackets> type34f;
        //private static EnumFormatter<Enums.SpellTargetTypes> type35f;
        //private static EnumFormatter<Enums.SpellTypes> type36f;
        //private static EnumFormatter<Enums.Stats> type37f;
        //private static EnumFormatter<Enums.StatusTypes> type38f;
        //private static EnumFormatter<Enums.StringVariableComparators> type39f;
        //private static EnumFormatter<Enums.TargetTypes> type40f;
        //private static EnumFormatter<Enums.VariableComparators> type41f;
        //private static EnumFormatter<Enums.VariableDataTypes> type42f;
        //private static EnumFormatter<Enums.VariableMods> type43f;
        //private static EnumFormatter<Enums.VariableTypes> type44f;
        //private static EnumFormatter<Enums.Vitals> type45f;
        //private static EnumFormatter<Enums.WarpDirection> type46f;
        //private static ReinterpretArrayFormatter<Guid> t50;
        //private static MultiDimensionalArrayFormatter<GameObjects.Maps.Tile> t51;
        //private static MultiDimensionalArrayFormatter<Guid> t52;
        //private static ReinterpretFormatter<Guid> t53;
        //private static ReinterpretArrayFormatter<byte> t54;
        //private static ArrayFormatter<GameObjects.Maps.Tile> t55;
        //private static ArrayFormatter<object> t56;
        //private static MultiDimensionalArrayFormatter<object> t57;
        //private static ReinterpretArrayFormatter<bool> t58;
        //private static KeyValuePairFormatter<Guid, string> t59;
        //private static KeyValuePairFormatter<string, string> t60;

    }
}
using System;
using System.Collections.Generic;

using Intersect.Collections;
using Intersect.Network.Packets;
using Intersect.Network.Packets.Client;
using Intersect.Network.Packets.Server;
using MessagePack;

namespace Intersect.Network
{
    //Packets
    [Union(0, typeof(AbstractTimedPacket))]
    [Union(1, typeof(ConnectionPacket))]
    [Union(2, typeof(EditorPacket))]
    [Union(3, typeof(SlotQuantityPacket))]
    [Union(4, typeof(SlotSwapPacket))]

    //Client		
    [Union(5, typeof(AbandonQuestPacket))]
    [Union(6, typeof(AcceptTradePacket))]
    [Union(7, typeof(ActivateEventPacket))]
    [Union(8, typeof(AdminActionPacket))]
    [Union(9, typeof(BlockPacket))]
    [Union(10, typeof(BumpPacket))]
    [Union(11, typeof(Packets.Client.ChatMsgPacket))]
    [Union(12, typeof(CloseBagPacket))]
    [Union(13, typeof(CloseBankPacket))]
    [Union(14, typeof(CloseCraftingPacket))]
    [Union(15, typeof(CloseShopPacket))]
    [Union(16, typeof(CraftItemPacket))]
    [Union(17, typeof(CreateAccountPacket))]
    [Union(18, typeof(CreateCharacterPacket))]
    [Union(19, typeof(DeclineTradePacket))]
    [Union(20, typeof(DeleteCharacterPacket))]
    [Union(21, typeof(DirectionPacket))]
    [Union(22, typeof(EnterGamePacket))]
    [Union(23, typeof(EventInputVariablePacket))]
    [Union(24, typeof(EventResponsePacket))]
    [Union(25, typeof(ForgetSpellPacket))]
    [Union(26, typeof(FriendRequestResponsePacket))]
    [Union(27, typeof(GuildInviteAcceptPacket))]
    [Union(28, typeof(GuildInviteDeclinePacket))]
    [Union(29, typeof(GuildLeavePacket))]
    [Union(30, typeof(HotbarUpdatePacket))]
    [Union(31, typeof(LoginPacket))]
    [Union(32, typeof(LogoutPacket))]
    [Union(33, typeof(NeedMapPacket))]
    [Union(34, typeof(NewCharacterPacket))]
    [Union(35, typeof(OpenAdminWindowPacket))]
    [Union(36, typeof(Packets.Client.PartyInvitePacket))]
    [Union(37, typeof(PartyInviteResponsePacket))]
    [Union(38, typeof(PartyKickPacket))]
    [Union(39, typeof(PartyLeavePacket))]
    [Union(40, typeof(PickupItemPacket))]
    [Union(41, typeof(PictureClosedPacket))]
    [Union(42, typeof(QuestResponsePacket))]
    [Union(43, typeof(RequestFriendsPacket))]
    [Union(44, typeof(RequestGuildPacket))]
    [Union(45, typeof(RequestPasswordResetPacket))]
    [Union(46, typeof(ResetPasswordPacket))]
    [Union(47, typeof(SelectCharacterPacket))]
    [Union(48, typeof(Packets.Client.TradeRequestPacket))]
    [Union(49, typeof(TradeRequestResponsePacket))]
    [Union(50, typeof(UnequipItemPacket))]
    [Union(51, typeof(UpdateFriendsPacket))]
    [Union(52, typeof(UpdateGuildMemberPacket))]
    [Union(53, typeof(UpgradeStatPacket))]
    [Union(54, typeof(UseItemPacket))]
    [Union(55, typeof(UseSpellPacket))]

    //Server		
    [Union(56, typeof(ActionMsgPacket))]
    [Union(57, typeof(ActionMsgPackets))]
    [Union(58, typeof(AdminPanelPacket))]
    [Union(59, typeof(AnnouncementPacket))]
    [Union(60, typeof(BagPacket))]
    [Union(61, typeof(BankPacket))]
    [Union(62, typeof(CancelCastPacket))]
    [Union(63, typeof(CharacterCreationPacket))]
    [Union(64, typeof(CharacterPacket))]
    [Union(65, typeof(CharactersPacket))]
    [Union(66, typeof(ChatBubblePacket))]
    [Union(67, typeof(Packets.Server.ChatMsgPacket))]
    [Union(68, typeof(ConfigPacket))]
    [Union(69, typeof(CraftingTablePacket))]
    [Union(70, typeof(EnteringGamePacket))]
    [Union(71, typeof(EnterMapPacket))]
    [Union(72, typeof(EntityDashPacket))]
    [Union(73, typeof(EntityDiePacket))]
    [Union(74, typeof(EntityDirectionPacket))]
    [Union(75, typeof(EntityLeftPacket))]
    [Union(76, typeof(EntityMovementPackets))]
    [Union(77, typeof(EntityMovePacket))]
    [Union(78, typeof(EntityPacket))]
    [Union(79, typeof(EntityStatsPacket))]
    [Union(80, typeof(EntityVitalsPacket))]
    [Union(81, typeof(EntityZDimensionPacket))]
    [Union(82, typeof(EquipmentPacket))]
    [Union(83, typeof(ErrorMessagePacket))]
    [Union(84, typeof(EventDialogPacket))]
    [Union(85, typeof(ExperiencePacket))]
    [Union(86, typeof(FriendRequestPacket))]
    [Union(87, typeof(FriendsPacket))]
    [Union(88, typeof(GameDataPacket))]
    [Union(89, typeof(GameObjectPacket))]
    [Union(90, typeof(GuildInvitePacket))]
    [Union(91, typeof(GuildPacket))]
    [Union(92, typeof(HidePicturePacket))]
    [Union(93, typeof(HoldPlayerPacket))]
    [Union(94, typeof(HotbarPacket))]
    [Union(95, typeof(InputVariablePacket))]
    [Union(96, typeof(InventoryPacket))]
    [Union(97, typeof(InventoryUpdatePacket))]
    [Union(98, typeof(ItemCooldownPacket))]
    [Union(99, typeof(LabelPacket))]
    [Union(100, typeof(MapAreaPacket))]
    [Union(101, typeof(MapEntitiesPacket))]
    [Union(102, typeof(MapEntityStatusPacket))]
    [Union(103, typeof(MapEntityVitalsPacket))]
    [Union(104, typeof(MapGridPacket))]
    [Union(105, typeof(MapItemsPacket))]
    [Union(106, typeof(MapItemUpdatePacket))]
    [Union(107, typeof(MapListPacket))]
    [Union(108, typeof(MapPacket))]
    [Union(109, typeof(MoveRoutePacket))]
    [Union(110, typeof(NpcAggressionPacket))]
    [Union(111, typeof(OpenEditorPacket))]
    [Union(112, typeof(Packets.Server.PartyInvitePacket))]
    [Union(113, typeof(PartyMemberPacket))]
    [Union(114, typeof(PartyPacket))]
    [Union(115, typeof(PartyUpdatePacket))]
    [Union(116, typeof(PasswordResetResultPacket))]
    [Union(117, typeof(PlayAnimationPacket))]
    [Union(118, typeof(PlayAnimationPackets))]
    [Union(119, typeof(PlayerDeathPacket))]
    [Union(120, typeof(PlayMusicPacket))]
    [Union(121, typeof(PlaySoundPacket))]
    [Union(122, typeof(ProjectileDeadPacket))]
    [Union(123, typeof(QuestOfferPacket))]
    [Union(124, typeof(QuestProgressPacket))]
    [Union(125, typeof(ShopPacket))]
    [Union(126, typeof(ShowPicturePacket))]
    [Union(127, typeof(SpellCastPacket))]
    [Union(128, typeof(SpellCooldownPacket))]
    [Union(129, typeof(SpellPacket))]
    [Union(130, typeof(SpellsPacket))]
    [Union(131, typeof(SpellUpdatePacket))]
    [Union(132, typeof(StatPointsPacket))]
    [Union(133, typeof(StatusPacket))]
    [Union(134, typeof(StopMusicPacket))]
    [Union(135, typeof(StopSoundsPacket))]
    [Union(136, typeof(TargetOverridePacket))]
    [Union(137, typeof(TimeDataPacket))]
    [Union(138, typeof(TimePacket))]
    [Union(139, typeof(TradePacket))]
    [Union(140, typeof(Packets.Server.TradeRequestPacket))]

    //Editor
    [Union(141, typeof(Packets.Editor.LoginPacket))]
    [Union(142, typeof(Packets.Editor.PingPacket))]

    [MessagePackObject]
    public abstract class IntersectPacket : IPacket
    {
        [IgnoreMember]
        private byte[] mCachedData = null;

        [IgnoreMember]
        private byte[] mCachedCompresedData = null;

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <inheritdoc />
        [IgnoreMember]
        public virtual byte[] Data
        {
            get
            {
                if (mCachedData == null)
                    mCachedData = MessagePacker.Instance.Serialize(this) ?? throw new Exception("Failed to serialize packet.");

                return mCachedData;
            }
        }

        public virtual void ClearCachedData()
        {
            mCachedData = null;
        }

        [IgnoreMember]
        public virtual bool IsValid => true;
        [IgnoreMember]
        public virtual long ReceiveTime { get; set; }
        [IgnoreMember]
        public virtual long ProcessTime { get; set; }

        /// <inheritdoc />
        public virtual Dictionary<string, SanitizedValue<object>> Sanitize()
        {
            return null;
        }

    }

}

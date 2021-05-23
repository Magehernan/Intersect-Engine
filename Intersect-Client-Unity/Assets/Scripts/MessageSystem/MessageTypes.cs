namespace Intersect.Client.MessageSystem {
	public enum MessageTypes {
		None = 0,
		ShowLogin = 1,
		ShowRegister = 2,
		ShowMainMenu = 3,
		ChatMsgPacket = 4,
		CharactersPacket = 5,
		MapPacket = 6,
		EntityPacket = 7,
		EnteringGamePacket = 8,
		NetworkStatus = 9,
		EntityMovePacket = 10,
		JoinGamePacket = 11,
		EntityLeftPacket = 12,
		EntityPositionPacket = 13
	}
}
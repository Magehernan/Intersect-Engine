using Intersect.Enums;
using System;
using System.Collections.Generic;

namespace Intersect.Client.Interface.Game.Chat {

	public class ChatboxMsg {
		private static List<ChatboxMsg> sGameMessages = new List<ChatboxMsg>();

		// TODO: Move to a configuration file to make this player configurable?
		/// <summary>
		/// Contains the configuration of which message types to display in each chat tab.
		/// </summary>
		private static Dictionary<ChatboxTab, List<ChatMessageType>> sTabMessageTypes = new Dictionary<ChatboxTab, List<ChatMessageType>>() {
			// All has ALL tabs unlocked, so really we don't have to worry about that one.
			{ ChatboxTab.Local, new List<ChatMessageType> { ChatMessageType.Local, ChatMessageType.PM, ChatMessageType.Admin } },
			{ ChatboxTab.Party, new List<ChatMessageType> { ChatMessageType.Party, ChatMessageType.PM, ChatMessageType.Admin } },
			{ ChatboxTab.Global, new List<ChatMessageType> { ChatMessageType.Global, ChatMessageType.PM, ChatMessageType.Admin } },
			{ ChatboxTab.Guild, new List<ChatMessageType> { ChatMessageType.Guild, ChatMessageType.PM, ChatMessageType.Admin } },
			{ ChatboxTab.System, new List<ChatMessageType> {
				ChatMessageType.Experience, ChatMessageType.Loot, ChatMessageType.Inventory, ChatMessageType.Bank,
				ChatMessageType.Combat, ChatMessageType.Quest, ChatMessageType.Crafting, ChatMessageType.Trading,
				ChatMessageType.Friend, ChatMessageType.Spells, ChatMessageType.Notice, ChatMessageType.Error,
				ChatMessageType.Admin } },
		};

		/// <summary>
		/// The contents of this message.
		/// </summary>
		public string Message { get; } = string.Empty;

		/// <summary>
		/// The color of this message.
		/// </summary>
		public Color Color { get; }

		// The target of this message.
		public string Target { get; } = string.Empty;

		/// <summary>
		/// The type of this message.
		/// </summary>
		public ChatMessageType Type { get; }

		public ChatboxMsg(string msg, Color clr, ChatMessageType type, string target = "") {
			Message = msg;
			Color = clr;
			Target = target;
			Type = type;
		}


		/// <summary>
		/// Adds a new chat message to the stored list.
		/// </summary>
		/// <param name="msg">The message to add.</param>
		public static void AddMessage(ChatboxMsg msg) {
			sGameMessages.Add(msg);
		}
		public static List<ChatboxMsg> GetMessages() {
			return sGameMessages;
		}

		/// <summary>
		/// Retrieves all messages that should be displayed in the provided tab.
		/// </summary>
		/// <param name="tab">The tab for which to retrieve all messages.</param>
		/// <returns>Returns a list of chat messages.</returns>
		public static List<ChatboxMsg> GetMessages(ChatboxTab tab) {
			List<ChatboxMsg> output = new List<ChatboxMsg>();

			// Are we looking for all messages?
			if (tab == ChatboxTab.All) {
				output = GetMessages();
			} else {
				// No, sort them out! Select what we want to display in this tab.
				foreach (ChatboxMsg message in sGameMessages) {
					if (sTabMessageTypes[tab].Contains(message.Type)) {
						output.Add(message);
					}
				}
			}

			return output;
		}

		/// <summary>
		/// Clears all stored messages.
		/// </summary>
		public static void ClearMessages() {
			sGameMessages.Clear();
		}
	}
}

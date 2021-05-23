using Intersect.Client.Utils;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Intersect.Client.MessageSystem {

	public static class MessageManager {
		private static readonly Dictionary<MessageTypes, List<Action<object>>> _listeners = new Dictionary<MessageTypes, List<Action<object>>>();

		private static readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();
		private static bool _running = false;

		private struct Message {
			public MessageTypes type;
			public object message;
		}

		public static void AttachListener(MessageTypes type, Action<object> handler) {
			if (!_running) {
				_running = true;
				Singleton.Instance.StartCoroutine(ProcessMessagesCoroutine());
			}

			if (!_listeners.ContainsKey(type)) {
				_listeners.Add(type, new List<Action<object>>());
			}

			List<Action<object>> listenerList = _listeners[type];

			if (listenerList.Contains(handler)) {
				Debug.LogErrorFormat($"MessageSystem - Attached duplicate listener: {type}");
			} else {
				listenerList.Add(handler);
			}
		}

		public static void DetachListener(MessageTypes type, Action<object> handler) {
			if (!_listeners.ContainsKey(type)) {
				Debug.LogErrorFormat($"MessageSystem - Detached non-existant listener: {type}");
			} else {
				//elimino el handler de la lista
				_listeners[type].Remove(handler);
				//si la lista esta vacia la borro
				if (_listeners[type].Count == 0) {
					_listeners.Remove(type);
					//si no quedan mas escuchando lo paro
					if (_listeners.Count == 0) {
						_running = false;
					}
				}
			}
		}

		public static bool SendMessage(MessageTypes type, object msg = null, bool now = false) {
			if (!_listeners.ContainsKey(type)) {
				return false;
			}

			//Debug.Log($"MessageSystem - SendMessage: {type} now: {now}");
			if (now) {
				List<Action<object>> handler = _listeners[type];
				for (int i = 0; i < handler.Count; i++) {
					handler[i](msg);
				}
			} else {
				_messageQueue.Enqueue(new Message() { type = type, message = msg });
			}
			return true;
		}

		private const double maxQueueProcessingTime = 0.16667;
		private static IEnumerator ProcessMessagesCoroutine() {
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			while (_running) {

				while (_messageQueue.Count > 0) {
					if (!timer.IsRunning) {
						timer.Start();
					}
					if (_messageQueue.TryDequeue(out Message message)) {
						if (timer.Elapsed.TotalMilliseconds > maxQueueProcessingTime) {
							yield return null;
							timer.Reset();
							timer.Start();
						}

						if (_listeners.ContainsKey(message.type)) {
							List<Action<object>> handler = _listeners[message.type];
							for (int i = handler.Count - 1; i >= 0; i--) {
								try {
									handler[i](message.message);
								} catch (Exception e) {
									Debug.LogError(e);
								}
							}
						}
					}
				}

				yield return null;
				if (timer.IsRunning) {
					timer.Reset();
				}
			}
		}
	}
}
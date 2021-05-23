using Intersect.Client.UnityGame.Audio;
using Intersect.Client.UnityGame.Graphics;
using Intersect.Client.UnityGame.Graphics.Maps;
using Intersect.Enums;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Intersect.Client.UnityGame
{

    public class UnityFactory : MonoBehaviour
    {

        [Header("Maps")]
        [SerializeField]
        private MapRenderer mapRendererPrefab;
        [SerializeField]
        private Transform mapTilesParent;
        [SerializeField]
        private Tilemap tilemapPrefab;

        [Header("Entities")]
        [SerializeField]
        private EntityRenderer entityRendererPrefab;
        [SerializeField]
        private PlayerEntityRenderer playerEntityPrefab;
        [SerializeField]
        private EntityRenderer projectilEntityPrefab;


        [Header("Chat")]
        [SerializeField]
        private ChatBubbleRenderer chatBubbleRendererPrefab;

        [Header("Lights")]
        [SerializeField]
        private LightRenderer lightRendererPrefab;

        [Header("Items")]
        [SerializeField]
        private ItemRenderer itemRendererPrefab;

        [Header("Animations")]
        [SerializeField]
        private AnimationRenderer animationRendererPrefab;

        [Header("Action Messages")]
        [SerializeField]
        private ActionMessageRenderer actionMessageRendererPrefab;

        [Header("Audio")]
        [SerializeField]
        private AudioPlayer audioPlayerPrefab;

        private static UnityFactory instance;

        private void Awake()
        {
            instance = this;
        }

        private static T Instantiate<T>(T prefab, string name, Transform parent = null) where T : Behaviour
        {
            T instance = Instantiate(prefab, parent, false);
            instance.name = name;
            return instance;
        }

        public static MapRenderer GetMapRenderer(string name)
        {
            return Instantiate(instance.mapRendererPrefab, $"{nameof(MapRenderer)} ({name})", instance.mapTilesParent);
        }

        internal static Tilemap GetTilemap(string name, Transform parent = null)
        {
            return Instantiate(instance.tilemapPrefab, $"{nameof(Tilemap)} ({name})", parent);
        }

        public static LightRenderer GetLightRender(string name, Transform parent = default)
        {
            return Instantiate(instance.lightRendererPrefab, $"{nameof(LightRenderer)} ({name})", parent);
        }

        public static EntityRenderer GetEntityRenderer(EntityTypes entityType, string name)
        {
            return entityType switch
            {
                EntityTypes.GlobalEntity => Instantiate(instance.entityRendererPrefab, $"EntityRenderer ({name})"),
                EntityTypes.Player => Instantiate(instance.playerEntityPrefab, $"PlayerEntityRenderer ({name})"),
                EntityTypes.Resource => Instantiate(instance.entityRendererPrefab, $"ResouceEntityRenderer ({name})"),
                EntityTypes.Projectile => Instantiate(instance.projectilEntityPrefab, $"ProjectileEntityRenderer ({name})"),
                EntityTypes.Event => Instantiate(instance.entityRendererPrefab, $"EventEntityRenderer ({name})"),
                _ => throw new NotImplementedException($"EntityRenderer not implemented: {entityType}"),
            };
        }

        public static ItemRenderer GetItemRenderer(string name)
        {
            return Instantiate(instance.itemRendererPrefab, $"{nameof(ItemRenderer)} ({name})");
        }

        public static AnimationRenderer GetAnimationRenderer(string name)
        {
            return Instantiate(instance.animationRendererPrefab, $"{nameof(AnimationRenderer)} ({name})");
        }

        public static ActionMessageRenderer GetActionMessageRenderer(string name)
        {
            return Instantiate(instance.actionMessageRendererPrefab, $"{nameof(ActionMessageRenderer)} ({name})");
        }

        public static AudioPlayer GetAudioPlayer(string name)
        {
            return Instantiate(instance.audioPlayerPrefab, $"{nameof(AudioPlayer)} ({name})");
        }

        public static ChatBubbleRenderer GetChatBubble(string name, Transform chatBubbleParent)
        {
            return Instantiate(instance.chatBubbleRendererPrefab, $"{nameof(ChatBubbleRenderer)} ({name})", chatBubbleParent);
        }
    }
}
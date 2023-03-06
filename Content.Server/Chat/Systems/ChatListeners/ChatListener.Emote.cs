using Robust.Server.GameObjects;
using Robust.Shared.Utility;
using Content.Shared.Chat;
using Content.Shared.IdentityManagement;

namespace Content.Server.Chat.Systems
{
    /// <summary>
    /// </summary>
    public sealed class EmoteListenerSystem : ChatListenerSystem
    {
        private ISawmill _sawmill = default!;

        [Dependency] private readonly ChatSystem _chatSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            Before = new Type[] { typeof(SayListenerSystem) };

            InitializeListeners();

            _sawmill = Logger.GetSawmill("chat.emote");
        }

        public override void OnGetRecipients(ref EntityChatGetRecipientsEvent args)
        {
            if (args.Handled || args.Chat.ClaimedBy != this.GetType())
                return;

            // TODO: unhardcode
            var emoteRange = 10;

            foreach (var (playerEntity, distance) in _chatSystem.GetPlayerEntitiesInRange(args.Chat.Source, emoteRange))
            {
                var recipientData = new EntityChatData();
                recipientData.SetData(ChatRecipientDataSay.Distance, distance);
                args.Chat.Recipients.TryAdd(playerEntity, recipientData);
            }

            args.Handled = true;
        }

        public override void OnChat(ref GotEntityChatEvent args)
        {
            if (args.Handled || args.Chat.ClaimedBy != this.GetType())
                return;

            args.Handled = true;

            if (!TryComp<ActorComponent>(args.Recipient, out var actorComponent))
                return;

            var identity = args.RecipientData.GetData<string>(ChatRecipientDataSay.Identity) ?? Identity.Name(args.Chat.Source, EntityManager);
            var message = args.RecipientData.GetData<string>(ChatRecipientDataSay.Message) ?? args.Chat.Message;
            var wrappedMessage = args.RecipientData.GetData<string>(ChatRecipientDataSay.WrappedMessage) ?? Loc.GetString("chat-manager-entity-me-wrap-message",
                ("entityName", identity),
                ("message", FormattedMessage.EscapeText(message)));

            _chatManager.ChatMessageToOne(args.Chat.Channel,
                message,
                wrappedMessage,
                args.Chat.Source,
                false, // hideChat,
                actorComponent.PlayerSession.ConnectedClient);
        }
    }

    public sealed partial class ChatSystem
    {
        /// <summary>
        /// Try to send an emote message from an entity.
        /// </summary>
        public bool TrySendEmote(EntityUid source, string message, EntityUid? speaker = null)
        {
            var chat = new EntityChat(source, message)
            {
                Channel = ChatChannel.Emotes,
                ClaimedBy = typeof(EmoteListenerSystem),
            };

            if (speaker != null)
                chat.SetData(ChatDataSay.RelayedSpeaker, speaker);

            return TrySendChat(source, chat);
        }
    }
}

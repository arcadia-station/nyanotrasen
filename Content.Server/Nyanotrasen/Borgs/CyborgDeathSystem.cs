using Content.Server.Chat.Systems;
using Content.Shared.Mobs;
using Content.Shared.Tag;
using Content.Shared.IdentityManagement;

namespace Content.Server.Borgs
{
    public sealed class CyborgDeath : EntitySystem
    {
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CyborgDeathComponent, MobStateChangedEvent>(OnChangeState);
        }
        
        private void OnChangeState(EntityUid uid, CyborgDeathComponent component, MobStateChangedEvent args)
        {
            if (args.NewMobState == MobState.Dead){
                string message = Loc.GetString("death-gasp-borg", ("ent", Identity.Entity(uid, EntityManager)));

                //Plays death sound and prints death message/popup
                _audio.PlayPvs("/Audio/Nyanotrasen/Mobs/Borg/borg_deathsound.ogg", uid);
                _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Emote, false, force:true);

                //Stop dead borg from being movable AA cards by removing their ability to bump doors.
                _tagSystem.RemoveTag(uid, "DoorBumpOpener");
            
            }
            else if(args.NewMobState == MobState.Alive && args.OldMobState == MobState.Dead)
            {
                //Gives back Borg ability to bump doors when they are alive again
                _tagSystem.AddTag(uid, "DoorBumpOpener");
            }
            else
                return;
        }
    }
}
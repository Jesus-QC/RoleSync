using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using UnityEngine.Networking;
using Player = Exiled.Events.Handlers.Player;

namespace RoleSync.Plugin
{
    public class RoleSync : Plugin<Configuration>
    {
        public override string Name { get; } = "RoleSync";
        public override string Prefix { get; } = "role_sync";
        public override string Author { get; } = "Jesus-QC";
        public override Version Version { get; } = new Version(1, 0, 1);
        public override Version RequiredExiledVersion { get; } = new Version(5, 3, 0);

        public override void OnEnabled()
        {
            Player.Verified += OnVerified;
            
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Verified -= OnVerified;
            
            base.OnDisabled();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            Timing.RunCoroutine(CheckSynced(ev.Player));
        }

        private IEnumerator<float> CheckSynced(Exiled.API.Features.Player player)
        {
            var url = $"https://rolesync.scpsl.tools/api/syncedusers/{Config.ServerToken}/{player.RawUserId}";

            Log.Debug($"Requested {url}", Config.ShowDebug);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());
                Log.Debug("Request done", Config.ShowDebug);

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    if(webRequest.responseCode == 404)
                        yield break;
                    
                    Log.Error($"Error sending the message: {webRequest.responseCode} - {webRequest.error}");
                    yield break;
                }

                var groupName = webRequest.downloadHandler.text;

                if (string.IsNullOrEmpty(groupName))
                {
                    Log.Debug($"Group name null for user {player.Nickname} with id {player.RawUserId}",
                        Config.ShowDebug);
                }

                if (!Server.PermissionsHandler.GetAllGroupsNames().Contains(groupName))
                {
                    Log.Debug(
                        $"User {player.Nickname} with id {player.RawUserId} was found but not synced because the ra group {groupName} was not found.",
                        Config.ShowDebug);
                    yield break;
                }

                var g = Server.PermissionsHandler.GetGroup(groupName);
                player.SetRank(g.BadgeText, g);
                Log.Debug($"User {player.Nickname} with id {player.RawUserId} synced successfully.");
            }
        }
    }
}
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using System;

namespace WhisperPlugin;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
public static class ChatCommands_RpcSendChatPrefix
{
    public static void SetName(PlayerControl target, string name)
    {
        var HostData = AmongUsClient.Instance.GetHost();
        if (HostData != null && !HostData.Character.Data.Disconnected)
        {
            foreach (var item in PlayerControl.AllPlayerControls)
            {
                MessageWriter nameWriter = AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.SetName, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(item));
                nameWriter.Write(name);
                AmongUsClient.Instance.FinishRpcImmediately(nameWriter);
            }
        }

    }

    public static PlayerControl GetPlayerByName(string name)
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.DefaultOutfit.PlayerName == name) return player;
        }
        return null;
    }

    public static bool Prefix(string chatText, PlayerControl __instance)
    {

        if (!chatText.StartsWith("/"))
        {
            return true;
        }
        string command = chatText.Substring(1).Split(' ')[0];
        PlayerControl sender = __instance;
        if (command == "귓" || command == "귓속말" || command == "w")
        {
            string playerName = chatText.Substring(command.Length + 2).Split("'")[1];
            PlayerControl player = GetPlayerByName(playerName);
            if (!player)
            {
                HudManager.Instance.Notifier.AddItem("<#fff000>플레이어가 존재하지 않습니다.</color>");
                return false;
            }
            if (player == sender)
            {
                HudManager.Instance.Notifier.AddItem("<#fff000>자기 자신에게 귓속말할 수 없습니다.</color>");
                return false;
            }
            if (chatText.Length <= (command.Length + playerName.Length + 5))
            {
                HudManager.Instance.Notifier.AddItem("<#fff000>잘못된 명령어입니다.</color>");
                return false;
            }
            string whisperText = chatText.Substring(command.Length + playerName.Length + 5);
            string originalName = sender.Data.DefaultOutfit.PlayerName;
            SetName(sender, "<#ccc>" + originalName + " → " + playerName + "</color>");
            MessageWriter writer1 = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SendChat, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(player));
            MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SendChat, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(sender));
            writer1.Write(whisperText);
            writer2.Write(whisperText);
            AmongUsClient.Instance.FinishRpcImmediately(writer1);
            AmongUsClient.Instance.FinishRpcImmediately(writer2);
            SetName(sender, originalName);
            return false;
        }
        else if (command == "임포챗")
        {
            if (sender.Data.Role.TeamType != RoleTeamTypes.Impostor)
            {
                HudManager.Instance.Notifier.AddItem("<#fff000>임포스터인 경우에만 사용이 가능합니다.</color>");
                return false;
            }
            if (chatText.Length <= (command.Length + 2))
            {
                HudManager.Instance.Notifier.AddItem("<#fff000>잘못된 명령어입니다.</color>");
                return false;
            }
            string originalName = sender.Data.DefaultOutfit.PlayerName;
            SetName(sender, originalName + " → " + DestroyableSingleton<TranslationController>.Instance.GetString(DestroyableSingleton<ImpostorRole>.Instance.StringName, Il2CppSystem.Array.Empty<Il2CppSystem.Object>()));
            string sendText = chatText.Substring(command.Length + 2);
            foreach (var player in PlayerControl.AllPlayerControls)
            {
                if (player.Data.Role.TeamType == RoleTeamTypes.Impostor)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(sender.NetId, (byte)RpcCalls.SendChat, SendOption.None, AmongUsClient.Instance.GetClientIdFromCharacter(player));
                    writer.Write(sendText);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            }
            SetName(sender, originalName);
        }
        return false;
    }
}
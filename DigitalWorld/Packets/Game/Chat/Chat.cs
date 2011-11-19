using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;

namespace Digital_World.Packets.Game
{
    public enum ChatType : short
    {
        Unknown = 261,
        Normal = 263,
        Party,
        Guild,
        Whisper,
        Shout,
        Megaphone
    }
    /// <summary>
    /// Chat Packet sent to speaker
    /// </summary>
    public class BaseChat:Packet
    {
        /// <summary>
        /// Send to speaker
        /// </summary>
        /// <param name="chatType"></param>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public BaseChat(ChatType chatType, string sender, string message)
        {
            packet.Type(1006);
            packet.WriteShort((short)chatType);
            packet.WriteString(sender);
            packet.WriteString(message);
            packet.WriteByte(0);
        }

        public BaseChat(ChatType chatType, string message)
        {
            packet.Type(1006);
            packet.WriteShort((short)chatType);
            packet.WriteString(message);
            packet.WriteByte(0);
        }

        /// <summary>
        /// Found with ChatType Unknown
        /// </summary>
        /// <param name="chatType"></param>
        /// <param name="message"></param>
        public BaseChat(ChatType chatType, Character Speaker, string message)
        {
            packet.Type(1006);
            packet.WriteShort((short)chatType);
            packet.WriteByte(0);
            packet.WriteShort(Speaker.TamerHandle);
            packet.WriteInt(Speaker.Location.PosX);
            packet.WriteInt(Speaker.Location.PosY);
            packet.WriteShort(267); //Another Chattype
            packet.WriteString(Speaker.Name);
            packet.WriteString(message);
            packet.WriteByte(0);
        }

        /// <summary>
        /// Send to speaker
        /// </summary>
        /// <param name="chatType"></param>
        /// <param name="message"></param>
        public BaseChat(ChatType chatType, short handle, string message)
        {
            packet.Type(1006);
            packet.WriteShort((short)chatType);
            packet.WriteShort(handle);
            packet.WriteString(message);
            packet.WriteByte(0);
        }
    }
}

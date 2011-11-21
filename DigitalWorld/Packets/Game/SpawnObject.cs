using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Digital_World.Packets.Game
{
    public class SpawnObject :Packet
    {
        public SpawnObject(short hObject, int oX, int oY, int X1, int Y1,uint Model, int X2, int Y2, short hDigimon, int dX, int dY)
        {
            packet.Type(1006);

            packet.WriteShort(260);
            packet.WriteByte(0);

            packet.WriteShort(hObject);
            packet.WriteInt(oX);
            packet.WriteInt(oY);

            packet.WriteShort(259);
            packet.WriteByte(0);
            packet.WriteInt(X1);
            packet.WriteInt(Y1);
            packet.WriteUInt(Model);
            packet.WriteInt(X2);
            packet.WriteInt(Y2);
            packet.WriteInt(2815);
            packet.WriteShort(0);

            packet.WriteShort(261);
            packet.WriteByte(0);
            packet.WriteShort(hDigimon);
            packet.WriteInt(dX);
            packet.WriteInt(dY);

            packet.WriteByte(0);
        }

        public SpawnObject(uint Model, int X, int Y)
        {
            packet.Type(1006);

            packet.WriteShort(259);
            packet.WriteByte(0);
            packet.WriteInt(X);
            packet.WriteInt(Y);
            packet.WriteUInt(Model);
            packet.WriteInt(X);
            packet.WriteInt(Y);
            packet.WriteByte(0xff);
            packet.WriteShort(99);
            packet.WriteShort(0);
            packet.WriteShort(0);
            //packet.WriteByte(0);
        }
    }
}

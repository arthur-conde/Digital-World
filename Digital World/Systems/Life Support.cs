using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Digital_World.Entities;

namespace Digital_World.Systems
{
    public partial class Yggdrasil
    {
        /// <summary>
        /// Restores HP and DS over time? Save Characters?
        /// </summary>
        public void LifeSupport(object state)
        {
            try
            {
                while (true)
                {
                    foreach (Client client in Clients)
                    {
                        if (client.Tamer == null) continue;
                        Character Tamer = client.Tamer;
                        Digimon Partner = Tamer.Partner;

                        for (int i = 0; i < Tamer.DigimonList.Length; i++)
                        {
                            //Check if in battle?
                            if (Tamer.DigimonList[i] == null) continue;
                            Digimon digimon = Tamer.DigimonList[i];

                            //Console.WriteLine("Recovering {0}...", digimon.Name);
                            digimon.Stats.Recover();
                        }

                        client.Send(new Packets.Game.Status(Tamer.DigimonHandle, Partner.Stats));
                    }

                    Thread.Sleep(10000);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Life Support System\n{0}", e);
            }
        }
    }
}

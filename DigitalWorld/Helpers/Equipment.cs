using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Digital_World.Entities;
using System.Collections;

namespace Digital_World.Helpers
{
    [Serializable]
    public class Equipment :IEnumerable<Item>
    {
        public enum Slot : int
        {
            Head = 0,
            Body,
            Hand,
            Legs,
            Feet,
            Accessory,
            Unknown1,
            Unknown2,
            Unknown3
        }

        protected List<Item> m_equip;
        
        /// <summary>
        /// Initialize new Equipment with empty slots.
        /// </summary>
        public Equipment()
        {
            m_equip = new List<Item>();
            for (int i = 0; i < 9; i++)
                m_equip.Add(new Item());
        }

        public Item this[int index]
        {
            get
            {
                index %= 9;
                return m_equip[index];
            }
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return m_equip.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (m_equip as IEnumerable).GetEnumerator();
        }
    }
}

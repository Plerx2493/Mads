using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModularAssistentForDiscordServer.Entities
{
    internal class MadsDiscordUser
    {
        private ulong ID;
        private Dictionary<ulong,int> NumberOfWarnings;
        private int NumberOfBans;
        private int NumberOfMutes;
        private int NumberOfKicks;
        private Dictionary<ulong, string> ModComments;

        public MadsDiscordUser(ulong id)
        {
            ID = id;
            NumberOfWarnings = new();
            NumberOfBans = 0;
            NumberOfMutes = 0;
            NumberOfKicks = 0;
            ModComments = new Dictionary<ulong, string>();
        }

        public ulong GetID()
        {
            return ID;
        }

        public int GetNumberOfWarnings(ulong id)
        {
            if (NumberOfWarnings.ContainsKey(id))
            {
                return NumberOfWarnings[id];
            }
            return 0;
        }

        public int GetNumberOfBans()
        {
            return NumberOfBans;
        }

        public int GetNumberOfMutes()
        {
            return NumberOfMutes;
        }

        public int GetNumberOfKicks()
        {
            return NumberOfKicks;
        }

        public Dictionary<ulong, string> GetModComments()
        {
            return ModComments;
        }

        public void AddModComment(ulong id, string comment)
        {
            if (ModComments.ContainsKey(id))
            {
                ModComments[id] += comment;
            }
            else
            {
                ModComments.Add(id, comment);
            }
        }

        public void OverrideModComment(ulong id, string comment)
        {
            if (ModComments.ContainsKey(id))
            {
                ModComments[id] = comment;
            }
            else
            {
                ModComments.Add(id, comment);
            }
        }
        public void RemoveModComment(ulong id)
        {
            ModComments.Remove(id);
        }

        public void AddWarnings(ulong id, int number)
        {
            if (NumberOfWarnings.ContainsKey(id))
            {
                NumberOfWarnings[id] += number;
            }
            else
            {
                NumberOfWarnings.Add(id, number);
            }
        }

        public void AddBans(int number)
        {
            NumberOfBans += number;
        }

        public void AddMutes(int number)
        {
            NumberOfMutes += number;
        }

        public void AddKicks(int number)
        {
            NumberOfKicks += number;
        }
        
        public void RemoveWarnings(ulong id, int number)
        {
            if (NumberOfWarnings.ContainsKey(id))
            {
                NumberOfWarnings[id] -= number;
                if (NumberOfWarnings[id] == 0) NumberOfWarnings.Remove(id);
            }
        }

        
    }
}

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
        private Dictionary<ulong, int> NumberOfWarnings;
        private Dictionary<ulong, int> NumberOfBans;
        private Dictionary<ulong, int> NumberOfMutes;
        private Dictionary<ulong, int> NumberOfKicks;
        private Dictionary<ulong, string> ModComments;

        public MadsDiscordUser(ulong id)
        {
            ID = id;
            NumberOfWarnings =  new();
            NumberOfBans     =  new();
            NumberOfMutes    =  new();
            NumberOfKicks    =  new();
            ModComments      =  new();
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

        public int GetNumberOfBans(ulong id)
        {
            if (NumberOfBans.ContainsKey(id)) 
            {
                return NumberOfBans[id];
            }
            return 0;
        }

        public int GetNumberOfMutes(ulong id)
        {
            if (NumberOfMutes.ContainsKey(id))
            {
                return NumberOfMutes[id];
            }
            return 0;
        }

        public int GetNumberOfKicks(ulong id)
        {
            if (NumberOfKicks.ContainsKey(id))
            {
                return NumberOfKicks[id];
            }
            return 0;
        }

        public string GetModComments(ulong id)
        {
            if (ModComments.ContainsKey(id))
            {
                return ModComments[id];
            }
            return "";
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

        public void AddBans(ulong id, int number)
        {
            if (NumberOfBans.ContainsKey(id))
            {
                NumberOfBans[id] += number;
            }
            else
            {
                NumberOfBans.Add(id, number);
            }
        }

        public void AddMutes(ulong id, int number)
        {
            if (NumberOfMutes.ContainsKey(id))
            {
                NumberOfMutes[id] += number;
            }
            else
            {
                NumberOfMutes.Add(id, number);
            }
        }

        public void AddKicks(ulong id, int number)
        {
            if (NumberOfKicks.ContainsKey(id))
            {
                NumberOfKicks[id] += number;
            }
            else
            {
                NumberOfKicks.Add(id, number);
            }
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

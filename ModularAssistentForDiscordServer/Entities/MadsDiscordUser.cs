namespace MADS.Entities
{
    internal class MadsDiscordUser
    {
        private readonly ulong _id;
        private Dictionary<ulong, int> _numberOfWarnings;
        private Dictionary<ulong, int> _numberOfBans;
        private Dictionary<ulong, int> _numberOfMutes;
        private Dictionary<ulong, int> _numberOfKicks;
        private Dictionary<ulong, string> _modComments;

        public MadsDiscordUser(ulong id)
        {
            _id = id;
            _numberOfWarnings = new Dictionary<ulong, int>();
            _numberOfBans = new Dictionary<ulong, int>();
            _numberOfMutes = new Dictionary<ulong, int>();
            _numberOfKicks = new Dictionary<ulong, int>();
            _modComments = new Dictionary<ulong, string>();
        }

        public ulong GetId()
        {
            return _id;
        }

        public int GetNumberOfWarnings(ulong id)
        {
            return _numberOfWarnings.ContainsKey(id) ? _numberOfWarnings[id] : 0;
        }

        public int GetNumberOfBans(ulong id)
        {
            return _numberOfBans.ContainsKey(id) ? _numberOfBans[id] : 0;
        }

        public int GetNumberOfMutes(ulong id)
        {
            return _numberOfMutes.ContainsKey(id) ? _numberOfMutes[id] : 0;
        }

        public int GetNumberOfKicks(ulong id)
        {
            return _numberOfKicks.ContainsKey(id) ? _numberOfKicks[id] : 0;
        }

        public string GetModComments(ulong id)
        {
            return _modComments.ContainsKey(id) ? _modComments[id] : "";
        }

        public void AddModComment(ulong id, string comment)
        {
            if (_modComments.ContainsKey(id))
            {
                _modComments[id] += comment;
            }
            else
            {
                _modComments.Add(id, comment);
            }
        }

        public void OverrideModComment(ulong id, string comment)
        {
            if (_modComments.ContainsKey(id))
            {
                _modComments[id] = comment;
            }
            else
            {
                _modComments.Add(id, comment);
            }
        }

        public void RemoveModComment(ulong id)
        {
            _modComments.Remove(id);
        }

        public void AddWarnings(ulong id, int number)
        {
            if (_numberOfWarnings.ContainsKey(id))
            {
                _numberOfWarnings[id] += number;
            }
            else
            {
                _numberOfWarnings.Add(id, number);
            }
        }

        public void AddBans(ulong id, int number)
        {
            if (_numberOfBans.ContainsKey(id))
            {
                _numberOfBans[id] += number;
            }
            else
            {
                _numberOfBans.Add(id, number);
            }
        }

        public void AddMutes(ulong id, int number)
        {
            if (_numberOfMutes.ContainsKey(id))
            {
                _numberOfMutes[id] += number;
            }
            else
            {
                _numberOfMutes.Add(id, number);
            }
        }

        public void AddKicks(ulong id, int number)
        {
            if (_numberOfKicks.ContainsKey(id))
            {
                _numberOfKicks[id] += number;
            }
            else
            {
                _numberOfKicks.Add(id, number);
            }
        }

        public void RemoveWarnings(ulong id, int number)
        {
            if (_numberOfWarnings.ContainsKey(id))
            {
                _numberOfWarnings[id] -= number;
                if (_numberOfWarnings[id] == 0) _numberOfWarnings.Remove(id);
            }
        }
    }
}
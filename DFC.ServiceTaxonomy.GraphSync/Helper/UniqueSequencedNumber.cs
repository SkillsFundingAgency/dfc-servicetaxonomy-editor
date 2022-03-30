using System.Collections.Generic;

namespace DFC.ServiceTaxonomy.GraphSync.Helper
{
    public static class UniqueSequencedNumber
    {
        private static int _Number;
        private static readonly Dictionary<string, int> _Dictionary = new Dictionary<string, int>();

        public static int GetNumber(string id)
        {
            if (_Dictionary.ContainsKey(id))
            {
                return _Dictionary[id];
            }

            _Dictionary.Add(id, _Number);
            return _Number++;
        }
    }
}

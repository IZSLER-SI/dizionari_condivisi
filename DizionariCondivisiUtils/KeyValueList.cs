using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace it.invisiblefarm.dizionaricondivisi.DizionariCondivisiUtils
{
    /// <summary>
    /// Incapsula una <code>List<KeyValuePair></code> implementando il metodo Add().
    /// Questo aggiunge la possibilità di inizializzare staticamente un oggetto con una sintassi comoda:
    /// <code>
    /// new KeyValueList<string, string> { { "key", "value" } }
    /// </code>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            foreach (KeyValuePair<TKey, TValue> kvp in this)
                if (kvp.Key.Equals(key))
                    return true;
            return false;
        }

        public KeyValuePair<TKey, TValue> Get(TKey key)
        {
            foreach (KeyValuePair<TKey, TValue> kvp in this)
                if (kvp.Key.Equals(key))
                    return kvp;
            throw new KeyNotFoundException("Key not found: " + key.ToString());
        }
    }
}

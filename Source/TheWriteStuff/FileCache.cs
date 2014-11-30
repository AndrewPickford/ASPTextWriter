using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class FileCache : MonoBehaviour
    {
        struct CacheEntry
        {
            public Byte[] data;
            public long lastAccess;
        }

        public static FileCache Instance { get; private set; }

        private Dictionary<string, CacheEntry> _cache;
        private long _totalBytes = 0;
        private long _maxBytes = 20 * 1024 * 1024;

        public byte[] getData(string fileName)
        {
            if (_cache.ContainsKey(fileName))
            {
                CacheEntry entry = _cache[fileName];
                if (Global.Debugging) Debug.Log(String.Format("TWS: Get {0} from cache", fileName));

                entry.lastAccess = DateTime.UtcNow.Ticks;
                return entry.data;
            }
            else
            {
                CacheEntry entry = new CacheEntry();

                entry.data = System.IO.File.ReadAllBytes(fileName);
                entry.lastAccess = DateTime.UtcNow.Ticks;

                addToCache(fileName, entry);
                if (Global.Debugging) Debug.Log(String.Format("TWS: Add {0} to file cache", fileName));

                return entry.data;
            }
        }

        private void addToCache(string fileName, CacheEntry entry)
        {
            _cache[fileName] = entry;
            calculateTotalSize();

            while (_totalBytes > _maxBytes)
            {
                deleteOldest();
            }
        }

        private void calculateTotalSize()
        {
            long _totalBytes = 0;
            foreach(KeyValuePair<string, CacheEntry> pair in _cache)
            {
                _totalBytes += pair.Value.data.Length;
            }
        }

        private void deleteOldest()
        {
            string entryName = string.Empty;
            long oldest = DateTime.UtcNow.Ticks;

            foreach (KeyValuePair<string, CacheEntry> pair in _cache)
            {
                if (pair.Value.lastAccess < oldest)
                {
                    oldest = pair.Value.lastAccess;
                    entryName = pair.Key;
                }
            }

            if (entryName != string.Empty)
            {
                _cache.Remove(entryName);
                calculateTotalSize();
            }
        }

        public void Awake()
        {
            if (Instance != null) return;

            Instance = this;
            DontDestroyOnLoad(this);

            _cache = new Dictionary<string, CacheEntry>();
        }
    }
}

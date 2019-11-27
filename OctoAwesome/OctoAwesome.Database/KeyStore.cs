﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OctoAwesome.Database
{
    internal class KeyStore<TTag> : IDisposable where TTag : ITag, new()
    {
        private readonly Dictionary<TTag, Key<TTag>> keys;
        private readonly Writer writer;
        private readonly Reader reader;

        public KeyStore(Writer writer, Reader reader)
        {
            keys = new Dictionary<TTag, Key<TTag>>();

            this.writer = writer;
            this.reader = reader;
        }

        public void Open()
        {
            writer.Open();
            var buffer = reader.Read(0, -1);

            for (int i = 0; i < buffer.Length; i += Key<TTag>.KEY_SIZE)
            {
                var key = Key<TTag>.FromBytes(buffer, i);
                keys.Add(key.Tag, key);
            }
        }

        internal Key<TTag> GetKey(TTag tag)
            => keys[tag];

        internal bool Contains(TTag tag)
        {
            return keys.ContainsKey(tag);
        }

        internal void Add(Key<TTag> key)
        {
            keys.Add(key.Tag, key);
            writer.ToEnd();
            writer.WriteAndFlush(key.GetBytes(), 0, Key<TTag>.KEY_SIZE);
        }

        internal void Remove(TTag tag, out Key<TTag> key)
        {
            key = keys[tag];
            keys.Remove(tag);
            writer.ToEnd();
            writer.WriteAndFlush(key.GetBytes(), 0, Key<TTag>.KEY_SIZE);
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
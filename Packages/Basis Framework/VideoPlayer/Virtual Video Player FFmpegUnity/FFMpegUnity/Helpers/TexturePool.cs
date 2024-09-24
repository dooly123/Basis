using System;
using System.Collections.Generic;
using UnityEngine;

namespace FFmpeg.Unity.Helpers
{
    [Serializable]
    public sealed class TexturePool : IDisposable
    {
        [Serializable]
        public class TexturePoolState
        {
            public bool inUse = false;
            public Texture2D texture = null;
            public double pts;
        }

        public List<TexturePoolState> pool = new List<TexturePoolState>();
        public int index = 0;

        public TexturePool(int size)
        {
            pool.Capacity = size;
            for (int i = 0; i < size; i++)
            {
                pool.Add(new TexturePoolState()
                {
                    texture = new Texture2D(16, 16, TextureFormat.RGB24, false),
                });
            }
        }

        public TexturePoolState Get()
        {
            for (int i = 0; i < pool.Count && pool[index % pool.Count].inUse; i++)
                index++;
            if (pool[index % pool.Count].inUse)
            {
                Debug.Log($"Adding to texture pool {pool.Count}");
                var n = new TexturePoolState()
                {
                    texture = new Texture2D(16, 16, TextureFormat.RGB24, false),
                };
                pool.Add(n);
                index = pool.Count - 1;
            }
            pool[index % pool.Count].inUse = true;
            return pool[index % pool.Count];
        }

        public void Release(TexturePoolState state)
        {
            if (state == null)
                return;
            state.inUse = false;
            UnityEngine.Object.DestroyImmediate(state.texture);
            state.texture = new Texture2D(16, 16, TextureFormat.RGB24, false);
        }

        public void Dispose()
        {
            foreach (var state in pool)
            {
                UnityEngine.Object.DestroyImmediate(state.texture);
            }
        }
    }
}

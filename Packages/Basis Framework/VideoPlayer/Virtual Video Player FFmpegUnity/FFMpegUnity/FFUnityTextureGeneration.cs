using UnityEngine;
namespace FFmpeg.Unity
{
    [System.Serializable]
    public class FFUnityTextureGeneration
    {
        public int PresumedInitalTextureSizeX = 1280;
        public int PresumedInitalTextureSizeY = 720;
        public Texture2D texture;
        public bool ActivelyRenderering = true;
        public FilterMode FilterMode = FilterMode.Bilinear;
        public TextureWrapMode TextureWrapMode = TextureWrapMode.Clamp;
        public TextureFormat TextureFormat = TextureFormat.RGB24;
        public bool hasMipChain = false;
        public int CachedlastWidth;
        public int CachedlastHeight;
        public void InitializeTexture()
        {
            InitializeTexture(PresumedInitalTextureSizeY, PresumedInitalTextureSizeX);
        }
        // Call this method to initialize the texture based on the width and height from the incoming FFTexData
        public void InitializeTexture(int width, int height)
        {
            texture = new Texture2D(width, height, TextureFormat, hasMipChain);
            texture.filterMode = FilterMode; // Adjust this based on your requirements (e.g., Point, Trilinear)
            texture.wrapMode = TextureWrapMode;  // Adjust this if needed
            CachedlastWidth = width;
            CachedlastHeight = height;
            // Optionally mark it as non-readable to save memory and improve performance.
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
        }

        // Efficiently updates the texture with new data every frame
        public void UpdateTexture(FFTexData texData)
        {
            if (ActivelyRenderering)
            {
                if (CachedlastWidth != texData.width || CachedlastHeight != texData.height)
                {
                    InitializeTexture(texData.width, texData.height);  // Reinitialize texture if dimensions changed
                }

                // Load raw byte data (assumes RGB24 format)
                texture.LoadRawTextureData(texData.data);
                // Apply the texture updates to GPU (false to not generate mipmaps for performance)
                texture.Apply(updateMipmaps: false);
            }
        }
    }
}
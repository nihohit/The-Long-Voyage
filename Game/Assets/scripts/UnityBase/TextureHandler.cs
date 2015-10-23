using Assets.Scripts.Base;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UnityBase
{
    #region ITextureHandler

    public interface ITextureHandler<T>
    {
        void UpdateMarkerTexture(T item, SpriteRenderer renderer);

        Texture2D GetTexture(T ent);

        Texture2D GetNullTexture();
    }

    #endregion ITextureHandler

    #region TextureHandler

    /// <summary>
    /// Abstract class for texture replacement and changes in textures
    /// </summary>
    public abstract class TextureHandler
    {
        protected Dictionary<string, Texture2D> GetDictionary(string folderName)
        {
            var textures = Resources.LoadAll<Texture2D>(folderName);
            return textures.ToDictionary(
                texture => texture.name,
                texture => texture);
        }

        protected void UpdateTexture(string itemName, SpriteRenderer renderer, IDictionary<string, Texture2D> dictionary, string dictionaryName)
        {
            var newTexture = dictionary.Get(itemName, dictionaryName);
            ReplaceTexture(renderer, newTexture);
        }

        // the default switching color is white
        protected Texture2D GetColoredTexture(Texture2D copiedTexture, Color replacementColor, string textureName)
        {
            return GetColoredTexture(copiedTexture, Color.white, replacementColor, textureName);
        }

        // CopiedTexture is the original Texture  which you want to copy.
        protected Texture2D GetColoredTexture(Texture2D copiedTexture, Color originalColor, Color replacementColor, string textureName)
        {
            // Create a new Texture2D, which will be the copy.
            var texture = new Texture2D(copiedTexture.width, copiedTexture.height)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

            // Choose your filtermode and wrapmode here.
            int y = 0;
            while (y < texture.height)
            {
                int x = 0;

                while (x < texture.width)
                {
                    texture.SetPixel(
                        x,
                        y,
                        copiedTexture.GetPixel(x, y) == originalColor ? replacementColor : copiedTexture.GetPixel(x, y));

                    ++x;
                }
                ++y;
            }

            // Name the texture, if you want.
            texture.name = textureName;

            // This finalizes it. If you want to edit it still, do it before you finish with .Apply(). Do NOT expect to edit the image after you have applied. It did NOT work for me to edit it after this function.
            texture.Apply();

            // Return the variable, so you have it to assign to a permanent variable and so you can use it.
            return texture;
        }

        public static void ReplaceTexture(SpriteRenderer renderer, Texture2D newTexture)
        {
            Assert.NotNull(renderer, "renderer");
            Assert.NotNull(renderer.sprite, "sprite");
            renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f, 0.5f));
            renderer.sprite.name = newTexture.name;
        }

        public Texture2D MergeTextures(Texture2D bottom, Texture2D top, string textureName)
        {
            // Create a new Texture2D, which will be the copy.
            var texture = new Texture2D(bottom.width, bottom.height)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };

            // Choose your filtermode and wrapmode here.
            int y = 0;
            while (y < texture.height)
            {
                int x = 0;

                while (x < texture.width)
                {
                    texture.SetPixel(
                        x,
                        y,
                        top.GetPixel(x, y) == Color.clear ? bottom.GetPixel(x, y) : top.GetPixel(x, y));

                    ++x;
                }
                ++y;
            }

            // Name the texture, if you want.
            texture.name = textureName;

            // This finalizes it. If you want to edit it still, do it before you finish with .Apply(). Do NOT expect to edit the image after you have applied. It did NOT work for me to edit it after this function.
            texture.Apply();

            // Return the variable, so you have it to assign to a permanent variable and so you can use it.
            return texture;
        }
    }

    #endregion TextureHandler
}
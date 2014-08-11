using UnityEngine;

namespace Assets.scripts.UnityBase
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
        //the default switching color is white
        protected Texture2D GetColoredTexture(Texture2D copiedTexture, Color replacementColor, string textureName)
        {
            return GetColoredTexture(copiedTexture, Color.white, replacementColor, textureName);
        }

        //CopiedTexture is the original Texture  which you want to copy.
        protected Texture2D GetColoredTexture(Texture2D copiedTexture, Color originalColor, Color replacementColor, string textureName)
        {
            //Create a new Texture2D, which will be the copy.
            Texture2D texture = new Texture2D(copiedTexture.width, copiedTexture.height);

            //Choose your filtermode and wrapmode here.
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int y = 0;
            while (y < texture.height)
            {
                int x = 0;

                while (x < texture.width)
                {
                    if (copiedTexture.GetPixel(x, y) == originalColor)
                    {
                        texture.SetPixel(x, y, replacementColor);
                    }
                    else
                    {
                        //This line of code is REQUIRED. Do NOT delete it. This is what copies the image as it was, without any change.
                        texture.SetPixel(x, y, copiedTexture.GetPixel(x, y));
                    }

                    ++x;
                }
                ++y;
            }

            //Name the texture, if you want.
            texture.name = (textureName);

            //This finalizes it. If you want to edit it still, do it before you finish with .Apply(). Do NOT expect to edit the image after you have applied. It did NOT work for me to edit it after this function.
            texture.Apply();

            //Return the variable, so you have it to assign to a permanent variable and so you can use it.
            return texture;
        }

        public static void ReplaceTexture(SpriteRenderer renderer, Texture2D newTexture, string name)
        {
            renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f, 0.5f));
            renderer.sprite.name = name;
        }

        public Texture2D MergeTextures(Texture2D bottom, Texture2D top, string textureName)
        {
            //Create a new Texture2D, which will be the copy.
            Texture2D texture = new Texture2D(bottom.width, bottom.height);

            //Choose your filtermode and wrapmode here.
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int y = 0;
            while (y < texture.height)
            {
                int x = 0;

                while (x < texture.width)
                {
                    if (top.GetPixel(x, y) == Color.clear)
                    {
                        texture.SetPixel(x, y, bottom.GetPixel(x, y));
                    }
                    else
                    {
                        texture.SetPixel(x, y, top.GetPixel(x, y));
                    }

                    ++x;
                }
                ++y;
            }

            //Name the texture, if you want.
            texture.name = (textureName);

            //This finalizes it. If you want to edit it still, do it before you finish with .Apply(). Do NOT expect to edit the image after you have applied. It did NOT work for me to edit it after this function.
            texture.Apply();

            //Return the variable, so you have it to assign to a permanent variable and so you can use it.
            return texture;
        }
    }

    #endregion TextureHandler
}
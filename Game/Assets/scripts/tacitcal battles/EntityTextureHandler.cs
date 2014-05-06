using System.Collections.Generic;
using UnityEngine;

public class EntityTextureHandler : TextureHandler
{
    private Dictionary<string, Texture2D> m_knownTextures = new Dictionary<string, Texture2D>();
    private Dictionary<Loyalty, Color> m_affiliationColors = new Dictionary<Loyalty, Color>
    {
        {Loyalty.Bandits, Color.red},
        {Loyalty.EnemyArmy, Color.black},
        {Loyalty.Friendly, Color.yellow},
        {Loyalty.Player, Color.blue},
    }; //inactive or monster units should have unique visuals.

    public EntityTextureHandler()
    { }

    public void UpdateEntityTexture(Entity ent)
    {
        var name = "{0}_{1}".FormatWith(ent.Loyalty, ent.GetType().ToString());
        var renderer = ent.Marker.GetComponent<SpriteRenderer>();
        var newTexture = m_knownTextures.TryGetOrAdd(name, () => GetTexture(ent, name, renderer));
        renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f,0.5f));

        renderer.sprite.name = name;
    }

    private Texture2D GetTexture(Entity ent, string name, SpriteRenderer renderer)
    {
        var oldTexture = ent.Marker.GetComponent<SpriteRenderer>().sprite.texture;
        Color replacementColor;

        //if the color isn't in the list of affiliation, we just return
        if(!m_affiliationColors.TryGetValue(ent.Loyalty, out replacementColor))
        {
            return oldTexture;
        }
        return CopyTexture2D(oldTexture, replacementColor, name);
    }
}

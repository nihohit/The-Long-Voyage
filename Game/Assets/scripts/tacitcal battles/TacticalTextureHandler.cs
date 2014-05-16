using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TacticalTextureHandler : TextureHandler
{
    private Dictionary<string, Texture2D> m_knownEntityTextures = new Dictionary<string, Texture2D>();
    private Dictionary<string, Texture2D> m_knownEffectsTextures;

    private Dictionary<Loyalty, Color> m_affiliationColors = new Dictionary<Loyalty, Color>
    {
        {Loyalty.Bandits, Color.red},
        {Loyalty.EnemyArmy, Color.black},
        {Loyalty.Friendly, Color.yellow},
        {Loyalty.Player, Color.blue},
    }; //inactive or monster units should have unique visuals.

    public TacticalTextureHandler()
    { 
        var textures = Resources.LoadAll<Texture2D>("effects");
        m_knownEffectsTextures = textures.ToDictionary(texture => texture.name,
                                                       texture => texture);
    }

    public void UpdateEntityTexture(Entity ent)
    {
        var name = "{0}_{1}".FormatWith(ent.Loyalty, ent.GetType().ToString());
        var renderer = ent.Marker.GetComponent<SpriteRenderer>();
        var newTexture = m_knownEntityTextures.TryGetOrAdd(name, () => GetEntityTexture(ent, name, renderer));
        renderer.sprite = Sprite.Create(newTexture, renderer.sprite.rect, new Vector2(0.5f,0.5f));

        renderer.sprite.name = name;
    }

    public Texture2D GetEffectTexture(string effectName)
    {
        return m_knownEffectsTextures[effectName];
    }

    private Texture2D GetEntityTexture(Entity ent, string name, SpriteRenderer renderer)
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

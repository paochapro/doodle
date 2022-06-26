using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Doodle;

public class Group<T> where T : Entity
{
    static protected List<T> group = new List<T>();
    static protected int lastGroupID = 0;

    static public int Count => group.Count;

    static public T Get(int i) => group[i]; 

    //General
    static public void Update(GameTime gameTime)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Update(gameTime);
    }
    static public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < group.Count; ++i)
            group[i].Draw(spriteBatch);
    }
    static public void Add(T ent)
    {
        ent.groupID = lastGroupID++;
        group.Add(ent);
        Entities.Add(ent);
    }

    protected Group() {}

    //Destroy
    static public void Destroy(int id)
    {
        if (group.Count == 0) return;
        Entities.Destroy(group[id].entityID);

        group.RemoveAt(id);
        lastGroupID--;

        //UpdateIDs
        foreach (T ent in group)
        {
            if (ent.groupID > id)
                ent.groupID--;
        }
    }
    static public void Destroy(Entity entity)
    {
        Destroy(entity.groupID);
    }
    //Clear
    static public void Clear()
    {
        group.ForEach(e => Entities.Destroy(e));
        group.Clear();
        lastGroupID = 0;
    }

/*    static public T? Collides(Rectangle rectangle, bool abort = false)
    {
        T? ent = null;

        for (int i = 0; i < group.Count; ++i)
        {
            if (rectangle.Intersects(rectangle))
            {
                ent = group[i];
                ent.OnTouch();
                if (abort) break;
            }
        }

        return ent;
    }*/
}
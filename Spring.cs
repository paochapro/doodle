using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

class Springs : Group<Spring>
{
    public const int springChance = 5;

    static public void SpawnSpring(int x, int y)
    {
        Vector2 pos = new(
            Random(x, x + Platform.width - Spring.size.X),
            y - Spring.size.Y
        );

        Add(new Spring(pos));
    }
}

class Spring : Entity
{
    static readonly Texture2D sTexture = MonoGame.LoadTexture("spring");
    public static readonly Point size = new(24, 24);

    public Spring(Vector2 pos)
        : base(new RectangleF(pos, size), sTexture)
    {
    }
    public override void Update(GameTime gameTime) => DestroyOOB();
    public override void Destroy() => Springs.Destroy(groupID);
}
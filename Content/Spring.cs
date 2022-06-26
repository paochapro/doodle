using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

class Springs : Group<Spring>
{
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
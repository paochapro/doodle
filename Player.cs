using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

internal class Player : Entity
{
    //Stats
    public const float jumpHeight = 1300;
    const float MAX_GRAVITY = 2500;
    const float MAX_ACCELARATION = 8000;
    const float drag = 0.87f;

    const float springLaunchHeight = jumpHeight * 1.6f;

    //Delta applied
    const float gravity = 2500;
    const float accelaration = 6000;

    //Static
    public static readonly Point size = new(36,48);
    static readonly Texture2D idle = MonoGame.LoadTexture("player_idle");
    readonly Vector2 defaultPosition = new(center(0, MyGame.screenSize.X, size.X), 0);

    //Base
    float dt = 0;
    Vector2 oldPosition;
    Vector2 velocity;

    int direction;
    const int defaultDirection = 1;

    //Bonus
    double bonusWastedTime;
    public bool bonusActivated { get; private set; }
    Bonus.BonusData bonus;

    //Debug
    bool update = true;
    bool pressingQ;

    public Player() : base( new RectangleF(0,0,size.X, size.Y), idle )
    {
        Death(false);
    }

    public override void Update(GameTime gameTime)
    {
        //Freeze
        if (MyGame.keys.IsKeyDown(Keys.Q) && !pressingQ)
        {
            velocity = Vector2.Zero;
            update = !update;
        }
        pressingQ = MyGame.keys.IsKeyDown(Keys.Q);

        if (!update) return;

        dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Movement();
        rectangle.Position += velocity * dt;

        //Left and right portal
        if (rectangle.X + size.X <= 0)
            rectangle.X = MyGame.screenSize.X;

        if (rectangle.X > MyGame.screenSize.X)
            rectangle.X = 0 - size.X;

        if(bonusActivated)
        {
            if(bonusWastedTime > Bonus.bonusTime)
            {
                bonusActivated = false;
                bonusWastedTime = 0;
                MyGame.bonusActivated = false;
            }
            velocity.Y = -bonus.speed;
            bonusWastedTime += gameTime.ElapsedGameTime.TotalSeconds;

            return;
        }

        //Death
        if (rectangle.Y + size.Y >= MyGame.DeathPit)
        {
            if(MyGame.God)
                Jump();
            else
            {
                SoundEffect fall = MonoGame.Load<SoundEffect>("fall");
                fall.Play();

                Death();
            }
        }

        //Collision
        PlatformCollision();
        OtherCollision();

        oldPosition = rectangle.Position;
    }

    private void Jump(bool spring = false)
    {
        SoundEffectInstance sndJump = spring ? MonoGame.Load<SoundEffect>("snd_spring").CreateInstance() :
                                               MonoGame.Load<SoundEffect>("jump").CreateInstance();

        sndJump.Pitch = RandomFloat(-0.2f, 0.0f);
        sndJump.Play();

        velocity.Y = spring ? -springLaunchHeight : -jumpHeight;
    }

    private void PlatformCollision()
    {
        for (int i = 0; i < Platforms.Count; ++i)
        {
            Platform platform = Platforms.Get(i);

            if (platform.PlatformType == Platforms.PlatformType.Spinning)
            {
                if (velocity.Y < 0) 
                    continue;
            }
            else
            {
                //Semi-solid
                if (oldPosition.Y + rectangle.Height > platform.Rect.Y)
                    continue;
            }

            if (rectangle.Intersects(platform.Rect))
            {
                //What platform should do on interaction with player
                platform.OnTouch();

                //If its a trap, dont move the player
                if (platform.PlatformType == Platforms.PlatformType.Trap)
                    continue;

                //Rounding y coordinate
                rectangle.Y = (float)Math.Round(rectangle.Y);

                //Moving player up until he doesn't touch the platform
                while (rectangle.Intersects(platform.Rect))
                    rectangle.Y -= 1;

                Jump();
            }
        }
    }

    private void Movement()
    {
        KeyboardState keys = MyGame.keys;
        int moveX = Convert.ToInt32(keys.IsKeyDown(Keys.Right)) - Convert.ToInt32(keys.IsKeyDown(Keys.Left));

        velocity.X += moveX * accelaration * dt;
        velocity.X = velocity.X * drag;

        if (Math.Abs(velocity.X) < 1)
            velocity.X = 0;
        
        direction = velocity.X != 0 ? Math.Sign(velocity.X) : direction;

        velocity.Y += gravity * dt;
        velocity.Y = clamp(velocity.Y, -MAX_GRAVITY, MAX_GRAVITY);
        velocity.X = clamp(velocity.X, -MAX_ACCELARATION, MAX_ACCELARATION);

        if (keys.IsKeyDown(Keys.I))
        {
            Jump();
        }
    }

    private void Death(bool resetGame = true)
    {
        if (resetGame)
            MyGame.Death();

        texture = idle;
        velocity = Vector2.Zero;
        rectangle.Position = defaultPosition;
        oldPosition = rectangle.Position;
        bonusWastedTime = 0;
        direction = defaultDirection;
        bonusRect = Rectangle.Empty;
        bonusOffset = Point.Zero;
    }

    private void ActivateBonus(Bonus.BonusData bonus)
    {
        Enemies.OnBonus();
        MyGame.bonusActivated = true;

        bonusRect = bonus.playerTexture.Bounds;
        bonusOffset = bonus.offset.ToPoint();

        bonusActivated = true;
        this.bonus = bonus;
        bonus.sound.CreateInstance().Play();
    }
    private void OtherCollision()
    {
        //Bonuses
        for(int i = 0; i < Bonuses.Count; ++i)
        {
            Bonus bonus = Bonuses.Get(i);

            if(rectangle.Intersects(bonus.Rect))
            {
                ActivateBonus( Bonus.bonusGeneralData[bonus.Type] );
                bonus.OnTouch();
            }
        }

        //Springs
        for (int i = 0; i < Springs.Count; ++i)
        {
            //Semi-solid
            Spring spring = Springs.Get(i);

            if (oldPosition.Y + rectangle.Height > spring.Rect.Y)
                continue;

            if (rectangle.Intersects(spring.Rect))
            {
                Jump(true);
                spring.OnTouch();
            }
        }

        if (MyGame.God) return;

        //Enemies
        for (int i = 0; i < Enemies.Count; ++i)
        {
            Enemy enemy = Enemies.Get(i);

            if (rectangle.Intersects(enemy.Rect))
            {
                enemy.OnTouch();
                Death();
            }
        }
    }

    Rectangle bonusRect = Rectangle.Empty;
    Point bonusOffset = Point.Zero;

    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;
        final.Location -= MyGame.Camera.ToPoint();

        SpriteEffects flip = (direction == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var drawPlayer = () => spriteBatch.Draw(texture, final, null, Color.White, 0f, new(0, 0), flip, 0f);

        if (bonusActivated)
        {
            bonusRect.Location = final.Location;

            if (direction == -1)
                bonusRect.X = final.Right - bonusRect.Width;

            bonusRect.Location += new Point(bonusOffset.X * direction, bonusOffset.Y);

            var drawBonus = () => spriteBatch.Draw(bonus.playerTexture, bonusRect, Color.White);

            if (bonus.behind) {
                drawBonus();
                drawPlayer();
            }
            else {
                drawPlayer();
                drawBonus();
            }

            return;
        }

        drawPlayer();
    }
}
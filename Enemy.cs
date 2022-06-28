using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Doodle;

using static Doodle.Utils;

class Enemies : Group<Enemy>
{
    public static int enemyChance { get; private set; }
    const int enemyDefaultChance = 0;
    const int enemyStartChance = 5;

    public const int trapChance = 30;

    public const int minPossibleDistance = Enemy.height + 15;

    static readonly Dictionary<Enemy.EnemyType, Type> enemyTypes = new()
    {
        [Enemy.EnemyType.Moving] = typeof(MovingEnemy),
        [Enemy.EnemyType.Static] = typeof(StaticEnemy),
        [Enemy.EnemyType.Blackhole] = typeof(Blackhole),
    };

    static public void OnBonus()
    {
        for(int i =0; i < Count; ++i)
        {
            Enemy enemy = Get(i);

            if (enemy.Rect.Y + enemy.Rect.Height < MyGame.Camera.Y)
                Destroy(enemy);
        }
    }

    static public void SpawnEnemy(int previousY, int currentY, int x)
    {
        bool isLeft = Chance(50);

        int leftStart = 0;
        int leftEnd = x - Enemy.width;
        int rightStart = x + Platform.width;
        int rightEnd = MyGame.screenSize.X - Enemy.width;

        if (Enemy.size.X > leftEnd  - leftStart)  isLeft = false;
        if (Enemy.size.X > rightEnd - rightStart) isLeft = true;

        int enemyX = isLeft ? Random(leftStart, leftEnd) : Random(rightStart, rightEnd);
        int enemyY = Random(max: previousY - minPossibleDistance, min: currentY + minPossibleDistance);

        Enemy.EnemyType enemyType = (Enemy.EnemyType)Random(0, (int)Enemy.EnemyType.MaxTypes);
        Add((Enemy)Activator.CreateInstance(enemyTypes[enemyType], new Vector2(enemyX, enemyY))!);
    }

    static public void DifficultyChange(float diff)
    {
        if(diff > 3)
            enemyChance = enemyStartChance;
    }

    static public void Reset()
    {
        Clear();
        enemyChance = enemyDefaultChance;
    }
}

abstract class Enemy : Entity
{
    public enum EnemyType
    {
        Moving,
        Static,
        Blackhole,
        MaxTypes
    }

    static public readonly Point size = new(width, height);
    public const int width = 64;
    public const int height = 64;

    public Enemy(Vector2 pos, Texture2D texture) 
        : base( new RectangleF(pos,size), texture) 
    {
    }

    public override void Update(GameTime gameTime) => DestroyOOB();
    public override void Destroy() => Enemies.Destroy(groupID);
}

class MovingEnemy : Enemy
{
    static readonly Texture2D sTexture = MonoGame.LoadTexture("enemy_moving");

    protected float leftLimit = 0 + size.X / 2;
    protected float rightLimit = MyGame.screenSize.X - size.X - size.X / 2;

    protected int speed = 300;
    protected int movement;

    public MovingEnemy(Vector2 pos)
        : base(pos, sTexture)
    {
        movement = speed;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (rectangle.X > rightLimit) movement = -speed;
        if (rectangle.X < leftLimit) movement = +speed;

        rectangle.X += movement * dt;

        base.Update(gameTime);
    }
}

class StaticEnemy : MovingEnemy
{
    static readonly Texture2D sTexture1 = MonoGame.LoadTexture("enemy_static1");
    static readonly Texture2D sTexture2 = MonoGame.LoadTexture("enemy_static2");

    const int shake = 5;

    public StaticEnemy(Vector2 pos)
        : base(pos)
    {
        texture = Chance(50) ? sTexture1 : sTexture2;
        leftLimit = pos.X - shake;
        rightLimit = pos.X + shake;
        speed = 150;
    }
}
class Blackhole : Enemy
{
    static readonly Texture2D sTexture = MonoGame.LoadTexture("enemy_blackhole");

    public Blackhole(Vector2 pos)
        : base(pos, sTexture)
    {}
}
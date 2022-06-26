using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Doodle;

using static Doodle.Utils;

class Enemies : Group<Enemy>
{
    const int trapChance = 30;

    static int enemyChance;
    const int enemyDefaultChance = 5;

    static bool failedEnemySpawn = false;

    static readonly Dictionary<Enemy.EnemyType, Type> enemyTypes = new()
    {
        [Enemy.EnemyType.Moving] = typeof(MovingEnemy),
        [Enemy.EnemyType.Static] = typeof(StaticEnemy),
        [Enemy.EnemyType.Blackhole] = typeof(Blackhole),
    };

    static bool bonusActivated;

    static public void OnBonus()
    {
        bonusActivated = true;

        for(int i =0; i < Count; ++i)
        {
            Enemy enemy = Get(i);

            if (enemy.Rect.Y + enemy.Rect.Height < MyGame.Camera.Y)
                Destroy(enemy);
        }
    }

    static public void BonusEnd() => bonusActivated = false;

    static public void SpawnObstacle(int platformY, int nextPlatformY)
    {
        //0-nothing, 1-trap platform, 2-enemy
        int chance = Chance(100 - trapChance - enemyChance, trapChance, enemyChance);

        //If there was not enough distance between platforms for enemy to spawn
        //Try to spawn it again next time until its done
        if (failedEnemySpawn)
        {
            chance = 2;
            failedEnemySpawn = false;
        }

        //If nothing, dont spawn anything
        if (chance == 0)  return;

        //If bonus is activated, dont spawn the enemies
        if (bonusActivated) 
            chance = 1;

        //Not enough space?
        int obstacleHeight = chance == 1 ? Platform.height : Enemy.size.Y;
        int distanceBetween = platformY - nextPlatformY - Platform.height;

        if (distanceBetween < Platforms.minPossibleDistance + obstacleHeight)
        {
            if (chance == 2)
            {
                failedEnemySpawn = true;
                print("failed enemy spawn!");
            }

            return;
        }

        Vector2 pos = new(
            Random(0, MyGame.screenSize.X - Platform.width),//Good
            Random(max: platformY - Platforms.minPossibleDistance, min: nextPlatformY + Platforms.minPossibleDistance) //70%
        );

        if (chance == 2) //Enemy
        {
            print("spawned enemy");

            Enemy.EnemyType enemyType = (Enemy.EnemyType)Random(0, (int)Enemy.EnemyType.MaxTypes);
            Add( (Enemy)Activator.CreateInstance(enemyTypes[enemyType], pos)! );
        }

        if (chance == 1) //Trap platform
        {
            Platforms.AddTrap(pos);
        }
    }

    static public void DifficultyChange(float diff)
    {
        if(diff > 3) 
            enemyChance = enemyDefaultChance;
    }

    static public void Reset()
    {
        Clear();
        enemyChance = 0;
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

    static public readonly Point size = new(64, 64);

    public Enemy(Vector2 pos, Texture2D texture) 
        : base( new RectangleF(pos,size), texture) 
    { 
    }

    public override void Update(GameTime gameTime)
    {
        DestroyOOB();
    }

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
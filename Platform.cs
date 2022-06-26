using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

/*class Platforms : Group<Platform>
{
    enum PlatformType
    {
        Simple,
        Movable,
        Breakable,
        MaxTypes
    }
    static Dictionary<PlatformType, int> platformChances = new Dictionary<PlatformType, int>()
    {
        [PlatformType.Simple] = 0,
        [PlatformType.Movable] = 0,
        [PlatformType.Breakable] = 0,
    };

    //Default platform chances
    const int simpleDefaultChance = 90;
    const int movableDefaultChance = 0;
    const int breakableDefaultChance = 10;

    //Start platform
    static readonly int startX = center(MyGame.screenSize.X, Platform.width);
    static readonly int startY = 128;

    const int maxTypes = (int)PlatformType.MaxTypes;

    //Next platform
    static Point nextPlatform;
    static PlatformType randomType;
    const int minPossibleDistance = 15 + Platform.height;
    const int maxPossibleDistance = 330 - Platform.height;

    //Distance chacnces
    const int defaultSmallChance = 95;
    const int defaultMedChance = 5;
    const int defaultLargeChance = 0;

    static int smallDistChance = 0;
    static int medDistChance = 0;
    static int largeDistChance = 0;

    //Trap and bonus chances
    static int trapChance;


    const int trapDefaultChance = 30;
    const int bonusDefaultChance = 1;

    static readonly Dictionary<PlatformType, Type> platformTypes = new Dictionary<PlatformType, Type>()
    {
        [PlatformType.Simple] = typeof(SimplePlatform),
        [PlatformType.Movable] = typeof(MovablePlatform),
        [PlatformType.Breakable] = typeof(BreakablePlatform),
    };
    static private void Add(PlatformType platformType, Vector2 pos)
    {
        Add( (Platform)Activator.CreateInstance(platformTypes[platformType], pos)! );
    }
    static private void SpawnObstacle(int platformY, int nextPlatformY)
    {
        int trapHeight = 45;

        //Not enough space
        int distanceBetween = Math.Abs(nextPlatformY) - Math.Abs(platformY) - Platform.height;
        if(distanceBetween < minPossibleDistance + trapHeight)
            return;

        //Obstacle random position
        int obstacleX = Random(0, MyGame.screenSize.X - Platform.width); //Good
        int obstacleY = Random(max: platformY - minPossibleDistance, min: nextPlatformY + minPossibleDistance); //70%
        
        //Trap platform
        if (Chance(trapChance))
        {
            Add(new TrapPlatform(new Vector2(obstacleX, obstacleY)));
        }
    }
    static public int Generate()
    {
        //Platform position
        int x = nextPlatform.X;
        int y = nextPlatform.Y;

        //Adding random platform
        Add(randomType, new Vector2(x,y));

        //Next platform random distance
        int chance = Chance(smallDistChance, medDistChance, largeDistChance) + 1;
        const int portion = (maxPossibleDistance - minPossibleDistance) / 3;

        int maxDistance = portion * chance + minPossibleDistance;
        int minDistance = maxDistance - portion;

        //Next platform position
        nextPlatform.Y = Random(max: y - minDistance, min: y - maxDistance);
        nextPlatform.X = Random(0, MyGame.screenSize.X - Platform.width);

        //Getting random platform type
        int platformTypeIndex = Chance(platformChances.Values.ToArray());   //Getting chance on some platform
        randomType = platformChances.Keys.ElementAt(platformTypeIndex);     //Getting platform type at index in platformChances

        //Adding obstacles
        SpawnObstacle(y, nextPlatform.Y); //Good

        //Adding bonuses
        Bonuses.SpawnBonus(x,y);

        //Debug
        Vector2 p1 = new Vector2(x,y);
        Vector2 p2 = new Vector2(x,nextPlatform.Y);
        DebugLines.Add(new DebugLine(p1, p2));

        //Returning distance beetween next and this platform
        int distance = y - nextPlatform.Y; //Math.Abs(nextPlatform.Y) - Math.Abs(y);
        return -distance;
    }
    static public void Reset()
    {
        Clear();
        Bonuses.Reset();
        nextPlatform        = new(startX, startY);
        randomType          = PlatformType.Simple;
        trapChance          = trapDefaultChance;
        smallDistChance     = defaultSmallChance;
        medDistChance       = defaultMedChance;
        largeDistChance     = defaultLargeChance;

        platformChances[PlatformType.Simple]    = simpleDefaultChance;
        platformChances[PlatformType.Breakable] = breakableDefaultChance;
        platformChances[PlatformType.Movable]   = movableDefaultChance;
    }
    static public void DifficultyChange()
    {
        //Distance changes
        const int smallDistDecrease = 15;
        const int medDistIncrease = 5;
        const int largeDistIncrease = 10;

        if ((medDistIncrease + largeDistIncrease - smallDistDecrease) != 0)
            throw new Exception("distance chance changes not equals zero in Platforms:DifficultyChange");

        smallDistChance -= smallDistDecrease;
        medDistChance += medDistIncrease;
        largeDistChance += largeDistIncrease;

        //Platform types changes
        const int simpleDecrease = 10;
        const int breakableIncrease = 5;
        const int movableIncrease = 5;

        if ( (breakableIncrease + movableIncrease - simpleDecrease) != 0)
            throw new Exception("platform chance changes not equals zero in Platforms:DifficultyChange");

        platformChances[PlatformType.Simple] -= simpleDecrease;
        platformChances[PlatformType.Breakable] += breakableIncrease;
        platformChances[PlatformType.Movable] += movableIncrease;

        print("Chances:" + " S-" + platformChances[PlatformType.Simple] + " B-" + platformChances[PlatformType.Breakable] + " M-" + platformChances[PlatformType.Movable]);
    }
}*/

class Platforms : Group<Platform>
{
    public enum PlatformType
    {
        Simple,
        Movable,
        Spinning,
        Breakable,
        Trap,
        MaxTypes
    }
    public static readonly Dictionary<PlatformType, Type> platformTypes = new Dictionary<PlatformType, Type>()
    {
        [PlatformType.Simple] = typeof(SimplePlatform),
        [PlatformType.Movable] = typeof(MovablePlatform),
        [PlatformType.Breakable] = typeof(BreakablePlatform),
        [PlatformType.Spinning] = typeof(SpinningPlatform),
        [PlatformType.Trap] = typeof(TrapPlatform),
    };
    static Dictionary<PlatformType, int> platformChances = new Dictionary<PlatformType, int>()
    {
        [PlatformType.Simple] = 0,
        [PlatformType.Movable] = 0,
        [PlatformType.Breakable] = 0,
        [PlatformType.Spinning] = 0,
    };

    //Default platform chances
    const int simpleDefaultChance = 100-breakableDefaultChance-spinningDefaultChance-movableDefaultChance;
    const int movableDefaultChance = 0;
    const int breakableDefaultChance = 15;
    const int spinningDefaultChance = 0;

    //Start platform
    static readonly Vector2 startPlatform = new(center(MyGame.screenSize.X, Platform.width), 128);

    //Distance chacnces
    const int defaultSmallChance = 95;
    const int defaultMedChance = 5;
    const int defaultLargeChance = 0;

    static int smallDistChance = 0;
    static int medDistChance = 0;
    static int largeDistChance = 0;

    static private void Add(PlatformType platformType, Vector2 pos)
    {
        Add((Platform)Activator.CreateInstance(platformTypes[platformType], pos)!);
    }
    static public void AddTrap(int previousY, int currentY)
    {
        Vector2 pos = new(
            Random(0, MyGame.screenSize.X - Platform.width),//Good
            Random(max: previousY - Platforms.minPossibleDistance, min: currentY + Platforms.minPossibleDistance) //70%
        );

        Add(new TrapPlatform(pos));
    }

    //Next platform
    static Point previousPlatform;
    static PlatformType platformType;
    public const int minPossibleDistance = 15 + Platform.height;
    public const int maxPossibleDistance = 330 - Platform.height;

    static bool failedBonusSpawn = false;
    public static bool bonusActivated = true;

    static public int Generate()
    {
        //Random distance
        int chance = Chance(smallDistChance, medDistChance, largeDistChance) + 1;
        const int portion = (maxPossibleDistance - minPossibleDistance) / 3;

        int maxDistance = (portion * chance + minPossibleDistance);
        int minDistance = (maxDistance - portion);

        //Position
        int x = Random(0, MyGame.screenSize.X - Platform.width);
        int y = Random(max: previousPlatform.Y - minDistance, min: previousPlatform.Y - maxDistance);

        //Getting random platform type
        int platformTypeIndex = Chance(platformChances.Values.ToArray());   //Getting chance on some platform

        if (platformTypeIndex == -1)
        {
            foreach (var platformChance in platformChances)
                print(platformChance);

            throw new Exception("platform type index chance -1 in Generate");
        }

        if(platformType == PlatformType.Spinning)
            if (platformChances.Keys.ElementAt(platformTypeIndex) == PlatformType.Spinning)
                platformTypeIndex = platformChances[PlatformType.Simple];

        platformType = platformChances.Keys.ElementAt(platformTypeIndex);   //Getting platform type at index in platformChances

        Add(platformType, new Vector2(x, y));

        //Distance beetween previous and this platform
        int distance = previousPlatform.Y - y;


        //Bonuses and springs
        SpawnGroundItem(x, y);

        //Enemies
        SpawnObstacle(previousPlatform.Y, y);

        //Debug
        Vector2 p1 = new Vector2(previousPlatform.X, y);
        Vector2 p2 = new Vector2(previousPlatform.X, previousPlatform.Y);
        DebugLines.Add(new DebugLine(p1, p2));

        previousPlatform = new Point(x, y);

        if (platformType == PlatformType.Spinning)
            previousPlatform.Y -= (int)SpinningPlatform.rotationHeight;

        return -distance;
    }

    static private void SpawnGroundItem(int x, int y)
    {
        //0-nothing, 1-spring, 2-bonus
        int chance = Chance(100 - Springs.springChance - Bonuses.bonusChance, Springs.springChance, Bonuses.bonusChance);

        if (failedBonusSpawn)
        {
            chance = 2;
            failedBonusSpawn = false;
        }

        if (platformType != PlatformType.Simple)
        {
            if(chance == 2)
            {
                failedBonusSpawn = true;
                print("failed bonus spawn");
            }

            return;
        }

        if (chance == 1) Springs.SpawnSpring(x, y);
        if (chance == 2) Bonuses.SpawnBonus(x, y);
        
    }

    const int trapChance = 30;
    static bool failedEnemySpawn = false;

    static public void SpawnObstacle(int previousY, int currentY)
    {
        //0-nothing, 1-trap platform, 2-enemy
        int chance = Chance(100 - trapChance - Enemies.enemyChance, trapChance, Enemies.enemyChance);

        //If there was not enough distance between platforms for enemy to spawn
        //Try to spawn it again next time until its done
        if (failedEnemySpawn)
        {
            chance = 2;
            failedEnemySpawn = false;
        }

        //If nothing, dont spawn anything
        if (chance == 0) return;

        //If bonus is activated, dont spawn the enemies
        if (bonusActivated)
        {
            chance = 1;
        }

        //Not enough space?
        int obstacleHeight = chance == 1 ? Platform.height : Enemy.size.Y;
        int distanceBetween = previousY - currentY - Platform.height;

        if (distanceBetween < minPossibleDistance + obstacleHeight)
        {
            if (chance == 2)
            {
                failedEnemySpawn = true;
                print("failed enemy spawn!");
            }

            return;
        }

        //Enemy
        if (chance == 2)
        {
            print("spawned enemy");
            Enemies.SpawnEnemy(previousY, currentY);
        }
        //Trap platform
        if (chance == 1) AddTrap(previousY, currentY);
    }

    static public void Reset()
    {
        Clear();
        previousPlatform = startPlatform.ToPoint();
        platformType = PlatformType.MaxTypes;

        smallDistChance = defaultSmallChance;
        medDistChance = defaultMedChance;
        largeDistChance = defaultLargeChance;
        platformChances[PlatformType.Spinning] = spinningDefaultChance;
        platformChances[PlatformType.Simple] = simpleDefaultChance;
        platformChances[PlatformType.Breakable] = breakableDefaultChance;
        platformChances[PlatformType.Movable] = movableDefaultChance;
        failedBonusSpawn = false;

        //First platform
        Add(new SimplePlatform(startPlatform));
    }
    static public void DifficultyChange(float diff)
    {
        //Distance changes
        const int smallDistDecrease = 15;
        const int medDistIncrease = 5;
        const int largeDistIncrease = 10;

        if ((medDistIncrease + largeDistIncrease - smallDistDecrease) != 0)
            throw new Exception("distance chance changes not equals zero in Platforms:DifficultyChange");

        smallDistChance -= smallDistDecrease;
        medDistChance += medDistIncrease;
        largeDistChance += largeDistIncrease;

        //Platform types changes
        const int breakableIncrease = 5;
        const int movableIncrease = 5;
        const int spinningIncrease = 1;
        const int simpleDecrease = breakableIncrease + movableIncrease + spinningIncrease;

        if ((breakableIncrease + movableIncrease + spinningIncrease - simpleDecrease) != 0)
            throw new Exception("platform chance changes not equals zero in Platforms:DifficultyChange");

        platformChances[PlatformType.Simple] -= simpleDecrease;
        platformChances[PlatformType.Breakable] += breakableIncrease;
        platformChances[PlatformType.Movable] += movableIncrease;
        platformChances[PlatformType.Spinning] += spinningIncrease;

        print("Chances:");
        print("{");
        foreach(var pair in platformChances)
            print("\t" + pair.Key + ": " + pair.Value);

        print("}");
    }
}

//Base
abstract class Platform : Entity
{
    public static readonly Texture2D defaultTexture = MonoGame.LoadTexture("platform");
    public const int width = 75;
    public const int height = 25;
    public static readonly Point rectSize = new(width, height);

    Platforms.PlatformType platformType;
    public Platforms.PlatformType PlatformType => platformType;

    protected Color color;

    public Platform(Vector2 position, Texture2D texture, Color color)
        : base(new RectangleF(position, rectSize), texture)
    {
        this.color = color;

        foreach(var kv in Platforms.platformTypes)
        {
            if (kv.Value == GetType())
                platformType = kv.Key;
        }
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        Rectangle final = (Rectangle)rectangle;
        final.Location -= MyGame.Camera.ToPoint();
        final.Y -= 5;

        spriteBatch.Draw(texture, final, color);
    }

    public override void OnTouch() {}

    public override void Update(GameTime gameTime) => DestroyOOB();

    public override void Destroy() => Platforms.Destroy(groupID);
    
}

//Simple
class SimplePlatform : Platform
{
    static readonly Color sColor = Color.Lime;

    public SimplePlatform(Vector2 position)
        : base(position, defaultTexture, sColor)
    {
    }
}

//Moving
class MovablePlatform : Platform
{
    static readonly Color sColor = Color.Aquamarine;

    const int minSpeed = 180;
    const int maxSpeed = 260;
    int movement = 0;
    int speed = 200;

    public MovablePlatform(Vector2 position)
        : base(position, defaultTexture, sColor)
    {
        speed = Random(minSpeed, maxSpeed);
        movement = speed;
    }
    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (rectangle.X + rectangle.Width > MyGame.screenSize.X) 
            movement = -speed;

        if (rectangle.X < 0) 
            movement = +speed;

        rectangle.Position += new Vector2(movement, 0) * dt;

        base.Update(gameTime);
    }
}

//Trap
class TrapPlatform : Platform
{
    static readonly Texture2D sTexture = MonoGame.LoadTexture("platform");
    static readonly Color sColor = Color.Yellow;

    public TrapPlatform(Vector2 position)
        : base(position, sTexture, sColor)
    {
    }

    public override void OnTouch() => Destroy();
}

//Breakable
class BreakablePlatform : Platform
{
    static readonly Color sColor = Color.White;

    public BreakablePlatform(Vector2 position)
        : base(position, defaultTexture, sColor)
    {
    }
    public override void OnTouch() => Destroy();
}
class SpinningPlatform : Platform
{
    static readonly Color sColor = Color.Aquamarine;
    public static readonly float rotationHeight = 440;

    public SpinningPlatform(Vector2 pos)
       : base(pos, defaultTexture, sColor)
    {
        deathY = pos.Y - rotationHeight;
        rectangle.X = MyGame.screenSize.X/2-width/2;
    }

    const float maxVelocity = 280;
    const float minVelocity = 20;
    static float generalAcceleration = 180;
    Vector2 acceleration = new(generalAcceleration, -generalAcceleration);
    Vector2 velocity = new Vector2(maxVelocity, 0);

    float deathY;

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        //OOB
        if (deathY > MyGame.DeathPit)
            Destroy();

        //Makes circle
        if (Math.Abs(velocity.X) >= maxVelocity)
        {
            velocity.X = maxVelocity * Math.Sign(velocity.X);
            acceleration.X = -acceleration.X;
        }

        if (Math.Abs(velocity.X) <= minVelocity)
        {
            velocity.X = -minVelocity * Math.Sign(velocity.X);
        }

        if (Math.Abs(velocity.Y) >= maxVelocity)
        {
            velocity.Y = maxVelocity * Math.Sign(velocity.Y);
            acceleration.Y = -acceleration.Y;
        }

        if (Math.Abs(velocity.Y) <= minVelocity)
        {
            velocity.Y = -minVelocity * Math.Sign(velocity.Y);
        }

        rectangle.Position += new Vector2(velocity.X,velocity.Y) * dt;

        velocity.X += acceleration.X * dt;
        velocity.Y += acceleration.Y * dt;

        DebugLines.Add(new DebugLine(new Vector2(rectangle.X + width/2, rectangle.Y), new Vector2(rectangle.X + width / 2 + 1, rectangle.Y + 1), Color.Red, false));

        base.Update(gameTime);
    }
}
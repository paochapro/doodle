using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace Doodle;

using static Utils;

/*class Bonuses : Group<Bonus>
{
    static readonly Dictionary<Bonus.BonusType, int> bonusChances = new Dictionary<Bonus.BonusType, int>()
    {
        [Bonus.BonusType.Propeller] = defaultPropellerChance,
        [Bonus.BonusType.Jetpack] = defaultJetpackChance,
        [Bonus.BonusType.Rocket] = defaultRocketChance,
    };
    static public void SpawnBonus(int x, int y)
    {
        if (!Chance(bonusChance)) return;

        int bonusX = Random(x, x + Platform.width - Bonus.size.X);
        int bonusY = y - Bonus.size.Y;
        Vector2 pos = new(bonusX, bonusY);

        int bonusTypeIndex = Chance(bonusChances.Values.ToArray());
        Bonus.BonusType type = (Bonus.BonusType)bonusTypeIndex;

        Add(new Bonus(pos, type));
    }
    static public void DifficultyChange()
    {
        int totalPropellerDecrease = 0;

        const int rocketChanceIncrease = 15;
        const int jetpackChanceIncrease = 15;

        if (MyGame.Diffuculty > 2)
            bonusChance = 4;

        if (MyGame.Diffuculty > 3)
        {
            bonusChances[Bonus.BonusType.Jetpack] += jetpackChanceIncrease;
            totalPropellerDecrease += jetpackChanceIncrease;
        }
        if (MyGame.Diffuculty > 5)
        {
            bonusChance = 6;
            bonusChances[Bonus.BonusType.Rocket] += rocketChanceIncrease;
            totalPropellerDecrease += rocketChanceIncrease;
        }
        bonusChances[Bonus.BonusType.Propeller] -= totalPropellerDecrease;

        print("Bonus chances: " + "P: " + bonusChances[Bonus.BonusType.Propeller] + " J: " + bonusChances[Bonus.BonusType.Jetpack] + " R: " + bonusChances[Bonus.BonusType.Rocket] + " G: " + bonusChance);
    }
    static public void Reset()
    {
        bonusChance = defaultBonusChance;
        bonusChances[Bonus.BonusType.Propeller] = defaultPropellerChance;
        bonusChances[Bonus.BonusType.Jetpack] = defaultJetpackChance;
        bonusChances[Bonus.BonusType.Rocket] = defaultRocketChance;
    }

    const int defaultPropellerChance = 100;
    const int defaultJetpackChance = 0;
    const int defaultRocketChance = 0;


    public static int bonusChance { get; private set; }
    const int defaultBonusChance = 0;
}*/

class Bonuses : Group<Bonus>
{
    const int defaultPropellerChance = 100;
    const int defaultJetpackChance = 0;
    const int defaultRocketChance = 0;

    public static int bonusChance { get; private set; }
    const int defaultBonusChance = 0;

    static readonly Dictionary<Bonus.BonusType, int> bonusChances = new()
    {
        [Bonus.BonusType.Propeller] = defaultPropellerChance,
        [Bonus.BonusType.Jetpack] = defaultJetpackChance,
        [Bonus.BonusType.Rocket] = defaultRocketChance,
    };
    static public void SpawnBonus(int x, int y)
    {
        Vector2 pos = new(
            Random(x, x + Platform.width - Bonus.size.X),
            y - Bonus.size.Y
        );

        int bonusTypeIndex = Chance(bonusChances.Values.ToArray());
        Bonus.BonusType type = (Bonus.BonusType)bonusTypeIndex;

        Add(new Bonus(pos, type));


    }
    static public void DifficultyChange(float diff)
    {
        int totalPropellerDecrease = 0;

        const int rocketChanceIncrease = 15;
        const int jetpackChanceIncrease = 15;

        if (diff > 2)
            bonusChance = 3;

        if (diff > 3)
        {
            bonusChances[Bonus.BonusType.Jetpack] += jetpackChanceIncrease;
            totalPropellerDecrease += jetpackChanceIncrease;
        }
        if (diff > 5)
        {
            bonusChance = 6;
            bonusChances[Bonus.BonusType.Rocket] += rocketChanceIncrease;
            totalPropellerDecrease += rocketChanceIncrease;
        }
        bonusChances[Bonus.BonusType.Propeller] -= totalPropellerDecrease;

        //print("Bonus chances: " + "P: " + bonusChances[Bonus.BonusType.Propeller] + " J: " + bonusChances[Bonus.BonusType.Jetpack] + " R: " + bonusChances[Bonus.BonusType.Rocket] + " G: " + bonusChance);
    }
    static public void Reset()
    {
        Clear();
        bonusChance = defaultBonusChance;
        bonusChances[Bonus.BonusType.Propeller] = defaultPropellerChance;
        bonusChances[Bonus.BonusType.Jetpack] = defaultJetpackChance;
        bonusChances[Bonus.BonusType.Rocket] = defaultRocketChance;
    }
}

class Bonus : Entity
{
    public enum BonusType
    {
        Propeller,
        Jetpack,
        Rocket,
        MaxTypes
    }
    public readonly record struct BonusData(
        Texture2D bonusTexture,
        Texture2D playerTexture,
        Vector2 offset,
        SoundEffect sound,
        float speed,
        bool behind
    );
    public static readonly Dictionary<BonusType, BonusData> bonusGeneralData = new()
    {
        [BonusType.Propeller] =
        new BonusData()
        {
            bonusTexture = MonoGame.LoadTexture("bonus_propeller"),
            playerTexture = MonoGame.LoadTexture("bonus_propeller"),
            offset = new(3, -10),
            sound = MonoGame.Load<SoundEffect>("bonus")!,
            speed = Player.jumpHeight,
            behind = false,
        },
        [BonusType.Jetpack] =
        new BonusData()
        {
            bonusTexture = MonoGame.LoadTexture("bonus_jetpack"),
            playerTexture = MonoGame.LoadTexture("player_jetpack"),
            offset = new(-13, 10),
            sound = MonoGame.Load<SoundEffect>("bonus")!,
            speed = 1800,
            behind = true,
        },
        [BonusType.Rocket] =
        new BonusData()
        {
            bonusTexture = MonoGame.LoadTexture("bonus_rocket"),
            playerTexture = MonoGame.LoadTexture("player_rocket"),
            offset = new(-13, 10),
            sound = MonoGame.Load<SoundEffect>("bonus")!,
            speed = 2500,
            behind = true,
        },
    };

    public const double bonusTime = 2.5;
    readonly BonusType bonusType;

    //Bonus in-game
    static public readonly Point size = new(32,32);
    public BonusType Type => bonusType;

    public Bonus(Vector2 pos, BonusType type) 
        : base( new RectangleF(pos.X, pos.Y, size.X, size.Y), bonusGeneralData[type].bonusTexture)
    {
        //Loading data for specific type
        bonusType = type;
    }
    public override void Update(GameTime gameTime)
    {
        //Possible animations
        DestroyOOB();
    }
    public override void Destroy() => Bonuses.Destroy(groupID);
    public override void OnTouch() => Destroy();
    
}


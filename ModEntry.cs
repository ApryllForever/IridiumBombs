using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System.Reflection;
using static StardewValley.Menus.CoopMenu;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Emit;
using StardewValley.Monsters;
using StardewValley.Enchantments;

namespace IridiumBombs
{
    internal class ModEntry : Mod
    {
        public static string IridiumBomb = "Iridium Bomb";
        public static string IridiumClusterBomb = "Iridium Cluster Bomb";
        public static string IridiumAmmo = "Iridium Ammo";
        internal static IMonitor? ModMonitor { get; set; }
        internal static IModHelper? Helper { get; set; }

        public static ModConfig Config;

        public static bool bombinate;

        Texture sapphire;

        public override void Entry(IModHelper helper)
        {

            var harmony = new Harmony(this.ModManifest.UniqueID);
           
            ModMonitor = Monitor;
            Helper = helper;

            Config = Helper.ReadConfig<ModConfig>();
            sapphire = helper.ModContent.Load<Texture2D>("Assets/Objects.png");

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.canBePlacedHere)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CanBePlacedHere_Postfix))
               );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.PlacementAction_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.isPlaceable)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.IsPlaceable_Postfix))
            );

            harmony.Patch(
            original: AccessTools.DeclaredMethod(typeof(Bug), nameof(Bug.takeDamage)),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BugTakeDamage_Postfix))
         );

            harmony.Patch(
    original: AccessTools.Method(typeof(GameLocation), "explode"),
    transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.explode_Transpiler)));
        }


        private void OnGameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Bomb Delay",
                getValue: () => Config.Delay,
                setValue: value => Config.Delay = value
            );
        }


            private static void CanBePlacedHere_Postfix(StardewValley.Object __instance, GameLocation l, Vector2 tile, ref bool __result)
        {

            if (!__instance.Name.Equals(IridiumBomb, StringComparison.OrdinalIgnoreCase) && !__instance.Name.Equals(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase) || __instance.bigCraftable.Value)
            {
                return;
            }
            else
            {

                if ((!l.isTilePlaceable(tile, true) || l.isTileOccupiedByFarmer(tile) != null))
                {
                    __result = true;
                }
            }
        }




        private static bool PlacementAction_Prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            Vector2 placementTile = new Vector2(x, y);

            if (!__instance.Name.Equals(IridiumBomb, StringComparison.OrdinalIgnoreCase) && !__instance.Name.Equals(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {

                if (__instance.Name.Contains(IridiumBomb, StringComparison.OrdinalIgnoreCase))
                    {
                   // Game1.currentLocation.playSound("ApryllForever.NuclearBomb_Siren", null, null, StardewValley.Audio.SoundContext.Default);

                    bool success = DoIridiumExplosionAnimation(location, x, y, who);
                    if (success)
                    {
                        __result = true;
                    }
                }

                if (__instance.Name.Contains(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase))
                {
                    // Game1.currentLocation.playSound("ApryllForever.NuclearBomb_Siren", null, null, StardewValley.Audio.SoundContext.Default);

                    bool success = DoIridiumClusterExplosionAnimation(location, x, y, who);
                    if (success)
                    {
                        __result = true;
                    }
                }
                return false;
            }
        }



        private static void IsPlaceable_Postfix(StardewValley.Object __instance, ref bool __result)
        {
            if (__instance.Name.Contains(IridiumBomb, StringComparison.OrdinalIgnoreCase) && __instance.Name.Contains(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase))
            {
                __result = true;
            }
        }

        private static bool DoIridiumExplosionAnimation(GameLocation location, int x, int y, Farmer who)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);


            bombinate = true;

            foreach (TemporaryAnimatedSprite temporarySprite2 in location.temporarySprites)
            {
                if (temporarySprite2.position.Equals(placementTile * 64f))
                {
                    return false;
                }
            }

            StardewValley.Object iridiumBomb = new StardewValley.Object();

            DelayedAction.functionAfterDelay(delegate
            {
                bombinate = true;
            },
            1500 + Config.Delay
            );




            int idNum;
            idNum = Game1.random.Next();
            location.playSound("thudStep");


            TemporaryAnimatedSprite TASa = new TemporaryAnimatedSprite(936, 100f, 1, 24, placementTile * 64f, flicker: false, flipped: false, location, who)
            {
                totalNumberOfLoops =  Config.Delay * 10,
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };

            TemporaryAnimatedSprite TAS = new TemporaryAnimatedSprite(936, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
                delayBeforeAnimationStart = Config.Delay * 1000,
                bombRadius = 11,
                bombDamage = 443, //This is an extremely beautiful prime number
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };

            TemporaryAnimatedSprite TAS2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
            {
                id = idNum
            };
            TemporaryAnimatedSprite TAS3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 100,
                id = idNum
            };
            TemporaryAnimatedSprite TAS4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 200,
                id = idNum
            };
            location.netAudio.StartPlaying("fuse");

            // TAS.texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png");

            Game1.Multiplayer.broadcastSprites(location, TASa);
            Game1.Multiplayer.broadcastSprites(location, TAS);

            Game1.Multiplayer.broadcastSprites(location, TAS2);

            Game1.Multiplayer.broadcastSprites(location, TAS3);
            Game1.Multiplayer.broadcastSprites(location, TAS4);

            DelayedAction.functionAfterDelay(delegate
            {
                bombinate = false;


            },
            10000 + Config.Delay
            );



                return true;
        }


        private static bool DoIridiumClusterExplosionAnimation(GameLocation location, int x, int y, Farmer who)
        {
            Vector2 placementTile = new Vector2(x / 64, y / 64);

            foreach (TemporaryAnimatedSprite temporarySprite2 in location.temporarySprites)
            {
                if (temporarySprite2.position.Equals(placementTile * 64f))
                {
                    return false;
                }
            }

            StardewValley.Object iridiumBomb = new StardewValley.Object();

            DelayedAction.functionAfterDelay(delegate
            {
                bombinate = true;
            },
            2000 + Config.Delay
            );




            int idNum;
            idNum = Game1.random.Next();
            location.playSound("thudStep");

            TemporaryAnimatedSprite TASa = new TemporaryAnimatedSprite(937, 100f, 1, 24, placementTile * 64f, flicker: false, flipped: false, location, who)
            {
                totalNumberOfLoops =  Config.Delay * 10,
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };

            TemporaryAnimatedSprite TAS = new TemporaryAnimatedSprite(937, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
                 delayBeforeAnimationStart =  Config.Delay * 1000,
                bombRadius = 7,
                bombDamage = 199,
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };

            TemporaryAnimatedSprite TASmojo = new TemporaryAnimatedSprite(937, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
                //totalNumberOfLoops = 1,
                // delayBeforeAnimationStart = 0000,
                bombRadius = 7,
                bombDamage = 99,
                delayBeforeAnimationStart = (Config.Delay * 1000) + 300,
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };

            TemporaryAnimatedSprite TAScutie = new TemporaryAnimatedSprite(937, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
                //totalNumberOfLoops = 1,
                // delayBeforeAnimationStart = 0000,
                bombRadius = 7,
                bombDamage = 99,
                delayBeforeAnimationStart = (Config.Delay * 1000) + 600,
                shakeIntensity = 1f,
                shakeIntensityChange = 0.0002f,
                //color = Color.DarkRed,
                //texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png"),
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };


            TemporaryAnimatedSprite TAS2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
            {
                id = idNum
            };
            TemporaryAnimatedSprite TAS3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 100,
                id = idNum
            };
            TemporaryAnimatedSprite TAS4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 200,
                id = idNum
            };
            location.netAudio.StartPlaying("fuse");

            // TAS.texture = Helper.ModContent.Load<Texture2D>("Assets/Objects.png");
            Game1.Multiplayer.broadcastSprites(location, TASa);
            Game1.Multiplayer.broadcastSprites(location, TAS);
            Game1.Multiplayer.broadcastSprites(location, TASmojo);
            Game1.Multiplayer.broadcastSprites(location, TAScutie);

            Game1.Multiplayer.broadcastSprites(location, TAS2);

            Game1.Multiplayer.broadcastSprites(location, TAS3);
            Game1.Multiplayer.broadcastSprites(location, TAS4);

            DelayedAction.functionAfterDelay(delegate
            {
                bombinate = false;
            },
          10000 + Config.Delay
          );




            /* 
             * Stuff someone wrote for me, does not destroy the Rocks or what not
             * 
            TemporaryAnimatedSprite bombSprite = new("TileSheets\\bobbers", new Rectangle(0, 160, 16, 16), 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                initialParentTileIndex = 80, // this doesnt seem to matter, just need not 0
                currentParentTileIndex = 80,
                Parent = location,
                bombRadius = 5,
                shakeIntensity = 0.5f,
                shakeIntensityChange = 0.002f,
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            };
            bombSprite.position.X = (int)bombSprite.position.X;
            bombSprite.position.Y = (int)bombSprite.position.Y;
            Game1.Multiplayer.broadcastSprites(location, bombSprite);
            


            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
            {
                id = idNum
            });
            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 100,
                id = idNum
            });
            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
            {
                delayBeforeAnimationStart = 200,
                id = idNum
            });
            location.netAudio.StartPlaying("fuse"); */



            return true;
        }






        /*
         *
         * 
         * TemporaryAnimatedSprite bombSprite = new("TileSheets\\bobbers", new Rectangle(16, 32, 16, 16), 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
{
    initialParentTileIndex = 18, // this doesnt seem to matter, just need not 0
    currentParentTileIndex = 18,
    Parent = location,
    bombRadius = 9,
    shakeIntensity = 0.5f,
    shakeIntensityChange = 0.002f,
    extraInfoForEndBehavior = idNum,
    endFunction = location.removeTemporarySpritesWithID
};
bombSprite.position.X = (int)bombSprite.position.X;
bombSprite.position.Y = (int)bombSprite.position.Y;
Game1.Multiplayer.broadcastSprites(location, bombSprite);

        TemporaryAnimatedSprite bombSprite = new("TileSheets\\bobbers", new Rectangle(16, 16, 0, 160), 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
{
    initialParentTileIndex = 80, // this doesnt seem to matter, just need not 0
    currentParentTileIndex = 80,
    Parent = location,
    bombRadius = 5,
    shakeIntensity = 0.5f,
    shakeIntensityChange = 0.002f,
    extraInfoForEndBehavior = idNum,
    endFunction = location.removeTemporarySpritesWithID
};
bombSprite.position.X = (int)bombSprite.position.X;
bombSprite.position.Y = (int)bombSprite.position.Y;
Game1.Multiplayer.broadcastSprites(location, bombSprite);


        */


        public static IEnumerable<CodeInstruction> explode_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var newCodes = new List<CodeInstruction>();
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Stfld && ((FieldInfo)codes[i].operand).Name == "alphaFade") // if at the part of code that sets alphaFade...
                {
                    newCodes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.FixAlphaFade)))); // ...intercept before setting and use custom formula
                }
                newCodes.Add(codes[i]);
            }
            return newCodes.AsEnumerable();
        }
        public static float FixAlphaFade(float alphaFade)
        {
            return alphaFade < 0.009f ? 0.009f : alphaFade; // if alphaFade is less than for a Mega Bomb, make it equal the Mega Bomb value
        }

        private static void BugTakeDamage_Postfix(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who, Bug __instance, ref int __result)
        {
            if (isBomb)
            {
                if (bombinate == true)
                {
                    int actualDamage = Math.Max(1, damage - __instance.resilience.Value);
                    __instance.Health -= actualDamage;
                    __instance.currentLocation.playSound("hitEnemy");
                    __instance.setTrajectory(xTrajectory / 3, yTrajectory / 3);
                    if (__instance.isHardModeMonster.Value)
                    {
                        __instance.FacingDirection = Math.Abs((__instance.FacingDirection + Game1.random.Next(-1, 2)) % 4);
                        __instance.Halt();
                        __instance.setMovingInFacingDirection();
                    }
                    if (__instance.Health <= 0)
                    {
                        __instance.deathAnimation();
                    }


                    __result = actualDamage;
                }
            }
        }

    }
}

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

namespace IridiumBombs
{
    internal class ModEntry : Mod
    {
        public static string IridiumBomb = "Iridium Bomb";
        public static string IridiumClusterBomb = "Iridium Cluster Bomb";
        public static string IridiumAmmo = "Iridium Ammo";
        internal static IMonitor? ModMonitor { get; set; }
        internal static IModHelper? Helper { get; set; }

        Texture sapphire;

        public override void Entry(IModHelper helper)
        {

            var harmony = new Harmony(this.ModManifest.UniqueID);

            ModMonitor = Monitor;
            Helper = helper;

            sapphire = helper.ModContent.Load<Texture2D>("Assets/Objects.png");


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
        }



        private static void CanBePlacedHere_Postfix(StardewValley.Object __instance, GameLocation l, Vector2 tile, ref bool __result)
        {

            if (!__instance.Name.Contains(IridiumBomb, StringComparison.OrdinalIgnoreCase)|| !__instance.Name.Contains(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase) || __instance.bigCraftable.Value)
            {
                return;
            }
            else
            {

                if ((!l.isTilePlaceable


                    (tile, true) || l.isTileOccupiedByFarmer(tile) != null))
                {
                    __result = true;
                }
            }
        }




        private static bool PlacementAction_Prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            Vector2 placementTile = new Vector2(x, y);

            if (!__instance.Name.Contains(IridiumBomb, StringComparison.OrdinalIgnoreCase) || __instance.bigCraftable.Value)
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
            if (__instance.Name.Contains(IridiumBomb, StringComparison.OrdinalIgnoreCase) || __instance.Name.Contains(IridiumClusterBomb, StringComparison.OrdinalIgnoreCase))
            {
                __result = true;
            }
        }

        private static bool DoIridiumExplosionAnimation(GameLocation location, int x, int y, Farmer who)
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

            

            int idNum;
            idNum = Game1.random.Next();
            location.playSound("thudStep");


            TemporaryAnimatedSprite TAS = new TemporaryAnimatedSprite(892, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
               // delayBeforeAnimationStart = 0000,
                bombRadius = 11,
                bombDamage = 199,
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
            Game1.Multiplayer.broadcastSprites(location, TAS);

            Game1.Multiplayer.broadcastSprites(location, TAS2);

            Game1.Multiplayer.broadcastSprites(location, TAS3);
            Game1.Multiplayer.broadcastSprites(location, TAS4);





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


            int idNum;
            idNum = Game1.random.Next();
            location.playSound("thudStep");
            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(iridiumBomb.ParentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
            {
                shakeIntensity = 0.5f,
                shakeIntensityChange = 0.002f,
                bombRadius = 73,
                bombDamage = 9999,
                extraInfoForEndBehavior = idNum,
                endFunction = location.removeTemporarySpritesWithID
            });
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
            location.netAudio.StartPlaying("fuse");



            return true;
        }









    }
}

using System;
using BepInEx;
using UnityEngine;
using RWCustom;
using System.Xml.Schema;
using IL;
using MoreSlugcats;
using static MonoMod.InlineRT.MonoModRule;
using Random = UnityEngine.Random;
using Menu.Remix.MixedUI;
using ImprovedInput;
using IL.MoreSlugcats;
using Love = MoreSlugcats.Love;
using STOracleBehavior = MoreSlugcats.STOracleBehavior;
using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;
using CLOracleBehavior = MoreSlugcats.CLOracleBehavior;
using System.Diagnostics.Eventing.Reader;
using Smoke;
using Noise;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "GourmandIsGod", "0.1.1")]
    public class Plugin : BaseUnityPlugin
    {
        public int monkCooldown = 0;    
        public BombSmoke smoke;
        public bool explosionIsForShow;
        public Color explodeColor = new Color(1f, 0.4f, 0.3f);
        private OptionsMenu1 optionsMenuInstance;
        private bool initialized;
        bool flag = true;
        bool monkAscension = false;
        bool deactivateAscension;
        private BodyChunk hitChunk;
        private const string MOD_ID = "mills888.GourmandIsGod";


        //keybinding shenanagins
        public static readonly PlayerKeybind SaintFlight = PlayerKeybind.Register("GourmandIsGod:Flight", "GourmandIsGOD", "SaintFlight", KeyCode.A, KeyCode.None);
        public static readonly PlayerKeybind ArtificersBoom = PlayerKeybind.Register("GourmandIsGod:Boom", "GourmandIsGOD", "ArtiBoom", KeyCode.S, KeyCode.None);
        public static readonly PlayerKeybind SpearMastersSpears = PlayerKeybind.Register("GourmandIsGod:Spears", "GourmandIsGOD", "SpearMastersSpears", KeyCode.D, KeyCode.None);
        public static readonly PlayerKeybind GourmandsBarf = PlayerKeybind.Register("GourmandIsGod:Food", "GourmandIsGod", "GourmandsBarf", KeyCode.F, KeyCode.None);
       


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            //  On.Creature.Die += GODSSMITE;
            On.Creature.Stun += GODSMITE;
            On.Player.Update += Player_Update;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        private void GODSMITE(On.Creature.orig_Stun orig, Creature self, int st)
        {
            if (self.abstractCreature.karmicPotential < 5 && ISGORMAUND)
            {
                Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
                self.room.AddObject(new SootMark(self.room, vector, 80f, true));
                if (explosionIsForShow)
                {
                    self.room.AddObject(new Explosion(self.room, self, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
                }
                self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
                self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
                self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 a = Custom.RNV();
                    if (self.room.GetTile(vector + a * 20f).Solid)
                    {
                        if (!self.room.GetTile(vector - a * 20f).Solid)
                        {
                            a *= -1f;
                        }
                        else
                        {
                            a = Custom.RNV();
                        }
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        self.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                    }
                    self.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
                }
                if (smoke != null)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                    }
                }
                for (int l = 0; l < 6; l++)
                {
                    self.room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
                }
                self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
                for (int m = 0; m < self.abstractPhysicalObject.stuckObjects.Count; m++)
                {
                    self.abstractPhysicalObject.stuckObjects[m].Deactivate();
                }
                self.room.PlaySound(SoundID.Bomb_Explode, vector);
                self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
                bool flag = hitChunk != null;
                for (int n = 0; n < 5; n++)
                {
                    if (self.room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    if (smoke == null)
                    {
                        smoke = new BombSmoke(self.room, vector, null, explodeColor);
                        self.room.AddObject(smoke);
                    }
                    if (hitChunk != null)
                    {
                        smoke.chunk = hitChunk;
                    }
                    else
                    {
                        smoke.chunk = null;
                        smoke.fadeIn = 1f;
                    }
                    smoke.pos = vector;
                    smoke.stationary = true;
                    smoke.DisconnectSmoke();
                }
                else if (smoke != null)
                {
                    smoke.Destroy();
                }

                self.deaf = 0;
                self.dead = true;
            }
        }



        /* private void GODSSMITE(On.RoomRain.orig_CreatureSmashedInGround orig, RoomRain self, Creature crit, float speed)
         {
             Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
             self.room.AddObject(new SootMark(self.room, vector, 80f, true));
             if (!self.explosionIsForShow)
             {
                 self.room.AddObject(new Explosion(self.room, self, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, self.thrownBy, 0.7f, 160f, 1f));
             }
             self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, self.explodeColor));
             self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
             self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, self.explodeColor));
             self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
             for (int i = 0; i < 25; i++)
             {
                 Vector2 a = Custom.RNV();
                 if (self.room.GetTile(vector + a * 20f).Solid)
                 {
                     if (!self.room.GetTile(vector - a * 20f).Solid)
                     {
                         a *= -1f;
                     }
                     else
                     {
                         a = Custom.RNV();
                     }
                 }
                 for (int j = 0; j < 3; j++)
                 {
                     self.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(self.explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                 }
                 self.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), self.explodeColor, Random.Range(3, 11)));
             }
             if (smoke != null)
             {
                 for (int k = 0; k < 8; k++)
                 {
                     smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                 }
             }
             for (int l = 0; l < 6; l++)
             {
                 self.room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
             }
             self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
             for (int m = 0; m < self.abstractPhysicalObject.stuckObjects.Count; m++)
             {
                 self.abstractPhysicalObject.stuckObjects[m].Deactivate();
             }
             self.room.PlaySound(SoundID.Bomb_Explode, vector);
             self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
             bool flag = hitChunk != null;
             for (int n = 0; n < 5; n++)
             {
                 if (self.room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                 {
                     flag = true;
                     break;
                 }
             }
             if (flag)
             {
                 if (smoke == null)
                 {
                     smoke = new BombSmoke(self.room, vector, null, self.explodeColor);
                     self.room.AddObject(smoke);
                 }
                 if (hitChunk != null)
                 {
                     smoke.chunk = hitChunk;
                 }
                 else
                 {
                     smoke.chunk = null;
                     smoke.fadeIn = 1f;
                 }
                 smoke.pos = vector;
                 smoke.stationary = true;
                 smoke.DisconnectSmoke();
             }
             else if (smoke != null)
             {
                 smoke.Destroy();
             }
             self.Destroy();        }

        /*  private void GODSSMITE(On.Creature.orig_Die orig, Creature self)
          {
              if(self.abstractCreature.)
              {

              }
              Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
              self.room.AddObject(new SootMark(self.room, vector, 80f, true));
              if (explosionIsForShow)
              {
                  self.room.AddObject(new Explosion(self.room, self, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
              }
              self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
              self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
              self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
              self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
              for (int i = 0; i < 25; i++)
              {
                  Vector2 a = Custom.RNV();
                  if (self.room.GetTile(vector + a * 20f).Solid)
                  {
                      if (!self.room.GetTile(vector - a * 20f).Solid)
                      {
                          a *= -1f;
                      }
                      else
                      {
                          a = Custom.RNV();
                      }
                  }
                  for (int j = 0; j < 3; j++)
                  {
                      self.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                  }
                  self.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
              }
              if (smoke != null)
              {
                  for (int k = 0; k < 8; k++)
                  {
                      smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                  }
              }
              for (int l = 0; l < 6; l++)
              {
                  self.room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
              }
              self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
              for (int m = 0; m < self.abstractPhysicalObject.stuckObjects.Count; m++)
              {
                  self.abstractPhysicalObject.stuckObjects[m].Deactivate();
              }
              self.room.PlaySound(SoundID.Bomb_Explode, vector);
              self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
              bool flag = hitChunk != null;
              for (int n = 0; n < 5; n++)
              {
                  if (self.room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                  {
                      flag = true;
                      break;
                  }
              }
              if (flag)
              {
                  if (smoke == null)
                  {
                      smoke = new BombSmoke(self.room, vector, null, explodeColor);
                      self.room.AddObject(smoke);
                  }
                  if (hitChunk != null)
                  {
                      smoke.chunk = hitChunk;
                  }
                  else
                  {
                      smoke.chunk = null;
                      smoke.fadeIn = 1f;
                  }
                  smoke.pos = vector;
                  smoke.stationary = true;
                  smoke.DisconnectSmoke();
              }
              else if (smoke != null)
              {
                  smoke.Destroy();
              }
          }*/



        /* private void GODSSMITE(On.RoomRain.orig_CreatureSmashedInGround orig, RoomRain self, Creature crit, float speed)
         {
             Vector2 vector = Vector2.Lerp(self.firstChunk.pos, self.firstChunk.lastPos, 0.35f);
             self.room.AddObject(new SootMark(self.room, vector, 80f, true));
             if (!self.explosionIsForShow)
             {
                 self.room.AddObject(new Explosion(self.room, self, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, self.thrownBy, 0.7f, 160f, 1f));
             }
             self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, self.explodeColor));
             self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
             self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, self.explodeColor));
             self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));
             for (int i = 0; i < 25; i++)
             {
                 Vector2 a = Custom.RNV();
                 if (self.room.GetTile(vector + a * 20f).Solid)
                 {
                     if (!self.room.GetTile(vector - a * 20f).Solid)
                     {
                         a *= -1f;
                     }
                     else
                     {
                         a = Custom.RNV();
                     }
                 }
                 for (int j = 0; j < 3; j++)
                 {
                     self.room.AddObject(new Spark(vector + a * Mathf.Lerp(30f, 60f, Random.value), a * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(self.explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                 }
                 self.room.AddObject(new Explosion.FlashingSmoke(vector + a * 40f * Random.value, a * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), self.explodeColor, Random.Range(3, 11)));
             }
             if (smoke != null)
             {
                 for (int k = 0; k < 8; k++)
                 {
                     smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                 }
             }
             for (int l = 0; l < 6; l++)
             {
                 self.room.AddObject(new ScavengerBomb.BombFragment(vector, Custom.DegToVec(((float)l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value)));
             }
             self.room.ScreenMovement(new Vector2?(vector), default(Vector2), 1.3f);
             for (int m = 0; m < self.abstractPhysicalObject.stuckObjects.Count; m++)
             {
                 self.abstractPhysicalObject.stuckObjects[m].Deactivate();
             }
             self.room.PlaySound(SoundID.Bomb_Explode, vector);
             self.room.InGameNoise(new InGameNoise(vector, 9000f, self, 1f));
             bool flag = hitChunk != null;
             for (int n = 0; n < 5; n++)
             {
                 if (self.room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                 {
                     flag = true;
                     break;
                 }
             }
             if (flag)
             {
                 if (smoke == null)
                 {
                     smoke = new BombSmoke(self.room, vector, null, self.explodeColor);
                     self.room.AddObject(smoke);
                 }
                 if (hitChunk != null)
                 {
                     smoke.chunk = hitChunk;
                 }
                 else
                 {
                     smoke.chunk = null;
                     smoke.fadeIn = 1f;
                 }
                 smoke.pos = vector;
                 smoke.stationary = true;
                 smoke.DisconnectSmoke();
             }
             else if (smoke != null)
             {
                 smoke.Destroy();
             }
             self.Destroy();
         }*/

        public static AbstractPhysicalObject RandomStomachItem(PhysicalObject caller)
        {
            float value = Random.value;
            AbstractPhysicalObject abstractPhysicalObject;
            if (value <= 0.32894737f)
            {
                abstractPhysicalObject = new AbstractPhysicalObject(caller.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.4276316f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.Mushroom, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else if (value <= 0.5065789f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else if (value <= 0.6118421f)
            {
                abstractPhysicalObject = new WaterNut.AbstractWaterNut(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, false);
            }
            else if (value <= 0.6644737f)
            {
                abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Deer), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.7302632f)
            {
                abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.VultureGrub), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.79605263f)
            {
                abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Slugcat), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.82894737f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.PuffBall, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else if (value <= 0.8486842f)
            {
                abstractPhysicalObject = new AbstractPhysicalObject(caller.room.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.9144737f)
            {
                abstractPhysicalObject = new BubbleGrass.AbstractBubbleGrass(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), 1f, -1, -1, null);
            }
            else if (value <= 0.93421054f)
            {
                abstractPhysicalObject = new SporePlant.AbstractSporePlant(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, false, (double)Random.value < 0.5);
            }
            else if (value <= 0.46710527f)
            {
                Color color = new Color(1f, 0.8f, 0.3f);
                int ownerIterator = 1;
                if (Random.value <= 0.35f)
                {
                    color = new Color(0.44705883f, 0.9019608f, 0.76862746f);
                    ownerIterator = 0;
                }
                else if (Random.value <= 0.05f)
                {
                    color = new Color(0f, 1f, 0f);
                    ownerIterator = 2;
                }
                abstractPhysicalObject = new OverseerCarcass.AbstractOverseerCarcass(caller.room.world, null, caller.abstractPhysicalObject.pos, caller.room.game.GetNewID(), color, ownerIterator);
            }
            else if (value <= 0.4736842f)
            {
                abstractPhysicalObject = new AbstractConsumable(caller.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null);
            }
            else if (value <= 0.9934211f)
            {
                abstractPhysicalObject = new AbstractPhysicalObject(caller.room.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.79605263f)
            {
                abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.TubeWorm), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.796052000f)
            {
                abstractPhysicalObject = new AbstractCreature(caller.room.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DaddyLongLegs), null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID());
            }
            else if (value <= 0.8f)
                {
                    abstractPhysicalObject = new VultureMask.AbstractVultureMask(caller.room.world, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), caller.abstractPhysicalObject.ID.RandomSeed, (double)Random.value <= 0.05);
                }
                else
                {
                    abstractPhysicalObject = new DataPearl.AbstractDataPearl(caller.room.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, caller.room.GetWorldCoordinate(caller.firstChunk.pos), caller.room.game.GetNewID(), -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
                }
                if (AbstractConsumable.IsTypeConsumable(abstractPhysicalObject.type))
                {
                    (abstractPhysicalObject as AbstractConsumable).isFresh = false;
                    (abstractPhysicalObject as AbstractConsumable).isConsumed = true;
                }
                return abstractPhysicalObject;
        }

        public bool ISGORMAUND;
        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            bool flag2 = this.initialized;
            if (!flag2)
            {


                this.initialized = true;
                this.optionsMenuInstance = new OptionsMenu1(this);

                optionsMenuInstance = new OptionsMenu1(this);
                try
                {
                    MachineConnector.SetRegisteredOI("mills888.GourmandIsGod", this.optionsMenuInstance);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
                    Logger.LogError(ex);
                    Logger.LogMessage("WHOOPS");
                }
            }
        }



        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {

            orig(self, eu);
            if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                self.aerobicLevel = 0;
                self.exhausted = false;
                self.gourmandExhausted = false;
                ISGORMAUND = true;

            }

            if (true)

            {
                if(self.IsPressed(GourmandsBarf))
                {
                    self.objectInStomach = RandomStomachItem(self);
                }
            }



            if (OptionsMenu1.gourmandFlightActive.Value == true && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                if (monkCooldown > -1)
                {
                    monkCooldown--;
                }

                if (self.IsPressed(SaintFlight) && !monkAscension && monkCooldown < 0)
                {
                        monkCooldown = 10;
                    monkAscension = true;
                    self.wantToJump = 0;
                    self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk, false, 1f, 1f);
                    self.room.AddObject(new ShockWave(self.bodyChunks[1].pos, 100f, 0.07f, 6, false));
                    for (int i = 0; i < 10; i++)
                    {
                        self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), false));
                    }
                }
                if (self.IsPressed(SaintFlight) && monkAscension && monkCooldown < 0)
                {
                    monkCooldown = 10;
                    deactivateAscension = true;

                }

                if (deactivateAscension)
                {
                    deactivateAscension = false;
                    self.room.PlaySound(SoundID.HUD_Pause_Game, self.mainBodyChunk, false, 1f, 0.5f);
                    monkAscension = false;
                }








                if (monkAscension)
                {
                    self.buoyancy = 0f;
                    self.godDeactiveTimer = 0f;
                    self.animation = Player.AnimationIndex.None;
                    self.bodyMode = Player.BodyModeIndex.Default;
                    if (self.tongue != null && self.tongue.Attached)
                    {
                        self.tongue.Release();
                    }
                    if (self.godWarmup > -20f)
                    {
                        self.godWarmup -= 1f;
                    }
                    if ((self.room == null || !self.room.game.setupValues.saintInfinitePower) && self.karmaCharging == 0 && self.godWarmup <= 0f)
                    {
                        self.godTimer -= 1f;
                    }
                    if (self.dead || self.stun >= 20)
                    {
                        deactivateAscension = false;
                    }
                    if (self.godTimer <= 0f)
                    {
                        self.godRecharging = true;
                        self.godTimer = -15f;
                        deactivateAscension = false;
                    }
                    else
                    {
                        self.godRecharging = false;
                    }
                    if (flag && self.AI == null && self.godTimer <= self.maxGodTime * 0.9f && self.room.game.session is StoryGameSession && !(self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmicBurstMessage)
                    {
                        (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmicBurstMessage = true;
                        self.room.game.cameras[0].hud.textPrompt.AddMessage(self.room.game.rainWorld.inGameTranslator.Translate("Hold throw and directional inputs while flying to perform an ascension."), 80, 240, true, true);
                    }
                    self.gravity = 0f;
                    self.airFriction = 0.5f;
                    float num = 2.45f;
                    if (self.killWait >= 0.2f && !self.forceBurst)
                    {
                        self.airFriction = 0.1f;
                        self.bodyChunks[0].vel = Custom.RNV() * Mathf.Lerp(0f, 20f, self.killWait);
                        num = 0f;
                    }
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + num;
                        self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + (num - 1f);
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - num;
                        self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - (num - 1f);
                    }
                    if (self.input[0].x > 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + num;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + (num - 1f);
                    }
                    else if (self.input[0].x < 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - num;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - (num - 1f);
                    }
                    float num2 = 10f;
                    float num3 = 400f;
                    float num4 = 1f;
                    float num5 = 2f;
                    float num6 = 0.7f;
                    if (!self.input[0].thrw && !self.forceBurst)
                    {
                        if (self.voidSceneTimer == 0)
                        {
                            self.burstX *= num6;
                            self.burstY *= num6;
                        }
                        self.burstVelX *= num6;
                        self.burstVelY *= num6;
                        self.killPressed = false;
                        self.killFac *= 0.8f;
                        self.killWait *= 0.95f;
                        return;
                    }
                    if (!self.killPressed)
                    {
                        if (!self.forceBurst)
                        {
                            self.killWait = Mathf.Min(self.killWait + 0.035f, 1f);
                            if (self.killWait == 1f)
                            {
                                self.killFac += 0.025f;
                            }
                        }
                        if (self.input[0].x != 0)
                        {
                            self.burstVelX = Mathf.Clamp(self.burstVelX + (float)self.input[0].x * num4, -num2, num2);
                        }
                        else if (self.burstVelX < -num5)
                        {
                            self.burstVelX += num5;
                        }
                        else if (self.burstVelX > num5)
                        {
                            self.burstVelX -= num5;
                        }
                        else
                        {
                            self.burstVelX = 0f;
                        }
                        if (self.input[0].y != 0)
                        {
                            self.burstVelY = Mathf.Clamp(self.burstVelY + (float)self.input[0].y * num4, -num2, num2);
                        }
                        else if (self.burstVelY < -num5)
                        {
                            self.burstVelY += num5;
                        }
                        else if (self.burstVelY > num5)
                        {
                            self.burstVelY -= num5;
                        }
                        else
                        {
                            self.burstVelY = 0f;
                        }
                        if (!self.forceBurst)
                        {
                            self.burstX = Mathf.Clamp(self.burstX + self.burstVelX, -num3, num3);
                            self.burstY = Mathf.Clamp(self.burstY + self.burstVelY, -num3, num3);
                        }
                        else if (flag)
                        {
                            float num7 = self.wormCutsceneTarget.x - (self.mainBodyChunk.pos.x + self.burstX);
                            float num8 = self.wormCutsceneTarget.y - (self.mainBodyChunk.pos.y + self.burstY + 60f);
                            if (Custom.DistLess(Vector2.zero, new Vector2(num7, num8), 450f))
                            {
                                float num9 = 0.02f;
                                if (self.wormCutsceneLockon)
                                {
                                    num9 = 0.25f;
                                }
                                if (num7 > 0f)
                                {
                                    self.burstX += Mathf.Clamp(num7 * num9, 2.5f, self.wormCutsceneLockon ? 100f : 10f);
                                }
                                else
                                {
                                    self.burstX += Mathf.Clamp(num7 * num9, self.wormCutsceneLockon ? -100f : -10f, -2.5f);
                                }
                                if (num8 > 0f)
                                {
                                    self.burstY += Mathf.Clamp(num8 * num9, 2.5f, self.wormCutsceneLockon ? 100f : 10f);
                                }
                                else
                                {
                                    self.burstY += Mathf.Clamp(num8 * num9, self.wormCutsceneLockon ? -100f : -10f, -2.5f);
                                }
                                if (Custom.DistLess(Vector2.zero, new Vector2(num7, num8), 40f) && self.killWait == 1f)
                                {
                                    self.killFac += 0.025f;
                                    self.wormCutsceneLockon = true;
                                }
                            }
                        }
                    }
                    if (self.killFac >= 1f)
                    {
                        num = 60f;
                        Vector2 vector2 = new Vector2(self.mainBodyChunk.pos.x + self.burstX, self.mainBodyChunk.pos.y + self.burstY + 60f);
                        bool flag2 = false;
                        for (int i = 0; i < self.room.physicalObjects.Length; i++)
                        {
                            for (int j = self.room.physicalObjects[i].Count - 1; j >= 0; j--)
                            {
                                if (j >= self.room.physicalObjects[i].Count)
                                {
                                    j = self.room.physicalObjects[i].Count - 1;
                                }
                                PhysicalObject physicalObject = self.room.physicalObjects[i][j];
                                if (physicalObject != self)
                                {
                                    foreach (BodyChunk bodyChunk in physicalObject.bodyChunks)
                                    {
                                        if (Custom.DistLess(bodyChunk.pos, vector2, num + bodyChunk.rad) && self.room.VisualContact(bodyChunk.pos, vector2))
                                        {
                                            bodyChunk.vel += Custom.RNV() * 36f;
                                            if (physicalObject is Creature)
                                            {
                                                if (!(physicalObject as Creature).dead)
                                                {
                                                    flag2 = true;
                                                }
                                                (physicalObject as Creature).Die();
                                            }
                                            if (physicalObject is SeedCob && !(physicalObject as SeedCob).AbstractCob.opened && !(physicalObject as SeedCob).AbstractCob.dead)
                                            {
                                                (physicalObject as SeedCob).spawnUtilityFoods();
                                            }
                                            if (self.room.game.session is StoryGameSession && physicalObject is Oracle && flag)
                                            {
                                                if ((physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.CL && !(self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles)
                                                {
                                                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripPebbles = true;
                                                    self.room.PlaySound(SoundID.SS_AI_Talk_1, self.mainBodyChunk, false, 1f, 0.4f);
                                                    Vector2 pos = (physicalObject as Oracle).bodyChunks[0].pos;
                                                    self.room.AddObject(new ShockWave(pos, 500f, 0.75f, 18, false));
                                                    self.room.AddObject(new Explosion.ExplosionLight(pos, 320f, 1f, 5, Color.white));
                                                    Debug.Log("Ascend saint pebbles");
                                                    ((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).dialogBox.Interrupt("...", 1);
                                                    if (((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).currentConversation != null)
                                                    {
                                                        ((physicalObject as Oracle).oracleBehavior as CLOracleBehavior).currentConversation.Destroy();
                                                    }
                                                    (physicalObject as Oracle).health = 0f;
                                                    flag2 = true;
                                                }
                                                if ((physicalObject as Oracle).ID == Oracle.OracleID.SL && !(self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon && (physicalObject as Oracle).glowers > 0 && (physicalObject as Oracle).mySwarmers.Count > 0)
                                                {
                                                    for (int l = 0; l < (physicalObject as Oracle).mySwarmers.Count; l++)
                                                    {
                                                        (physicalObject as Oracle).mySwarmers[l].ExplodeSwarmer();
                                                    }
                                                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.ripMoon = true;
                                                    Debug.Log("Ascend saint moon");
                                                    ((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).dialogBox.Interrupt("...", 1);
                                                    if (((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).currentConversation != null)
                                                    {
                                                        ((physicalObject as Oracle).oracleBehavior as SLOracleBehaviorHasMark).currentConversation.Destroy();
                                                    }
                                                    Vector2 pos2 = (physicalObject as Oracle).bodyChunks[0].pos;
                                                    self.room.AddObject(new ShockWave(pos2, 500f, 0.75f, 18, false));
                                                    self.room.AddObject(new Explosion.ExplosionLight(pos2, 320f, 1f, 5, Color.white));
                                                    flag2 = true;
                                                }
                                            }
                                            if (physicalObject is Oracle && (physicalObject as Oracle).ID == MoreSlugcatsEnums.OracleID.ST && (physicalObject as Oracle).Consious)
                                            {
                                                Vector2 pos3 = (physicalObject as Oracle).bodyChunks[0].pos;
                                                self.room.AddObject(new ShockWave(pos3, 500f, 0.75f, 18, false));
                                                ((physicalObject as Oracle).oracleBehavior as STOracleBehavior).AdvancePhase();
                                                self.bodyChunks[0].vel = Vector2.zero;
                                                flag2 = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        for (int m = 0; m < self.room.updateList.Count; m++)
                        {
                            if (self.room.updateList[m] is Love)
                            {
                                Love love = self.room.updateList[m] as Love;
                                if (love.animator != null && love.timeUntilReboot == 0 && Custom.DistLess(love.pos, vector2, 100f))
                                {
                                    love.InitiateReboot();
                                    flag2 = true;
                                }
                            }
                        }
                        if (flag2 || self.voidSceneTimer > 0)
                        {
                            self.room.PlaySound(SoundID.Firecracker_Bang, self.mainBodyChunk, false, 1f, 0.75f + UnityEngine.Random.value);
                            self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk, false, 1f, 0.5f + UnityEngine.Random.value * 0.5f);
                        }
                        else
                        {
                            self.room.PlaySound(SoundID.Snail_Pop, self.mainBodyChunk, false, 1f, 1.5f + UnityEngine.Random.value);
                        }
                        for (int n = 0; n < 20; n++)
                        {
                            self.room.AddObject(new Spark(vector2, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                        }
                        self.killFac = 0f;
                        self.killWait = 0f;
                        self.killPressed = true;
                        if (self.voidSceneTimer > 0)
                        {
                            self.voidSceneTimer = 0;
                            deactivateAscension = false;
                            self.controller = null;
                            self.forceBurst = false;
                            return;
                        }
                    }
                }
                else
                {
                    if (self.godWarmup < 60f && self.godDeactiveTimer > 200f)
                    {
                        self.godWarmup += 1f;
                    }
                    self.godDeactiveTimer += 1f;
                    self.killPressed = false;
                    self.killFac *= 0.8f;
                    self.killWait *= 0.5f;
                    float num10 = 0.15f * (self.maxGodTime / 400f);
                    if (self.godRecharging)
                    {
                        num10 = 0.15f * (self.maxGodTime / 400f);
                    }
                    self.godTimer = Mathf.Min(self.godTimer + num10, self.maxGodTime);
                }

            }


            if (OptionsMenu1.PyroJumpEnabled.Value && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                if (self.IsPressed(ArtificersBoom) && self.pyroJumpCooldown < 0)
                {
                    self.noGrabCounter = 5;
                    Vector2 pos = self.firstChunk.pos;
                    for (int i = 0; i < 8; i++)
                    {
                        self.room.AddObject(new Explosion.ExplosionSmoke(pos, Custom.RNV() * 5f * Random.value, 1f));
                    }
                    self.room.AddObject(new Explosion.ExplosionLight(pos, 160f, 1f, 3, Color.white));
                    for (int j = 0; j < 10; j++)
                    {
                        Vector2 a = Custom.RNV();
                        self.room.AddObject(new Spark(pos + a * Random.value * 40f, a * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                    }
                    self.room.PlaySound(SoundID.Fire_Spear_Explode, pos, 0.3f + Random.value * 0.3f, 0.5f + Random.value * 2f);
                    self.room.InGameNoise(new Noise.InGameNoise(pos, 8000f, self, 1f));
                    if (self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
                    {
                        float num3 = (float)self.input[0].x;
                        float num4 = (float)self.input[0].y;
                        while (num3 == 0f && num4 == 0f)
                        {
                            num3 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                            num4 = (float)(((double)Random.value <= 0.33) ? 0 : (((double)Random.value <= 0.5) ? 1 : -1));
                        }
                        self.bodyChunks[0].vel.x = 9f * num3;
                        self.bodyChunks[0].vel.y = 9f * num4;
                        self.bodyChunks[1].vel.x = 8f * num3;
                        self.bodyChunks[1].vel.y = 8f * num4;
                        self.pyroJumpCooldown = 150f;
                    }
                    else
                    {
                        if (self.input[0].x != 0)
                        {
                            self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f * 3f;
                            self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f * 3f;
                            self.jumpBoost = 6f;
                        }
                        if (self.input[0].x == 0 || self.input[0].y == 1)
                        {
                            self.bodyChunks[0].vel.y = 11f * 3f;
                            self.bodyChunks[1].vel.y = 10f * 3f;
                        }
                        if (self.input[0].y == 1)
                        {
                            self.bodyChunks[0].vel.x = 10f * 3 * (float)self.input[0].x;
                            self.bodyChunks[1].vel.x = 8f * 3 * (float)self.input[0].x;
                        }
                        else
                        {
                            self.bodyChunks[0].vel.x = 15f * 3 * (float)self.input[0].x;
                            self.bodyChunks[1].vel.x = 13f * 3 * (float)self.input[0].x;
                        }
                        self.animation = Player.AnimationIndex.Flip;
                        self.pyroJumpCooldown = 20f;
                        self.bodyMode = Player.BodyModeIndex.Default;
                    }
                }
                if (self.pyroJumpCooldown > -1)
                {
                    self.pyroJumpCooldown--;
                }
            }



            if (true)
            {
                if (OptionsMenu1.SpearAbility.Value && self.IsPressed(SpearMastersSpears) && self.FreeHand() > -1)
                {
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 1f + Random.value * 0.5f);
                    AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), self.room.game.GetNewID(), false);
                    self.room.abstractRoom.AddEntity(abstractSpear);
                    abstractSpear.pos = self.abstractCreature.pos;
                    abstractSpear.RealizeInRoom();
                    self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                }
            }
        }

    }




    public class OptionsMenu1 : OptionInterface
    {
      //  public static Configurable<bool> gourmandFlightActive;
        public OptionsMenu1(Plugin plugin)
        {
            gourmandFlightActive = this.config.Bind<bool>("configgourmandFlightActive", true);
            PyroJumpEnabled = this.config.Bind<bool>("PyroJumpEnabled", true);
            SpearAbility = this.config.Bind<bool>("SpearAbilityActive", true);
        }
        public override void Initialize()
        {
            var opTab1 = new OpTab(this, "Default Canvas");
            this.Tabs = new[] { opTab1 }; // Add the tabs into your list of tabs. If there is only a single tab, it will not show the flap on the side because there is not need to.

            // Tab 1
            OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
            opTab1.AddItems(tab1Container);
            // You can put sprites with effects in the Remix Menu by using an OpContainer

            UIelement[] UIArrayElements = new UIelement[] // Labels in a fixed box size + alignment
            {
           /* new OpFloatSlider(configRadiusMul, new Vector2(50f, 400f), 100, 2)
                {
                    max = 500f,
                    min = 1f,
                    hideLabel = false,
                    _increment = 1,
                    mousewheelTick = 25,
                },*/
                //arti shit
                new OpCheckBox(PyroJumpEnabled, 25f, 400f),
                new OpLabel(25f, 450f, "Artificer jump(check settings for keybind)"),

                //saint shit
                new OpCheckBox(gourmandFlightActive, 400f, 400f),
                 new OpLabel(350f, 450f, "Flight (check settings for keybind)"),
                 //spear shit
                 new OpCheckBox(SpearAbility, 400f, 100f),
                 new OpLabel(400f, 150f, "Summon spear (check settings for keybind)")
            };
            opTab1.AddItems(UIArrayElements);

          /*  UIelement[] UIArrayElements2 = new UIelement[] //create an array of ui elements
            {
                //new OpSlider(testFloatSlider, new Vector2(50, 400), 100){max = 100, hideLabel = false}, // Using "hideLabel = true" makes the number disappear but the shadow of where the number would be still appears, why lol.

            };*/
        }

        // Configurable values. They are bound to the config in constructor, and then passed to UI elements.
        // They will contain values set in the menu. And to fetch them in your code use their NAME.Value. For example to get the boolean testCheckBox.Value, to get the integer testSlider.Value
        //public readonly Configurable<TYPE> NAME;
        public static Configurable<bool> gourmandFlightActive;
        public static Configurable<bool> PyroJumpEnabled;
        public static Configurable<bool> SpearAbility;
    }
}
    


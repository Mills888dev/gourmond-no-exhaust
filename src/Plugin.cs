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

using IL.MoreSlugcats;
using Love = MoreSlugcats.Love;
using STOracleBehavior = MoreSlugcats.STOracleBehavior;
using MoreSlugcatsEnums = MoreSlugcats.MoreSlugcatsEnums;
using CLOracleBehavior = MoreSlugcats.CLOracleBehavior;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "GourmandIsGod", "0.1.0")]
    public class Plugin : BaseUnityPlugin


    {
        private OptionsMenu1 optionsMenuInstance;
        private bool initialized;
        bool flag = true;
        bool monkAscension = false;
        bool deactivateAscension;
        private const string MOD_ID = "mills888.GourmandIsGod";

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            On.Player.Update += Player_Update;
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;

        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (this.initialized)
            {
                return;
            }
            this.initialized = true;

            optionsMenuInstance = new OptionsMenu1(this);
            try
            {
                MachineConnector.SetRegisteredOI("Plugin", optionsMenuInstance);
            }
            catch (Exception ex)
            {
                Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
                Logger.LogError(ex);
                Logger.LogMessage("WHOOPS");
            }
        }



        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (OptionsMenu1.gourmandFlightActive.Value == true)
            {
                orig(self, eu);
                if (self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
                {
                    self.aerobicLevel = 0;
                    self.exhausted = false;
                    self.gourmandExhausted = false;

                }
                if (self.input[0].pckp && !monkAscension && self.input[0].jmp)
                {
                    monkAscension = true;
                    self.wantToJump = 0;
                    self.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, self.mainBodyChunk, false, 1f, 1f);
                    self.room.AddObject(new ShockWave(self.bodyChunks[1].pos, 100f, 0.07f, 6, false));
                    for (int i = 0; i < 10; i++)
                    {
                        self.room.AddObject(new WaterDrip(self.bodyChunks[1].pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), false));
                    }
                }
                if (self.input[0].pckp && monkAscension && !self.input[0].jmp)
                {
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
                    self.airFriction = 0.7f;
                    float num = 2.75f;
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
        }
        }




    public class OptionsMenu1 : OptionInterface
    {
        public static Configurable<bool> gourmandFlightActive;
        public OptionsMenu1(Plugin plugin)
        {
            gourmandFlightActive = this.config.Bind<bool>("configgourmandFlightActive", false);
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
                new OpCheckBox(gourmandFlightActive, 300f, 400f),
               // new OpLabel(50f, 450f, "explosion size(default = 100)"),
                 new OpLabel(250f, 450f, "Flight (to activate press shift+z"),

            };
            opTab1.AddItems(UIArrayElements);

            UIelement[] UIArrayElements2 = new UIelement[] //create an array of ui elements
            {
                //new OpSlider(testFloatSlider, new Vector2(50, 400), 100){max = 100, hideLabel = false}, // Using "hideLabel = true" makes the number disappear but the shadow of where the number would be still appears, why lol.

            };
        }

        // Configurable values. They are bound to the config in constructor, and then passed to UI elements.
        // They will contain values set in the menu. And to fetch them in your code use their NAME.Value. For example to get the boolean testCheckBox.Value, to get the integer testSlider.Value
        //public readonly Configurable<TYPE> NAME;        
    }
}
    


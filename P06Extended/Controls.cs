using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using System.Linq;
using System.Globalization;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using System.Reflection;
using P06X.Helpers;
using TMPro;

namespace P06X
{
    public class XDebug : XSingleton<XDebug>
    {
        private void Start()
        {
            this.RealFixedDeltaTime = Time.fixedDeltaTime;
            if (XDebug.COMMENT)
            {
                new XDebug.DebugInt("effect idx", 0, 1, KeyCode.PageUp, KeyCode.PageDown, delegate (int x)
                {
                    this._T2_ = x;
                }, () => this._T2_, XDebug.DebugInt.LogT.OnChange);
            }
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_ground</color>", 1f, 0.05f, KeyCode.Keypad2, KeyCode.Keypad1, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SMGround.Value = x;
            }, () => XDebug.CustomSpeedMultiplier.Ground, XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_air</color>", 1f, 0.05f, KeyCode.Keypad5, KeyCode.Keypad4, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SMAir.Value = x;
            }, () => XDebug.CustomSpeedMultiplier.Air, XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_spindash</color>", 1f, 0.05f, KeyCode.Keypad8, KeyCode.Keypad7, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SMSpindash.Value = x;
            }, () => XDebug.CustomSpeedMultiplier.Spindash, XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_flying</color>", 1f, 0.05f, KeyCode.Keypad9, KeyCode.Keypad6, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SMFly.Value = x;
            }, () => XDebug.CustomSpeedMultiplier.Flying, XDebug.DebugFloat.LogT.OnChange);
            new XDebug.DebugFloat("<color=#d214fc>cstm_spd_climbing</color>", 1f, 0.05f, KeyCode.Keypad3, KeyCode.KeypadPeriod, delegate (float x)
            {
                XSingleton<XDebug>.Instance.SMClimb.Value = x;
            }, () => XDebug.CustomSpeedMultiplier.Climbing, XDebug.DebugFloat.LogT.OnChange);
            XSingleton<XFiles>.Instance.Load();
            Debug.Log(Singleton<XModMenu>.Instance.name);
            Dictionary<string, object> settingsDict = this.GetSettingsDict();
            if (settingsDict != null && settingsDict["Load automatically"] != null && (bool)settingsDict["Load automatically"])
            {
                this.LoadSettings();
            }
            else
            {
                XDebug.Comment("to prevent random output from initalized xvalues");
                this.Box_EndTime = Time.time;
            }
            Singleton<XModMenu>.Instance.Menu.OnClose += delegate
            {
                if (XSingleton<XDebug>.Instance.Saving_AutoSave.Value)
                {
                    XSingleton<XDebug>.Instance.SaveSettings();
                }
            };
        }

        private void Update()
        {
            if (this.CanHandleInput())
            {
                this.HandleInput();
            }
            XDebug.Comment("================== Time slowdown ==================");
            if (Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.RightControl))
                {
                    this.RealFixedDeltaTime = Time.fixedDeltaTime;
                    Time.fixedDeltaTime /= 10f;
                }
                Time.timeScale = 0.1f;
            }
            else if (Input.GetKeyUp(KeyCode.RightControl))
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = this.RealFixedDeltaTime;
            }
            XDebug.Comment("================== buggy speedup ==================");
            if (Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Delete) || Input.GetKey(KeyCode.Insert) || Input.GetButton("Right Stick Button"))
            {
                Time.timeScale = 8f;
            }
            else if (Input.GetKeyUp(KeyCode.Keypad0) || Input.GetKeyUp(KeyCode.Delete) || Input.GetKeyUp(KeyCode.Insert) || Input.GetButtonUp("Right Stick Button"))
            {
                Time.timeScale = 1f;
            }
            if (Singleton<GameManager>.Instance.GameState == GameManager.State.Paused)
            {
                Time.timeScale = 0f;
            }
            XDebug.Comment("================== Log Box handling ==================");
            if (this.Box_GameObject != null && this.Box_GameObject.activeSelf && Time.time > this.Box_EndTime)
            {
                this.Box_GameObject.SetActive(false);
            }
            if (this.Extra_DisplaySpeedo.Value)
            {
                this.SpeedoLog();
            }
            else if (this.Speedo_GameObject != null && this.Speedo_GameObject.activeSelf)
            {
                this.Speedo_GameObject.SetActive(false);
            }
            if (this.BoxExtra != null && this.BoxExtra.GameObject.activeSelf && this.BoxExtra.HideTime < Time.time)
            {
                this.BoxExtra.GameObject.SetActive(false);
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
            {
                XDebug.DBG = !XDebug.DBG;
                if (XDebug.DBG)
                {
                    if (XSingleton<XFiles>.Instance.Particle == null)
                    {
                        XSingleton<XFiles>.Instance.Load();
                    }
                    this.Log("==DEBUG MODE <color=#00ee00>ENABLED</color>==", 6f, 18f);
                    this.LogExtra("Try pressing random keys on your kb ;P", 6f, 18f);
                    return;
                }
                this.Log("==DEBUG MODE <color=#ee0000>DISABLED</color>==", 2.5f, 18f);
            }
        }

        public XDebug()
        {
            this.SpeedMultiplier = 1f;
            this.Characters = new Tuple<string, int>[]
            {
            new Tuple<string, int>("sonic_new", 0),
            new Tuple<string, int>("shadow", 5),
            new Tuple<string, int>("silver", 1),
            new Tuple<string, int>("tails", 2),
            new Tuple<string, int>("knuckles", 3),
            new Tuple<string, int>("rouge", 7),
            new Tuple<string, int>("omega", 6),
            new Tuple<string, int>("amy", 9),
            new Tuple<string, int>("blaze", 8),
            new Tuple<string, int>("princess", 4),
            new Tuple<string, int>("snow_board", 0),
            new Tuple<string, int>("sonic_fast", 0)
            };
            this._T2_ = 11;
            this._color_ = Color.white;
            this._ae_idxs_ = new int[]
            {
            18, 26, 41, 43, 49, 50, 59, 75, 77, 80,
            107, 112, 119, 122, 125, 129
            };
            this._TMP_ = 0.3f;
            this.SpeedMultiplier = 1f;
            this.Box_EndTime = 9999f;
            this.SRs = new List<Renderer>();
            XDebug.Comment("OLD COLORS: this.LA_Color = new Color(0f, 5f, 40f, 0.4f);this.SC_Color = new Color(0f, 23f, 255f, 0.3f);");
            this.LRs = new List<LineRenderer>();
        }

        public UI HUD
        {
            get
            {
                if (this._HUD == null)
                {
                    this._HUD = UnityEngine.Object.FindObjectOfType<UI>();
                }
                return this._HUD;
            }
        }

        public PlayerBase Player
        {
            get
            {
                if (this._player == null)
                {
                    this._player = UnityEngine.Object.FindObjectOfType<PlayerBase>();
                }
                return this._player;
            }
        }

        private UnityEngine.Object RandomOf(UnityEngine.Object[] array)
        {
            if (array == null || array.Length == 0)
            {
                return null;
            }
            int num = UnityEngine.Random.Range(0, array.Length - 1);
            return array[num];
        }

        public void DrawVectorFast(Vector3 start, Vector3 end, Color color, int idx)
        {
            if (!XDebug.DBG)
            {
                return;
            }
            if (idx < 0)
            {
                return;
            }
            if (this.LRs.Count <= idx)
            {
                for (int i = this.LRs.Count; i <= idx; i++)
                {
                    LineRenderer lineRenderer = new GameObject(string.Format("LR wrapper {0}", idx)).AddComponent<LineRenderer>();
                    UnityEngine.Object.DontDestroyOnLoad(lineRenderer);
                    lineRenderer.material = new Material(Shader.Find("Standard"));
                    lineRenderer.widthMultiplier = 0.05f;
                    lineRenderer.enabled = false;
                    this.LRs.Add(lineRenderer);
                }
            }
            if (!this.LRs[idx].enabled)
            {
                this.LRs[idx].material.EnableKeyword("_EMISSION");
                this.LRs[idx].material.color = color;
                this.LRs[idx].material.SetVector("_EmissionColor", color * 2f);
                this.LRs[idx].enabled = true;
            }
            this.LRs[idx].SetPositions(new Vector3[] { start, end });
        }

        public void MessageLog(string message, string voiceName = null, float time = 4f)
        {
            if (this.HUD && this.Player)
            {
                this.HUD.StartMessageBox(new string[] { message }, new string[] { voiceName }, time);
            }
        }

        public SonicNew SonicNew
        {
            get
            {
                if (this._sonicNew == null)
                {
                    this._sonicNew = UnityEngine.Object.FindObjectOfType<SonicNew>();
                }
                return this._sonicNew;
            }
        }

        public static void Comment(string text)
        {
        }

        public void DrawSphereFast(Vector3 position, float radius, Color color, int idx)
        {
            if (!XDebug.DBG)
            {
                return;
            }
            if (idx < 0)
            {
                return;
            }
            if (this.SRs.Count <= idx)
            {
                for (int i = this.SRs.Count; i <= idx; i++)
                {
                    GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Collider>());
                    Renderer component = gameObject.GetComponent<Renderer>();
                    if (component.material == null)
                    {
                        component.material = new Material(Shader.Find("Standard"));
                    }
                    component.enabled = false;
                    this.SRs.Add(component);
                }
            }
            if (!this.SRs[idx].enabled)
            {
                this.SRs[idx].material.EnableKeyword("_EMISSION");
                this.SRs[idx].material.color = color;
                if (color.a >= 0.7f)
                {
                    this.SRs[idx].material.SetVector("_EmissionColor", color * 2f);
                    this.SRs[idx].enabled = true;
                }
            }
            this.SRs[idx].transform.position = position;
            this.SRs[idx].transform.localScale = Vector3.one * radius;
        }

        public void Dump(object obj)
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                Debug.Log("default obj");
            }
        }

        public SonicFast SonicFast
        {
            get
            {
                if (this._sonicFast == null)
                {
                    this._sonicFast = UnityEngine.Object.FindObjectOfType<SonicFast>();
                }
                return this._sonicFast;
            }
        }

        private AudioClip MakeSubclip(AudioClip clip, float start, float stop)
        {
            int frequency = clip.frequency;
            float num = stop - start;
            int num2 = (int)((float)frequency * num);
            AudioClip audioClip = AudioClip.Create(clip.name + "-sub", num2, 2, frequency, false);
            float[] array = new float[num2];
            clip.GetData(array, (int)((float)frequency * start));
            audioClip.SetData(array, 0);
            return audioClip;
        }

        public AudioClip DodgeClip
        {
            get
            {
                if (this._DodgeClip == null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/Objects/kdv/Rope") as GameObject);
                    AudioClip clipLow = gameObject.GetComponent<Rope>().ClipLow;
                    gameObject.SetActive(false);
                    this._DodgeClip = this.MakeSubclip(clipLow, 0f, 0.34f);
                }
                return this._DodgeClip;
            }
        }

        public AudioClip DodgeClipFull
        {
            get
            {
                if (this._DodgeClipFull == null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/Objects/kdv/Rope") as GameObject);
                    AudioClip clipLow = gameObject.GetComponent<Rope>().ClipLow;
                    gameObject.SetActive(false);
                    this._DodgeClipFull = clipLow;
                }
                return this._DodgeClipFull;
            }
        }

        private void Awake()
        {
            this.CreateXCanvas();
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            UnityEngine.Object.DontDestroyOnLoad(this);
            Application.runInBackground = true;
            XDebug.Comment("only here initializing XValues won't crash");
            this.InitializeXValues();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (XDebug.DBG)
            {
                Singleton<GameData>.Instance.Sonic.Lives = 993;
            }
            Time.timeScale = 1f;
            XDebug.Comment("Recalculate some scene-dependent variables...");
            this._sonicNew = null;
            this._sonicFast = null;
            this._HUD = null;
            this._player = null;
            if (this.ULTRA_FPS)
            {
                Time.fixedDeltaTime = 1f / (float)Screen.currentResolution.refreshRate;
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
            }
            else
            {
                Time.fixedDeltaTime = 0.016666668f;
                Application.targetFrameRate = 60;
            }
            XDebug.Comment("check mod files integrity");
            if (scene.name == "MainMenu" && XSingleton<XFiles>.Instance.Check())
            {
                XSingleton<XFiles>.Instance.Load();
            }
            XDebug.Comment("custom music check and update");
            if (this.PlayCustomMusic.Value)
            {
                this.StartCustomMusic();
            }
        }

        public void LB_SetAchors(Vector2 anchor)
        {
            this.Box_GameObject.GetComponent<RectTransform>().anchorMin = anchor;
            this.Box_GameObject.GetComponent<RectTransform>().anchorMax = anchor;
        }

        public void LB_Move(Vector3 vector)
        {
            Vector3 vector2 = (this.Box_Container.GetComponent<RectTransform>().localPosition += vector);
            this.MessageLog(string.Format("pos: {0}", vector2), null, 1.5f);
            this.Box_EndTime = Time.time + 1.5f;
        }

        public void Log(string message, float duration = 1.5f, float fontSize = 18f)
        {
            if (this.Box_GameObject == null)
            {
                this.Box_GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
                this.Box_GameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
                this.Box_GameObject.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
                this.Box_Script = this.Box_GameObject.GetComponent<MessageBox>();
                this.Box_Script.Set("Duration", 9999999f);
                XDebug.Comment("this.Box_Script.transform.SetParent(UnityEngine.Object.FindObjectOfType<Canvas>().transform, false);");
                XDebug.Comment("this.Box_Script.transform.SetParent(Singleton<XModMenu>.Instance.Canvas.transform, false);");
                this.Box_Script.transform.SetParent(this.XCanvas.transform, false);
                Vector2 vector = new Vector2(350f, 45f);
                Vector2 vector2 = new Vector2(325f, 30f);
                this.Box_Text = this.Box_GameObject.GetComponentInChildren<TextMeshProUGUI>();
                this.Box_Text.overflowMode = TextOverflowModes.Linked;
                this.Box_Text.alignment = TextAlignmentOptions.CenterGeoAligned;
                this.Box_Text.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                this.Box_Text.GetComponent<RectTransform>().localPosition = Vector3.zero;
                this.Box_Text.GetComponent<RectTransform>().sizeDelta = vector2;
                this.Box_Container = this.Box_GameObject.FindInChildren("Box");
                this.Box_Container.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                this.Box_Container.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                this.Box_Container.GetComponent<RectTransform>().sizeDelta = vector;
                if (SceneManager.GetActiveScene().name != "MainMenu")
                {
                    this.Box_Container.GetComponent<RectTransform>().localPosition = new Vector3(0f, -22.5f, 0f);
                }
                else
                {
                    this.Box_GameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 0.5f);
                    this.Box_GameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.5f);
                    this.Box_Container.GetComponent<RectTransform>().localPosition = new Vector3(-175f, -160f, 0f);
                }
            }
            this.Box_GameObject.SetActive(true);
            this.Box_Text.text = message;
            this.Box_Text.fontSize = fontSize;
            this.Box_EndTime = Time.time + duration;
        }

        public void XSwitchTo(string Who, int ID, PlayerBase CurrentPlayer)
        {
            Vector3 velocity = CurrentPlayer._Rigidbody.velocity;
            float curSpeed = CurrentPlayer.Get<float>("CurSpeed");
            PlayerBase component = (UnityEngine.Object.Instantiate(Resources.Load("DefaultPrefabs/Player/" + Who), CurrentPlayer.transform.position, CurrentPlayer.transform.rotation) as GameObject).GetComponent<PlayerBase>();
            component.SetPlayer(ID, Who);
            component._Rigidbody.velocity = velocity;
            component.Set("CurSpeed", curSpeed);
            component.StartPlayer(false);
            component.Get<UI>("HUD").UseCrosshair(true, false);
            if (CurrentPlayer.Get<bool>("HasShield"))
            {
                component.Set("HasShield", true);
                component.Set("ShieldObject", CurrentPlayer.Get<GameObject>("ShieldObject"));
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.position = component.transform.position + component.transform.up * ((!component.GetPrefab("omega")) ? 0.25f : 0.5f);
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.rotation = Quaternion.identity;
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.SetParent(component.transform);
                CurrentPlayer.Get<GameObject>("ShieldObject").transform.localScale = Vector3.one * ((!component.GetPrefab("omega")) ? 1f : 1.5f);
            }
            if (XDebug.DBG && this._T2_ != 11)
            {
                if (this._ae_ == null)
                {
                    this._ae_ = Resources.LoadAll<GameObject>("defaultprefabs/effect/");
                }
                GameObject fx2 = UnityEngine.Object.Instantiate<GameObject>(this._ae_[this._ae_idxs_[this._T2_]], XDebug.Finder<PlayerBase>.Instance.transform.position + this._fx_offset_, Quaternion.identity, component.gameObject.transform);
                this.Log(string.Format("idx: {0} ({1})", this._T2_, this._ae_[this._ae_idxs_[this._T2_]].name), 2f, 14f);
                this.Invoke(delegate
                {
                    UnityEngine.Object.Destroy(fx2);
                }, 2f);
            }
            else
            {
                GameObject fx = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("defaultprefabs/effect/player/shadow/SnapDashFX"), component.gameObject.transform.position + component.gameObject.transform.up * 0.15f, component.gameObject.transform.rotation, component.gameObject.transform);
                ParticleSystem[] componentsInChildren = fx.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    ParticleSystem.MainModule main = componentsInChildren[i].main;
                    main.startColor = XDebug.ColorConstants.SmoothSwitchOverride;
                    main.startColor = XDebug.ColorConstants.SmoothSwitchOverride;
                }
                this.Invoke(delegate
                {
                    UnityEngine.Object.Destroy(fx);
                }, 2f);
            }
            UnityEngine.Object.Destroy(CurrentPlayer.gameObject);
        }

        private void ResetLUA()
        {
            Assembly ass = Assembly.GetAssembly(typeof(SonicNew));

            ass.GetType("STHLua.Sonic_New_Lua").Set("c_run_speed_max", 25f);

            ass.GetType("STHLua.Sonic_New_Lua").Set("c_run_speed_max", 25f);
            ass.GetType("STHLua.Sonic_New_Lua").Set("c_jump_run", 13f);
            ass.GetType("STHLua.Sonic_New_Lua").Set("c_spindash_spd", 30f);
            ass.GetType("STHLua.Sonic_New_Lua").Set("c_speedup_speed_max", 42f);
            ass.GetType("STHLua.Sonic_Fast_Lua").Set("c_walk_speed_max", 55f);
            ass.GetType("STHLua.Sonic_Fast_Lua").Set("c_run_speed_max", 80f);
            ass.GetType("STHLua.Sonic_Fast_Lua").Set("c_lightdash_speed", 120f);
            ass.GetType("STHLua.Sonic_Fast_Lua").Set("c_lightdash_mid_speed", 90f);
            ass.GetType("STHLua.Sonic_Fast_Lua").Set("c_lightdash_mid_speed_super", 110f);
            ass.GetType("STHLua.Tails_Lua").Set("c_run_speed_max", 16.5f);
            ass.GetType("STHLua.Tails_Lua").Set("c_jump_run", 12f);
            ass.GetType("STHLua.Tails_Lua").Set("c_flight_speed_max", 17f);
            ass.GetType("STHLua.Tails_Lua").Set("c_speedup_speed_max", 25.5f);
            ass.GetType("STHLua.Shadow_Lua").Set("c_run_speed_max", 23f);
            ass.GetType("STHLua.Shadow_Lua").Set("c_speedup_speed_max", 38f);
            ass.GetType("STHLua.Shadow_Lua").Set("c_jump_run", 11f);
            ass.GetType("STHLua.Shadow_Lua").Set("c_spindash_spd", 28f);
            ass.GetType("STHLua.Knuckles_Lua").Set("c_climb_speed", 7f);
            ass.GetType("STHLua.Knuckles_Lua").Set("c_run_speed_max", 16.5f);
            ass.GetType("STHLua.Knuckles_Lua").Set("c_speedup_speed_max", 25.5f);
            ass.GetType("STHLua.Knuckles_Lua").Set("c_jump_run", 12f);
            ass.GetType("STHLua.Knuckles_Lua").Set("c_flight_speed_max", 17f);
            ass.GetType("STHLua.Omega_Lua").Set("c_run_speed_max", 20f);
            ass.GetType("STHLua.Omega_Lua").Set("l_jump_run", 16f);
            ass.GetType("STHLua.Omega_Lua").Set("c_speedup_speed_max", 30f);
            ass.GetType("STHLua.Princess_Lua").Set("c_run_speed_max", 25f);
            ass.GetType("STHLua.Princess_Lua").Set("c_speedup_speed_max", 42f);
            ass.GetType("STHLua.Princess_Lua").Set("c_jump_run", 13f);
            ass.GetType("STHLua.Rouge_Lua").Set("c_run_speed_max", 16.5f);
            ass.GetType("STHLua.Rouge_Lua").Set("c_speedup_speed_max", 25.5f);
            ass.GetType("STHLua.Rouge_Lua").Set("c_jump_run", 12f);
            ass.GetType("STHLua.Rouge_Lua").Set("c_flight_speed_max", 17f);
            ass.GetType("STHLua.Rouge_Lua").Set("c_climb_speed", 7f);
            ass.GetType("STHLua.Silver_Lua").Set("c_run_speed_max", 16.5f);
            ass.GetType("STHLua.Silver_Lua").Set("c_speedup_speed_max", 25.5f);
            ass.GetType("STHLua.Silver_Lua").Set("c_jump_run", 12f);
            ass.GetType("STHLua.Silver_Lua").Set("c_float_walk_speed", 18.5f);

            ass.GetType("STHLua.Shadow_Lua").Set("c_homing_spd", 40f);
            ass.GetType("STHLua.Sonic_New_Lua").Set("c_homing_spd", 40f);
            ass.GetType("STHLua.Princess_Lua").Set("c_homing_spd", 40f);
            //Sonic_New_Lua.c_homing_spd = (Shadow_Lua.c_homing_spd = (Princess_Lua.c_homing_spd = 40f));
        }

        private void LateUpdate()
        {
            if (XDebug.Cfg.Cheats.InfiniteGauge && XDebug.Finder<UI>.Instance)
            {
                XDebug.Finder<UI>.Instance.Set("ActionDisplay",
                    XDebug.Finder<UI>.Instance.Get<float>("MaxActionGauge"));
                XDebug.Finder<UI>.Instance.Set("ChaosMaturityDisplay",
                    XDebug.Finder<UI>.Instance.Get<float>("MaxActionGauge"));
            }
            if (XDebug.Cfg.Cheats.Immune && XDebug.Finder<PlayerBase>.Instance)
            {
                XDebug.Finder<PlayerBase>.Instance.Set("ImmunityTime", 1E+10f);
            }
            if (XDebug.Cfg.Cheats.InfiniteRings)
            {
                XDebug.Cheats.Rings = 999999999;
            }
        }

        public bool UltraFPS
        {
            get
            {
                return this.ULTRA_FPS;
            }
            set
            {
                this.ULTRA_FPS = value;
                if (this.ULTRA_FPS)
                {
                    Time.fixedDeltaTime = 1f / (float)Screen.currentResolution.refreshRate;
                    Application.targetFrameRate = Screen.currentResolution.refreshRate;
                    this.Log(string.Format("Ultra Smooth FPS <color=#00ee00>enabled</color>\ndelta is now {0}", Time.fixedDeltaTime), 4f, 18f);
                    this.RealFixedDeltaTime = Time.fixedDeltaTime;
                    return;
                }
                Time.fixedDeltaTime = 0.016666668f;
                Application.targetFrameRate = 60;
                this.Log(string.Format("Ultra Smooth FPS <color=#ee0000>disabled</color>\ndelta is now {0}", Time.fixedDeltaTime), 4f, 18f);
                this.RealFixedDeltaTime = Time.fixedDeltaTime;
            }
        }

        private void CreateXCanvas()
        {
            GameObject gameObject = new GameObject("xdebug log and mod menu canvas");
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            XCanvas = gameObject.AddComponent<Canvas>();
            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            this.XCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1280f, 720f);
            this.XCanvas.transform.position += new Vector3(0f, 0f, -99999f);
            this.XCanvas.sortingOrder = 11;
        }

        private void StartCustomMusic()
        {
            AudioSource musicSource = this.GetMusicSource();
            if (musicSource != null && XSingleton<XFiles>.Instance.Gandalf != null)
            {
                musicSource.Stop();
                this.backupClip = musicSource.clip;
                musicSource.clip = XSingleton<XFiles>.Instance.Gandalf;
                musicSource.Play();
                return;
            }
        }

        private AudioSource GetMusicSource()
        {
            AudioSource audioSource = null;
            if (XDebug.Finder<StageManager>.Instance)
            {
                audioSource = XDebug.Finder<StageManager>.Instance.Get<AudioSource>("BGMPlayer");
            }
            else if (XDebug.Finder<MainMenu>.Instance)
            {
                audioSource = XDebug.Finder<MainMenu>.Instance.Music;
            }
            else if (XDebug.Finder<TitleScreen>.Instance)
            {
                audioSource = XDebug.Finder<TitleScreen>.Instance.MusicAudio;
            }
            return audioSource;
        }

        private void StartNormalMusic()
        {
            AudioSource musicSource = this.GetMusicSource();
            if (musicSource != null)
            {
                musicSource.Stop();
                if (musicSource.clip == XSingleton<XFiles>.Instance.Gandalf)
                {
                    musicSource.clip = this.backupClip;
                }
                musicSource.Play();
            }
        }

        private void InitializeXValues()
        {
            for (int i = 0; i < this.dbg_toggles.Length; i++)
            {
                this.dbg_toggles[i] = new XValue<bool>(delegate (bool on)
                {
                }, false);
            }
            for (int j = 0; j < this.dbg_floats.Length; j++)
            {
                this.dbg_floats[j] = new XValue<float>(delegate (float x)
                {
                }, 1f);
            }
            this.tmp0 = new XValue<float>(delegate (float x)
            {
            }, 0.6f);
            this.tmp1 = new XValue<float>(delegate (float x)
            {
            }, -0.25f);
            this.tmp2 = new XValue<float>(delegate (float x)
            {
            }, -0.1f);
            this.Saving_AutoLoad = new XValue<bool>(delegate (bool on)
            {
            }, true);
            this.Saving_AutoSave = new XValue<bool>(delegate (bool on)
            {
            }, true);
            this.UltraSmoothFPS = new XValue<bool>(delegate (bool on)
            {
                XSingleton<XDebug>.Instance.UltraFPS = on;
            }, false);
            this.Extra_DisplaySpeedo = new XValue<bool>(delegate (bool on)
            {
                if (on)
                {
                    this.Log("Speed is now <color=#00ee00>displayed</color>", 1.5f, 18f);
                    return;
                }
                this.Log("Speed is <color=#ee0000>not displayed</color>", 1.5f, 18f);
            }, true);
            this.Invincible = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.Immune = on;
                if (XDebug.Cfg.Cheats.Immune)
                {
                    this.Log("Infinite immunity <color=#00ee00>enabled</color>", 1.5f, 18f);
                    return;
                }
                this.Log("Infinite immunity <color=#ee0000>disabled</color>", 1.5f, 18f);
                if (XDebug.Finder<PlayerBase>.Instance)
                {
                    XDebug.Comment("If there was a immune player then disable its immunity");
                    XDebug.Finder<PlayerBase>.Instance.Set("ImmunityTime", Time.time);
                }
            }, false);
            this.InfiniteGauge = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.InfiniteGauge = on;
                if (XDebug.Cfg.Cheats.InfiniteGauge)
                {
                    this.Log("Action gauge is now <color=#8f07f0>infinite</color>", 1.5f, 16f);
                    return;
                }
                this.Log("Action gauge is now normal", 1.5f, 16f);
            }, false);
            this.PlayCustomMusic = new XValue<bool>(delegate (bool playcustom)
            {
                if (playcustom)
                {
                    XSingleton<XDebug>.Instance.StartCustomMusic();
                    this.Log("Playing <color=#ffb536>custom</color> music...", 1.5f, 18f);
                    return;
                }
                XSingleton<XDebug>.Instance.StartNormalMusic();
                this.Log("Playing <color=#3dcfff>original</color> music...", 1.5f, 18f);
            }, false);
            this.MaxedOutGems = new XValue<bool>(delegate (bool on)
            {
                if (!on)
                {
                    if (XDebug.Finder<UI>.Instance && XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel") != null)
                    {
                        if (this.MaxedOutGems_Data != null)
                        {
                            int[] agl = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                            this.MaxedOutGems_Data.CopyTo(agl, 0);
                            XDebug.Comment("[investigate] ^^^^^^^^ ");
                            XDebug.Comment("XDebug.Finder<UI>.Instance.ActiveGemLevel = MaxedOutGems_Data.Clone() as int[];");
                        }
                        else
                        {
                            var agl = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                            for (int i = 0; i < agl.Length; i++)
                            {
                                agl[i] = 0;
                            }
                            //Array.Fill<int>(XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel"), 0);
                        }
                    }
                }
                else
                {
                    if (XDebug.Finder<UI>.Instance == null || XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel") == null)
                    {
                        this.Log("Error: unable to set gem levels currently", 3f, 14f);
                        XSingleton<XDebug>.Instance.MaxedOutGems.Value = false;
                        return;
                    }
                    if (!XDebug.Cfg.Cheats.MaxedOutGems)
                    {
                        XDebug.Comment("make a copy");
                        this.MaxedOutGems_Data = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel").Clone() as int[];
                    }
                    for (int k = 0; k < 9; k++)
                    {
                        XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[k] = 2;
                    }
                    XSingleton<XDebug>.Instance.Log("All gems <color=#de921f>maxed out</color>", 1.5f, 18f);
                }
                XDebug.Cfg.Cheats.MaxedOutGems = on;
            }, false);
            this.InfiniteLives = new XValue<bool>(delegate (bool on)
            {
                int num = 9999999;
                if (on)
                {
                    XDebug.Cheats.Lives += num;
                    return;
                }
                XDebug.Cheats.Lives = Math.Max(XDebug.Cheats.Lives - num, 3);
            }, false);
            this.InfiniteRings = new XValue<bool>(delegate (bool on)
            {
                XDebug.Cfg.Cheats.InfiniteRings = on;
                if (!on)
                {
                    XDebug.Cheats.Rings = 50;
                }
            }, false);
            this.Cheat_ChainJumpZeroDelay = new XValue<bool>(delegate (bool on)
            {
                this.Log("Zero delay for chain jump " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, false);
            this.Cheat_IgnoreWaterDeath = new XValue<bool>(delegate (bool on)
            {
                this.Log("Water immunity " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, false);
            this.Moveset_FreeWaterSliding = new XValue<bool>(delegate (bool on)
            {
                this.Log("Free Water Sliding" + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_WallJumping = new XValue<bool>(delegate (bool on)
            {
                this.Log("Wall Jump " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_ClimbAll = new XValue<bool>(delegate (bool on)
            {
                this.Log("Climb All (for K&R) " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_Boost = new XValue<bool>(delegate (bool on)
            {
                this.Log("Boost :X " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_AHMovement = new XValue<bool>(delegate (bool on)
            {
                this.Log("After Homing Movement " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Moveset_AHMovementMaxSpeed = new XValue<float>(delegate (float value)
            {
                this.Log("Max After Hom. Speed: " + value.ToString("0.00"), 1.5f, 16f);
            }, 10f);
            this.TeleportLocation = new XValue<string>(delegate (string s)
            {
            }, "test_b_sn");
            this.ASCSpinClamp = new XValue<bool>(delegate (bool on)
            {
                XDebug.IMMEDIATE_SPINDASH_CLAMP = on;
                this.Log(string.Format("<color=#fc9b14>Immediate_Spindash_Clamp</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.LogExtra("It's useful if you wanna increase ground spd, but leave spindash on normal\ncause on deafult spindash spd is clamped to 2 * MaxSpeed...", 5f, 0f);
            }, true);
            this.ASCLuaRecalc = new XValue<bool>(delegate (bool on)
            {
                this.Log(string.Format("<color=#fc9b14>Lua_recalc</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.LogExtra("If enabled, then other speed realated values will be updated (e.g. acceleration)", 3.14f, 0f);
            }, true);
            this.Other_OgCameraControls = new XValue<bool>(delegate (bool on)
            {
                if (on)
                {
                    this.Log("Using LB/RB as <color=#fc9b14>camera controls</color>.\nTo dodge, press the LStick and then LB/RB.", 3f, 12.5f);
                    return;
                }
                this.Log("Using LB/RB as <color=#fc9b14>dodge controls</color> (default).", 2f, 12.5f);
            }, true);
            this.Other_UltraFPSFix = new XValue<bool>(delegate (bool on)
            {
                this.Log("UltraFPS Fix " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.Other_CheckP06Version = new XValue<bool>(delegate (bool on)
            {
                this.Log("Check P-06 Version " + (on ? "<color=#00ee00>enabled</color>" : "<color=#ee0000>disabled</color>"), 1.5f, 16f);
            }, true);
            this.SMGround = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Ground = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_ground</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMAir = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Air = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_air</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMSpindash = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Spindash = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_spindash</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMFly = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Flying = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_flying</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMClimb = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Climbing = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_climbing</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMHoming = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.Homing = x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.UpdateLUA();
                this.Log(string.Format("<color=#d214fc>cstm_spd_homing</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMHomingAttackFasterBy = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.HomingAttackTimeShortener = 1f / x;
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.Log(string.Format("<color=#d214fc>homing_attack_faster_by</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.SMAfterHomingRotation = new XValue<float>(delegate (float x)
            {
                XDebug.CustomSpeedMultiplier.AfterHomingRotationSpeed = x * 0.75f * ReflectionExtensions.GetLuaStruct("Sonic_New_Lua").Get<float>("c_rotation_speed");
                XDebug.USING_CUSTOM_SPEEDS = true;
                this.Log(string.Format("<color=#d214fc>after_homing_rotation</color> = {0}", x), 1.25f, 16f);
            }, 1f);
            this.EverySpeedMultiplier = new XValue<float>(delegate (float mul)
            {
                this.SpeedMultiplier = mul;
                this.SMGround.Value = mul;
                this.SMAir.Value = mul;
                this.SMSpindash.Value = mul;
                this.SMFly.Value = mul;
                this.SMClimb.Value = mul;
                this.SMHoming.Value = mul;
                this.SMHomingAttackFasterBy.Value = mul;
                this.SMAfterHomingRotation.Value = mul;
                this.Log(string.Format("<color=#ff9500>Speed Multiplier</color>: {0}", this.SpeedMultiplier.ToString("0.000")), 1.5f, 18f);
                this.UpdateLUA();
                XDebug.USING_CUSTOM_SPEEDS = false;
            }, 1f);
            this.Boost_BaseSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("<color=#ff9500>Base Boost Speed</color>: {0}", spd.ToString("0.00")), 1.5f, 16f);
            }, 40f);
            this.Boost_NextLevelDeltaSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Boost levels will increase speed by: {0}", spd.ToString("0.00")), 1.5f, 14f);
            }, 20f);
            this.Boost_RotSpeed = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Rotation speed when boosting: {0}", spd.ToString("0.00")), 1.5f, 14f);
            }, 3f);
            this.Boost_AccelTime = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Boost will accelerate to target speed in {0}s", spd.ToString("0.00")), 1.5f, 14f);
            }, 0.5f);
            this.Boost_NextLevelThreshold = new XValue<float>(delegate (float spd)
            {
                this.Log(string.Format("Faster boost when speed it at most {0} less than current level target", spd.ToString("0.00")), 1.5f, 14f);
            }, 5f);
            this.BoxExtra.GameObject.SetActive(false);
        }

        public void TeleportToSection(string section)
        {
            if (section == null || section.Length <= 0)
            {
                this.Log("Couldn't teleport to: <color=#8f0000>" + section + "</color>", 5f, 18f);
                return;
            }
            section = section.Trim();
            if (SceneUtility.GetBuildIndexByScenePath(section) != -1)
            {
                this.Log("Switching to: <color=#8f07f0>" + section + "</color>", 3.5f, 18f);
                Game.ChangeArea(section, "");
                return;
            }
            this.Log("<color=#ee0000>Section</color> \"" + section + "\" <color=#ee0000>doesn't exist</color>!", 3f, 16f);
        }

        private void UpdateLUA()
        {
            XDebug.Comment("[pending]");
            /*
            this.ResetLUA();
            Sonic_New_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Sonic_New_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Sonic_Fast_Lua.c_walk_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Sonic_Fast_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Tails_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Tails_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Shadow_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Shadow_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Knuckles_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Knuckles_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Omega_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Omega_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Princess_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Princess_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Rouge_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Rouge_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Silver_Lua.c_run_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Silver_Lua.c_speedup_speed_max *= XDebug.CustomSpeedMultiplier.Ground;
            Silver_Lua.c_float_walk_speed *= XDebug.CustomSpeedMultiplier.Ground;
            Sonic_New_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Sonic_Fast_Lua.c_lightdash_speed *= XDebug.CustomSpeedMultiplier.Air;
            Sonic_Fast_Lua.c_lightdash_mid_speed *= XDebug.CustomSpeedMultiplier.Air;
            Sonic_Fast_Lua.c_lightdash_mid_speed_super *= XDebug.CustomSpeedMultiplier.Air;
            Tails_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Shadow_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Knuckles_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Omega_Lua.l_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Princess_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Rouge_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Silver_Lua.c_jump_run *= XDebug.CustomSpeedMultiplier.Air;
            Sonic_New_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Spindash;
            Shadow_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Spindash;
            Tails_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
            Shadow_Lua.c_spindash_spd *= XDebug.CustomSpeedMultiplier.Flying;
            Knuckles_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
            Rouge_Lua.c_flight_speed_max *= XDebug.CustomSpeedMultiplier.Flying;
            Knuckles_Lua.c_climb_speed *= XDebug.CustomSpeedMultiplier.Climbing;
            Rouge_Lua.c_climb_speed *= XDebug.CustomSpeedMultiplier.Climbing;
            Sonic_New_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
            Shadow_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
            Princess_Lua.c_homing_spd *= XDebug.CustomSpeedMultiplier.Homing;
            if (this.ASCLuaRecalc.Value)
            {
                this.RecalcLua();
            }
            */
        }

        private void RecalcLua()
        {
            /*
            Sonic_New_Lua.c_run_acc = (Sonic_New_Lua.c_run_speed_max - Sonic_New_Lua.c_walk_speed_max) / Sonic_New_Lua.l_run_acc;
            Sonic_New_Lua.c_speedup_acc = (Sonic_New_Lua.c_speedup_speed_max - Sonic_New_Lua.c_walk_speed_max) / Sonic_New_Lua.l_speedup_acc;
            Sonic_New_Lua.c_bound_jump_spd_0 = Common_Lua.HeightToSpeed(Sonic_New_Lua.l_bound_jump_height0);
            Sonic_New_Lua.c_bound_jump_spd_1 = Common_Lua.HeightToSpeed(Sonic_New_Lua.l_bound_jump_height1);
            Sonic_New_Lua.c_homing_brake = (Sonic_New_Lua.c_homing_spd - Sonic_New_Lua.c_jump_run_orig) / Sonic_New_Lua.c_homing_time;
            Sonic_Fast_Lua.c_run_acc = (Sonic_Fast_Lua.c_run_speed_max - Sonic_Fast_Lua.c_walk_speed_max) / Sonic_Fast_Lua.l_run_acc;
            Knuckles_Lua.c_run_acc = (Knuckles_Lua.c_run_speed_max - Knuckles_Lua.c_walk_speed_max) / Knuckles_Lua.l_run_acc;
            Knuckles_Lua.c_speedup_acc = (Knuckles_Lua.c_speedup_speed_max - Knuckles_Lua.c_walk_speed_max) / Knuckles_Lua.l_speedup_acc;
            Omega_Lua.c_run_acc = (Omega_Lua.c_run_speed_max - Omega_Lua.c_walk_speed_max) / Omega_Lua.l_run_acc;
            Omega_Lua.c_jump_walk = Omega_Lua.l_jump_walk / (2f * Mathf.Sqrt(2f * Omega_Lua.l_jump_hight / 9.81f));
            Omega_Lua.c_jump_run = Omega_Lua.l_jump_run / (2f * Mathf.Sqrt(2f * Omega_Lua.l_jump_hight / 9.81f));
            Omega_Lua.c_speedup_acc = (Omega_Lua.c_speedup_speed_max - Omega_Lua.c_walk_speed_max) / Omega_Lua.l_speedup_acc;
            Princess_Lua.c_run_acc = (Princess_Lua.c_run_speed_max - Princess_Lua.c_walk_speed_max) / Princess_Lua.l_run_acc;
            Princess_Lua.c_jump_walk = Common_Lua.HeightAndDistanceToSpeed(Princess_Lua.l_jump_walk, Princess_Lua.l_jump_hight);
            Princess_Lua.c_speedup_acc = (Princess_Lua.c_speedup_speed_max - Princess_Lua.c_walk_speed_max) / Princess_Lua.l_speedup_acc;
            Princess_Lua.c_homing_brake = (Princess_Lua.c_homing_spd - Princess_Lua.c_jump_run_orig) / Princess_Lua.c_homing_time;
            Rouge_Lua.c_run_acc = (Rouge_Lua.c_run_speed_max - Rouge_Lua.c_walk_speed_max) / Rouge_Lua.l_run_acc;
            Rouge_Lua.c_speedup_acc = (Rouge_Lua.c_speedup_speed_max - Rouge_Lua.c_walk_speed_max) / Rouge_Lua.l_speedup_acc;
            Shadow_Lua.c_run_acc = (Shadow_Lua.c_run_speed_max - Shadow_Lua.c_walk_speed_max) / Shadow_Lua.l_run_acc;
            Shadow_Lua.c_speedup_acc = (Shadow_Lua.c_speedup_speed_max - Shadow_Lua.c_walk_speed_max) / Shadow_Lua.l_speedup_acc;
            Shadow_Lua.c_homing_brake = (Shadow_Lua.c_homing_spd - Shadow_Lua.c_jump_run_orig) / Shadow_Lua.c_homing_time;
            Silver_Lua.c_run_acc = (Silver_Lua.c_run_speed_max - Silver_Lua.c_walk_speed_max) / Silver_Lua.l_run_acc;
            Silver_Lua.c_speedup_acc = (Silver_Lua.c_speedup_speed_max - Silver_Lua.c_walk_speed_max) / Silver_Lua.l_speedup_acc;
            Silver_Lua.c_tele_dash_speed = Silver_Lua.l_tele_dash / Silver_Lua.c_tele_dash_time;
            Silver_Lua.c_psi_gauge_catch_ride = Silver_Lua.psi_power / Silver_Lua.l_psi_gauge_catch_ride;
            Silver_Lua.c_psi_gauge_float = Silver_Lua.psi_power / (Silver_Lua.l_psi_gauge_float / (Silver_Lua.c_float_walk_speed / 1.85f));
            Silver_Lua.c_psi_gauge_teleport_dash_burn = Silver_Lua.psi_power / (Silver_Lua.l_psi_gauge_float / (Silver_Lua.c_float_walk_speed / 2f));
            Sonic_Fast_Lua.c_run_acc = (Sonic_Fast_Lua.c_run_speed_max - Sonic_Fast_Lua.c_walk_speed_max) / Sonic_Fast_Lua.l_run_acc;
            Tails_Lua.c_run_acc = (Tails_Lua.c_run_speed_max - Tails_Lua.c_walk_speed_max) / Tails_Lua.l_run_acc;
            Tails_Lua.c_speedup_acc = (Tails_Lua.c_speedup_speed_max - Tails_Lua.c_walk_speed_max) / Tails_Lua.l_speedup_acc;
            */
        }

        public void LoadSettings()
        {
            Dictionary<string, object> settingsDict = this.GetSettingsDict();
            if (settingsDict == null)
            {
                this.Log("user_settings.xml file <color=#ee0000>not found</color>!", 1.5f, 18f);
                return;
            }
            this.Dump(settingsDict);
            foreach (XUISection xuisection in Singleton<XModMenu>.Instance.Menu.Sections)
            {
                if (settingsDict.ContainsKey("[is_collapsed]" + xuisection.gameObject.name))
                {
                    xuisection.Toggle(!(bool)settingsDict["[is_collapsed]" + xuisection.gameObject.name]);
                }
                foreach (XUIItem xuiitem in xuisection.Items)
                {
                    if (settingsDict.ContainsKey(xuiitem.Name))
                    {
                        XUIToggleButton xuitoggleButton;
                        XUIFloatAdjuster xuifloatAdjuster;
                        XUIStringInput xuistringInput;
                        if ((xuitoggleButton = xuiitem as XUIToggleButton) != null)
                        {
                            xuitoggleButton.BindedXValue.Value = (bool)settingsDict[xuiitem.Name];
                        }
                        else if ((xuifloatAdjuster = xuiitem as XUIFloatAdjuster) != null)
                        {
                            xuifloatAdjuster.BindedXValue.Value = (float)settingsDict[xuiitem.Name];
                        }
                        else if ((xuistringInput = xuiitem as XUIStringInput) != null)
                        {
                            xuistringInput.BindedXValue.Value = (string)settingsDict[xuiitem.Name];
                        }
                    }
                }
            }
            this.Log("Settings <color=#00ee00>loaded</color>", 1.5f, 18f);
        }

        public void SaveSettings()
        {
            XUIMenu menu = Singleton<XModMenu>.Instance.Menu;
            List<ValueTuple<string, object>> list = new List<ValueTuple<string, object>>();
            foreach (XUISection xuisection in menu.Sections)
            {
                list.Add(new ValueTuple<string, object>("[is_collapsed]" + xuisection.gameObject.name, xuisection.IsCollapsed));
                foreach (XUIItem xuiitem in xuisection.Items)
                {
                    XDebug.Comment("xD i know it's bad design, but i didn't want to change xui classes cause they decompile really badly");
                    XUIToggleButton xuitoggleButton;
                    XUIFloatAdjuster xuifloatAdjuster;
                    XUIStringInput xuistringInput;
                    if ((xuitoggleButton = xuiitem as XUIToggleButton) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuitoggleButton.State));
                    }
                    else if ((xuifloatAdjuster = xuiitem as XUIFloatAdjuster) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuifloatAdjuster.Value));
                    }
                    else if ((xuistringInput = xuiitem as XUIStringInput) != null)
                    {
                        list.Add(new ValueTuple<string, object>(xuiitem.Name, xuistringInput.Value));
                    }
                }
            }
            XUtility.SerializeObjectToXml<List<ValueTuple<string, object>>>(list, Application.dataPath + "\\mods\\user_settings.xml");
            this.Dump(list);
            this.Log("Settings <color=#00ee00>saved</color>", 1.5f, 18f);
        }

        private void HandleInput()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKey(KeyCode.F1) && Time.time - this._ringsPrevClick > 0.125f)
                {
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        XDebug.Cheats.Rings += 50;
                        XDebug.Comment("if was just pressed then wait a little more");
                        this._ringsPrevClick = Time.time + 0.75f;
                        this.Log("50 rings <color=#00ee00>added!</color>", 0.7f, 16f);
                    }
                    else
                    {
                        XDebug.Cheats.Rings += 51;
                        this._ringsPrevClick = Time.time;
                        this.Log("Now adding moooooooooooore......", 0.5f, 16f);
                    }
                    XDebug.Finder<PlayerBase>.Instance.Audio.PlayOneShot(XSingleton<XFiles>.Instance.RingSound);
                }
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    XDebug.Cheats.Lives += 100;
                    this.Log("<color=#00ee00>+100</color> Lives!", 0.5f, 16f);
                }
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    this.Invincible.Value = !this.Invincible.Value;
                }
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    string[] array = File.ReadAllLines(Application.dataPath + "/mods/next_area.txt");
                    this.TeleportToSection(array[0]);
                }
                if (Input.GetKeyDown(KeyCode.F4))
                {
                    this.InfiniteGauge.Value = !this.InfiniteGauge.Value;
                }
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    XDebug.Cheats.CurrentGemLevel++;
                    this.Log(string.Format("Active gem level: {0}", XDebug.Cheats.CurrentGemLevel + 1), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    XSingleton<XDebug>.Instance.MaxedOutGems.Value = true;
                }
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    XDebug.Cheats.GetAllGems();
                }
                if (Input.GetKeyDown(KeyCode.F9))
                {
                    this.LoadSettings();
                }
                else if (Input.GetKeyDown(KeyCode.F10))
                {
                    this.SaveSettings();
                }
            }
            else
            {
                XDebug.Comment("================== Ultra FPS (experimental) ==================");
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    this.UltraSmoothFPS.Value = !this.UltraSmoothFPS.Value;
                }
                XDebug.Comment("================== Speed Override ==================");
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    this.EverySpeedMultiplier.Value -= 0.025f;
                }
                else if (Input.GetKeyDown(KeyCode.F3))
                {
                    this.EverySpeedMultiplier.Value += 0.05f;
                }
                XDebug.Comment("================== Custom BGM ==================");
                if (Input.GetKeyDown(KeyCode.F11))
                {
                    this.PlayCustomMusic.Value = false;
                }
                if (Input.GetKeyDown(KeyCode.F10))
                {
                    this.PlayCustomMusic.Value = true;
                }
                XDebug.Comment("================== Info ==================");
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    this.Log(string.Format("tgt: {0}FPS, fdt: {1}s", Application.targetFrameRate, Time.fixedDeltaTime.ToString("0.0000")), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.F7))
                {
                    this.Log("Sonic P-06<color=#00ee00>X</color> " + XDebug.P06X_VERSION + " [for Version 4.6]", 3f, 14f);
                }
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    this.Log("log seems to be working", 1.5f, 18f);
                }
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                XDebug.Comment("whatever");
                this.LogExtra("sgflkdsjf dfk sjdlkfj sdf fd sdjlkjlkjl kjsdf kjlk jwlekjr", 1.5f, 0f);
            }
            XDebug.Comment("Load/Save Settings");
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                XDebug.IMMEDIATE_SPINDASH_CLAMP = !XDebug.IMMEDIATE_SPINDASH_CLAMP;
                this.Log(string.Format("<color=#fc9b14>Immediate_Spindash_Clamp</color> = {0} (default: true)", XDebug.IMMEDIATE_SPINDASH_CLAMP), 1.5f, 16f);
                this.Invoke(delegate
                {
                    this.Log("It's useful if you wanna increase ground spd, but leave spindash on normal", 3f, 14f);
                }, 1.75f);
                this.Invoke(delegate
                {
                    this.Log("cause on deafult spindash spd is clamped to 2 * MaxSpeed...", 2.5f, 14f);
                }, 4.75f);
            }
            XDebug.Comment("================== Char Switch ==================");
            if (Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetButton("Left Trigger"))
            {
                bool flag = false;
                if (this.PrevDPadX == 0f && Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X") != 0f)
                {
                    this.CurrentCharIdx = 0;
                    for (int i = 0; i < this.Characters.Length; i++)
                    {
                        if (this.Characters[i].Item1 == this.Player.PlayerPrefab.ToString())
                        {
                            this.CurrentCharIdx = i;
                            break;
                        }
                    }
                    this.CurrentCharIdx = (this.CurrentCharIdx + ((Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X") > 0f) ? 1 : (this.Characters.Length - 1))) % this.Characters.Length;
                    flag = true;
                }
                if (flag)
                {
                    this.JustUsedLeftTrigger = true;
                    this.XSwitchTo(this.Characters[this.CurrentCharIdx].Item1, this.Characters[this.CurrentCharIdx].Item2, XDebug.Finder<PlayerBase>.Instance);
                }
            }
            else if (!Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetButtonUp("Left Trigger"))
            {
                this.JustUsedLeftTrigger = false;
            }
            this.PrevDPadX = Singleton<RInput>.Instance.Get<Rewired.Player>("P").GetAxis("D-Pad X");
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                this.XSwitchTo("sonic_new", 0, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                this.XSwitchTo("shadow", 5, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                this.XSwitchTo("silver", 1, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                this.XSwitchTo("tails", 2, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                this.XSwitchTo("knuckles", 3, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                this.XSwitchTo("rouge", 7, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 5;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                this.XSwitchTo("omega", 6, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 6;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                this.XSwitchTo("sonic_fast", 0, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                this.XSwitchTo("princess", 4, XDebug.Finder<PlayerBase>.Instance);
                this.CurrentCharIdx = 7;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                this.XSwitchTo("snow_board", 0, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.Underscore))
            {
                this.XSwitchTo("amy", 9, XDebug.Finder<PlayerBase>.Instance);
            }
            else if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.Equals))
            {
                this.XSwitchTo("blaze", 8, XDebug.Finder<PlayerBase>.Instance);
            }
            XDebug.Comment("================== DEBUG MODE ==================");
            if (XDebug.DBG)
            {
                if (XDebug.COMMENT && Input.GetKeyDown(KeyCode.Keypad5))
                {
                    XDebug.SWITCH = !XDebug.SWITCH;
                    this.Log(string.Format("switch: {0}", XDebug.SWITCH), 1.5f, 18f);
                }
                if (XDebug.COMMENT)
                {
                    XDebug.Comment("this was controlling the position of the logbox");
                    Vector3 vector = Vector3.zero;
                    if (Input.GetKey(KeyCode.Keypad4))
                    {
                        vector += new Vector3(-5f, 0f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad6))
                    {
                        vector += new Vector3(5f, 0f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad8))
                    {
                        vector += new Vector3(0f, 2.5f, 0f);
                    }
                    if (Input.GetKey(KeyCode.Keypad2))
                    {
                        vector += new Vector3(0f, -2.5f, 0f);
                    }
                    if (vector != Vector3.zero && Time.time >= this.LB_NextMoveTime)
                    {
                        this.LB_Move(vector);
                        this.LB_NextMoveTime = Time.time + 0.02f;
                    }
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        this.Box_GameObject.transform.SetParent(null, true);
                    }
                }
                if (Input.GetKeyDown(KeyCode.Backslash))
                {
                    Game.ChangeArea("csc_b_sn", "");
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    this.Dump(Directory.GetParent(Application.dataPath).GetFiles("*sonic*.exe")[0].LastWriteTimeUtc);
                    this.Log("ticks: " + Directory.GetParent(Application.dataPath).GetFiles("*sonic*.exe")[0].LastWriteTimeUtc.Ticks.ToString(), 1.5f, 18f);
                }
                if (Input.GetKeyDown(KeyCode.M))
                {
                    string text = "<color=#28DD00>BGM</color> paused...";
                    XDebug.Finder<StageManager>.Instance.Get<AudioSource>("BGMPlayer").Stop();
                    string text2 = "all01_v12_bz";
                    this.MessageLog(text, text2, 4f);
                }
                XDebug.Comment("Life hack");
                if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    Singleton<GameData>.Instance.Sonic.Lives = 993;
                    this.HUD.Set<int>("Lives", 993);
                }
                if (Input.GetKeyDown(KeyCode.BackQuote))
                {
                    this.Box_GameObject.SetActive(!this.Box_GameObject.activeSelf);
                }
                if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    if (this.SonicNew)
                    {
                        this.SonicNew.SonicEffects.PM.sonic.Set<bool>("IsSuper", !this.SonicNew.SonicEffects.PM.sonic.Get<bool>("IsSuper"));
                    }
                    if (this.SonicFast)
                    {
                        this.SonicFast.SonicFastEffects.PM.sonic_fast.Set<bool>("IsSuper", !this.SonicFast.SonicFastEffects.PM.sonic_fast.Get<bool>("IsSuper"));
                    }
                }
            }
        }

        public Dictionary<string, object> GetSettingsDict()
        {
            if (!File.Exists(Application.dataPath + "\\mods\\user_settings.xml"))
            {
                return null;
            }
            List<ValueTuple<string, object>> list = XUtility.DeserializeXmlToObject<List<ValueTuple<string, object>>>(Application.dataPath + "\\mods\\user_settings.xml");
            if (list != null)
            {
                this.Dump(list);
                Dictionary<string, object> dictionary = list.ToDictionary((ValueTuple<string, object> x) => x.Item1, (ValueTuple<string, object> x) => x.Item2);
                this.Dump(dictionary);
                return dictionary;
            }
            return null;
        }

        public bool CanHandleInput()
        {
            GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            return currentSelectedGameObject == null || !currentSelectedGameObject.active || currentSelectedGameObject.GetComponent<TMP_InputField>() == null;
        }

        public void SpeedoLog()
        {
            if (this.Speedo_GameObject == null)
            {
                this.Speedo_GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
                this.Speedo_GameObject_Rect = this.Speedo_GameObject.GetComponent<RectTransform>();
                MessageBox component = this.Speedo_GameObject.GetComponent<MessageBox>();
                component.Set("Duration", 9999999f);
                component.transform.SetParent(this.XCanvas.transform, false);
                Vector2 vector = new Vector2(175f, 45f);
                Vector2 vector2 = new Vector2(150f, 30f);
                this.Speedo_Text = this.Speedo_GameObject.GetComponentInChildren<TextMeshProUGUI>();
                this.Speedo_Text.overflowMode = TextOverflowModes.Linked;
                this.Speedo_Text.alignment = TextAlignmentOptions.MidlineRight;
                this.Speedo_Text.fontSize = 16f;
                this.Speedo_Text_Rect = this.Speedo_Text.GetComponent<RectTransform>();
                this.Speedo_Text_Rect.anchorMin = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.anchorMax = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.pivot = new Vector2(0.5f, 0.5f);
                this.Speedo_Text_Rect.localPosition = Vector3.zero;
                this.Speedo_Text_Rect.sizeDelta = vector2;
                this.Speedo_GameObject_Rect.anchorMin = new Vector2(1f, 0f);
                this.Speedo_GameObject_Rect.anchorMax = new Vector2(1f, 0f);
                this.Speedo_Container_Rect = this.Speedo_GameObject.FindInChildren("Box").GetComponent<RectTransform>();
                this.Speedo_Container_Rect.anchorMin = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.anchorMax = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.pivot = new Vector2(1f, 0f);
                this.Speedo_Container_Rect.sizeDelta = vector;
                this.Speedo_Container_Rect.localPosition = new Vector3(0f, 0f, 0f);
            }
            this.Speedo_GameObject.SetActive(true);
            if (XDebug.Finder<PlayerBase>.Instance)
            {
                float num = X_GetActualPlayerSpeedForward();
                float num2 = num / 100f;
                string text = ColorUtility.ToHtmlStringRGB(new Color(2f * (1f - num2), 2f * num2, 0f));
                string text2 = ((num <= XDebug.Cfg.Speedo.MaxDisplayableSpeed) ? num.ToString("0.00") : (XDebug.Cfg.Speedo.MaxDisplayableSpeed.ToString("0.0") + "+"));
                this.Speedo_Text.text = string.Concat(new string[] { "Speed: <color=#", text, ">", text2, "</color>" });
                return;
            }
            this.Speedo_Text.text = "Speed: ---";
        }

        public float X_GetActualPlayerSpeedForward()
        {
            PlayerBase player = XDebug.Finder<PlayerBase>.Instance;
            return Vector3.Dot(player._Rigidbody.velocity, player.transform.forward.normalized);
        }


        public void LogExtra(string message, float duration = 1.5f, float fontOverride = 0f)
        {
            if (this.BoxExtra == null)
            {
                Vector2 vector = new Vector2(350f, 100f);
                Vector2 vector2 = new Vector2(325f, 80f);
                Vector2 vector3 = new Vector2(0.5f, 1f);
                Vector3 vector4 = new Vector3(0f, -50f, 0f);
                this.BoxExtra = new XLogBox(vector, vector2, vector3, vector4);
            }
            this.BoxExtra.GameObject.SetActive(true);
            this.BoxExtra.HideTime = Time.time + duration;
            this.BoxExtra.Text.text = message;
            this.BoxExtra.Font = fontOverride;
        }

        public bool BlockTriggerActions()
        {
            return XDebug.Finder<PlayerBase>.Instance != null && X_CanPlayerStartBoosting();
        }

        private Rewired.Player GetRInput()
        {
            return Singleton<RInput>.Instance.Get<Rewired.Player>("P");
        }
        public class XPlayerBase
        {
            public float X_BStopTime = 0;
            public float X_BReboostTime = 0.5f;
        };
        XPlayerBase xPlayerBase = new XPlayerBase();
        //[todo] [investigate] should be created on playerbase awake postfix or smth

        public bool X_CanPlayerStartBoosting()
        {
            bool trig = GetRInput().GetButton("Left Trigger");
            return trig && Time.time - xPlayerBase.X_BStopTime <= xPlayerBase.X_BReboostTime;
        }

        private UI _HUD;

        private PlayerBase _player;

        private static XDebug _instance;

        private List<LineRenderer> LRs;

        public static bool DBG = false;

        public static bool EXP = true;

        private SonicNew _sonicNew;

        public static bool OFF = true;

        private GameObject Box_GameObject;

        private MessageBox Box_Script;

        private TextMeshProUGUI Box_Text;

        private GameObject Box_Container;

        private List<Renderer> SRs;

        private SonicFast _sonicFast;

        public static int SNF_DDG_MODE;

        public static bool COMMENT = false;

        private AudioClip _DodgeClip;

        private AudioClip _DodgeClipFull;

        private float Box_EndTime;

        private float LB_NextMoveTime;

        private bool ULTRA_FPS;

        public static bool SWITCH = true;

        public static bool FASTER_STOMPDASH = true;

        private float RealFixedDeltaTime;

        public static readonly string P06X_VERSION = "1.8.0a";

        private float SpeedMultiplier;

        private float _TMP_;

        private int _T2_;

        private GameObject[] _ae_;

        private int[] _ae_idxs_;

        private Vector3 _fx_offset_;

        private Color _color_;

        private Tuple<string, int>[] Characters;

        public int CurrentCharIdx;

        private float PrevDPadY;

        public bool JustUsedLeftTrigger;

        public static bool IMMEDIATE_SPINDASH_CLAMP = true;

        public static bool USING_CUSTOM_SPEEDS = false;

        private float _ringsPrevClick;

        public XValue<bool> UltraSmoothFPS;

        public Canvas XCanvas;

        public XValue<bool> Invincible;

        public XValue<bool> InfiniteGauge;

        public XValue<bool> PlayCustomMusic;

        public XValue<float> EverySpeedMultiplier;

        public XValue<bool> MaxedOutGems;

        public XValue<bool> InfiniteRings;

        public XValue<bool> InfiniteLives;

        private int[] MaxedOutGems_Data;

        public XValue<string> TeleportLocation;

        public XValue<float> SMGround;

        public XValue<float> SMAir;

        public XValue<float> SMSpindash;

        public XValue<float> SMFly;

        public XValue<float> SMClimb;

        public XValue<float> SMHoming;

        public XValue<bool> ASCSpinClamp;

        public XValue<bool> ASCLuaRecalc;

        public XValue<bool> Saving_AutoLoad;

        private AudioClip backupClip;

        public XValue<float> tmp0;

        public XValue<float> tmp1;

        public XValue<float> tmp2;

        public XValue<bool>[] dbg_toggles = new XValue<bool>[5];

        public XValue<float>[] dbg_floats = new XValue<float>[5];

        public XValue<bool> Other_OgCameraControls;

        public XValue<bool> Cheat_IgnoreWaterDeath;

        public XValue<bool> Moveset_FreeWaterSliding;

        private GameObject Speedo_GameObject;

        private TextMeshProUGUI Speedo_Text;

        private RectTransform Speedo_Text_Rect;

        private RectTransform Speedo_Container_Rect;

        public XValue<bool> Extra_DisplaySpeedo;

        private RectTransform Speedo_GameObject_Rect;

        public XValue<bool> Saving_AutoSave;

        public XValue<float> SMHomingAttackFasterBy;

        public XValue<float> SMAfterHomingRotation;

        public XValue<bool> Moveset_WallJumping;

        public XValue<bool> Moveset_Boost;

        public XValue<bool> Cheat_ChainJumpZeroDelay;

        public XValue<bool> Other_UltraFPSFix;

        private float PrevDPadX;

        public XValue<float> Boost_BaseSpeed;

        public XValue<float> Boost_NextLevelDeltaSpeed;

        public XValue<float> Boost_NextLevelThreshold;

        public XValue<float> Boost_RotSpeed;

        public XValue<float> Boost_AccelTime;

        public XValue<bool> Other_CheckP06Version;

        private XLogBox BoxExtra;

        public XValue<bool> Moveset_AHMovement;

        public XValue<float> Moveset_AHMovementMaxSpeed;

        public XValue<bool> Moveset_ClimbAll;

        public class Finder<T> where T : UnityEngine.Object
        {
            public static T Instance
            {
                get
                {
                    if (XDebug.Finder<T>._instance != null)
                    {
                        return XDebug.Finder<T>._instance;
                    }
                    XDebug.Finder<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
                    return XDebug.Finder<T>._instance;
                }
            }

            private static T _instance;
        }

        private class DebugFloat : MonoBehaviour
        {
            private void Update()
            {
                this.Value = this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.2f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.2f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset / 2f;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugFloat.LogT.Constant || (this.LogType == XDebug.DebugFloat.LogT.OnChange && flag))
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value.ToString("0.0000")), 1.25f, 16f);
                }
            }

            public DebugFloat(string name, float init, float offset, KeyCode plus, KeyCode minus, Action<float> copy, Func<float> bind, XDebug.DebugFloat.LogT logType = XDebug.DebugFloat.LogT.None)
            {
                XDebug.DebugFloat debugFloat = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugFloat>();
                UnityEngine.Object.DontDestroyOnLoad(debugFloat.gameObject);
                debugFloat.Name = name;
                debugFloat.Value = init;
                debugFloat.Plus = plus;
                debugFloat.Minus = minus;
                debugFloat.Offset = offset;
                debugFloat.LogType = logType;
                debugFloat.UpdateActualValue = copy;
                debugFloat.BindToVariable = bind;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private float Offset;

            private string Name;

            private float Value;

            private float prevTime;

            private Action<float> UpdateActualValue;

            private XDebug.DebugFloat.LogT LogType;

            private Func<float> BindToVariable;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }

        private class DebugInt : MonoBehaviour
        {
            private void Update()
            {
                this.BindToVariable();
                bool flag = false;
                if (Input.GetKey(this.Plus))
                {
                    if (Input.GetKeyDown(this.Plus) || Time.time - this.prevTime > 0.25f)
                    {
                        flag = true;
                        this.prevTime = Time.time;
                        this.Value += this.Offset;
                    }
                }
                else if (Input.GetKey(this.Minus) && (Input.GetKeyDown(this.Minus) || Time.time - this.prevTime > 0.25f))
                {
                    flag = true;
                    this.prevTime = Time.time;
                    this.Value -= this.Offset;
                }
                if (flag)
                {
                    this.UpdateActualValue(this.Value);
                }
                if (this.LogType == XDebug.DebugInt.LogT.Constant)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                    return;
                }
                if (flag && this.LogType == XDebug.DebugInt.LogT.OnChange)
                {
                    XSingleton<XDebug>.Instance.Log(string.Format("{0} = {1}", this.Name, this.Value), 1.25f, 16f);
                }
            }

            public DebugInt(string name, int init, int offset, KeyCode plus, KeyCode minus, Action<int> copy, Func<int> bind, XDebug.DebugInt.LogT logType = XDebug.DebugInt.LogT.None)
            {
                XDebug.DebugInt debugInt = new GameObject(name + " [wrapper]").AddComponent<XDebug.DebugInt>();
                UnityEngine.Object.DontDestroyOnLoad(debugInt.gameObject);
                debugInt.Name = name;
                debugInt.Value = init;
                debugInt.Plus = plus;
                debugInt.Minus = minus;
                debugInt.Offset = offset;
                debugInt.LogType = logType;
                debugInt.BindToVariable = bind;
                debugInt.UpdateActualValue = copy;
            }

            private KeyCode Plus;

            private KeyCode Minus;

            private int Offset;

            private string Name;

            private int Value;

            private float prevTime;

            private Action<int> UpdateActualValue;

            private Func<int> BindToVariable;

            private XDebug.DebugInt.LogT LogType;

            public enum LogT
            {
                None,
                Constant,
                OnChange
            }
        }

        private struct ColorConstants
        {
            public static Color SmoothSwitchOverride = new Color(0.7216338f, 0.1462264f, 1f, 0.5f) * 1.75f;
        }

        public struct CustomSpeedMultiplier
        {
            public static float Ground = 1f;

            public static float Air = 1f;

            public static float Spindash = 1f;

            public static float Flying = 1f;

            public static float Climbing = 1f;

            public static float Homing = 1f;

            public static float HomingAttackTimeShortener = 1f;

            public static float AfterHomingRotationSpeed = 1f;
        }

        public struct Cheats
        {
            public static int Rings
            {
                get
                {
                    return Singleton<GameManager>.Instance._PlayerData.rings;
                }
                set
                {
                    Singleton<GameManager>.Instance._PlayerData.rings = value;
                }
            }

            public static int Lives
            {
                get
                {
                    return Singleton<GameManager>.Instance.GetLifeCount();
                }
                set
                {
                    GameData.StoryData storyData = Singleton<GameManager>.Instance.GetStoryData();
                    storyData.Lives = value;
                    Singleton<GameManager>.Instance.SetStoryData(storyData);
                }
            }

            public static int ActiveGemId
            {
                get
                {
                    SonicNew instance = XDebug.Finder<SonicNew>.Instance;
                    if (instance == null)
                    {
                        return -1;
                    }
                    return instance.Get<int>(" GemSelector");
                }
            }

            public static int CurrentGemLevel
            {
                get
                {
                    if (XDebug.Finder<SonicNew>.Instance && XDebug.Cheats.ActiveGemId >= 0)
                    {
                        return XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[XDebug.Cheats.ActiveGemId];
                    }
                    return -1;
                }
                set
                {
                    if (XDebug.Finder<SonicNew>.Instance != null && XDebug.Cheats.ActiveGemId >= 0)
                    {
                        XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel")[XDebug.Cheats.ActiveGemId] = Mathf.Clamp(value, 0, 2);
                        return;
                    }
                }
            }

            public static void GetAllGems()
            {
                SonicNew instance = XDebug.Finder<SonicNew>.Instance;
                if (instance == null)
                {
                    return;
                }
                GameData.GlobalData gameData = Singleton<GameManager>.Instance.GetGameData();
                gameData.ObtainedGems.Clear();
                for (int i = 0; i <= 8; i++)
                {
                    gameData.ObtainedGems.Add(i);
                }
                Singleton<GameManager>.Instance.SetGameData(gameData);
                instance.Set("GemData", gameData);
                instance.Set("ObtainedGemIndex", 8);
                instance.Set("GemSelector", 8);
                instance.Set("ActiveGem", SonicNew.Gem.Rainbow);
                instance.Get<UI>("HUD").UpdateGemPanel(gameData);
                XDebug.Comment("[fix]?");
                for (int j = 0; j < gameData.ObtainedGems.Count - 1; j++)
                {
                    instance.Get<UI>("HUD").GemSlots[j].GetComponent<Image>().sprite = instance.Get<UI>("HUD").GemImages[j + 1];
                }
                XSingleton<XDebug>.Instance.Log("All gems <color=#ff9a00>are yours ;)</color>", 1.5f, 18f);
            }

            public static void MaxOutAllGems()
            {
                XDebug.Cfg.Cheats.MaxedOutGems = true;
                int[] activeGemLevel = XDebug.Finder<UI>.Instance.Get<int[]>("ActiveGemLevel");
                if (activeGemLevel == null)
                {
                    return;
                }
                for (int i = 0; i < 9; i++)
                {
                    activeGemLevel[i] = 2;
                }
                XSingleton<XDebug>.Instance.Log("All gems <color=#de921f>maxed out</color>", 1.5f, 18f);
            }

            public static void GetLightMemoryShard()
            {
                global::Shadow instance = XDebug.Finder<global::Shadow>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Shadow!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasLightMemoryShard", true);
                XSingleton<XDebug>.Instance.Log("Light Memory Shard <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetSigil()
            {
                XDebug.Comment("gameData.SetFlag(Game.LotusOfResilience); ???");
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasSigilOfAwakening", true);
                XSingleton<XDebug>.Instance.Log("Sigil of Awakening <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetFlame()
            {
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasFlameOfControl", true);
                XSingleton<XDebug>.Instance.Log("Flame of Control <color=#ff9a00>collected</color>", 1.5f, 18f);
            }

            public static void GetLotus()
            {
                Silver instance = XDebug.Finder<Silver>.Instance;
                if (instance == null)
                {
                    XSingleton<XDebug>.Instance.Log("You're not Silver!", 1.5f, 18f);
                    return;
                }
                instance.Set("HasLotusOfResilience", true);
                instance.Animator.SetBool("Has Lotus", instance.Get<bool>("HasLotusOfResilience"));
                XSingleton<XDebug>.Instance.Log("Lotus of Resilience <color=#ff9a00>collected</color>", 1.5f, 18f);
            }
        }

        [Serializable]
        public struct Cfg
        {
            public struct Cheats
            {
                public static bool InfiniteGauge;

                public static bool Immune;

                public static bool MaxedOutGems;

                public static bool InfiniteRings;

                public static bool InfiniteLives;
            }

            public struct FWS
            {
                public static float YWaterOffset = 0.5f;

                public static float MinActivationSpeed = 8.5f;

                public static float SpeedBoost = 1.25f;

                public static float AccelTime = 0.65f;

                public static float MinRunAnimationSpeed = 27f;

                public static float YMaxWaterRaycastDist = 0.501f;

                public static float RunningBrakeSpeed = 25f;
            }

            public struct MachSpeedSecondJump
            {
                public static float FXPlaybackScale = 0.6f;

                public static float HueShift = -0.25f;

                public static float Offset = -0.1f;
            }

            public struct WJ
            {
                public static float MaxWaitTime = 0.75f;

                public static float MinDotNormal = -0.5f;

                public static float MaxDotNormal = 0.5f;

                public static float UpOffset = -0.25f;

                public static float NormalOffset = 0.5f;

                public static Vector3 MeshRotation = new Vector3(90f, 0f, 0f);

                public static float JumpStrength = 25f;

                public static float MinHeightAboveGround = 1f;
            }

            public struct Speedo
            {
                public static float MaxDisplayableSpeed = 999.9f;
            }

            public struct Boost
            {
                public static float FirstSpeed = 40f;
            }
        }
    }


    public class XEffects : XSingleton<XEffects>
    {
        public void DestroyDodgeFX()
        {
            ParticleSystem.EmissionModule emission = this.DodgeFX.GetComponent<ParticleSystem>().emission;
            emission.rateOverTimeMultiplier = 0f;
            emission.burstCount = emission.burstCount;
        }

        public void CreateDodgeFX()
        {
            if (this.DodgeFX == null)
            {
                this.DodgeFX = new GameObject("X_DodgeFX");
                PlayerBase instance = XDebug.Finder<PlayerBase>.Instance;
                if (instance == null)
                {
                    return;
                }
                GameObject gameObject = instance.gameObject;
                this.DodgeFX.transform.SetParent(gameObject.transform, false);
                string playerName = instance.Get<string>("PlayerName");
                SkinnedMeshRenderer skinnedMeshRenderer = null;
                try
                {
                    skinnedMeshRenderer = gameObject.FindInChildren("Mesh").FindInChildren(playerName + "_Root").FindInChildren(playerName + "_Root")
                        .GetComponent<SkinnedMeshRenderer>();
                }
                catch (Exception)
                {
                    XDebug.Comment("[ERROR]");
                    return;
                }
                ParticleSystem particleSystem = this.DodgeFX.AddComponent<ParticleSystem>();
                ParticleSystem.MainModule main = particleSystem.main;
                main.duration = 10f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(-0.3f, 0.8f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.05f);
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                emission.rateOverTime = XEffects.DodgeParams.Rate;
                XDebug.Comment("emission.SetBurst(0, new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(200f), 1, 2f))");
                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
                shape.skinnedMeshRenderer = skinnedMeshRenderer;
                shape.useMeshColors = false;
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                velocityOverLifetime.enabled = false;
                velocityOverLifetime.orbitalX = 0.1f;
                velocityOverLifetime.orbitalY = 0.15f;
                velocityOverLifetime.orbitalZ = 0.3f;
                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
                colorOverLifetime.enabled = true;
                XDebug.Comment("//Colors BACKUP:\r\n\t\t\t\tColor startColor = new Color(0.34117648f, 0.2509804f, 0.9607843f) * this.GLOW;\r\n\t\t\t\tColor endColor = new Color(0.64705884f, 0.11764706f, 0.7921569f) * this.GLOW;\r\n\t\t\t");
                XDebug.Comment("new Color(0f, 0.09411765f, 0.9607843f) * this.GLOW; new Color(0.64705884f, 0.11764706f, 1f) * this.GLOW;");
                Color color = XEffects.DodgeParams.Color * XEffects.DodgeParams.Glow;
                if ((XDebug.Finder<SonicNew>.Instance && XDebug.Finder<SonicNew>.Instance.Get<bool>("IsSuper")) || (XDebug.Finder<SonicFast>.Instance && XDebug.Finder<SonicFast>.Instance.Get<bool>("IsSuper")))
                {
                    color = XEffects.DodgeParams.ColorSuper * XEffects.DodgeParams.Glow;
                }
                else if (playerName == "shadow")
                {
                    color = XEffects.DodgeParams.ColorShadow;
                }
                Gradient gradient = new Gradient();
                gradient.SetKeys(new GradientColorKey[]
                {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
                }, new GradientAlphaKey[]
                {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
                });
                colorOverLifetime.color = gradient;
                ParticleSystem.TrailModule trails = particleSystem.trails;
                trails.enabled = true;
                trails.minVertexDistance = 0.05f;
                trails.worldSpace = true;
                XDebug.Comment("Renderer");
                ParticleSystemRenderer component = this.DodgeFX.GetComponent<ParticleSystemRenderer>();
                component.renderMode = ParticleSystemRenderMode.None;
                component.trailMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                component.trailMaterial.mainTexture = XSingleton<XFiles>.Instance.Particle;
            }
            this.DodgeFX.GetComponent<ParticleSystem>().Emit(XEffects.DodgeParams.Burst);
            this.DodgeFX.GetComponent<ParticleSystem>().emissionRate = XEffects.DodgeParams.Rate;
        }

        public void DestroyStompFX(bool quickMode = false)
        {
            base.StartCoroutine(this.DestroyStompFX_Coroutine(quickMode));
        }

        private IEnumerator DestroyStompFX_Coroutine(bool quickMode)
        {
            GameObject reference = this.StompFX;
            reference.transform.parent = null;
            ParticleSystem[] particleSystems = this.StompFX.GetComponentsInChildren<ParticleSystem>();
            float startTime = Time.time;
            float[] emissionRate = new float[3];
            for (int i = 0; i < 3; i++)
            {
                emissionRate[i] = particleSystems[i].emissionRate;
            }
            Color fadingColor = XEffects.StompParams.Color;
            float duration = 0.33f;
            while (Time.time - startTime < duration)
            {
                if (quickMode)
                {
                    reference.transform.position -= new Vector3(0f, XEffects.StompParams.SinkSpeed * Time.deltaTime, 0f);
                }
                float num = Mathf.Sqrt((Time.time - startTime) / duration);
                for (int j = 0; j < 3; j++)
                {
                    fadingColor.a = Mathf.Lerp(XEffects.StompParams.Color.a, 0f, Mathf.Sqrt(num));
                    particleSystems[j].startColor = fadingColor;
                    particleSystems[j].emissionRate = Mathf.Lerp(emissionRate[j], emissionRate[j] / 2f, num);
                }
                yield return null;
            }
            UnityEngine.Object.Destroy(reference);
            yield break;
        }

        public void CreateStompFX()
        {
            if (this.StompFXPrefab == null)
            {
                GameObject gameObject = Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX");
                this.StompFXPrefab = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
                XDebug.Comment("!!!!!!!!!!!!!!!!!!!!");
                this.StompFXPrefab.GetComponent<AudioSource>().clip = (XSingleton<XDebug>.Instance.SonicNew ? XSingleton<XDebug>.Instance.SonicNew.JumpDashKickback : null);
                this.StompFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
                UnityEngine.Object.Destroy(this.StompFXPrefab.GetComponent<MonoBehaviour>());
                ParticleSystem[] componentsInChildren = this.StompFXPrefab.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < 3; i++)
                {
                    componentsInChildren[i].loop = true;
                    if (XSingleton<XDebug>.Instance.Player.Get<string>("PlayerName") == "shadow")
                    {
                        componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
                    }
                    else
                    {
                        componentsInChildren[i].startColor = XEffects.StompParams.Color;
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", XEffects.StompParams.TintColor);
                    }
                }
                for (int j = 3; j < componentsInChildren.Length; j++)
                {
                    componentsInChildren[j].enableEmission = false;
                    UnityEngine.Object.Destroy(componentsInChildren[j]);
                }
                this.StompFXPrefab.SetActive(false);
            }
            SonicNew instance = XDebug.Finder<SonicNew>.Instance;
            Transform transform = ((instance != null) ? instance.transform : null);
            if (!transform)
            {
                Shadow instance2 = XDebug.Finder<Shadow>.Instance;
                transform = ((instance2 != null) ? instance2.transform : null);
            }
            if (!transform)
            {
                SonicFast instance3 = XDebug.Finder<SonicFast>.Instance;
                transform = ((instance3 != null) ? instance3.transform : null);
            }
            if (transform == null)
            {
                throw new Exception("no shadow no sonic thass bad");
            }
            this.StompFX = UnityEngine.Object.Instantiate<GameObject>(this.StompFXPrefab, transform.position + transform.up * XEffects.StompParams.Offset, transform.rotation * XEffects.StompParams.Rotation);
            this.StompFX.SetActive(true);
            this.StompFX.transform.SetParent(transform);
        }

        private void CreateStompTornadoFX(RaycastHit where)
        {
            if (this.StompTornadoFXPrefab == null)
            {
                this.StompTornadoFXPrefab = new GameObject("X_StompTornadoFX");
                ParticleSystem particleSystem = this.StompTornadoFXPrefab.AddComponent<ParticleSystem>();
                XDebug.Comment("========= main =========");
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = false;
                main.duration = 1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 1f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(-0.25f, 2.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                XDebug.Comment("new Color(0.1529412f, 0.2f, 1f, 1f) * 3.5f;");
                XDebug.Comment("new Color(0.4386792f, 0.9042454f, 1f, 1f);");
                XDebug.Comment("this makes no effect actually: ");
                main.startColor = new Color(0f, 23f, 255f, 1f) * XEffects.StompTornadoParams.Glow / 2f;
                XDebug.Comment("I guess.....");
                main.gravityModifier = -0.1f;
                main.gravityModifier = 0f;
                XDebug.Comment("========= emiss ========");
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                emission.rateOverTime = 0f;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                new ParticleSystem.Burst(0f, 40)
                });
                XDebug.Comment("========= shape ========");
                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.rotation = new Vector3(-90f, 0f, 0f);
                shape.angle = 15f;
                shape.radius = 2.5f;
                shape.radiusThickness = 0.9f;
                XDebug.Comment("========= vel o/lt ========");
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.orbitalX = 0.1f;
                velocityOverLifetime.orbitalY = -5f;
                velocityOverLifetime.orbitalZ = 0.1f;
                velocityOverLifetime.radial = 0.1f;
                velocityOverLifetime.speedModifier = 2f;
                XDebug.Comment("======== size o/lt ========");
                ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve animationCurve = new AnimationCurve();
                animationCurve.AddKey(0f, 0f);
                animationCurve.AddKey(0.5f, 0.8f);
                animationCurve.AddKey(1f, 0f);
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                XDebug.Comment("========= trails =========");
                ParticleSystem.TrailModule trails = particleSystem.trails;
                trails.enabled = true;
                trails.lifetime = 0.1f;
                XDebug.Comment("========= rend =========");
                ParticleSystemRenderer component = this.StompTornadoFXPrefab.GetComponent<ParticleSystemRenderer>();
                Material material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                material.mainTexture = XFiles.LoadPNG(Application.dataPath + "/mods/Particle.png");
                component.material = material;
                component.trailMaterial = material;
                Light light = new GameObject("The Light").AddComponent<Light>();
                light.transform.position = new Vector3(999999f, 0f, 0f);
                light.color = Color.blue;
                light.range = 5f;
                light.intensity = 3f;
                ParticleSystem.LightsModule lights = particleSystem.lights;
                lights.enabled = true;
                lights.light = light;
                lights.ratio = 1f;
                lights.sizeAffectsRange = true;
                lights.maxLights = 100;
            }
            ParticleSystem component2 = UnityEngine.Object.Instantiate<GameObject>(this.StompTornadoFXPrefab, where.point + where.normal * 0.1f, Quaternion.FromToRotation(Vector3.up, where.normal)).GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main2 = component2.main;
            main2.loop = false;
            main2.startColor = XEffects.StompTornadoParams.Color * XEffects.StompTornadoParams.Glow;
            component2.Play();
            XDebug.Comment("shouldn't we destroy it later????");
        }

        public void CreateStompCrashFX(RaycastHit whereHit)
        {
            this.CreateStompTornadoFX(whereHit);
            if (this.StompCrashFXPrefab == null)
            {
                this.StompCrashFXPrefab = UnityEngine.Object.Instantiate<GameObject>(XSingleton<XDebug>.Instance.SonicNew.SonicEffects.LightAttackFX, Vector3.zero, Quaternion.identity);
                ParticleSystem[] componentsInChildren = this.StompCrashFXPrefab.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i <= 10; i++)
                {
                    if (i != 9 && i != 8)
                    {
                        componentsInChildren[i].enableEmission = false;
                        UnityEngine.Object.Destroy(componentsInChildren[i]);
                    }
                    else
                    {
                        componentsInChildren[i].startColor = XEffects.StompCrashParams.Color;
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", XEffects.StompCrashParams.Color);
                        componentsInChildren[i].startSize *= 0.45f;
                        ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
                        emission.burstCount = 1;
                        if (i == 8)
                        {
                            emission.SetBurst(0, XEffects.StompCrashParams.Burst);
                            componentsInChildren[i].startLifetime = XEffects.StompCrashParams.StartLifetime;
                        }
                        emission.enabled = true;
                    }
                }
                this.StompCrashFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
                UnityEngine.Object.Destroy(this.StompCrashFXPrefab.GetComponent<MonoBehaviour>());
                this.StompCrashFXPrefab.GetComponent<AudioSource>().enabled = false;
                UnityEngine.Object.Destroy(this.StompCrashFXPrefab.GetComponent<AudioSource>());
            }
            this.StompCrashFX = UnityEngine.Object.Instantiate<GameObject>(this.StompCrashFXPrefab, whereHit.point + whereHit.normal * XEffects.StompCrashParams.Offset, Quaternion.FromToRotation(Vector3.up, whereHit.normal) * XEffects.StompCrashParams.Rotation);
            ParticleSystem[] componentsInChildren2 = this.StompCrashFX.GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                componentsInChildren2[j].Play();
            }
        }

        public void CreateStompCrashShadowFX(RaycastHit whereHit)
        {
            this.CreateStompTornadoShadowFX(whereHit);
            if (this.StompCrashShadowFXPrefab == null)
            {
                this.StompCrashShadowFXPrefab = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX"), Vector3.zero, Quaternion.identity);
                ParticleSystem[] componentsInChildren = this.StompCrashShadowFXPrefab.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i <= 10; i++)
                {
                    if (i != 9 && i != 8)
                    {
                        componentsInChildren[i].enableEmission = false;
                        UnityEngine.Object.Destroy(componentsInChildren[i]);
                    }
                    else
                    {
                        componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
                        componentsInChildren[i].startColor = this.DGBCOL2;
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", this.DGBCOL2);
                        componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
                        componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
                        componentsInChildren[i].startSize *= 0.45f;
                        ParticleSystem.EmissionModule emission = componentsInChildren[i].emission;
                        emission.burstCount = 1;
                        if (i == 8)
                        {
                            emission.SetBurst(0, XEffects.StompCrashParams.Burst);
                            componentsInChildren[i].startLifetime = XEffects.StompCrashParams.StartLifetime;
                        }
                        emission.enabled = true;
                    }
                }
                this.StompCrashShadowFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
                UnityEngine.Object.Destroy(this.StompCrashShadowFXPrefab.GetComponent<MonoBehaviour>());
                this.StompCrashShadowFXPrefab.GetComponent<AudioSource>().enabled = false;
                UnityEngine.Object.Destroy(this.StompCrashShadowFXPrefab.GetComponent<AudioSource>());
            }
            this.StompCrashShadowFX = UnityEngine.Object.Instantiate<GameObject>(this.StompCrashShadowFXPrefab, whereHit.point + whereHit.normal * XEffects.StompCrashParams.Offset, Quaternion.FromToRotation(Vector3.up, whereHit.normal) * XEffects.StompCrashParams.Rotation);
            ParticleSystem[] componentsInChildren2 = this.StompCrashShadowFX.GetComponentsInChildren<ParticleSystem>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                componentsInChildren2[j].Play();
            }
        }

        private void CreateStompTornadoShadowFX(RaycastHit where)
        {
            if (this.StompTornadoShadowFXPrefab == null)
            {
                this.StompTornadoShadowFXPrefab = new GameObject("X_StompTornadoFX");
                ParticleSystem particleSystem = this.StompTornadoShadowFXPrefab.AddComponent<ParticleSystem>();
                XDebug.Comment("========= main =========");
                ParticleSystem.MainModule main = particleSystem.main;
                main.loop = false;
                main.duration = 1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 1f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(-0.25f, 2.5f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
                main.gravityModifier = -0.1f;
                main.gravityModifier = 0f;
                XDebug.Comment("this makes no effect actually: ");
                main.startColor = new Color(255f, 0f, 0f, 1f) * XEffects.StompTornadoParams.Glow / 2f;
                XDebug.Comment("I guess.....");
                XDebug.Comment("========= emiss ========");
                ParticleSystem.EmissionModule emission = particleSystem.emission;
                emission.enabled = true;
                emission.rateOverTime = 0f;
                emission.SetBursts(new ParticleSystem.Burst[]
                {
                new ParticleSystem.Burst(0f, 40)
                });
                XDebug.Comment("========= shape ========");
                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.rotation = new Vector3(-90f, 0f, 0f);
                shape.angle = 15f;
                shape.radius = 2.5f;
                shape.radiusThickness = 0.9f;
                XDebug.Comment("========= vel o/lt ========");
                ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particleSystem.velocityOverLifetime;
                velocityOverLifetime.enabled = true;
                velocityOverLifetime.orbitalX = 0.1f;
                velocityOverLifetime.orbitalY = -5f;
                velocityOverLifetime.orbitalZ = 0.1f;
                velocityOverLifetime.radial = 0.1f;
                velocityOverLifetime.speedModifier = 2f;
                XDebug.Comment("======== size o/lt ========");
                ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve animationCurve = new AnimationCurve();
                animationCurve.AddKey(0f, 0f);
                animationCurve.AddKey(0.5f, 0.8f);
                animationCurve.AddKey(1f, 0f);
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, animationCurve);
                XDebug.Comment("========= trails =========");
                ParticleSystem.TrailModule trails = particleSystem.trails;
                trails.enabled = true;
                trails.lifetime = 0.1f;
                XDebug.Comment("========= rend =========");
                ParticleSystemRenderer component = this.StompTornadoShadowFXPrefab.GetComponent<ParticleSystemRenderer>();
                Material material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                material.mainTexture = XFiles.LoadPNG(Application.dataPath + "/mods/Particle.png");
                component.material = material;
                component.trailMaterial = material;
                Light light = new GameObject("The Light").AddComponent<Light>();
                light.transform.position = new Vector3(999999f, 0f, 0f);
                light.color = Color.yellow;
                light.range = 5f;
                light.intensity = 3f;
                ParticleSystem.LightsModule lights = particleSystem.lights;
                lights.enabled = true;
                lights.light = light;
                lights.ratio = 1f;
                lights.sizeAffectsRange = true;
                lights.maxLights = 100;
            }
            ParticleSystem component2 = UnityEngine.Object.Instantiate<GameObject>(this.StompTornadoShadowFXPrefab, where.point + where.normal * 0.1f, Quaternion.FromToRotation(Vector3.up, where.normal)).GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main2 = component2.main;
            main2.loop = false;
            main2.startColor = new Color(2.75f, 0f, 0f, 0.3f);
            main2.startColor = this.DBGCOL1;
            main2.startColor = this.DGBCOL2;
            main2.startColor = new Color(2.75f, 0f, 0f, 0.3f);
            component2.Play();
            XDebug.Comment("shouldn't we destroy it later????");
        }

        public void CreateStompShadowFX()
        {
            if (this.StompShadowFXPrefab == null)
            {
                GameObject gameObject = Resources.Load<GameObject>("defaultprefabs/effect/player/sonic/LightAttackFX");
                this.StompShadowFXPrefab = UnityEngine.Object.Instantiate<GameObject>(gameObject, Vector3.zero, Quaternion.identity);
                XDebug.Comment("!!!!!!!!!!!!!!!!!!!!");
                this.StompShadowFXPrefab.GetComponent<AudioSource>().clip = (XSingleton<XDebug>.Instance.SonicNew ? XSingleton<XDebug>.Instance.SonicNew.JumpDashKickback : null);
                this.StompShadowFXPrefab.GetComponent<MonoBehaviour>().enabled = false;
                UnityEngine.Object.Destroy(this.StompShadowFXPrefab.GetComponent<MonoBehaviour>());
                ParticleSystem[] componentsInChildren = this.StompShadowFXPrefab.GetComponentsInChildren<ParticleSystem>();
                for (int i = 0; i < 3; i++)
                {
                    componentsInChildren[i].loop = true;
                    componentsInChildren[i].startColor = new Color(1f, 0f, 0f);
                    componentsInChildren[i].GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1f, 0f, 0f));
                }
                for (int j = 3; j < componentsInChildren.Length; j++)
                {
                    componentsInChildren[j].enableEmission = false;
                    UnityEngine.Object.Destroy(componentsInChildren[j]);
                }
                this.StompShadowFXPrefab.SetActive(false);
            }
            Transform transform = XDebug.Finder<Shadow>.Instance.transform;
            if (transform == null)
            {
                throw new Exception("no shadow thass bad");
            }
            this.StompShadowFX = UnityEngine.Object.Instantiate<GameObject>(this.StompShadowFXPrefab, transform.position + transform.up * XEffects.StompParams.Offset, transform.rotation * XEffects.StompParams.Rotation);
            this.StompShadowFX.SetActive(true);
            this.StompShadowFX.transform.SetParent(transform);
        }

        public void DestroyStompShadowFX(bool quickMode = false)
        {
            base.StartCoroutine(this.DestroyStompShadowFX_Coroutine(quickMode));
        }

        private IEnumerator DestroyStompShadowFX_Coroutine(bool quickMode)
        {
            GameObject reference = this.StompShadowFX;
            reference.transform.parent = null;
            ParticleSystem[] particleSystems = this.StompShadowFX.GetComponentsInChildren<ParticleSystem>();
            float startTime = Time.time;
            float[] emissionRate = new float[3];
            for (int i = 0; i < 3; i++)
            {
                emissionRate[i] = particleSystems[i].emissionRate;
            }
            Color fadingColor = XEffects.StompParams.ColorShadow;
            float duration = 0.33f;
            while (Time.time - startTime < duration)
            {
                if (quickMode)
                {
                    reference.transform.position -= new Vector3(0f, XEffects.StompParams.SinkSpeed * Time.deltaTime, 0f);
                }
                float num = Mathf.Sqrt((Time.time - startTime) / duration);
                for (int j = 0; j < 3; j++)
                {
                    fadingColor.a = Mathf.Lerp(XEffects.StompParams.ColorShadow.a, 0f, Mathf.Sqrt(num));
                    particleSystems[j].startColor = fadingColor;
                    particleSystems[j].emissionRate = Mathf.Lerp(emissionRate[j], emissionRate[j] / 2f, num);
                }
                yield return null;
            }
            UnityEngine.Object.Destroy(reference);
            yield break;
        }

        public void CreateSecondJumpFX()
        {
            if (this.SecondJumpFXPrefab == null)
            {
                this.SecondJumpFXPrefab = Resources.Load<GameObject>("defaultprefabs/effect/player/amy/DoubleJumpFX");
            }
            Transform transform = XDebug.Finder<SonicFast>.Instance.transform;
            GameObject gameObject;
            if (XDebug.Cfg.MachSpeedSecondJump.HueShift < 0f)
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(this.SecondJumpFXPrefab, transform);
            }
            else
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(this.SecondJumpFXPrefab, transform.position, Quaternion.identity);
            }
            gameObject.transform.position += -Vector3.up * XDebug.Cfg.MachSpeedSecondJump.Offset;
            ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                float num;
                float num2;
                float num3;
                Color.RGBToHSV(componentsInChildren[i].startColor, out num, out num2, out num3);
                num = ((num + XDebug.Cfg.MachSpeedSecondJump.HueShift) % 1f + 1f) % 1f;
                Color color = Color.HSVToRGB(num, num2, num3);
                componentsInChildren[i].startColor = color;
                componentsInChildren[i].playbackSpeed = XDebug.Cfg.MachSpeedSecondJump.FXPlaybackScale;
            }
            componentsInChildren[0].enableEmission = false;
            componentsInChildren[1].enableEmission = false;
            UnityEngine.Object.Destroy(gameObject, 2.5f);
        }

        private GameObject DodgeFX;

        private GameObject StompFX;

        private GameObject StompFXPrefab;

        private GameObject StompTornadoFXPrefab;

        private GameObject StompCrashFXPrefab;

        private GameObject StompCrashFX;

        private Color DBGCOL1 = new Color(0.9f, 0f, 0f, 1f);

        private Color DGBCOL2 = new Color(1f, 0.71f, 0.18f, 0.5f);

        private GameObject StompCrashShadowFXPrefab;

        private GameObject StompCrashShadowFX;

        private GameObject StompTornadoShadowFXPrefab;

        private GameObject StompShadowFXPrefab;

        private GameObject StompShadowFX;

        private GameObject SecondJumpFXPrefab;

        private struct DodgeParams
        {
            public static float Rate = 50f;

            public static int Burst = 100;

            public static float Glow = 2.75f;

            public static Color Color = new Color(0.1529412f, 0.2f, 1f, 1f);

            public static Color ColorSuper = new Color(0.921f, 0.686f, 0.0352f);

            public static Color ColorShadow = new Color(1f, 0.71f, 0.18f, 0.12f);

            public static Color ColorShadow2 = new Color(1f, 0.529f, 0.125f, 0.125f);
        }

        private struct StompParams
        {
            public static float Offset = 0.65f;

            public static Quaternion Rotation = Quaternion.Euler(90f, 0f, 0f);

            public static Color Color = new Color(0f, 23f, 255f, 0.3f);

            public static Color TintColor = new Color(0f, 23f, 255f, 0.004f);

            public static float SinkSpeed = 22f;

            public static Color ColorShadow = new Color(1f, 0f, 0f, 0.3f);
        }

        private struct StompTornadoParams
        {
            public static Color Color = new Color(0.1529412f, 0.2f, 1f, 1f);

            public static float Glow = 2.75f;
        }

        public struct StompCrashParams
        {
            public static float Offset = 1.8f;

            public static Quaternion Rotation = Quaternion.Euler(90f, 0f, 0f);

            public static Color Color = new Color(0f, 23f, 255f, 0.05f);

            public static ParticleSystem.Burst Burst = new ParticleSystem.Burst(0f, 1, 1, 2, 0.1f);

            public static float StartLifetime = 0.4f;
        }
    }

    public class XFiles : XSingleton<XFiles>
    {
        private void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
        }

        public void Load()
        {
            base.StartCoroutine(this.LoadAll_C());
        }

        private void Start()
        {
        }

        public bool Check()
        {
            if (!Directory.Exists(this.Mods))
            {
                Directory.CreateDirectory(this.Mods);
            }
            string text = this.Mods + "xconfig.ini";
            if (!File.Exists(text))
            {
                File.WriteAllLines(text, new string[]
                {
                "0",
                "P06X" + XDebug.P06X_VERSION
                });
            }
            long num = 0L;
            try
            {
                string[] files = Directory.GetFiles(Application.dataPath, "*.resource");
                for (int i = 0; i < files.Length; i++)
                {
                    num += new FileInfo(files[i]).Length;
                }
            }
            catch
            {
                XSingleton<XDebug>.Instance.Log("There's a problem with checking your P-06 version - couldn't access the file!", 100f, 14f);
                return false;
            }
            if (num != XFiles.VersionHash && XSingleton<XDebug>.Instance.Other_CheckP06Version.Value)
            {
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>version error</color>(most likely)\n This mod release isn't comp. with your P-06 ver.", 100f, 12.5f);
                XDebug.Comment("\n the game <color=#ee0000>may crash</color>");
                File.WriteAllLines(text, new string[]
                {
                "0",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " version error at ",
                    DateTime.Now
                })
                });
                return false;
            }
            XDebug.Comment("Always CheckModFiles()");
            string text2 = null;
            try
            {
                text2 = File.ReadAllLines(text)[0];
            }
            catch (Exception ex)
            {
                if (text2 == null)
                {
                    XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>critical file error</color>\nexception" + ex.Message, 100f, 14f);
                    return false;
                }
            }
            string text3 = null;
            foreach (string text4 in this.Required)
            {
                if (!File.Exists(Application.dataPath + "/mods/" + text4))
                {
                    text3 = text4;
                    break;
                }
            }
            XDebug.Comment("1 -> 1");
            if (text2 == "1" && text3 == null)
            {
                XSingleton<XDebug>.Instance.Log("P-06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " by 4ndrelus for Version 4.6", 3f, 13.8f);
            }
            XDebug.Comment("0 -> 1");
            if (text2 == "0" && text3 == null)
            {
                File.WriteAllLines(text, new string[]
                {
                "1",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " installed ",
                    DateTime.Now
                })
                });
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#00ee00>installed correctly</color>", 60f, 14f);
                XSingleton<XDebug>.Instance.LogExtra("Thanks for using the mod!\n\n1) Press F12 to open the Mod Menu\n\n2) Have fun :D", 60f, 14f);
            }
            XDebug.Comment("0/1 -> 0");
            if (text3 != null)
            {
                File.WriteAllLines(text, new string[]
                {
                "0",
                string.Concat(new object[]
                {
                    "P06X",
                    XDebug.P06X_VERSION,
                    " error at ",
                    DateTime.Now
                })
                });
                XSingleton<XDebug>.Instance.Log(string.Concat(new string[]
                {
                "P-06 eXtended ",
                XDebug.P06X_VERSION,
                " <color=#ee0000>files error</color>\n",
                text3,
                " missing in the mods folder!"
                }), 100f, 14f);
                return false;
            }
            try
            {
                this.ConfigFile = File.ReadAllLines(text);
            }
            catch (Exception ex2)
            {
                XSingleton<XDebug>.Instance.Log("P-06 eXtended " + XDebug.P06X_VERSION + " <color=#ee0000>weird file error</color>\nexception" + ex2.Message, 100f, 14f);
                return false;
            }
            return true;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            Texture2D texture2D = null;
            if (File.Exists(filePath))
            {
                byte[] array = File.ReadAllBytes(filePath);
                texture2D = new Texture2D(2, 2);
                texture2D.LoadImage(array);
            }
            return texture2D;
        }

        private IEnumerator LoadOGG2(Action<AudioClip> action, string filename)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(this.Mods + filename, AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();
                action(DownloadHandlerAudioClip.GetContent(www));
            }
        }

        public XFiles()
        {
            this.Mods = Application.dataPath + "/mods/";
            this.Required = new string[] { "Particle.png", "custom_music.ogg", "stomp_land.ogg", "ring.ogg" };
        }

        private IEnumerator LoadAll_C()
        {
            this.Particle = XFiles.LoadPNG(this.Mods + "Particle.png");
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.Gandalf = clip;
            }, "custom_music.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.StompLand = clip;
            }, "stomp_land.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.RingSound = clip;
            }, "ring.ogg"));
            yield return base.StartCoroutine(this.LoadOGG2(delegate (AudioClip clip)
            {
                this.WallLand = clip;
            }, "land_concrete.ogg"));
            if (XDebug.COMMENT)
            {
                XSingleton<XDebug>.Instance.Log("All P06<color=#00ee00>X</color>" + XDebug.P06X_VERSION + " files <color=#00ee00>loaded correctly</color>", 1f, 14f);
            }
            yield break;
        }

        public IEnumerator LoadMp3(Action<AudioClip> action, string filename)
        {
            XDebug.Comment("THIS IS BROKEN currently");
            string text = string.Format("file://{0}", this.Mods + filename);
            WWW www = new WWW(text);
            yield return www;
            AudioClip audioClip = www.GetAudioClip(false, false, AudioType.MPEG);
            action(audioClip);
            yield break;
        }

        private static string CalculateMD5(string filename)
        {
            string text;
            using (MD5 md = MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(filename))
                {
                    text = BitConverter.ToString(md.ComputeHash(fileStream)).Replace("-", "").ToLowerInvariant();
                }
            }
            return text;
        }

        public string Mods;

        public string[] Required;

        private static XFiles _instance;

        public AudioClip Gandalf;

        public Texture2D Particle;

        public AudioClip StompLand;

        public static long VersionCode;

        public string[] ConfigFile;

        public AudioClip RingSound;

        public AudioClip WallLand;

        public static long VersionHash = 697226251L;
    }

    public class XLogBox
    {
        public Vector2 Size
        {
            get
            {
                return this._size;
            }
            set
            {
                this._size = value;
                this.Container_Rect.sizeDelta = value;
            }
        }

        public Vector2 InnerSize
        {
            get
            {
                return this._innerSize;
            }
            set
            {
                this._innerSize = value;
                this.Text_Rect.sizeDelta = value;
            }
        }

        public float Font
        {
            get
            {
                return this._font;
            }
            set
            {
                if (value <= 0f)
                {
                    this.Text.enableAutoSizing = true;
                }
                else
                {
                    this.Text.enableAutoSizing = false;
                    this.Text.fontSize = value;
                }
                this._font = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
                this.Container_Rect.localPosition = value;
            }
        }

        private void Update()
        {
            if (this.GameObject.activeSelf && Time.time > this.HideTime)
            {
                this.GameObject.SetActive(false);
            }
        }

        public XLogBox(Vector2 size, Vector2 innerSize, Vector2 anchor, Vector2 position)
        {
            this.GameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Defaultprefabs/UI/MessageBox_E3") as GameObject, Vector3.zero, Quaternion.identity);
            this.GameObject_Rect = this.GameObject.GetComponent<RectTransform>();
            this.GameObject.GetComponent<MessageBox>().Set("Duration", 9999999f);
            this.GameObject.transform.SetParent(XSingleton<XDebug>.Instance.XCanvas.transform, false);
            this.Text = this.GameObject.GetComponentInChildren<TextMeshProUGUI>();
            this.Text.overflowMode = TextOverflowModes.Linked;
            this.Text.alignment = TextAlignmentOptions.Center;
            this.Text.enableAutoSizing = true;
            this.Text.fontSizeMin = 2f;
            this.Text_Rect = this.Text.GetComponent<RectTransform>();
            this.Text_Rect.anchorMin = new Vector2(0.5f, 0.5f);
            this.Text_Rect.anchorMax = new Vector2(0.5f, 0.5f);
            this.Text_Rect.pivot = new Vector2(0.5f, 0.5f);
            this.Text_Rect.localPosition = Vector3.zero;
            this.Text_Rect.sizeDelta = innerSize;
            this.GameObject_Rect.anchorMin = anchor;
            this.GameObject_Rect.anchorMax = anchor;
            this.Container_Rect = this.GameObject.FindInChildren("Box").GetComponent<RectTransform>();
            this.Container_Rect.anchorMin = anchor;
            this.Container_Rect.anchorMax = anchor;
            this.Container_Rect.pivot = anchor;
            this.Container_Rect.sizeDelta = size;
            this.Container_Rect.localPosition = position;
        }

        public Vector2 Anchor
        {
            get
            {
                return this._anchor;
            }
            set
            {
                this._anchor = value;
                this.GameObject_Rect.anchorMin = value;
                this.GameObject_Rect.anchorMax = value;
                this.Container_Rect.anchorMin = value;
                this.Container_Rect.anchorMin = value;
                this.Container_Rect.pivot = value;
            }
        }

        public GameObject GameObject;

        public TextMeshProUGUI Text;

        public RectTransform Text_Rect;

        public RectTransform GameObject_Rect;

        public RectTransform Container_Rect;

        private Vector2 _size = new Vector2(175f, 45f);

        private Vector2 _innerSize = new Vector2(150f, 30f);

        private float _font;

        private Vector3 _position = new Vector3(0f, 100f, 0f);

        public float HideTime;

        private Vector2 _anchor = new Vector2(1f, 0f);
    }


    public class XModMenu : Singleton<XModMenu>
    {
        private void Awake()
        {
            this.Menu = new XUIMenu("P-06X " + XDebug.P06X_VERSION, KeyCode.F12);
            this.Canvas = this.Menu.gameObject.transform.parent.GetComponent<Canvas>();
            if (XDebug.DBG)
            {
                XUISection xuisection = this.Menu.AddSection(new XUISection("Debug"));
                for (int i = 0; i < XSingleton<XDebug>.Instance.dbg_toggles.Length; i++)
                {
                    xuisection.AddItem(new XUIToggleButton("dbg_toggle_" + i.ToString(), XSingleton<XDebug>.Instance.dbg_toggles[i]));
                }
                for (int j = 0; j < XSingleton<XDebug>.Instance.dbg_floats.Length; j++)
                {
                    xuisection.AddItem(new XUIFloatAdjuster("dbg_slider_" + j.ToString(), XSingleton<XDebug>.Instance.dbg_floats[j], -0.1f, 0.5f, 3));
                }
            }
            XUISection xuisection2 = this.Menu.AddSection(new XUISection("Quick Settings"));
            xuisection2.AddItem(new XUIToggleButton("Ultra Smooth FPS", XSingleton<XDebug>.Instance.UltraSmoothFPS));
            xuisection2.AddItem(new XUIToggleButton("Custom Music", XSingleton<XDebug>.Instance.PlayCustomMusic));
            xuisection2.AddItem(new XUIFloatAdjuster("Global Speed Multiplier", XSingleton<XDebug>.Instance.EverySpeedMultiplier, -0.1f, 0.2f, 3));
            xuisection2.AddItem(new XUIToggleButton("Free Water Sliding", XSingleton<XDebug>.Instance.Moveset_FreeWaterSliding));
            xuisection2.AddItem(new XUIToggleButton("Wall Jump", XSingleton<XDebug>.Instance.Moveset_WallJumping));
            xuisection2.AddItem(new XUIToggleButton("Climb All Walls", XSingleton<XDebug>.Instance.Moveset_ClimbAll));
            xuisection2.AddItem(new XUIToggleButton("Boost", XSingleton<XDebug>.Instance.Moveset_Boost));
            xuisection2.AddItem(new XUIToggleButton("Speedometer", XSingleton<XDebug>.Instance.Extra_DisplaySpeedo));
            if (XDebug.COMMENT)
            {
                xuisection2.AddItem(new XUIToggleButton("<color=#4a4a4a>A</color>fter <color=#4a4a4a>H</color>oming <color=#4a4a4a>M</color>ovement", XSingleton<XDebug>.Instance.Moveset_AHMovement));
                xuisection2.AddItem(new XUIFloatAdjuster("<color=#4a4a4a>AHM</color> Max Speed", XSingleton<XDebug>.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
            }
            else
            {
                xuisection2.AddItem(new XUIToggleButton("After Homing Movement", XSingleton<XDebug>.Instance.Moveset_AHMovement));
                xuisection2.AddItem(new XUIFloatAdjuster("AHM Max Speed", XSingleton<XDebug>.Instance.Moveset_AHMovementMaxSpeed, -1f, 2f, 1, 0f, float.PositiveInfinity));
            }
            XUISection xuisection3 = this.Menu.AddSection(new XUISection("Cheats"));
            xuisection3.AddItem(new XUIToggleButton("Always Invincible", XSingleton<XDebug>.Instance.Invincible));
            xuisection3.AddItem(new XUIToggleButton("Infinite Gauge", XSingleton<XDebug>.Instance.InfiniteGauge));
            xuisection3.AddItem(new XUIToggleButton("Maxed Out Gems", XSingleton<XDebug>.Instance.MaxedOutGems));
            xuisection3.AddItem(new XUIFunctionButton("Get All Gems", delegate
            {
                XDebug.Cheats.GetAllGems();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Light Memory Shard", delegate
            {
                XDebug.Cheats.GetLightMemoryShard();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Lotus of Resilience", delegate
            {
                XDebug.Cheats.GetLotus();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Flame of Control", delegate
            {
                XDebug.Cheats.GetFlame();
            }));
            xuisection3.AddItem(new XUIFunctionButton("Get Sigil of Awakening", delegate
            {
                XDebug.Cheats.GetSigil();
            }));
            xuisection3.AddItem(new XUIToggleButton("Infinite Rings", XSingleton<XDebug>.Instance.InfiniteRings));
            xuisection3.AddItem(new XUIToggleButton("Infinite Lives", XSingleton<XDebug>.Instance.InfiniteLives));
            xuisection3.AddItem(new XUIStringInput("Teleport location", XSingleton<XDebug>.Instance.TeleportLocation, null, TMP_InputField.CharacterValidation.None));
            xuisection3.AddItem(new XUIFunctionButton("TELEPORT", delegate
            {
                XSingleton<XDebug>.Instance.TeleportToSection(XSingleton<XDebug>.Instance.TeleportLocation.Value);
            }));
            xuisection3.AddItem(new XUIToggleButton("Water immunity", XSingleton<XDebug>.Instance.Cheat_IgnoreWaterDeath));
            xuisection3.AddItem(new XUIToggleButton("Faster Chain Jump", XSingleton<XDebug>.Instance.Cheat_ChainJumpZeroDelay));
            XUISection xuisection4 = this.Menu.AddSection(new XUISection("Advanced Speed Control"));
            xuisection4.AddItem(new XUIFloatAdjuster("Ground", XSingleton<XDebug>.Instance.SMGround, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Air", XSingleton<XDebug>.Instance.SMAir, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Spindash", XSingleton<XDebug>.Instance.SMSpindash, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Flying", XSingleton<XDebug>.Instance.SMFly, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Climbing", XSingleton<XDebug>.Instance.SMClimb, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("Homing", XSingleton<XDebug>.Instance.SMHoming, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("H. Attack", XSingleton<XDebug>.Instance.SMHomingAttackFasterBy, -0.05f, 0.1f, 3));
            xuisection4.AddItem(new XUIFloatAdjuster("After H. Rotation", XSingleton<XDebug>.Instance.SMAfterHomingRotation, -0.05f, 0.1f, 3));
            XUISection xuisection5 = this.Menu.AddSection(new XUISection("Boost"));
            xuisection5.AddItem(new XUIFloatAdjuster("Base Speed", XSingleton<XDebug>.Instance.Boost_BaseSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Delta Speed", XSingleton<XDebug>.Instance.Boost_NextLevelDeltaSpeed, -10f, 10f, 0, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Turn speed", XSingleton<XDebug>.Instance.Boost_RotSpeed, -1f, 1f, 1, 0f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Acceleration time", XSingleton<XDebug>.Instance.Boost_AccelTime, -0.1f, 0.25f, 2, 0.01f, float.PositiveInfinity));
            xuisection5.AddItem(new XUIFloatAdjuster("Level Up Threshold", XSingleton<XDebug>.Instance.Boost_NextLevelThreshold, -2f, 3f, 1, 0f, float.PositiveInfinity));
            this.Menu.AddSection(new XUISection("Other")).AddItem(new XUIToggleButton("Old Camera Controls", XSingleton<XDebug>.Instance.Other_OgCameraControls)).AddItem(new XUIToggleButton("FDT Fix (Beta)", XSingleton<XDebug>.Instance.Other_UltraFPSFix))
                .AddItem(new XUIToggleButton("Version check", XSingleton<XDebug>.Instance.Other_CheckP06Version));
            XUISection xuisection6 = this.Menu.AddSection(new XUISection("Saving"));
            xuisection6.AddItem(new XUIFunctionButton("Save Settings", delegate
            {
                XSingleton<XDebug>.Instance.SaveSettings();
            }));
            xuisection6.AddItem(new XUIFunctionButton("Load Settings", delegate
            {
                XSingleton<XDebug>.Instance.LoadSettings();
            }));
            xuisection6.AddItem(new XUIToggleButton("Load automatically", XSingleton<XDebug>.Instance.Saving_AutoLoad));
            xuisection6.AddItem(new XUIToggleButton("Save automatically", XSingleton<XDebug>.Instance.Saving_AutoSave));
        }



        private void Update()
        {
        }

        public Canvas Canvas;

        public XUIMenu Menu;
    }


    public class XSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (XSingleton<T>._instance == null)
                {
                    XSingleton<T>._instance = UnityEngine.Object.FindObjectOfType<T>();
                    if (XSingleton<T>._instance == null)
                    {
                        XSingleton<T>._instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                    }
                }
                return XSingleton<T>._instance;
            }
        }

        private void Start()
        {
            if (this != XSingleton<T>._instance)
            {
                UnityEngine.Object.Destroy(this);
                return;
            }
        }

        private static T _instance;
    }


    public struct XUIConfig
    {
        public static float item_width = 250f;

        public static float item_height = 30f;

        public static float title_item_height = XUIConfig.item_height * 1.25f;

        public static float title_text_margin = 7.5f;

        public static float menu_spacing = 12f;

        public static int menu_padding = 12;

        public static float section_spacing = 5f;

        public static Vector2 menu_size = new Vector2(290f, 525f);

        public static Vector2 menu_pos = new Vector2(20f, 20f);

        public static Vector2 menu_pivot = new Vector2(0f, 0f);

        public static Vector2 menu_anchor_min = new Vector2(0f, 0f);

        public static Vector2 menu_anchor_max = new Vector2(0f, 0f);
    }


    public class XUIFloatAdjuster : XUIItem
    {
        public float Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this.inputField.text = value.ToString(this.format);
                this._value = value;
            }
        }

        public void BuildVisuals(string name)
        {
            this.gameObject.name = "float adjuster: " + name;
            Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
            GameObject gameObject = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources));
            gameObject.transform.SetParent(this.gameObject.transform, false);
            TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
            component.enableAutoSizing = true;
            component.fontSizeMin = 8f;
            component.text = name;
            component.margin = Vector4.one * 6f;
            component.alignment = TextAlignmentOptions.Center;
            component.enableWordWrapping = false;
            component.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, XUIConfig.item_height);
            RectTransform component2 = gameObject.GetComponent<RectTransform>();
            component2.pivot = new Vector2(0f, 0.5f);
            component2.sizeDelta = new Vector2(sizeDelta.x * 0.5f, component2.sizeDelta.y);
            RectTransform rectTransform = component2;
            RectTransform rectTransform2 = component2;
            Vector2 vector = new Vector2(0f, 0.5f);
            rectTransform2.anchorMax = vector;
            rectTransform.anchorMin = vector;
            GameObject gameObject2 = TMP_DefaultControls.CreateInputField(default(TMP_DefaultControls.Resources));
            gameObject2.transform.SetParent(this.gameObject.transform, false);
            RectTransform component3 = gameObject2.GetComponent<RectTransform>();
            component3.pivot = new Vector2(1f, 0.5f);
            component3.sizeDelta = new Vector2(sizeDelta.x * 0.5f, 0f);
            component3.anchorMin = new Vector2(1f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
            ColorBlock colorBlock = default(ColorBlock);
            Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
            colorBlock.pressedColor = color + color2 / 2f;
            colorBlock.highlightedColor = colorBlock.pressedColor;
            colorBlock.selectedColor = colorBlock.pressedColor;
            colorBlock.normalColor = color;
            colorBlock.colorMultiplier = 1f;
            colorBlock.fadeDuration = 0.09f;
            this.inputField = gameObject2.GetComponent<TMP_InputField>();
            this.inputField.textComponent.alignment = TextAlignmentOptions.Center;
            this.inputField.textComponent.color = new Color(1f, 0.6039216f, 0f, 1f);
            this.inputField.colors = colorBlock;
            this.inputField.selectionColor = Color.black * 0.8f;
            this.inputField.characterValidation = TMP_InputField.CharacterValidation.Decimal;
            GameObject gameObject3 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
            gameObject3.transform.SetParent(this.inputField.transform, false);
            RectTransform component4 = gameObject3.GetComponent<RectTransform>();
            Vector2 vector2 = new Vector2(0f, 0.5f);
            component4.anchorMax = vector2;
            component4.anchorMin = vector2;
            component4.pivot = new Vector2(0f, 0.5f);
            component4.anchoredPosition = new Vector2(0f, 0f);
            component4.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
            component4.position += new Vector3(0f, 0f, -3f);
            TextMeshProUGUI componentInChildren = gameObject3.GetComponentInChildren<TextMeshProUGUI>();
            componentInChildren.color = Color.white;
            componentInChildren.text = "<";
            this.L = gameObject3.GetComponent<Button>();
            GameObject gameObject4 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
            gameObject4.transform.SetParent(this.inputField.transform, false);
            RectTransform component5 = gameObject4.GetComponent<RectTransform>();
            vector2 = new Vector2(1f, 0.5f);
            component5.anchorMax = vector2;
            component5.anchorMin = vector2;
            component5.pivot = new Vector2(1f, 0.5f);
            component5.anchoredPosition = new Vector2(0f, 0f);
            component5.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
            component5.position += new Vector3(0f, 0f, -3f);
            TextMeshProUGUI componentInChildren2 = gameObject4.GetComponentInChildren<TextMeshProUGUI>();
            componentInChildren2.color = Color.white;
            componentInChildren2.text = ">";
            this.R = gameObject4.GetComponent<Button>();
            ColorBlock colorBlock2 = default(ColorBlock);
            colorBlock2.normalColor = Color.black * 0f;
            colorBlock2.highlightedColor = (colorBlock2.pressedColor = (colorBlock2.selectedColor = colorBlock.highlightedColor));
            colorBlock2.fadeDuration = 0f;
            colorBlock2.colorMultiplier = 0f;
            this.L.colors = colorBlock2;
            this.R.colors = colorBlock2;
        }

        public XUIFloatAdjuster(string name, XValue<float> BindTo, float stepL = -0.25f, float stepR = 0.5f, int precision = 3)
            : this(name, BindTo, stepL, stepR, precision, float.NegativeInfinity, float.PositiveInfinity)
        {
        }

        public XUIFloatAdjuster(string name, XValue<float> BindTo, float stepL = -0.25f, float stepR = 0.5f, int precision = 3, float minValue = float.NegativeInfinity, float maxValue = float.PositiveInfinity)
        {
            XUIFloatAdjuster reference = this;
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.BindedXValue = BindTo;
            this.format = "0." + new string('0', precision);
            this.BuildVisuals(name);
            base.Name = name;
            this.Value = BindTo.Value;
            this.inputField.onSubmit.AddListener(delegate (string s)
            {
                reference.ApplyChanges = true;
                XSingleton<XDebug>.Instance.Invoke(delegate
                {
                    reference.inputField.interactable = false;
                    reference.inputField.interactable = true;
                }, 0f);
            });
            this.inputField.onEndEdit.AddListener(delegate (string s)
            {
                if (!reference.ApplyChanges || string.IsNullOrEmpty(s))
                {
                    reference.Value = BindTo.Value;
                }
                else
                {
                    BindTo.Value = reference.Clamped(float.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture.NumberFormat));
                }
                reference.ApplyChanges = false;
            });
            this.L.onClick.AddListener(delegate
            {
                BindTo.Value = reference.Clamped(BindTo.Value + stepL);
            });
            this.R.onClick.AddListener(delegate
            {
                BindTo.Value = reference.Clamped(BindTo.Value + stepR);
            });
            BindTo.OnChangeValue += delegate (float newValue)
            {
                reference.Value = newValue;
            };
        }

        private float Clamped(float value)
        {
            int num = this.format.Length - 2;
            float num2 = Mathf.Pow(10f, (float)num);
            return Mathf.Round(Mathf.Clamp(value, this.MinValue, this.MaxValue) * num2) / num2;
        }

        private string format;

        private bool ApplyChanges;

        private Button L;

        private Button R;

        private TMP_InputField inputField;

        private float _value;

        public readonly XValue<float> BindedXValue;

        private float MinValue = float.NegativeInfinity;

        private float MaxValue = float.PositiveInfinity;
    }


    public class XUIFunctionButton : XUIItem
    {
        private static Color ColorFromHex(int hex)
        {
            return new Color((float)((hex & 16711680) >> 16) / 255f, (float)((hex & 65280) >> 8) / 255f, (float)(hex & 255) / 255f);
        }

        public void BuildVisuals(string name)
        {
            this.gameObject.name = "function button: " + name;
            Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
            this.gameObject.GetComponent<Image>().enabled = false;
            GameObject gameObject = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
            gameObject.transform.SetParent(this.gameObject.transform, false);
            gameObject.GetComponentInChildren<TextMeshProUGUI>().color = XUIFunctionButton.ColorFromHex(4050943);
            gameObject.GetComponentInChildren<TextMeshProUGUI>().text = name;
            RectTransform component = gameObject.GetComponent<RectTransform>();
            component.pivot = new Vector2(0.5f, 0.5f);
            component.anchorMin = new Vector2(0.5f, 0.5f);
            component.anchorMax = new Vector2(0.5f, 0.5f);
            component.sizeDelta = new Vector2(sizeDelta.x - 10f, XUIConfig.item_height);
            Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
            ColorBlock colorBlock = default(ColorBlock);
            Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
            colorBlock.pressedColor = color + color2;
            colorBlock.highlightedColor = colorBlock.pressedColor;
            colorBlock.selectedColor = colorBlock.pressedColor;
            colorBlock.normalColor = color;
            colorBlock.colorMultiplier = 1f;
            colorBlock.fadeDuration = 0.09f;
            this.button = gameObject.GetComponent<Button>();
            this.button.colors = colorBlock;
        }

        public XUIFunctionButton(string name, Action action)
        {
            this.BuildVisuals(name);
            this.button.onClick.AddListener(delegate
            {
                action();
            });
            base.Name = name;
            this.Action = action;
        }

        private Button button;

        public readonly Action Action;
    }
    public class XUIItem
    {
        public XUIItem()
        {
            this.gameObject = DefaultControls.CreatePanel(default(DefaultControls.Resources));
            this.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);
            this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(XUIConfig.item_width, XUIConfig.item_height);
        }

        public string Name { get; set; }

        public GameObject gameObject;
    }

    public class XUIMenu
    {
        public List<XUISection> Sections
        {
            get
            {
                return this.sections;
            }
        }

        public XUISection AddSection(XUISection section)
        {
            section.gameObject.transform.SetParent(this.container.transform, false);
            this.sections.Add(section);
            return section;
        }

        public XUIMenu(string title, KeyCode toggleKey)
        {
            if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
            {
                UnityEngine.Object.DontDestroyOnLoad(new GameObject("EventSystem").AddComponent<EventSystem>().gameObject.AddComponent<StandaloneInputModule>());
            }
            this.gameObject = DefaultControls.CreateScrollView(default(DefaultControls.Resources));
            this.gameObject.transform.SetParent(XSingleton<XDebug>.Instance.XCanvas.transform, false);
            RectTransform component = this.gameObject.GetComponent<RectTransform>();
            ScrollRect component2 = this.gameObject.GetComponent<ScrollRect>();
            component.pivot = XUIConfig.menu_pivot;
            component.anchorMin = XUIConfig.menu_anchor_min;
            component.anchorMax = XUIConfig.menu_anchor_max;
            component.sizeDelta = XUIConfig.menu_size;
            component.anchoredPosition = XUIConfig.menu_pos;
            component2.scrollSensitivity = 40f;
            component2.movementType = ScrollRect.MovementType.Clamped;
            component2.horizontal = false;
            component2.verticalScrollbar.handleRect.sizeDelta = new Vector2(10f, 1f);
            this.container = this.gameObject.transform.GetChild(0).GetChild(0).gameObject;
            RectTransform component3 = this.container.GetComponent<RectTransform>();
            component3.anchorMin = new Vector2(0f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            ContentSizeFitter contentSizeFitter = this.container.AddComponent<ContentSizeFitter>();
            VerticalLayoutGroup verticalLayoutGroup = this.container.AddComponent<VerticalLayoutGroup>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            verticalLayoutGroup.childControlWidth = false;
            verticalLayoutGroup.padding = new RectOffset(XUIConfig.menu_padding, XUIConfig.menu_padding, XUIConfig.menu_padding, XUIConfig.menu_padding);
            verticalLayoutGroup.spacing = XUIConfig.menu_spacing;
            this.sections = new List<XUISection>();
            Image component4 = this.gameObject.transform.GetChild(2).GetComponent<Image>();
            component4.enabled = false;
            component4.transform.GetChild(0).GetChild(0).GetComponent<Image>()
                .color = Color.white;
            XDebug.Comment("new Color(0.4283019f, 0.7813739f, 1f, 0.5f);");
            XSingleton<XDebug>.Instance.XCanvas.gameObject.AddComponent<XUIMenu.XMenuController>().Init(this, toggleKey);
        }

        public event Action OnClose;

        public event Action OnOpen;

        private readonly GameObject container;

        private readonly List<XUISection> sections;

        public readonly GameObject gameObject;

        public class XMenuController : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(this.KeyToggle))
                {
                    Cursor.visible = !this.menu.gameObject.activeSelf;
                    this.menu.gameObject.SetActive(!this.menu.gameObject.activeSelf);
                    if (this.menu.gameObject.activeSelf && this.menu.OnOpen != null)
                    {
                        this.menu.OnOpen();
                    }
                    if (!this.menu.gameObject.activeSelf && this.menu.OnClose != null)
                    {
                        this.menu.OnClose();
                    }
                }
            }

            public void Init(XUIMenu menu, KeyCode KeyToggle)
            {
                this.menu = menu;
                this.KeyToggle = KeyToggle;
                menu.gameObject.SetActive(false);
            }

            public XUIMenu menu;

            public KeyCode KeyToggle;
        }
    }

    public class XUISection
    {
        public XUISection(string name = "New P06X Section")
        {
            this.gameObject = DefaultControls.CreatePanel(default(DefaultControls.Resources));
            this.gameObject.name = "section: " + name;
            this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(XUIConfig.item_width, 0f);
            VerticalLayoutGroup verticalLayoutGroup = this.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = XUIConfig.section_spacing;
            verticalLayoutGroup.padding.bottom = (int)XUIConfig.section_spacing;
            verticalLayoutGroup.childControlHeight = false;
            TextMeshProUGUI component = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources)).GetComponent<TextMeshProUGUI>();
            this.TitleText = component;
            RectTransform component2 = DefaultControls.CreatePanel(default(DefaultControls.Resources)).GetComponent<RectTransform>();
            component2.name = "title panel";
            component2.sizeDelta = new Vector2(XUIConfig.item_width, XUIConfig.title_item_height);
            component2.transform.SetParent(this.gameObject.transform, false);
            component.transform.SetParent(component2.transform, false);
            RectTransform component3 = component.GetComponent<RectTransform>();
            component3.offsetMin = Vector2.zero;
            component3.offsetMax = Vector2.zero;
            component3.anchorMin = new Vector2(0f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            component.text = name;
            component.margin = Vector4.one * XUIConfig.title_text_margin;
            component.alignment = TextAlignmentOptions.CenterGeoAligned;
            component.enableAutoSizing = true;
            Button button = component2.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(delegate
            {
                this.Toggle();
            });
            this.items = new List<XUIItem>();
        }

        public XUISection AddItem(XUIItem item)
        {
            item.gameObject.transform.SetParent(this.gameObject.transform, false);
            this.items.Add(item);
            return this;
        }

        public List<XUIItem> Items
        {
            get
            {
                return this.items;
            }
        }

        public void Toggle()
        {
            this.Toggle(this.IsCollapsed);
        }

        public bool IsCollapsed
        {
            get
            {
                return this._isCollapsed;
            }
            set
            {
                this._isCollapsed = value;
                Transform transform = this.gameObject.transform;
                int childCount = transform.childCount;
                for (int i = 1; i < childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(!this._isCollapsed);
                }
                if (!this._isCollapsed)
                {
                    if (this.TitleText.text.Length > 3 && this.TitleText.text.Substring(this.TitleText.text.Length - 3, 3) == "  +")
                    {
                        this.TitleText.text = this.TitleText.text.Substring(0, this.TitleText.text.Length - 3);
                        return;
                    }
                }
                else
                {
                    TextMeshProUGUI titleText = this.TitleText;
                    titleText.text += "  +";
                }
            }
        }

        public void Toggle(bool Expand)
        {
            this.IsCollapsed = !Expand;
        }

        public GameObject gameObject;

        private readonly List<XUIItem> items;

        private TextMeshProUGUI TitleText;

        private bool _isCollapsed;
    }

    public class XUIStringInput : XUIItem
    {
        public string Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this.inputField.text = value;
            }
        }

        public void BuildVisuals(string name)
        {
            this.gameObject.name = "string input: " + name;
            Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
            GameObject gameObject = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources));
            gameObject.transform.SetParent(this.gameObject.transform, false);
            TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
            component.enableAutoSizing = true;
            component.fontSizeMin = 8f;
            component.text = name;
            component.margin = Vector4.one * 6f;
            component.alignment = TextAlignmentOptions.Center;
            component.enableWordWrapping = false;
            component.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, XUIConfig.item_height);
            RectTransform component2 = gameObject.GetComponent<RectTransform>();
            component2.pivot = new Vector2(0f, 0.5f);
            component2.sizeDelta = new Vector2(sizeDelta.x * 0.5f, component2.sizeDelta.y);
            RectTransform rectTransform = component2;
            RectTransform rectTransform2 = component2;
            Vector2 vector = new Vector2(0f, 0.5f);
            rectTransform2.anchorMax = vector;
            rectTransform.anchorMin = vector;
            GameObject gameObject2 = TMP_DefaultControls.CreateInputField(default(TMP_DefaultControls.Resources));
            gameObject2.transform.SetParent(this.gameObject.transform, false);
            RectTransform component3 = gameObject2.GetComponent<RectTransform>();
            component3.pivot = new Vector2(1f, 0.5f);
            component3.sizeDelta = new Vector2(sizeDelta.x * 0.5f, 0f);
            component3.anchorMin = new Vector2(1f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
            ColorBlock colorBlock = default(ColorBlock);
            Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
            colorBlock.pressedColor = color + color2 / 2f;
            colorBlock.highlightedColor = colorBlock.pressedColor;
            colorBlock.selectedColor = colorBlock.pressedColor;
            colorBlock.normalColor = (colorBlock.disabledColor = color);
            colorBlock.colorMultiplier = 1f;
            colorBlock.fadeDuration = 0.09f;
            this.inputField = gameObject2.GetComponent<TMP_InputField>();
            this.inputField.textComponent.alignment = TextAlignmentOptions.Center;
            this.inputField.textComponent.color = new Color(1f, 0.6039216f, 0f, 1f);
            this.inputField.colors = colorBlock;
            this.inputField.selectionColor = Color.black * 0.8f;
            this.inputField.characterValidation = this.validation;
            if (this.opts != null)
            {
                GameObject gameObject3 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
                gameObject3.transform.SetParent(this.inputField.transform, false);
                RectTransform component4 = gameObject3.GetComponent<RectTransform>();
                Vector2 vector2 = new Vector2(0f, 0.5f);
                component4.anchorMax = vector2;
                component4.anchorMin = vector2;
                component4.pivot = new Vector2(0f, 0.5f);
                component4.anchoredPosition = new Vector2(0f, 0f);
                component4.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
                component4.position += new Vector3(0f, 0f, -3f);
                TextMeshProUGUI componentInChildren = gameObject3.GetComponentInChildren<TextMeshProUGUI>();
                componentInChildren.color = Color.white;
                componentInChildren.text = "<";
                this.L = gameObject3.GetComponent<Button>();
                GameObject gameObject4 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
                gameObject4.transform.SetParent(this.inputField.transform, false);
                RectTransform component5 = gameObject4.GetComponent<RectTransform>();
                vector2 = new Vector2(1f, 0.5f);
                component5.anchorMax = vector2;
                component5.anchorMin = vector2;
                component5.pivot = new Vector2(1f, 0.5f);
                component5.anchoredPosition = new Vector2(0f, 0f);
                component5.sizeDelta = new Vector2(XUIConfig.item_width * 0.5f * 0.2f, XUIConfig.item_height);
                component5.position += new Vector3(0f, 0f, -3f);
                TextMeshProUGUI componentInChildren2 = gameObject4.GetComponentInChildren<TextMeshProUGUI>();
                componentInChildren2.color = Color.white;
                componentInChildren2.text = ">";
                this.R = gameObject4.GetComponent<Button>();
                ColorBlock colorBlock2 = default(ColorBlock);
                colorBlock2.normalColor = Color.black * 0f;
                colorBlock2.highlightedColor = (colorBlock2.pressedColor = (colorBlock2.selectedColor = colorBlock.highlightedColor));
                colorBlock2.fadeDuration = 0f;
                colorBlock2.colorMultiplier = 0f;
                this.L.colors = colorBlock2;
                this.R.colors = colorBlock2;
            }
        }

        public XUIStringInput(string name, XValue<string> BindTo, string[] options = null, TMP_InputField.CharacterValidation validation = TMP_InputField.CharacterValidation.None)
        {
            XUIStringInput rf = this;
            this.BindedXValue = BindTo;
            this.opts = options;
            this.validation = validation;
            this.BuildVisuals(name);
            base.Name = name;
            this.Value = BindTo.Value;
            this.inputField.onSubmit.AddListener(delegate (string s)
            {
                rf.ApplyChanges = true;
                XSingleton<XDebug>.Instance.Invoke(delegate
                {
                    rf.inputField.interactable = false;
                    rf.inputField.interactable = true;
                }, 0f);
            });
            this.inputField.onEndEdit.AddListener(delegate (string s)
            {
                if (!rf.ApplyChanges || string.IsNullOrEmpty(s))
                {
                    rf.Value = BindTo.Value;
                }
                else
                {
                    BindTo.Value = s;
                }
                rf.ApplyChanges = false;
            });
            if (options != null && options.Length != 0)
            {
                this.L.onClick.AddListener(delegate
                {
                    rf.optidx = (rf.optidx - 1 + options.Length) % options.Length;
                    BindTo.Value = rf.opts[rf.optidx];
                });
                this.R.onClick.AddListener(delegate
                {
                    rf.optidx = (rf.optidx + 1) % options.Length;
                    BindTo.Value = rf.opts[rf.optidx];
                });
            }
            BindTo.OnChangeValue += delegate (string newValue)
            {
                rf.Value = newValue;
            };
        }

        private bool ApplyChanges;

        private int optidx;

        private string[] opts;

        private TMP_InputField.CharacterValidation validation;

        private Button L;

        private Button R;

        private TMP_InputField inputField;

        private string _value;

        public readonly XValue<string> BindedXValue;
    }


    public class XUIToggleButton : XUIItem
    {
        private static Color ColorFromHex(int hex)
        {
            return new Color((float)((hex & 16711680) >> 16) / 255f, (float)((hex & 65280) >> 8) / 255f, (float)(hex & 255) / 255f);
        }

        public bool State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.actualButton.GetComponentInChildren<TextMeshProUGUI>().text = (value ? "<color=#00ee00>ENABLED</color>" : "<color=#ee0000>DISABLED</color>");
                this.state = value;
            }
        }

        public void BuildVisuals(string name)
        {
            this.gameObject.name = "toggle button: " + name;
            Vector2 sizeDelta = this.gameObject.GetComponent<RectTransform>().sizeDelta;
            GameObject gameObject = TMP_DefaultControls.CreateText(default(TMP_DefaultControls.Resources));
            gameObject.transform.SetParent(this.gameObject.transform, false);
            TextMeshProUGUI component = gameObject.GetComponent<TextMeshProUGUI>();
            component.enableAutoSizing = true;
            component.fontSizeMin = 8f;
            component.text = name;
            component.margin = Vector4.one * 6f;
            component.alignment = TextAlignmentOptions.Center;
            component.enableWordWrapping = false;
            component.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x / 2f, XUIConfig.item_height);
            RectTransform component2 = gameObject.GetComponent<RectTransform>();
            component2.pivot = new Vector2(0f, 0.5f);
            component2.sizeDelta = new Vector2(sizeDelta.x * 0.5f, component2.sizeDelta.y);
            RectTransform rectTransform = component2;
            RectTransform rectTransform2 = component2;
            Vector2 vector = new Vector2(0f, 0.5f);
            rectTransform2.anchorMax = vector;
            rectTransform.anchorMin = vector;
            GameObject gameObject2 = TMP_DefaultControls.CreateButton(default(TMP_DefaultControls.Resources));
            gameObject2.transform.SetParent(this.gameObject.transform, false);
            RectTransform component3 = gameObject2.GetComponent<RectTransform>();
            component3.pivot = new Vector2(1f, 0.5f);
            component3.sizeDelta = new Vector2(sizeDelta.x * 0.5f, 0f);
            component3.anchorMin = new Vector2(1f, 0f);
            component3.anchorMax = new Vector2(1f, 1f);
            Color color = new Color(0.24f, 0.24f, 0.24f, 1f);
            ColorBlock colorBlock = default(ColorBlock);
            Color color2 = new Color(0.1f, 0.1f, 0.1f, 0f);
            colorBlock.pressedColor = color + color2;
            colorBlock.highlightedColor = colorBlock.pressedColor;
            colorBlock.selectedColor = colorBlock.pressedColor;
            colorBlock.normalColor = color;
            colorBlock.colorMultiplier = 1f;
            colorBlock.fadeDuration = 0.09f;
            this.actualButton = gameObject2.GetComponent<Button>();
            this.actualButton.colors = colorBlock;
            this.button = this.actualButton;
        }

        public XUIToggleButton(string name, XValue<bool> BindTo)
        {
            this.BindedXValue = BindTo;
            this.BuildVisuals(name);
            base.Name = name;
            this.State = BindTo.Value;
            this.button.onClick.AddListener(delegate
            {
                BindTo.Value = !BindTo.Value;
            });
            BindTo.OnChangeValue += delegate (bool to)
            {
                this.State = to;
            };
        }

        private Button actualButton;

        private bool state;

        public Button button;

        public readonly XValue<bool> BindedXValue;
    }

    public static class XUtility
    {
        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(XUtility.InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
            yield break;
        }

        public static void SerializeObjectToXml<T>(T obj, string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (StreamWriter streamWriter = new StreamWriter(path))
            {
                xmlSerializer.Serialize(streamWriter, obj);
            }
        }

        public static T DeserializeXmlToObject<T>(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            T t;
            using (StreamReader streamReader = new StreamReader(path))
            {
                t = (T)((object)xmlSerializer.Deserialize(streamReader));
            }
            return t;
        }

        public static bool IsIn<T>(this T value, params T[] set)
        {
            foreach (T t in set)
            {
                if (value.Equals(t))
                {
                    return true;
                }
            }
            return false;
        }
    }


    public class XValue<T>
    {
        public XValue(Action<T> action, T initValue = default(T))
        {
            this.ChangeValue = action;
            this.Value = initValue;
        }

        public event Action<T> OnChangeValue;
        public T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this.ChangeValue(value);
                Action<T> onChangeValue = this.OnChangeValue;
                if (onChangeValue == null)
                {
                    return;
                }
                onChangeValue(value);
            }
        }

        private Action<T> ChangeValue;

        private T _value;
    }
















}
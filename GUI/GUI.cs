using Photon.Pun;
using Photon.Realtime;
using Repo;
using Repo.Player;
using RePo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace repogui
{
    public class MenuGUI : MonoBehaviour
    {
        private static MenuGUI _instance;
        private static bool _isDragging = false;
        private static Vector2 _dragStartMousePosition;
        private static Vector2 _dragStartWindowPosition;

        public static Rect GUIRect = new Rect(Screen.width / 2 - 350, Screen.height / 2 - 250, 700, 500);
        public static int selected_tab = 0;
        public static readonly string[] tabnames = { "PLAYER", "VISUAL", "OTHER", "INFO", "PLAYER LIST" };
        public static bool ShowWindow = true;

        private static Vector2 _scrollPlayer, _scrollVisual, _scrollOther, _scrollInfo, _scrollPlayerList;
        private static bool _autoRefreshPlayers = true;
        private static float _nextRefreshAt;

        private static bool _infStamOn;
        private static bool _crouchDelayOn;
        private static float _crouchDelay = 0.00f;
        private static float _grabRange = 10f;
        private static float _throwStrength = 10f;
        private static float _jumpForce = 20f;
        private static bool _mj;

        private static bool _noFogOn = false;
        private static bool _rgbFogOn;
        private static bool _fb;

        private static bool _playerEsp = false;
        private static bool _playerNameEsp = false;
        private static bool _playerDistanceEsp = false;
        private static bool _playerTracer = false;
        private static bool _playerSkeletonEsp = false;

        private static bool _itemEsp = false;
        private static bool _itemNameEsp = false;
        private static bool _itemTracer = false;
        private static bool _showValuableItems = true;
        private static bool _showCartItems = true;

        private static float _playerMaxDistance = 1000f;
        private static float _itemMaxDistance = 500f;

        public static bool showEnemyESP = false;
        public static bool showEnemyBoxes = true;
        public static bool showEnemyNames = true;
        public static bool showEnemyDistance = true;

        private static Color _playerEspColor = Color.green;
        private static Color _playerTracerColor = Color.yellow;
        private static Color _playerSkeletonColor = Color.red;
        private static Color _itemEspColor = Color.cyan;
        private static Color _itemTracerColor = Color.magenta;

        private static float _windowAlpha = 0f;
        private static float _windowScale = 0.8f;
        private static float _windowRotation = 0f;
        private static float _tabGlow = 0f;
        private static float _pulseEffect = 0f;
        private static float _slideInPosition = -100f;
        private static List<FloatingElement> _floatingElements = new List<FloatingElement>();

        private static Texture2D _cursorTexture;
        private static Texture2D _cursorClickTexture;
        private static bool _cursorChanged = false;

        private static Texture2D _gradientTexture;
        private static Texture2D _noiseTexture;
        private static Texture2D _glowTexture;
        private static Texture2D _checkmarkTexture;

        private static GUIStyle _windowStyle;
        private static GUIStyle _tabButtonStyle;
        private static GUIStyle _tabButtonSelectedStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _toggleStyle;
        private static GUIStyle _sliderStyle;
        private static GUIStyle _sliderThumbStyle;
        private static GUIStyle _labelStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _buttonHoverStyle;
        private static GUIStyle _scrollViewStyle;
        private static GUIStyle _playerInfoStyle;
        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _valueStyle;

        private static bool _stylesInitialized = false;

        class FloatingElement
        {
            public Rect position;
            public float speed;
            public float amplitude;
            public float phase;
            public Color color;
            public float size;
        }

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            InitializeResources();
            StartCoroutine(AnimateWindowIn());
            StartCoroutine(PulseAnimation());
            StartCoroutine(GlowAnimation());

            for (int i = 0; i < 8; i++)
            {
                _floatingElements.Add(new FloatingElement
                {
                    position = new Rect(
                        UnityEngine.Random.Range(-100, Screen.width + 100),
                        UnityEngine.Random.Range(-100, Screen.height + 100),
                        10 + UnityEngine.Random.Range(5, 25),
                        10 + UnityEngine.Random.Range(5, 25)
                    ),
                    speed = UnityEngine.Random.Range(0.5f, 2f),
                    amplitude = UnityEngine.Random.Range(10, 50),
                    phase = UnityEngine.Random.Range(0, Mathf.PI * 2),
                    color = new Color(
                        UnityEngine.Random.Range(0.3f, 0.6f),
                        UnityEngine.Random.Range(0.3f, 0.6f),
                        UnityEngine.Random.Range(0.8f, 1f),
                        0.1f
                    ),
                    size = UnityEngine.Random.Range(0.8f, 1.2f)
                });
            }
        }

        void OnDestroy()
        {
            _instance = null;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
            {
                ShowWindow = !ShowWindow;
                if (ShowWindow)
                    _instance.StartCoroutine(AnimateWindowIn());
            }

            for (int i = 0; i < _floatingElements.Count; i++)
            {
                var element = _floatingElements[i];
                element.position.x += Mathf.Sin(Time.time * element.speed + element.phase) * 0.5f;
                element.position.y += Mathf.Cos(Time.time * element.speed * 0.7f + element.phase) * 0.3f;
                _floatingElements[i] = element;
            }
            HandleWindowDragging();
        }

        private static void HandleWindowDragging()
        {
            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;

            Rect headerRect = new Rect(GUIRect.x, GUIRect.y, GUIRect.width, 40);

            bool isOverHeader = headerRect.Contains(mousePos);

            if (Input.GetMouseButtonDown(0) && isOverHeader && ShowWindow)
            {
                _isDragging = true;
                _dragStartMousePosition = mousePos;
                _dragStartWindowPosition = new Vector2(GUIRect.x, GUIRect.y);
            }

            if (_isDragging)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector2 mouseDelta = mousePos - _dragStartMousePosition;
                    GUIRect.x = _dragStartWindowPosition.x + mouseDelta.x;
                    GUIRect.y = _dragStartWindowPosition.y + mouseDelta.y;

                    GUIRect.x = Mathf.Clamp(GUIRect.x, 0, Screen.width - GUIRect.width);
                    GUIRect.y = Mathf.Clamp(GUIRect.y, 0, Screen.height - GUIRect.height);
                }
                else
                {
                    _isDragging = false;
                }
            }

            if (!_isDragging && _smoothMoveTarget.HasValue)
            {
                GUIRect.x = Mathf.Lerp(GUIRect.x, _smoothMoveTarget.Value.x, Time.deltaTime * 10);
                GUIRect.y = Mathf.Lerp(GUIRect.y, _smoothMoveTarget.Value.y, Time.deltaTime * 10);

                if (Vector2.Distance(new Vector2(GUIRect.x, GUIRect.y), _smoothMoveTarget.Value) < 1f)
                {
                    _smoothMoveTarget = null;
                }
            }
        }
        private static Vector2? _smoothMoveTarget = null;
        public static void SmoothMoveWindowTo(float x, float y)
        {
            _smoothMoveTarget = new Vector2(x, y);
        }

        void OnGUI()
        {
            if (!_stylesInitialized)
            {
                InitializeStyles();
            }

            if (!_cursorChanged)
            {
                _cursorTexture = CreateRoundedCursor(new Color(0, 0.8f, 1f, 1f), 32);
                _cursorClickTexture = CreateRoundedCursor(new Color(1f, 0.5f, 0f, 1f), 32);
                _cursorChanged = true;
            }

            if (ShowWindow)
            {
                Rect cursorRect = new Rect(Event.current.mousePosition.x - 16,
                                          Event.current.mousePosition.y - 16, 32, 32);
                GUI.DrawTexture(cursorRect, Event.current.type == EventType.MouseDown ?
                    _cursorClickTexture : _cursorTexture);
            }

            if (!ShowWindow && _windowAlpha > 0)
            {
                _windowAlpha = Mathf.Lerp(_windowAlpha, 0, Time.deltaTime * 10);
                if (_windowAlpha < 0.01f) _windowAlpha = 0;
            }

            if (!ShowWindow && _windowAlpha == 0) return;

            DrawAnimatedBackground();

            DrawFloatingElements();

            Matrix4x4 originalMatrix = GUI.matrix;

            Vector2 pivot = new Vector2(GUIRect.x + GUIRect.width / 2, GUIRect.y + GUIRect.height / 2);
            GUIUtility.RotateAroundPivot(Mathf.Sin(Time.time * 0.3f) * 0.5f * _windowRotation, pivot);
            GUIUtility.ScaleAroundPivot(Vector2.one * _windowScale, pivot);

            Color originalColor = GUI.color;
            GUI.color = new Color(1, 1, 1, _windowAlpha);
            GUIRect = GUI.Window(9999, GUIRect, DrawMainWindow, "", _windowStyle);
            GUI.color = originalColor;

            GUI.matrix = originalMatrix;


        }

        private void InitializeResources()
        {
            _gradientTexture = new Texture2D(256, 1);
            for (int x = 0; x < 256; x++)
            {
                float t = x / 255f;
                Color color = Color.Lerp(
                    new Color(0.1f, 0.1f, 0.2f, 1f),
                    new Color(0.05f, 0.05f, 0.1f, 1f),
                    t
                );
                _gradientTexture.SetPixel(x, 0, color);
            }
            _gradientTexture.Apply();

            _noiseTexture = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                    _noiseTexture.SetPixel(x, y, new Color(1, 1, 1, noise));
                }
            }
            _noiseTexture.Apply();

            _glowTexture = CreateGlowTexture(128, new Color(0, 0.8f, 1f, 0.3f));

            _checkmarkTexture = CreateCheckmarkTexture(16, Color.cyan);
        }

        private static void DrawAnimatedBackground()
        {
            float time = Time.time * 0.5f;
            Color bg1 = Color.Lerp(
                new Color(0.02f, 0.02f, 0.04f, 0.9f),
                new Color(0.03f, 0.01f, 0.02f, 0.9f),
                Mathf.Sin(time) * 0.5f + 0.5f
            );
            Color bg2 = Color.Lerp(
                new Color(0.01f, 0.02f, 0.03f, 0.9f),
                new Color(0.02f, 0.01f, 0.03f, 0.9f),
                Mathf.Cos(time * 0.7f) * 0.5f + 0.5f
            );
            GUI.color = bg1;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            DrawAnimatedLines();
        }

        private static void DrawAnimatedLines()
        {
            float time = Time.time;
            int lineCount = 8;

            for (int i = 0; i < lineCount; i++)
            {
                float offset = i * (Screen.height / lineCount);
                float y = (offset + time * 50) % Screen.height;
                float alpha = Mathf.Sin(time * 2 + i) * 0.1f + 0.1f;

                GUI.color = new Color(0, 0.5f, 1f, alpha);
                GUI.DrawTexture(new Rect(0, y, Screen.width, 1), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        private static void DrawFloatingElements()
        {
            foreach (var element in _floatingElements)
            {
                float pulse = Mathf.Sin(Time.time * element.speed + element.phase) * 0.5f + 0.5f;
                Color elementColor = element.color * (0.5f + pulse * 0.5f);

                GUI.color = elementColor;
                GUI.DrawTexture(element.position, Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        public static void DrawMainWindow(int id)
        {
            DrawWindowBackground();
            DrawWindowHeader();
            DrawTabs();
            DrawContentArea();
            DrawFooter();
            GUI.DragWindow(new Rect(0, 0, GUIRect.width, 40));
        }

        private static void DrawWindowBackground()
        {
            GUI.color = new Color(0.12f, 0.12f, 0.14f, 0.98f);
            GUI.DrawTexture(new Rect(0, 0, GUIRect.width, GUIRect.height), Texture2D.whiteTexture);

            if (_pulseEffect > 0)
            {
                GUI.color = new Color(0, 0.8f, 1f, _pulseEffect * 0.3f);
                GUI.DrawTexture(new Rect(-5, -5, GUIRect.width + 10, GUIRect.height + 10), _glowTexture);
            }

            GUI.color = new Color(1, 1, 1, 0.03f);
            GUI.DrawTextureWithTexCoords(new Rect(0, 0, GUIRect.width, GUIRect.height),
                _noiseTexture, new Rect(0, 0, GUIRect.width / 64f, GUIRect.height / 64f));

            GUI.color = Color.white;
        }

        private static void DrawWindowHeader()
        {
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 1f);
            GUI.DrawTexture(new Rect(0, 0, GUIRect.width, 50), Texture2D.whiteTexture);
            float linePos = Mathf.PingPong(Time.time * 100, GUIRect.width - 200);
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(100 + linePos, 48, 100, 2), Texture2D.whiteTexture);
            GUI.color = new Color(0.8f, 0.9f, 1f, 1f);
            GUI.Label(new Rect(20, 15, GUIRect.width - 40, 30), "⚡ REPO CONTROL PANEL ⚡", _headerStyle);
            Rect closeButtonRect = new Rect(GUIRect.width - 40, 15, 20, 20);
            bool isCloseHover = closeButtonRect.Contains(Event.current.mousePosition);
            GUI.color = isCloseHover ? Color.red : new Color(0.6f, 0.6f, 0.6f, 1f);
            if (GUI.Button(closeButtonRect, "✕", _buttonStyle))
            {
                ShowWindow = false;
            }
            GUI.color = Color.white;
        }

        private static void DrawTabs()
        {
            GUI.BeginGroup(new Rect(10, 60, GUIRect.width - 20, 40));

            float tabWidth = (GUIRect.width - 40) / tabnames.Length;

            for (int i = 0; i < tabnames.Length; i++)
            {
                Rect tabRect = new Rect(i * tabWidth, 0, tabWidth - 5, 35);
                bool isSelected = i == selected_tab;
                bool isHover = tabRect.Contains(Event.current.mousePosition);

                GUI.color = isSelected ?
                    new Color(0.2f, 0.3f, 0.4f, 1f) :
                    new Color(0.15f, 0.15f, 0.2f, 1f);

                if (isHover && !isSelected)
                    GUI.color = new Color(0.25f, 0.25f, 0.3f, 1f);

                GUI.DrawTexture(tabRect, Texture2D.whiteTexture);

                if (isSelected)
                {
                    GUI.color = new Color(0, 0.8f, 1f, _tabGlow);
                    GUI.DrawTexture(tabRect, Texture2D.whiteTexture);
                }

                GUI.color = isSelected ? Color.cyan :
                           isHover ? new Color(0.8f, 0.9f, 1f, 1f) :
                           new Color(0.7f, 0.7f, 0.8f, 1f);

                GUIStyle style = isSelected ? _tabButtonSelectedStyle : _tabButtonStyle;
                GUI.Label(new Rect(tabRect.x, tabRect.y + 8, tabRect.width, tabRect.height),
                         tabnames[i], style);

                if (GUI.Button(tabRect, "", GUIStyle.none))
                {
                    selected_tab = i;
                }
            }

            GUI.EndGroup();
            GUI.color = Color.white;
        }

        private static void DrawContentArea()
        {
            GUI.color = new Color(0.1f, 0.1f, 0.12f, 0.95f);
            GUI.DrawTexture(new Rect(15, 110, GUIRect.width - 30, GUIRect.height - 170), Texture2D.whiteTexture);
            GUI.color = new Color(0.2f, 0.2f, 0.25f, 1f);
            GUI.DrawTexture(new Rect(15, 110, GUIRect.width - 30, 1), Texture2D.whiteTexture);
            GUI.BeginGroup(new Rect(20, 115, GUIRect.width - 40, GUIRect.height - 180));

            switch (selected_tab)
            {
                case 0: DrawPlayerTab(); break;
                case 1: DrawVisualTab(); break;
                case 2: DrawOtherTab(); break;
                case 3: DrawInfoTab(); break;
                case 4: DrawPlayerListTab(); break;
            }

            GUI.EndGroup();
            GUI.color = Color.white;
        }

        private static void DrawPlayerTab()
        {
            _scrollPlayer = GUI.BeginScrollView(new Rect(0, 0, GUIRect.width - 60, GUIRect.height - 180),
                _scrollPlayer, new Rect(0, 0, GUIRect.width - 75, 600));

            float yPos = 0;
            DrawSectionHeader("PLAYER MODIFICATIONS", ref yPos);

            DrawModernToggle("Infinite Stamina", ref _infStamOn, yPos, () => PlayerMods.InfiniteStamina(_infStamOn));
            yPos += 35;

            DrawModernToggle("Custom Crouch Delay", ref _crouchDelayOn, yPos, () => PlayerMods.SetCrouchDelay(_crouchDelayOn, _crouchDelay));
            yPos += 35;

            if (_crouchDelayOn)
            {
                DrawModernSlider("Delay", ref _crouchDelay, 0f, 0.6f, "s", yPos, v => PlayerMods.SetCrouchDelay(true, v));
                yPos += 50;
            }

            DrawModernSlider("Grab Range", ref _grabRange, 0f, 50f, "m", yPos, v => PlayerMods.SetGrabRange(v));
            yPos += 50;

            DrawModernSlider("Throw Strength", ref _throwStrength, 0f, 50f, "x", yPos, v => PlayerMods.SetThrowStrength(v));
            yPos += 50;

            DrawModernSlider("Jump Force", ref _jumpForce, 0f, 60f, "x", yPos, v => PlayerMods.SetJumpForce(v));
            yPos += 50;

            DrawModernToggle("Multi Jump", ref _mj, yPos, () => PlayerMods.MultiJump(_mj));
            yPos += 35;

            GUI.EndScrollView();
        }

        private static void DrawVisualTab()
        {
            _scrollVisual = GUI.BeginScrollView(
                new Rect(0, 0, GUIRect.width - 60, GUIRect.height - 180),
                _scrollVisual,
                new Rect(0, 0, GUIRect.width - 75, 300)
            );

            float yPos = 0f;

            DrawSectionHeader("ENEMY ESP", ref yPos);

            DrawModernToggle(
                "Enable Enemy ESP",
                ref showEnemyESP,
                yPos,
                null
            );
            yPos += 35;

            GUI.enabled = showEnemyESP;

            DrawModernToggle(
                "Box ESP",
                ref showEnemyBoxes,
                yPos,
                null
            );
            yPos += 30;

            DrawModernToggle(
                "Name ESP",
                ref showEnemyNames,
                yPos,
                null
            );
            yPos += 30;

            DrawModernToggle(
                "Distance ESP",
                ref showEnemyDistance,
                yPos,
                null
            );
            yPos += 30;

            GUI.enabled = true;

            GUI.EndScrollView();
        }


        private static void DrawOtherTab()
        {
            _scrollOther = GUI.BeginScrollView(new Rect(0, 0, GUIRect.width - 60, GUIRect.height - 180),
                _scrollOther, new Rect(0, 0, GUIRect.width - 75, 400));

            float yPos = 0;
            DrawSectionHeader("VISUAL TWEAKS", ref yPos);

            DrawModernToggle("No Fog", ref _noFogOn, yPos, () => OtherMods.ApplyNoFog(_noFogOn));
            yPos += 35;

            DrawModernToggle("RGB Fog", ref _rgbFogOn, yPos, () => OtherMods.AppyRGBFOG(_rgbFogOn));
            yPos += 35;

            if (_rgbFogOn)
            {
                DrawModernSlider("RGB Speed", ref OtherMods.RgbFogSpeed, 0.1f, 3f, "x", yPos, v => OtherMods.RgbFogSpeed = v);
                yPos += 50;
            }

            DrawModernToggle("Full Bright", ref _fb, yPos, () => OtherMods.FullBright(_fb));
            yPos += 35;

            GUI.EndScrollView();
        }

        private static void DrawInfoTab()
        {
            _scrollInfo = GUI.BeginScrollView(new Rect(0, 0, GUIRect.width - 60, GUIRect.height - 180), _scrollInfo, new Rect(0, 0, GUIRect.width - 75, 600));

            float yPos = 0;

            DrawSectionHeader("PHOTON NETWORK", ref yPos);
            DrawInfoCard("State", PhotonNetwork.NetworkClientState.ToString(), ref yPos);
            DrawInfoCard("Connected", PhotonNetwork.IsConnected.ToString(), ref yPos);
            DrawInfoCard("In Room", PhotonNetwork.InRoom.ToString(), ref yPos);
            DrawInfoCard("Region", SafeStr(PhotonNetwork.CloudRegion), ref yPos);
            DrawInfoCard("Ping", PhotonNetwork.GetPing() + " ms", ref yPos);

            yPos += 20;

            DrawSectionHeader("ROOM INFO", ref yPos);
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            {
                var r = PhotonNetwork.CurrentRoom;
                DrawInfoCard("Name", r.Name, ref yPos);
                DrawInfoCard("Players", $"{r.PlayerCount}/{r.MaxPlayers}", ref yPos);
                DrawInfoCard("Open", r.IsOpen.ToString(), ref yPos);
                DrawInfoCard("Visible", r.IsVisible.ToString(), ref yPos);
                DrawInfoCard("Master", SafePlayer(PhotonNetwork.MasterClient), ref yPos);
            }
            else
            {
                DrawInfoCard("Status", "Not in room", ref yPos);
            }

            yPos += 20;

            DrawSectionHeader("LOCAL PLAYER", ref yPos);
            var lp = PhotonNetwork.LocalPlayer;
            if (lp != null)
            {
                DrawInfoCard("NickName", SafeStr(lp.NickName), ref yPos);
                DrawInfoCard("UserId", SafeStr(lp.UserId), ref yPos);
                DrawInfoCard("Actor#", lp.ActorNumber.ToString(), ref yPos);
                DrawInfoCard("IsMaster", PhotonNetwork.IsMasterClient.ToString(), ref yPos);
            }

            GUI.EndScrollView();
        }

        private static void DrawPlayerListTab()
        {
            GUI.BeginGroup(new Rect(0, 0, GUIRect.width - 60, 40));
            Rect refreshRect = new Rect(0, 5, 120, 35);
            bool isRefreshHover = refreshRect.Contains(Event.current.mousePosition);
            GUI.color = isRefreshHover ? new Color(0, 0.7f, 1f, 1f) : new Color(0, 0.6f, 0.9f, 1f);
            if (GUI.Button(refreshRect, "⟳ REFRESH", _buttonStyle))
            {
                if (PlayerList.Instance != null) PlayerList.Instance.Refresh();
                PulseOnce();
            }
            GUI.color = Color.white;
            _autoRefreshPlayers = GUI.Toggle(new Rect(130, 10, 150, 25), _autoRefreshPlayers, "Auto-refresh (2s)", _toggleStyle);
            GUI.EndGroup();
            if (_autoRefreshPlayers && Time.realtimeSinceStartup >= _nextRefreshAt)
            {
                if (PlayerList.Instance != null)
                    PlayerList.Instance.Refresh();
                _nextRefreshAt = Time.realtimeSinceStartup + 2f;
            }
            _scrollPlayerList = GUI.BeginScrollView(new Rect(0, 45, GUIRect.width - 60, GUIRect.height - 225), _scrollPlayerList, new Rect(0, 0, GUIRect.width - 75, 600));
            float yPos = 0;
            if (PlayerList.Instance != null)
            {
                var names = PlayerList.Instance.Names;
                if (names == null || names.Count == 0)
                {
                    GUI.Label(new Rect(10, yPos, 300, 25), "No players online", _labelStyle);
                    yPos += 30;
                }
                else
                {
                    for (int i = 0; i < names.Count; i++)
                    {
                        GUI.color = i % 2 == 0 ? new Color(0.15f, 0.15f, 0.18f, 0.5f) : new Color(0.12f, 0.12f, 0.15f, 0.5f);
                        GUI.DrawTexture(new Rect(0, yPos, GUIRect.width - 75, 28), Texture2D.whiteTexture);
                        GUI.color = Color.white;
                        GUI.Label(new Rect(15, yPos + 4, GUIRect.width - 90, 25), $"#{i + 1:00} • {names[i]}", _playerInfoStyle);
                        float ping = Mathf.PingPong(Time.time + i, 100);
                        Color pingColor = ping < 30 ? Color.green : ping < 70 ? Color.yellow : Color.red;
                        GUI.color = pingColor;
                        GUI.DrawTexture(new Rect(GUIRect.width - 110, yPos + 10, 8, 8), Texture2D.whiteTexture);

                        yPos += 30;
                    }
                }
            }

            GUI.EndScrollView();
        }

        private static void DrawFooter()
        {
            GUI.BeginGroup(new Rect(0, GUIRect.height - 50, GUIRect.width, 50));
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 1f);
            GUI.DrawTexture(new Rect(0, 0, GUIRect.width, 50), Texture2D.whiteTexture);
            GUI.color = new Color(0, 0.6f, 1f, Mathf.Sin(Time.time * 3) * 0.3f + 0.4f);
            GUI.DrawTexture(new Rect(0, 0, GUIRect.width, 2), Texture2D.whiteTexture);
            string status = $"Ping: {PhotonNetwork.GetPing()} ms";
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)
            {
                status += $" | Room: {PhotonNetwork.CurrentRoom.Name}";
                status += $" | Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
            }
            GUI.color = new Color(0.7f, 0.8f, 1f, 1f);
            GUI.Label(new Rect(15, 15, GUIRect.width - 30, 25), status + " | INSERT = Toggle Menu", _labelStyle);
            GUI.EndGroup();
            GUI.color = Color.white;
        }

        private static void DrawSectionHeader(string title, ref float yPos)
        {
            float lineWidth = Mathf.PingPong(Time.time * 50, 100) + 50;
            GUI.color = new Color(0, 0.7f, 1f, 0.5f);
            GUI.DrawTexture(new Rect(0, yPos + 30, lineWidth, 2), Texture2D.whiteTexture);
            GUI.color = new Color(0.8f, 0.9f, 1f, 1f);
            GUI.Label(new Rect(0, yPos, 300, 30), title, _sectionHeaderStyle);
            yPos += 40;
            GUI.color = Color.white;
        }

        private static void DrawModernToggle(string label, ref bool value, float yPos, Action onChanged)
        {
            Rect toggleRect = new Rect(0, yPos, GUIRect.width - 80, 30);
            bool isHover = toggleRect.Contains(Event.current.mousePosition);
            GUI.color = value ? new Color(0, 0.3f, 0.5f, 0.3f) : isHover ? new Color(0.2f, 0.2f, 0.25f, 0.3f) : new Color(0.15f, 0.15f, 0.2f, 0.3f);
            GUI.DrawTexture(toggleRect, Texture2D.whiteTexture);
            Rect boxRect = new Rect(5, yPos + 5, 20, 20);
            GUI.color = value ? new Color(0, 0.8f, 1f, 1f) : new Color(0.3f, 0.3f, 0.4f, 1f);
            GUI.DrawTexture(boxRect, Texture2D.whiteTexture);
            if (value)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(boxRect.x + 4, boxRect.y + 4, 12, 12), _checkmarkTexture);
            }
            GUI.color = value ? Color.cyan : isHover ? new Color(0.9f, 0.9f, 1f, 1f) : new Color(0.8f, 0.8f, 0.9f, 1f);
            GUI.Label(new Rect(35, yPos + 5, 300, 20), label, _toggleStyle);
            if (GUI.Button(toggleRect, "", GUIStyle.none))
            {
                value = !value;
                onChanged?.Invoke();
                PulseOnce();
            }
            GUI.color = Color.white;
        }

        private static void DrawModernSlider(string label, ref float value, float min, float max, string unit, float yPos, Action<float> onChanged)
        {
            Rect sliderRect = new Rect(0, yPos, GUIRect.width - 80, 40);
            GUI.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);
            GUI.DrawTexture(sliderRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.9f, 0.9f, 1f, 1f);
            GUI.Label(new Rect(5, yPos + 5, 200, 20), label, _labelStyle);
            string valueText = $"{value:F2}{unit}";
            GUI.Label(new Rect(GUIRect.width - 130, yPos + 5, 80, 20), valueText, _valueStyle);
            float trackY = yPos + 25;
            GUI.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            GUI.DrawTexture(new Rect(5, trackY, GUIRect.width - 90, 6), Texture2D.whiteTexture);
            float fillWidth = (value - min) / (max - min) * (GUIRect.width - 90);
            GUI.color = new Color(0, 0.7f, 1f, 1f);
            GUI.DrawTexture(new Rect(5, trackY, fillWidth, 6), Texture2D.whiteTexture);
            Rect thumbRect = new Rect(5 + fillWidth - 8, trackY - 7, 16, 20);
            bool isDragging = thumbRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDrag;
            GUI.color = isDragging ? new Color(0, 1f, 1f, 1f) : new Color(0, 0.8f, 1f, 1f);
            GUI.DrawTexture(thumbRect, Texture2D.whiteTexture);
            if (Event.current.type == EventType.MouseDown && sliderRect.Contains(Event.current.mousePosition))
            {
                float mouseX = Event.current.mousePosition.x - 5;
                float newValue = Mathf.Clamp(mouseX / (GUIRect.width - 90) * (max - min) + min, min, max);
                if (!Mathf.Approximately(newValue, value))
                {
                    value = newValue;
                    onChanged?.Invoke(value);
                }
            }

            GUI.color = Color.white;
        }

        private static void DrawColorPicker(string label, ref Color color, float yPos, Action<Color> onChanged)
        {
            Rect colorRect = new Rect(0, yPos, GUIRect.width - 80, 35);
            GUI.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);
            GUI.DrawTexture(colorRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.9f, 0.9f, 1f, 1f);
            GUI.Label(new Rect(5, yPos + 8, 200, 20), label, _labelStyle);
            Rect previewRect = new Rect(GUIRect.width - 130, yPos + 5, 60, 25);
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(previewRect.x - 1, previewRect.y - 1, previewRect.width + 2, previewRect.height + 2), Texture2D.whiteTexture);
            GUI.color = color;
            GUI.DrawTexture(previewRect, Texture2D.whiteTexture);
            Rect buttonRect = new Rect(GUIRect.width - 65, yPos + 5, 60, 25);
            bool isHover = buttonRect.Contains(Event.current.mousePosition);
            GUI.color = isHover ? new Color(0, 0.7f, 1f, 1f) : new Color(0, 0.6f, 0.9f, 1f);
            if (GUI.Button(buttonRect, "PICK", _buttonStyle))
            {
                Color[] colors = {
                    Color.green, Color.red, Color.blue, Color.yellow,
                    Color.cyan, Color.magenta, Color.white, new Color(1, 0.5f, 0)
                };

                int currentIndex = Array.IndexOf(colors, color);
                color = colors[(currentIndex + 1) % colors.Length];
                onChanged?.Invoke(color);
                PulseOnce();
            }

            GUI.color = Color.white;
        }

        private static void DrawInfoCard(string label, string value, ref float yPos)
        {
            Rect cardRect = new Rect(0, yPos, GUIRect.width - 80, 30);
            bool isHover = cardRect.Contains(Event.current.mousePosition);
            GUI.color = isHover ? new Color(0.2f, 0.2f, 0.25f, 0.5f) : new Color(0.15f, 0.15f, 0.18f, 0.3f);
            GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
            GUI.color = new Color(0.6f, 0.8f, 1f, 1f);
            GUI.Label(new Rect(10, yPos + 5, 120, 20), label + ":", _labelStyle);
            GUI.color = new Color(0.9f, 0.9f, 1f, 1f);
            GUI.Label(new Rect(140, yPos + 5, GUIRect.width - 220, 20), value, _labelStyle);
            yPos += 32;
            GUI.color = Color.white;
        }

        private static string SafeStr(string s) => string.IsNullOrEmpty(s) ? "(none)" : s;
        private static string SafePlayer(Player p) => p == null ? "(none)" : $"{(string.IsNullOrEmpty(p.NickName) ? "(no name)" : p.NickName)} [{p.ActorNumber}]";

        private IEnumerator AnimateWindowIn()
        {
            _windowAlpha = 0;
            _windowScale = 0.8f;
            _slideInPosition = -100f;

            float duration = 0.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                _windowAlpha = Mathf.SmoothStep(0, 1, t);
                _windowScale = Mathf.SmoothStep(0.8f, 1f, t);
                _slideInPosition = Mathf.SmoothStep(-100, 0, t);

                yield return null;
            }

            _windowAlpha = 1;
            _windowScale = 1;
            _slideInPosition = 0;
        }

        private IEnumerator PulseAnimation()
        {
            while (true)
            {
                _pulseEffect = Mathf.Sin(Time.time * 2) * 0.5f + 0.5f;
                yield return null;
            }
        }

        private IEnumerator GlowAnimation()
        {
            while (true)
            {
                _tabGlow = Mathf.Sin(Time.time * 3) * 0.3f + 0.3f;
                _windowRotation = Mathf.Sin(Time.time * 0.5f) * 0.2f;
                yield return null;
            }
        }

        private static void PulseOnce()
        {
            if (_instance != null)
                _instance.StartCoroutine(_instance.PulseOnceCoroutine());
        }

        private IEnumerator PulseOnceCoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _pulseEffect = Mathf.Sin(t * Mathf.PI) * 0.8f;
                yield return null;
            }

            _pulseEffect = 0;
        }

        private static void InitializeStyles()
        {
            _stylesInitialized = true;
            _windowStyle = new GUIStyle();
            _windowStyle.normal.background = CreateRoundedTexture(700, 500, 15, new Color(0.1f, 0.1f, 0.12f, 0.98f));
            _tabButtonStyle = new GUIStyle(GUI.skin.label);
            _tabButtonStyle.fontSize = 11;
            _tabButtonStyle.fontStyle = FontStyle.Bold;
            _tabButtonStyle.alignment = TextAnchor.MiddleCenter;
            _tabButtonSelectedStyle = new GUIStyle(_tabButtonStyle);
            _tabButtonSelectedStyle.fontSize = 12;
            _headerStyle = new GUIStyle(GUI.skin.label);
            _headerStyle.fontSize = 16;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            _headerStyle.normal.textColor = new Color(0.9f, 0.95f, 1f, 1f);
            _sectionHeaderStyle = new GUIStyle(GUI.skin.label);
            _sectionHeaderStyle.fontSize = 13;
            _sectionHeaderStyle.fontStyle = FontStyle.Bold;
            _sectionHeaderStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 1f);
            _toggleStyle = new GUIStyle(GUI.skin.label);
            _toggleStyle.fontSize = 12;
            _toggleStyle.normal.textColor = Color.white;
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = 11;
            _labelStyle.normal.textColor = new Color(0.9f, 0.9f, 1f, 1f);
            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = 11;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.normal.textColor = Color.cyan;
            _valueStyle.alignment = TextAnchor.MiddleRight;
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 11;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;
            _buttonStyle.normal.background = CreateRoundedTexture(1, 1, 5, new Color(0, 0.6f, 0.9f, 1f));
            _buttonStyle.hover.background = CreateRoundedTexture(1, 1, 5, new Color(0, 0.7f, 1f, 1f));
            _playerInfoStyle = new GUIStyle(GUI.skin.label);
            _playerInfoStyle.fontSize = 12;
            _playerInfoStyle.normal.textColor = new Color(0.9f, 0.9f, 1f, 1f);
        }

        private static Texture2D CreateRoundedTexture(int width, int height, int radius, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];

            float centerX = width / 2f;
            float centerY = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
                    float maxDistance = Mathf.Min(centerX, centerY);

                    float alpha = Mathf.Clamp01((maxDistance - distance) / radius);
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateGlowTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];

            float center = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(1 - distance / center);
                    alpha = Mathf.Pow(alpha, 2);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateCheckmarkTexture(int size, Color color)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isCheck = (x >= 2 && x <= size - 3 && y >= 2 && y <= size - 3);
                    if (isCheck)
                    {
                        if ((x == y - 2) || (x == y - 1) || (x == y) ||
                            (x + y == size - 2) || (x + y == size - 1) || (x + y == size))
                        {
                            pixels[y * size + x] = color;
                        }
                        else
                        {
                            pixels[y * size + x] = Color.clear;
                        }
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateRoundedCursor(Color color, int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);

                    if (distance <= radius)
                    {
                        float alpha = 1 - (distance / radius);
                        alpha = Mathf.Pow(alpha, 0.5f); // Softer edge
                        pixels[y * size + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}

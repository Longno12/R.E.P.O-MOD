using Photon.Pun;
using Repo.Player;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using GameGUI = repogui.MenuGUI;
using UGUI = UnityEngine.GUI;

namespace repocheeto
{
    public class Cheat : MonoBehaviour
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public int toggleKey = 45;
        public float toggleDelay = 0.5f;
        private bool toggled = true;
        private float lastToggleTime;

        private CursorLockMode _prevLock;
        private bool _prevVisible;
        private bool _savedCursorState;
        private static bool _isAnimating = false;
        private static Coroutine _windowAnim;

        public static List<PlayerController> targets = new List<PlayerController>();
        public static Material mat = new Material(Shader.Find("GUI/Text Shader"));

        private float colorChangeSpeed = 1f;
        private float timer = 0f;

        private void OnGUI()
        {
            float r = Mathf.PingPong(timer * colorChangeSpeed, 1f);
            float g = Mathf.PingPong(timer * colorChangeSpeed + 0.33f, 1f);
            float b = Mathf.PingPong(timer * colorChangeSpeed + 0.66f, 1f);

            UGUI.color = new Color(r, g, b);
            UGUI.Label(new Rect(10, 10, 400, 40), "https://discord.gg/rHAraREyCr R.E.P.O Mod");
            UGUI.color = Color.white;

            timer += Time.deltaTime;
            if (!toggled) return;

            if (UGUI.Button(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, GUIStyle.none))
            {
            }

            GameGUI.GUIRect = UGUI.Window( 69, GameGUI.GUIRect, GameGUI.DrawMainWindow, "R.E.P.O Mod | FPS: " + (1.0f / Mathf.Max(Time.deltaTime, 1e-6f)) + " | Toggle: INSERT" );
        }

        private void GUIToggleCheck()
        {
            if (GetAsyncKeyState(toggleKey) < 0)
            {
                if (Time.time - lastToggleTime >= toggleDelay)
                {
                    toggled = !toggled;
                    lastToggleTime = Time.time;

                    if (toggled)
                    {
                        if (!_savedCursorState)
                        {
                            _prevLock = Cursor.lockState;
                            _prevVisible = Cursor.visible;
                            _savedCursorState = true;
                        }
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                    }
                    else
                    {
                        Cursor.lockState = _prevLock;
                        Cursor.visible = _prevVisible;
                        _savedCursorState = false;
                    }
                }
            }
        }

        private void Update()
        {
            GUIToggleCheck();
            OtherMods.TickRgbFog();

            if (toggled)
            {
                if (Cursor.lockState != CursorLockMode.None) Cursor.lockState = CursorLockMode.None;
                if (!Cursor.visible) Cursor.visible = true;
                Input.ResetInputAxes();
            }
        }
    }
}

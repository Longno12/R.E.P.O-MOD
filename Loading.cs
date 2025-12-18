using repocheeto;
using repogui;
using UnityEngine;

namespace Loading
{
    public class Loader
    {
        static GameObject gameObject;

        public static void Load()
        {
            gameObject = new GameObject("REPO_Cheat");
            Object.DontDestroyOnLoad(gameObject);

            gameObject.AddComponent<Cheat>();
            gameObject.AddComponent<MenuGUI>();
        }

        public static void Unload()
        {
            if (gameObject != null)
                Object.Destroy(gameObject);
        }
    }
}

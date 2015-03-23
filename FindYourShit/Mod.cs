using ICities;
using System.Reflection;
using UnityEngine;

namespace fys
{
    public class UserMod : IUserMod
    {

        public string Description
        {
            get
            {
                return "Find your Citizens, Cars, Buildings, Seagulls & More";
            }
        }

        public string Name
        {
            get
            {
                return "FindIt: CTRL+F to find anything";
            }
        }
    }

    public class FindIt : LoadingExtensionBase
    {
        internal static GameObject panel;
        
        private static CustomRenderManager renderManager;
        internal static volatile RenderMode renderMode = RenderMode.DEFAULT;
        private static FastList<IRenderableManager> RenderManagers
        {
            get
            {
                // If only there was some kind of public API
                return (FastList<IRenderableManager>)typeof(RenderManager).GetField("m_renderables", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            panel = new GameObject("FindItSearchPanel", typeof(SearchForSomeoneRandomPanel));

            renderManager = new CustomRenderManager();
            RenderManagers.Add(renderManager);
        }

        public override void OnLevelUnloading()
        {
            GameObject.Destroy(panel);
            panel = null;

            RenderManagers.Remove(renderManager);
            renderManager = null;
        }
    }

    public class ChirpEvent : ChirperExtensionBase
    {
        public override void OnUpdate()
        {
            var go = FindIt.panel;
            if (go == null)
                return;

            SearchForSomeoneRandomPanel panel = go.GetComponent<SearchForSomeoneRandomPanel>();
            if (panel == null)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
                panel.Hide();
            if (Event.current.control && Input.GetKeyDown(KeyCode.F))
            {
                if (Event.current.shift)
                {
                    switch(FindIt.renderMode)
                    {
                        case RenderMode.DEFAULT:
                            FindIt.renderMode = RenderMode.SMALL_ICONS;
                            break;

                        case RenderMode.SMALL_ICONS:
                            FindIt.renderMode = RenderMode.BIG_ICONS;
                            break;

                        case RenderMode.BIG_ICONS:
                            FindIt.renderMode = RenderMode.DEFAULT;
                            break;
                    }
                }
                else
                {
                    panel.Toggle();
                }
            }
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown("enter"))
                panel.ContinueSearchMayhaps();
        }
    }
}

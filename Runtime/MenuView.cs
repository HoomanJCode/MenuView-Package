using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// ReSharper disable ReturnTypeCanBeEnumerable.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable VirtualMemberNeverOverridden.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace MenuViews
{
    public abstract class MenuView : MonoBehaviour
    {
        private static MenuView[] _views;
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        private static readonly Dictionary<int, MenuView> LastViews = new Dictionary<int, MenuView>();
        [SerializeField] private bool homePageOfParent;
        private MenuView[] _children;

        private bool _enabled;
        private MenuView _parent;

        public MenuView LastView => !LastViews.ContainsKey(Layer) ? default : LastViews[Layer];

        protected MenuView[] Children
        {
            get
            {
                // ReSharper disable once InvertIf
                if (_children == null)
                {
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    var menuViews = GetComponentsInChildren<MenuView>(true).ToList();
                    menuViews.Remove(this);
                    _children = menuViews.ToArray();
                }

                return _children;
            }
        }

        protected MenuView Parent
        {
            get
            {
                if (_parent) return _parent;
                var obj = transform.parent;
                while (obj.parent)
                {
                    // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                    _parent = obj.GetComponent<MenuView>();
                    if (_parent) return _parent;
                    obj = obj.parent;
                }

                return _parent;
            }
        }

        public int Layer => Parent ? Parent.SubLayer : default;

        private int SubLayer => GetHashCode();

        private bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                gameObject.SetActive(value);
            }
        }

        public static MenuView GetCurrentView(int layer)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var view in _views)
                if (view.Enabled && view.Layer == layer)
                    return view;
            return default;
        }

        public MenuView GetCurrentView()
        {
            return GetCurrentView(Layer);
        }

        protected abstract void Init();

        public static void ChangeToLastView(int layer)
        {
            if (!LastViews.ContainsKey(layer)) return;
            LastViews[layer].ChangeToThisView();
        }

        public void ChangeToLastView()
        {
            ChangeToLastView(Layer);
        }

        public virtual void ChangeToThisView()
        {
            ChangeCurrentView(gameObject, Layer);
        }

        private static void ChangeCurrentView(Object toMenuType, int layer)
        {
            var currentView = GetCurrentView(layer);
            if (currentView && currentView.gameObject != toMenuType)
            {
                if (!LastViews.ContainsKey(layer)) LastViews.Add(layer, currentView);
                LastViews[layer] = currentView;
            }

            foreach (var view in _views)
                if (view.Layer == layer)
                {
                    view.Enabled = view.gameObject == toMenuType;
                    if (view.gameObject != toMenuType) continue;
                    //find home page
                    foreach (var fView in view.Children)
                        if (fView.homePageOfParent && fView.Parent == view)
                        {
                            fView.ChangeToThisView();
                            break;
                        }
                }
        }

        public static void ChangeCurrentView<TMenuScript>() where TMenuScript : MenuView
        {
            ChangeCurrentView(typeof(TMenuScript));
        }
        public static void ChangeCurrentView(Type targetViewType)
        {
            if(!targetViewType.IsSubclassOf(typeof(MenuView)))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError($"Type: {targetViewType.Name} is not a menu view.");
                return;
            }

            foreach (var view in _views)
            {
                if (view.GetType() != targetViewType) continue;
                view.ChangeToThisView();
                return;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogError($"Not found {targetViewType.Name} view.");
        }


        public virtual void CloseThisView()
        {
            Enabled = false;
        }

        protected void CloseAll()
        {
            CloseAll(Layer);
        }

        public static void CloseAll(int layer)
        {
            foreach (var view in _views)
                if (view.Layer == layer)
                    view.CloseThisView();
        }

        public static void CloseView<TMenuScript>() where TMenuScript : MenuView
        {
            CloseView(typeof(TMenuScript));
        }
        public static void CloseView(Type targetViewType)
        {
            if(!targetViewType.IsSubclassOf(typeof(MenuView)))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                Debug.LogError($"Type: {targetViewType.Name} is not a menu view.");
                return;
            }
            
            foreach (var view in _views)
            {
                if (view.GetType() != targetViewType) continue;
                view.CloseThisView();
                return;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogError($"Not found {targetViewType.Name} view.");
        }

        public static TMenuScript GetView<TMenuScript>() where TMenuScript : MenuView
        {
            foreach (var view in _views)
            {
                if (view.GetType() != typeof(TMenuScript)) continue;
                return (TMenuScript) view;
            }

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            Debug.LogError($"Not found {typeof(TMenuScript).Name} view.");
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitMenus()
        {
            RescanViews();
            if(_onChangedSceneEventRegistered) return;
            _onChangedSceneEventRegistered = true;
            //we need to know views after scene change
            SceneManager.activeSceneChanged += (a, b) => RescanViews();
        }

        /// <summary>
        /// To prevent editor (SceneManager.activeSceneChanged) caching
        /// </summary>
        private static bool _onChangedSceneEventRegistered;

        private static void RescanViews()
        {
            _views = Resources.FindObjectsOfTypeAll<MenuView>();
            foreach (var view in _views) view.Init();
            foreach (var view in _views)
                if (view.Parent == null && view.homePageOfParent)
                {
                    view.ChangeToThisView();
                    break;
                }
        }
    }
}
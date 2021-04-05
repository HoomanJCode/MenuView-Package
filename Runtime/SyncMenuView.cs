using System;
using UnityEngine;
// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace MenuViews
{
    public class SyncMenuView : MonoBehaviour
    {
        private static Type _targetOfChange= typeof(SyncMenuView);
        private static bool _hasChangeView;
        private static Type _targetOfClose= typeof(SyncMenuView);
        private static bool _hasClose;
        private static int _targetLayerOfCloseAll;
        private static bool _hasCloseAll;
        private static bool _hasChange;

        private void Update()
        {
            if (!_hasChange) return;
            _hasChange = false;

            if (_hasCloseAll)
            {
                _hasCloseAll = false;
                MenuView.CloseAll(_targetLayerOfCloseAll);
            }

            if (_hasChangeView)
            {
                _hasChangeView = false;
                lock (_targetOfChange)
                    MenuView.ChangeCurrentView(_targetOfChange);
            }

            if (_hasClose)
            {
                _hasClose = false;
                lock (_targetOfClose)
                    MenuView.CloseView(_targetOfClose);
            }

        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitMenus()
        {
            new GameObject(nameof(SyncMenuView)).AddComponent<SyncMenuView>();
        }

        public static void ChangeCurrentView<TMenuScript>() where TMenuScript : MenuView
        {
            lock (_targetOfChange)
                _targetOfChange = typeof(TMenuScript);
            _hasChange = true;
            _hasChangeView = true;
        }
        public static void CloseView<TMenuScript>() where TMenuScript : MenuView
        {
            lock (_targetOfClose)
                _targetOfClose = typeof(TMenuScript);
            _hasChange = true;
            _hasClose = true;
        }

        public static void CloseAll(int layer)
        {
            _targetLayerOfCloseAll = layer;
            _hasChange = true;
            _hasCloseAll = true;
        }
    }
}
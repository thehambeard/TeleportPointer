using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.View;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace TeleportPointer
{
#if (DEBUG)
    [EnableReloading]
#endif
    internal static class Main
    {
        
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
            Main.Logger = modEntry.Logger;
            modEntry.OnUpdate = OnUpdate;
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Main.enabled = value;
            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float z)
        {
            if ((Game.Instance.CurrentMode != GameModeType.Default || Game.Instance.CurrentMode != GameModeType.Pause) && Main.enabled)
            {
                if (Input.GetKeyDown(KeyCode.KeypadMinus))
                    TeleportUnit(Game.Instance.Player.MainCharacter.Value, PointerPosition());
                else if (Input.GetKeyDown(KeyCode.KeypadPlus))
                    TeleportSelected();
                else if (Input.GetKeyDown(KeyCode.KeypadDivide))
                    _hover.LockUnit();
                else if (Input.GetKeyDown(KeyCode.KeypadMultiply))
                    if(_hover.Unit != null) TeleportUnit(_hover.Unit, PointerPosition());
            }

        }
        private static Vector3 PointerPosition()
        {
            Vector3 result = new Vector3();

            Camera camera = Game.GetCamera();
            RaycastHit raycastHit = default(RaycastHit);
            if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out raycastHit, camera.farClipPlane, 21761))
            {
                result = raycastHit.point;
            }
            return result;
        }

        private static void TeleportUnit(UnitEntityData unit, Vector3 position)
        {
            UnitEntityView view = unit.View;

            if (view != null) view.StopMoving();

            unit.Stop();
            unit.Position = position;

            foreach (var fam in unit.Familiars)
            {
                if (fam)
                    fam.TeleportToMaster(false);
            }
        }

        private static void TeleportSelected()
        {
            foreach (var unit in Game.Instance.UI.SelectionManager.SelectedUnits)
            {
                TeleportUnit(unit, PointerPosition());
            }
        }
        
        internal class HoverHandler : IUnitDirectHoverUIHandler, IDisposable
        {
            public UnitEntityData Unit { get; private set; }
            private UnitEntityData _currentUnit;

            public HoverHandler()
            {
                EventBus.Subscribe(this);
            }
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void HandleHoverChange([NotNull] UnitEntityView unitEntityView, bool isHover)
            {
                if(isHover) _currentUnit = unitEntityView.Data;
            }

            public void LockUnit()
            {
                if(_currentUnit != null)
                    Unit = _currentUnit;
            }
        }

        public static bool enabled;

        public static UnityModManager.ModEntry.ModLogger Logger;

        private static HoverHandler _hover = new HoverHandler();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.UI
{
    public static class UndoRedoSystem
    {
        private static Stack<UndoRedoAction> UndoStack = new();
        private static Stack<UndoRedoAction> RedoStack = new();
        private static UndoRedoAction CurrentAction = new();
        public static bool BlockEvent { get; set; } = false;
        public static bool BlockUpdate { get; set; } = false;

        /// <summary>
        /// Undoes one Action Chain
        /// </summary>
        public static void Undo()
        {
            if (UndoStack.Count <= 0) return;

            var action = UndoStack.Pop();
            foreach (var sub in action.actions)
            {
                BlockEvent = sub.ShouldBlockEvent;
                BlockUpdate = sub.ShouldBlockUpdate;
                sub.PropertyInfo.SetValue(sub.Target, sub.Before);
                BlockEvent = false;
                BlockUpdate = false;
            }
            RedoStack.Push(action);
            
        }

        /// <summary>
        /// Redoes one Action Chain
        /// </summary>
        public static void Redo()
        {
            if (RedoStack.Count <= 0) return;
            var action = RedoStack.Pop();
            foreach (var sub in action.actions)
            {
                BlockEvent = sub.ShouldBlockEvent;
                BlockUpdate = sub.ShouldBlockUpdate;
                sub.PropertyInfo.SetValue(sub.Target, sub.After);
                BlockEvent = false;
                BlockUpdate = false;
            }
            UndoStack.Push(action);
        }

        /// <summary>
        /// Ends an Action Chain
        /// </summary>
        public static void EndAction()
        {
            if (CurrentAction.actions is not null)
            {
                RedoStack.Clear();
                UndoStack.Push(CurrentAction);
            }
            else
            {
                LoggingSystem.LogError("CurrentAction.actions is null which should not happen");
            }
            CurrentAction = new();
        }

        /// <summary>
        /// Creates an action and does it it will get the before state by itselfe
        /// </summary>
        /// <param name="after">how it is after this action</param>
        /// <param name="propertyInfo">the property that is modified</param>
        /// <param name="target">the target object if the target is static type then just give it null</param>
        /// <param name="shouldBlockEvent">should it block possible UI Events</param>
        public static void DoAction(object after, PropertyInfo propertyInfo, object target, bool shouldBlockEvent = false, bool shouldBlockUpdate = false)
        {
            BlockEvent = shouldBlockEvent;
            BlockUpdate = shouldBlockUpdate;
            if (CurrentAction.actions is null) { LoggingSystem.LogError("CurrentAction is null which should never happen!"); BlockEvent = false; BlockUpdate = false; return; }
            object before = propertyInfo.GetValue(target);
            CurrentAction.actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, shouldBlockEvent, shouldBlockUpdate));
            propertyInfo.SetValue(target, after);
            BlockEvent = false;
            BlockUpdate = false;
        }

        /// <summary>
        /// Creates an action that already happened
        /// </summary>
        /// <param name="before">how it was before this action</param>
        /// <param name="after">how it is after this action</param>
        /// <param name="propertyInfo">the property that is modified</param>
        /// <param name="target">the target object if the target is static type then just give it null</param>
        /// <param name="shouldBlockEvent">should it block possible UI Events</param>
        public static void CreateAction(object before, object after, PropertyInfo propertyInfo, object target, bool shouldBlockEvent = false, bool shouldBlockUpdate = false)
        {
            BlockEvent= shouldBlockEvent;
            BlockUpdate = shouldBlockUpdate;
            CurrentAction.actions.Add(new SubUndoRedoAction(before, after, propertyInfo, target, shouldBlockEvent, shouldBlockUpdate));
            BlockEvent= false;
            BlockUpdate = false;
        }
    }

    public struct UndoRedoAction
    {
        internal List<SubUndoRedoAction> actions = new();
        public UndoRedoAction() { actions = new(); }
    }

    public struct SubUndoRedoAction
    {
        internal object Before { get; private set; }
        internal object After { get; private set; }
        internal PropertyInfo PropertyInfo { get; private set; }
        internal object Target { get; private set; }
        internal bool ShouldBlockEvent { get; private set; }
        internal bool ShouldBlockUpdate { get; private set; }

        public SubUndoRedoAction(object before, object after, PropertyInfo propertyInfo, object target, bool shouldBlockEvent, bool shouldBlockUpdate)
        {
            Before = before;
            After = after;
            PropertyInfo = propertyInfo;
            Target = target;
            ShouldBlockEvent = shouldBlockEvent;
            ShouldBlockUpdate = shouldBlockUpdate;
        }
    }
}

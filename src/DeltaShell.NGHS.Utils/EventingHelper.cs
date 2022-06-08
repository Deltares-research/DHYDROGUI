using System;
using DelftTools.Utils.Aop;

namespace DeltaShell.NGHS.Utils
{
    public static class EventingHelper
    {
        /// <summary>
        /// Does an action with event bubbling disabled and then restores the eventing to the previous state
        /// </summary>
        /// <param name="action">Action to perform with eventing disabled</param>
        public static void DoWithoutEvents(Action action)
        {
            DoWithEventing(false, action);
        }

        /// <summary>
        /// Does an action with event bubbling disabled and then restores the eventing to the previous state
        /// </summary>
        /// <param name="action">Action to perform with eventing enabled</param>
        public static void DoWithEvents(Action action)
        {
            DoWithEventing(true, action);
        }

        private static void DoWithEventing(bool bubblingEnabled, Action action)
        {
            var currentState = EventSettings.BubblingEnabled;
            EventSettings.BubblingEnabled = bubblingEnabled;
            try
            {
                action?.Invoke();
            }
            finally
            {
                EventSettings.BubblingEnabled = currentState;
            }
        }
    }
}
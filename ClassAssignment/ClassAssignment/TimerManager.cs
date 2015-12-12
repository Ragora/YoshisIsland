using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ClassAssignment
{
    /// <summary>
    /// A manager for timed events in the game.
    /// </summary>
    public class TimerManager
    {
        /// <summary>
        /// How much time has passed since the timer manager's initialization.
        /// </summary>
        private static float SimTime;

        /// <summary>
        /// A list of active timers that this timer manager is keeping track of.
        /// </summary>
        private static List<Timer> ActiveTimers;

        /// <summary>
        /// The timer is the staple of the timer manager system. It represents a scheduled method to be called at what time and if
        /// the timer should reschedule itself (recurring).
        /// </summary>
        public class Timer
        {
            /// <summary>
            /// A boolean representing whether or not this timer is recurring.
            /// </summary>
            public bool Recurring;

            /// <summary>
            /// A delegate representing the method to be called when the timer
            /// dispatches.
            /// </summary>
            public delegate void TimerTickCallback();
        
            /// <summary>
            /// The method to be called when the timer dispatches itself.
            /// </summary>
            public TimerTickCallback TimerTickResponder;

            /// <summary>
            /// The delta time from current time that this timer will schedule itself for.
            /// </summary>
            public float DeltaSeconds;

            /// <summary>
            /// The target simulation time to activate at.
            /// </summary>
            private float DestinationTime;

            /// <summary>
            /// A constructor accepting a delta time.
            /// </summary>
            /// <param name="deltaSeconds">
            /// The time to wait in seconds for dispatch.
            /// </param>
            public Timer(float deltaSeconds)
            {
                ActiveTimers.Add(this);

                this.DeltaSeconds = deltaSeconds;
                this.DestinationTime = SimTime + deltaSeconds;
            }

            /// <summary>
            /// Removes this timer from the timer management.
            /// </summary>
            public void Dispose()
            {
                ActiveTimers.Remove(this);
            }

            /// <summary>
            /// Updates the timer, calling its tick responder and removing it from the timer manager
            /// if necessary.
            /// </summary>
            /// <param name="deltaSeconds">
            /// How many seconds have passed since the last update.
            /// </param>
            public void Update(float deltaSeconds)
            {
                if (TimerManager.SimTime >= this.DestinationTime)
                {
                    if (this.TimerTickResponder != null)
                        this.TimerTickResponder();

                    if (!this.Recurring)
                        this.Dispose();
                    else
                        this.DestinationTime = SimTime + this.DeltaSeconds;
                }

            }
        }

        /// <summary>
        /// Initializes the timer manager.
        /// </summary>
        public static void Create()
        {
            ActiveTimers = new List<Timer>();
        }

        /// <summary>
        /// Updates the timer manager by looping through all active timers and updating them using the time
        /// value passed in.
        /// </summary>
        /// <param name="time">The game time instance passed in to the game's main update method.</param>
        public static void Update(GameTime time)
        {
            float deltaSeconds = (float)time.ElapsedGameTime.Milliseconds / 1000;

            SimTime += deltaSeconds;

            List<Timer> timers = ActiveTimers.ToList();
            foreach (Timer timer in timers)
                timer.Update(deltaSeconds);
        }

        /// <summary>
        /// Private constructor to prevent direct construction.
        /// </summary>
        private TimerManager() { }
    }
}

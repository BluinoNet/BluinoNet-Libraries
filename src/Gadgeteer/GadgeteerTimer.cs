﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace Gadgeteer
{
    /// <summary>
    /// Provides a timer to enable periodic status checks or actions, or a one-off stopwatch.
    /// </summary>
    /// <remarks>
    /// This class wraps the <see cref="Microsoft.SPOT.DispatcherTimer"/> class to provide 
    /// a simpler API for your Gadgeteer application.
    /// </remarks>
    public class GadgeteerTimer
    {
        private Timer dt;
        private static Hashtable activeTimers = new Hashtable();

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="interval">The interval for the timer.</param>
        public GadgeteerTimer(TimeSpan interval)
            : this(interval, BehaviorType.RunContinuously) { }

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="intervalMilliseconds">The interval for the timer.</param>
        public GadgeteerTimer(int intervalMilliseconds)
            : this(new TimeSpan(0, 0, 0, 0, intervalMilliseconds), BehaviorType.RunContinuously) { }

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="interval">The interval for the timer.</param>
        /// <param name="behavior">The behavior for the timer (run once or continuously).</param>
        public GadgeteerTimer(TimeSpan interval, BehaviorType behavior)
        {
            /*
            if (Program.Dispatcher == null)
            {
                Debug.WriteLine("WARN: null Program.Dispatcher in GT.Timer constructor");
            }
            */
            //this.dt.Interval = interval;
            //this.dt.Tick += new EventHandler(dt_Tick);
            this.dt = new Timer(dt_Tick, null, new TimeSpan(0, 0, 0), interval);
            this.Interval = interval;
            this.IsRunning = true;
            this.Behavior = behavior;
        }

        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="intervalMilliseconds">The interval for the timer.</param>
        /// <param name="behavior">The behavior for the timer (run once or continuously).</param>
        public GadgeteerTimer(int intervalMilliseconds, BehaviorType behavior)
            : this(new TimeSpan(0, 0, 0, 0, intervalMilliseconds), behavior) { }


        /// <summary>
        ///  The behavior of this timer - whether to run once or run continuously.
        /// </summary>
        public BehaviorType Behavior { get; set; }

        /// <summary>
        /// An enumeration of timer behaviours
        /// </summary>
        public enum BehaviorType
        {
            /// <summary>
            /// Run once when started, and then stop, so a single Tick event is generated.
            /// </summary>
            RunOnce,
            /// <summary>
            /// Run continually after being started, so Tick events are generated periodically.
            /// </summary>
            RunContinuously
        }

        void dt_Tick(object state)//(object sender, EventArgs e)
        {
            if (!IsRunning) return;
            try
            {
                if (Behavior == BehaviorType.RunOnce) Stop();
                if (Tick != null) Tick(this);
            }
            catch
            {
                Debug.WriteLine("Exception performing Timer operation");
            }
        }

        /// <summary>
        /// Starts the dispatch timer.
        /// </summary>
        /// <remarks>
        /// When you create a <see cref="Timer"/> object, by default it is not started
        /// (that is, <see cref="IsRunning"/> is <b>false</b>). When you want to use the timer,
        /// you must first call this method.
        /// </remarks>
        public void Start()
        {

            if (!activeTimers.Contains(this))
            {
                activeTimers.Add(this, null);
                //Debug.WriteLine("Added reference to active timer with interval " + Interval);
            }

            this.IsRunning = true;
        }

        /// <summary>
        /// Stops the dispatch timer.
        /// </summary>
        public void Stop()
        {
            if (activeTimers.Contains(this))
            {
                activeTimers.Remove(this);
                //Debug.WriteLine("Removed reference to inactive timer with interval " + Interval);
            }
            this.IsRunning = false;
        }

        /// <summary>
        /// Restarts the timer
        /// </summary>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Gets a value that indicates whether this timer is running.
        /// </summary>
        public bool IsRunning { set; get; }



        /// <summary>
        /// Gets the interval that was assigned to this timer.
        /// </summary>
        TimeSpan _Interval;
        public TimeSpan Interval
        {
            set
            {
                this._Interval = value;
                this.dt.Change(new TimeSpan(0, 0, 0), _Interval);
            }
            get { return _Interval; }
        }

        /// <summary>
        /// Represents the delegate that is used for the <see cref="Tick"/> event.
        /// </summary>
        /// <param name="timer">The timer associated with the tick.</param>
        public delegate void TickEventHandler(GadgeteerTimer timer);

        /// <summary>
        /// Raised after each timer interval specified by <see cref="Interval"/>.
        /// </summary>
        /// <remarks>
        /// This event is raised when the timer is running, that is <see cref="IsRunning"/> is <b>true</b>.
        /// Handle this event to perform periodic actions.
        /// </remarks>
        public event TickEventHandler Tick;

        /// <summary>
        /// Gets a hash code for this object
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return this.dt.GetHashCode();
        }


    }
}

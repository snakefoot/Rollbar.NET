﻿[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTest.Rollbar")]

namespace Rollbar
{
    using Rollbar.Diagnostics;
    using Rollbar.DTOs;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// Implements disposable implementation of IRollbar.
    /// 
    /// All the logging methods implemented in synchronous "blocking" fashion.
    /// Hence, the payload is either delivered or a method timed out when
    /// the methods return.
    /// 
    /// </summary>
    internal class RollbarLoggerBlockingWrapper
        : ILogger
        , IDisposable
    {
        private readonly RollbarLogger _asyncLogger = null;
        private readonly TimeSpan _timeout;

        private void Report(object dataObject, ErrorLevel level, IDictionary<string, object> custom = null)
        {
            using (var signal = this.CreateSignalObject())
            {
                this._asyncLogger.EnqueueAsync(dataObject, level, custom, this._timeout, signal);

                WaitAndCompleteReport(signal);
            }
        }

        private SemaphoreSlim CreateSignalObject()
        {
            SemaphoreSlim signal = 
                new SemaphoreSlim(initialCount: 0, maxCount: 1);

            return signal;
        }

        private void WaitAndCompleteReport(SemaphoreSlim signal)
        {
            if (!signal.Wait(this._timeout))
            {
                throw new TimeoutException("Posting a payload to the Rollbar API Service timed-out");
            }

            return;
        }

        public RollbarLoggerBlockingWrapper(RollbarLogger asyncLogger, TimeSpan timeout)
        {
            Assumption.AssertNotNull(asyncLogger, nameof(asyncLogger));

            this._asyncLogger = asyncLogger;
            this._timeout = timeout;
        }

        #region ILogger

        public ILogger AsBlockingLogger(TimeSpan timeout)
        {
            return new RollbarLoggerBlockingWrapper(this._asyncLogger, timeout);
        }

        public ILogger Log(Data data)
        {
            this.Report(data, data.Level.HasValue ? data.Level.Value : ErrorLevel.Debug);
            return this;
        }

        public ILogger Log(ErrorLevel level, object obj, IDictionary<string, object> custom = null)
        {
            if (this._asyncLogger.Config.LogLevel.HasValue && level < this._asyncLogger.Config.LogLevel.Value)
            {
                // nice shortcut:
                return this;
            }

            this.Report(obj, level, custom);
            return this;
        }

        public ILogger Critical(object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Critical, obj, custom);
        }

        public ILogger Error(object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Error, obj, custom);
        }

        public ILogger Warning(object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Warning, obj, custom);
        }

        public ILogger Info(object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Info, obj, custom);
        }

        public ILogger Debug(object obj, IDictionary<string, object> custom = null)
        {
            return this.Log(ErrorLevel.Debug, obj, custom);
        }

        #endregion ILogger

        //#region IRollbar

        //public ILogger Logger => this;

        //public IRollbarConfig Config
        //{
        //    get { return this._asyncLogger.Config; }
        //}

        //public IRollbar Configure(IRollbarConfig settings)
        //{
        //    Assumption.AssertNotNull(settings, nameof(settings));
        //    this._asyncLogger.Config.Reconfigure(settings);

        //    return this;
        //}

        //public IRollbar Configure(string accessToken)
        //{
        //    return this.Configure(new RollbarConfig(accessToken));
        //}

        //public event EventHandler<RollbarEventArgs> InternalEvent
        //{
        //    add { this._asyncLogger.InternalEvent += value; }
        //    remove { this._asyncLogger.InternalEvent -= value; }
        //}

        //#endregion IRollbar

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this._asyncLogger.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RollbarLoggerBlockingWrapper() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// This code added to correctly implement the disposable pattern.
        /// </remarks>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

    }
}

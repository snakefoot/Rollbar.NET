﻿namespace Rollbar.PlugIns.Serilog
{
    using global::Serilog;
    using global::Serilog.Configuration;
    using System;

    /// <summary>
    /// Class RollbarSinkExtensions.
    /// </summary>
    public static class RollbarSinkExtensions
    {
        /// <summary>
        /// Rollbars the sink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarAccessToken">The Rollbar access token.</param>
        /// <param name="rollbarEnvironment">The Rollbar environment.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  string rollbarAccessToken,
                  string rollbarEnvironment,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider = null)
        {
            IRollbarConfig config = new RollbarConfig(rollbarAccessToken)
            {
                Environment = rollbarEnvironment,
            };

            return loggerConfiguration.RollbarSink(config, rollbarBlockingLoggingTimeout, formatProvider);
        }

        /// <summary>
        /// Rollbars the sink.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="rollbarConfig">The Rollbar configuration.</param>
        /// <param name="rollbarBlockingLoggingTimeout">The Rollbar blocking logging timeout.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>LoggerConfiguration.</returns>
        public static LoggerConfiguration RollbarSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                  IRollbarConfig rollbarConfig,
                  TimeSpan? rollbarBlockingLoggingTimeout,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new RollbarSink(rollbarConfig, rollbarBlockingLoggingTimeout, formatProvider));
        }
    }
}

using System;
using log4net;
using System.Collections.Generic;
using System.Text;

namespace GestorDocumentosUtil
{
    public class LogManager
    {
        private static readonly Lazy<LogManager> _instance = new Lazy<LogManager>(() => new LogManager());
        internal static LogManager Instance { get { return _instance.Value; } }

        private ILog _logger;


        private LogManager()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = log4net.LogManager.GetLogger("Application.Logger");
        }

        private void Configure()
        {
            log4net.Config.XmlConfigurator.Configure();
            _logger = log4net.LogManager.GetLogger("Application.Logger");

        }

        internal void Info(string message)
        {
            if (_logger == null)
                Configure();
            _logger.Info(message);
        }

        internal void Warn(string message)
        {
            _logger.Warn(message);
        }

        internal void Debug(string message)
        {
            _logger.Debug(message);
        }

        internal void Error(string message)
        {
            if (_logger == null)
                Configure();
            _logger.Error(message);
        }

        internal void Error(Exception ex)
        {

            _logger.Error(ex.Message, ex);
        }

        internal void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        internal void Fatal(Exception ex)
        {
            _logger.Fatal(ex.Message, ex);
        }
    }
}

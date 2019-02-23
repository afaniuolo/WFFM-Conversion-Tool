using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace WFFM.ConversionTool.Library.Logging
{
	public class Log4NetAdapter<T> : ILogger
	{
		private readonly log4net.ILog m_Adaptee;

		public Log4NetAdapter()
		{
			m_Adaptee = LogManager.GetLogger(typeof(T));

			log4net.Config.XmlConfigurator.Configure();
			Trace.Listeners.Add(new Log4NetTraceListener());
		}

		public void Log(LogEntry entry)
		{
			//Here invoke m_Adaptee
			if (entry.Severity == LoggingEventType.Debug)
				m_Adaptee.Debug(entry.Message, entry.Exception);
			else if (entry.Severity == LoggingEventType.Information)
				m_Adaptee.Info(entry.Message, entry.Exception);
			else if (entry.Severity == LoggingEventType.Warning)
				m_Adaptee.Warn(entry.Message, entry.Exception);
			else if (entry.Severity == LoggingEventType.Error)
				m_Adaptee.Error(entry.Message, entry.Exception);
			else
				m_Adaptee.Fatal(entry.Message, entry.Exception);
		}
	}
}

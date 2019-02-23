using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFFM.ConversionTool.Library.Logging
{
	public class Log4NetTraceListener : System.Diagnostics.TraceListener
	{
		private readonly log4net.ILog _log;

		public Log4NetTraceListener()
		{
			_log = log4net.LogManager.GetLogger("System.Diagnostics.Redirection");
		}

		public Log4NetTraceListener(log4net.ILog log)
		{
			_log = log;
		}

		public override void Write(string message)
		{
			if (_log != null)
			{
				_log.Debug(message);
			}
		}

		public override void WriteLine(string message)
		{
			if (_log != null)
			{
				_log.Debug(message);
			}
		}
	}
}

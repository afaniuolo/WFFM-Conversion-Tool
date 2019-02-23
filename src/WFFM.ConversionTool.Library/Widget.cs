using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Logging;

namespace WFFM.ConversionTool.Library
{
	public class Widget
	{
		private ILogger logger;

		public Widget(ILogger iLogger)
		{
			this.logger = iLogger;
		}

		public void Foo()
		{
			logger.Log(new LogEntry(LoggingEventType.Information, "This is an INFO message."));
		}
	}
}

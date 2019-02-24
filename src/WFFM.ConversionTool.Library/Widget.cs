using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Logging;

namespace WFFM.ConversionTool.Library
{
	public class Widget
	{
		private ILogger logger;
		private Database.WFFM.WFFM _wffmContext;
		private SitecoreForms _sitecoreFormsContext;

		public Widget(ILogger iLogger, Database.WFFM.WFFM wffmContext, SitecoreForms sitecoreFormsContext)
		{
			this.logger = iLogger;
			this._wffmContext = wffmContext;
			this._sitecoreFormsContext = sitecoreFormsContext;
		}

		public void Foo()
		{
			logger.Log(new LogEntry(LoggingEventType.Information, "This is an INFO message."));
			logger.Log(new LogEntry(LoggingEventType.Information, _wffmContext.FieldDatas.Count().ToString()));
			logger.Log(new LogEntry(LoggingEventType.Information, _sitecoreFormsContext.FieldDatas.Count().ToString()));
		}
	}
}

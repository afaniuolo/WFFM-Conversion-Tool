using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using SimpleInjector;
using WFFM.ConversionTool.Library;
using WFFM.ConversionTool.Library.Logging;
using ILogger = WFFM.ConversionTool.Library.Logging.ILogger;

namespace WFFM.ConversionTool.Console
{
	class Program
	{
		static readonly Container container;

		static Program()
		{
			container = new Container();

			container.RegisterConditional(typeof(ILogger),
				c => typeof(Log4NetAdapter<>).MakeGenericType(c.Consumer.ImplementationType),
				Lifestyle.Singleton,
				c => true);

			container.Register<Widget>();

			container.Verify();
		}

		static void Main(string[] args)
		{
			var widget = container.GetInstance<Widget>();
			widget.Foo();
		}
	}
}

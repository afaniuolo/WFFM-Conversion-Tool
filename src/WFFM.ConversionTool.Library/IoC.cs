using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Logging;

namespace WFFM.ConversionTool.Library
{
	public static class IoC
	{
		public static Container Initialize()
		{
			Container container = new Container();

			container.RegisterConditional(typeof(ILogger),
				c => typeof(Log4NetAdapter<>).MakeGenericType(c.Consumer.ImplementationType),
				Lifestyle.Singleton,
				c => true);

			// Entity Framework Contexts registration
			container.RegisterSingleton<Database.WFFM.WFFM>(CreateNewSourceContext);
			container.RegisterSingleton<SitecoreForms>(CreateNewDestContext);

			container.Register<Widget>();

			container.Verify();

			return container;
		}

		private static Database.WFFM.WFFM CreateNewSourceContext()
		{
			var myContext = new Database.WFFM.WFFM();
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}

		private static SitecoreForms CreateNewDestContext()
		{
			var myContext = new SitecoreForms();
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}
	}
}

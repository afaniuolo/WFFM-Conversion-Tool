using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleInjector;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Repositories;

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
			container.RegisterSingleton<SourceMasterDb>(createMasterDbSourceContext);
			container.RegisterSingleton<DestMasterDb>(createMasterDbDestContext);

			container.Register<IDestMasterRepository, DestMasterRepository>();
			container.Register<ISourceMasterRepository, SourceMasterRepository>();

			container.Register<IFieldFactory, FieldFactory>();

			container.Register<IItemConverter, FormConverter>();

			container.Register<FormProcessor>();

			container.Verify();

			return container;
		}

		private static Database.WFFM.WFFM CreateNewSourceContext()
		{
			var myContext = new Database.WFFM.WFFM("name=WFFM");
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}

		private static SitecoreForms CreateNewDestContext()
		{
			var myContext = new SitecoreForms("name=SitecoreForms");
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}

		private static SourceMasterDb createMasterDbSourceContext()
		{
			var myContext = new SourceMasterDb("name=SourceMasterDb");
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}

		private static DestMasterDb createMasterDbDestContext()
		{
			var myContext = new DestMasterDb("name=DestMasterDb");
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}
	}
}

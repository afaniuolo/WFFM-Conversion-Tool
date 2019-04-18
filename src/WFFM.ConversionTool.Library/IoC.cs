using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleInjector;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library
{
	public static class IoC
	{
		private static Container container = new Container();

		public static Container Initialize()
		{
			container.RegisterConditional(typeof(ILogger),
				c => typeof(Log4NetAdapter<>).MakeGenericType(c.Consumer.ImplementationType),
				Lifestyle.Singleton,
				c => true);

			// Entity Framework Contexts registration
			container.RegisterSingleton<Database.WFFM.WFFM>(CreateNewSourceContext);
			container.RegisterSingleton<SitecoreForms>(CreateNewDestContext);
			container.RegisterSingleton<SourceMasterDb>(createMasterDbSourceContext);
			container.RegisterSingleton<DestMasterDb>(createMasterDbDestContext);

			// App Settings
			container.RegisterSingleton<AppSettings>(createAppSettings);
			container.Register<IMetadataProvider, MetadataProvider>();

			container.Register<IDestMasterRepository, DestMasterRepository>();
			container.Register<ISourceMasterRepository, SourceMasterRepository>();

			container.Register<IFieldFactory, FieldFactory>();

			container.Register<IItemConverter, ItemConverter>();
			container.Register<IItemFactory, ItemFactory>();

			container.Register<FormProcessor>();

			// Configuration to registere unregistered converter types
			container.ResolveUnregisteredType += (sender, e) =>
			{
				if (e.UnregisteredServiceType.IsGenericType &&
				    e.UnregisteredServiceType.GetGenericTypeDefinition() == typeof(BaseFieldConverter))
				{
					object baseConverter = container.GetInstance(typeof(BaseFieldConverter));

					// Register the instance as singleton.
					e.Register(() => baseConverter);
				}
			};

			container.Verify();

			return container;
		}

		public static IFieldConverter CreateInstance(string converterType)
		{
			try
			{
				if (string.IsNullOrEmpty(converterType))
					return null;

				var type = Type.GetType(converterType);
				return (IFieldConverter)container.GetInstance(type);
			}
			catch (Exception ex)
			{
				// TODO: Add Logging
				return null;
			}
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

		private static AppSettings createAppSettings()
		{
			//Read json file
			var appSettingsFile = System.IO.File.ReadAllText("AppSettings.json"); // TODO: Add null checks
			// Deserialize Json to Object
			return JsonConvert.DeserializeObject<AppSettings>(appSettingsFile);
		}
	}
}

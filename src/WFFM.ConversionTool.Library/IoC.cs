using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleInjector;
using WFFM.ConversionTool.Library.Database.Forms;
using WFFM.ConversionTool.Library.Providers;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Database.Master;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Logging;
using WFFM.ConversionTool.Library.Migrators;
using WFFM.ConversionTool.Library.Models.Metadata;
using WFFM.ConversionTool.Library.Processors;
using WFFM.ConversionTool.Library.Providers.FormsData;
using WFFM.ConversionTool.Library.Repositories;
using Container = SimpleInjector.Container;

namespace WFFM.ConversionTool.Library
{
	public static class IoC
	{
		private static Container container = new Container();

		private static readonly string _baseFieldConverterType = "WFFM.ConversionTool.Library.Converters.BaseFieldConverter, WFFM.ConversionTool.Library";

		public static Container Initialize()
		{
			container.RegisterConditional(typeof(ILogger),
				c => typeof(Log4NetAdapter<>).MakeGenericType(c.Consumer.ImplementationType),
				Lifestyle.Singleton,
				c => true);

			// Entity Framework Contexts registration
			container.RegisterSingleton<Library.Database.WFFM.WFFM>(CreateWffmDbContext);
			container.RegisterSingleton<SitecoreForms>(CreateExperienceFormsDbContext);
			container.RegisterSingleton<SourceMasterDb>(createMasterDbSourceContext);
			container.RegisterSingleton<DestMasterDb>(createMasterDbDestContext);

			// App Settings
			container.RegisterSingleton<AppSettings>(createAppSettings);
			container.Register<IMetadataProvider, MetadataProvider>();

			// Repositories
			container.Register<IDestMasterRepository, DestMasterRepository>();
			container.Register<ISourceMasterRepository, SourceMasterRepository>();
			container.Register<ISitecoreFormsDbRepository, SitecoreFormsDbRepository>();

			container.Register<IFieldFactory, FieldFactory>();
			container.Register<IFieldProvider, FieldProvider>();

			container.Register<IItemConverter, ItemConverter>();
			container.Register<IItemFactory, ItemFactory>();

			container.Register<FormProcessor>();
			container.Register<SubmitConverter>();
			container.Register<AppearanceConverter>();

			container.Register<DataMigrator>();

			RegisterFormsDataProvider();

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

		public static IFieldConverter CreateConverter(string converterName)
		{
			var converterType = _baseFieldConverterType;
			if (converterName != null)
			{
				var metaConverter = createAppSettings().converters.FirstOrDefault(c => c.name == converterName)?.converterType;
				if (!string.IsNullOrEmpty(metaConverter))
				{
					converterType = metaConverter;
				}
			}
			return CreateInstance(converterType);
		}

		private static IFieldConverter CreateInstance(string converterType)
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

		private static Library.Database.WFFM.WFFM CreateWffmDbContext()
		{
			var myContext = new Library.Database.WFFM.WFFM("name=WFFM");
			myContext.Configuration.ProxyCreationEnabled = false;
			return myContext;
		}

		private static SitecoreForms CreateExperienceFormsDbContext()
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

		private static void RegisterFormsDataProvider()
		{
			var appSettings = createAppSettings();
			if (string.Equals(appSettings.formsDataProvider, "sqlFormsDataProvider",
				StringComparison.InvariantCultureIgnoreCase))
			{
				container.Register<IDataProvider, SqlDataProvider>();
			}
			else if (string.Equals(appSettings.formsDataProvider, "analyticsFormsDataProvider",
				StringComparison.InvariantCultureIgnoreCase))
			{
				container.Register<IDataProvider, MongoDbDataProvider>();
			}
		}
	}
}

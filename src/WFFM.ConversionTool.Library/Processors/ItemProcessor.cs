using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Factories;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public abstract class ItemProcessor : IItemProcessor
	{
		private IDestMasterRepository _destMasterRepository;
		private IItemConverter _itemConverter;
		private IItemFactory _itemFactory;

		public ItemProcessor(IDestMasterRepository destMasterRepository, IItemConverter itemConverter, IItemFactory itemFactory)
		{
			_destMasterRepository = destMasterRepository;
			_itemConverter = itemConverter;
			_itemFactory = itemFactory;

		}

		public virtual void ConvertAndWriteItem(SCItem sourceItem, Guid parentId)
		{
			// Convert
			var destItems = _itemConverter.Convert(sourceItem, parentId);

			// Write to dest
			foreach (var destItem in destItems)
			{
				_destMasterRepository.AddOrUpdateSitecoreItem(destItem);
			}
		}

		public virtual Guid WriteNewItem(Guid destTemplateId, SCItem parentItem, string itemName)
		{
			var destItem = _itemFactory.Create(destTemplateId, parentItem, itemName);

			_destMasterRepository.AddOrUpdateSitecoreItem(destItem);

			return destItem.ID;
		}
	}
}

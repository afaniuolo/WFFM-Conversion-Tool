using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WFFM.ConversionTool.Library.Converters;
using WFFM.ConversionTool.Library.Models.Sitecore;
using WFFM.ConversionTool.Library.Repositories;

namespace WFFM.ConversionTool.Library.Processors
{
	public abstract class ItemProcessor : IItemProcessor
	{
		private IDestMasterRepository _destMasterRepository;
		private IItemConverter _itemConverter;

		public ItemProcessor(IDestMasterRepository destMasterRepository, IItemConverter itemConverter)
		{
			_destMasterRepository = destMasterRepository;
			_itemConverter = itemConverter;

		}

		public virtual void ConvertAndWriteItem(SCItem sourceItem, Guid parentId)
		{
			// Convert
			var destItem = _itemConverter.Convert(sourceItem, parentId);

			// Write to dest
			_destMasterRepository.AddOrUpdateSitecoreItem(destItem);
		}		
	}
}

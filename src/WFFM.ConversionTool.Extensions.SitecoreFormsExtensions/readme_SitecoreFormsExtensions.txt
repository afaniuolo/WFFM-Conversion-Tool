+------------------------------------------------------+
|              Installation Instructions               |
+------------------------------------------------------+

1) Copy the entire module extension folder in the WFFM Conversion Tool root folder. When prompted, choose to merge the existing folders.

2) Open the AppSettings.json file located in the WFFM Conversion Tool root folder and uncomment the following JSON object in the "converters" property:

//,
//{
//    "name": "FileUploadConverter",
//    "converterType": "WFFM.ConversionTool.Extensions.SitecoreFormsExtensions.Converters.FieldValueConverter.FileUploadConverter, WFFM.ConversionTool.Extensions.SitecoreFormsExtensions"
//}

3) In the AppSettings.json file, modify the following settings to configure the File Upload field data conversion process:

"extensions": {
        "sitecoreFormsExtensions_FileDownloadUrlBase": "https://myfile.com/{0}", // File Download Url Base for Sitecore Forms Extensions File Upload data.
        "sitecoreFormsExtensions_UseItemIdForFilename": "false" // Default value: "false". If "true", the media item filename for the Sitecore Forms Extension File Upload data is built using the media item ID instead of the media item name. 
    }
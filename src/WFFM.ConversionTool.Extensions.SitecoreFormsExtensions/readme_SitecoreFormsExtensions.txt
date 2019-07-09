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

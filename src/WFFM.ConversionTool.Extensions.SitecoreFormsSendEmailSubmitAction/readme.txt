+------------------------------------------------------+
|              Installation Instructions               |
+------------------------------------------------------+

1) Copy the entire module extension folder in the WFFM Conversion Tool root folder. When prompted, choose to merge the existing folders.

2) Open the AppSettings.json file located in the WFFM Conversion Tool root folder and uncomment the following JSON object in the "submitActions" property:

{
    "sourceSaveActionId": "{D4502A11-9417-4479-9F2A-485F45D2E2D0}",
    "destSubmitActionItemName": "Send Email",
    "destSubmitActionFieldValue": "{065C982A-BA27-45E6-BC9B-F2821D904021}",
    "destParametersConverterType": "WFFM.ConversionTool.Extensions.Sitecore_Forms_Send_Email_Submit_Action.Converters.SendEmailConverter, WFFM.ConversionTool.Extensions"
}
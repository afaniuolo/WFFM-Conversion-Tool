# WFFM Conversion Tool
The WFFM Conversion Tool is a console application that provides an automated solution to convert and migrate Sitecore Web Forms For Marketers (WFFM) forms items and their data to Sitecore Forms.

The tool provides the ability to analyze the source items in the source Sitecore instance, before executing the actual conversion and migration of items to the destination Sitecore instance.

The tool offers the users the choice to migrate saved WFFM forms data from a SQL database or from a MongoDB database source to the destination Sitecore Experience Forms SQL database.

## Sitecore Compatibility
The tool has been designed to support the migration of sitecore items from any version of Sitecore to Sitecore 9.1+. The tool has been tested migrating WFFM forms items and data starting from Sitecore version 6.5.

## Technical Requirements
### Software:
- .NET Framework 4.7.1+
- Windows OS
### Hardware:
- No particular hardware requirement

## How to Install the Tool
The tool is distributed in a ZIP archive. The latest release can be downloaded here: https://github.com/afaniuolo/WFFM-Conversion-Tool/releases/latest.

The tool can be installed in any server or local machine that has access to the databases involved in the conversion and migration process. The tool doesn't require the source and destination Sitecore instances to run in IIS while the conversion process occurs, because the tool connects directly to the Sitecore databases, without using Sitecore APIs or other Sitecore modules.

### Installation Steps
1) Download the [latest release](https://github.com/afaniuolo/WFFM-Conversion-Tool/releases/latest) of the tool.
2) Extract the ZIP archive in a folder on the machine where the tool will be executed.
3) Assign read-write permissions to the folder and its subitems for the user that will execute the tool.

## How to Configure the Tool
The tool needs to be configured before using it. The following steps describe the configuration options.

1) Open the `WFFM.ConversionTool.exe.config` file and populate the following connection strings:   
    - `SourceMasterDb` - The connection string to the source Sitecore Master database.
    - `DestMasterDb` - The connection string to the destination Sitecore Master database.
    - `SitecoreForms` - The connection string to the destination Sitecore Experience Forms database.
    - `WFFM` - The connection string to the source Sitecore WFFM database (if applicable).
    - `mongodb_analytics` - The connection string to the source Sitecore Analytics MongoDB database (if applicable)

    NOTE: *Only one between the WFFM and the mongodb_analytics connection string is required, and it depends on the type of  data source used in the source Sitecore instance to save WFFM forms data.*

2) Open the `AppSettings.json` file and modify the following settings, if needed:
    - `formsDataProvider` - This setting is used to configure the data provider used to read the stored forms data to migrate. Available values:
        - `sqlFormsDataProvider` - Used to select the SQL Sitecore WFFM database source (default value).
        - `analyticsFormsDataProvider` - Used to select the MongoDB Analytics database source.
    - `invalidItemNameChars` - This settings is used to configure the list of invalid characters to be excluded when creating a new item's name.
    - `enableReferencedItemCheck` - This setting is used to enable or disable the check existence of a referenced item in any form fields value. When enabled, the tool will not populate fields that refers an item that doesn't exist in the destination Sitecore master database.
    - `analysis_ExcludeBaseStandardFields` - This setting is used to exclude base standard fields of Sitecore items from the list of fields that are reported about in the *conversion analysis report* output. The exclusion of the base standard fields from the analysis reporting process doesn't exclude them from the conversion process.
    - `includeOnlyFormIds` - This setting is used to select a subset of source forms to convert, specifying an array of source form IDs. This selection applies to both forms items conversion process and forms data migration process.
    - `excludeFormIds` - This setting is used to exclude a subset of source forms from the conversion process, specifying an array of source form IDs. This selection applies to both forms items conversion process and forms data migration process.

## How to Use the Tool
The tool should be executed in a Command Prompt window application in order to control its input parameters and visualize the execution progress. 

In the context of a Sitecore upgrade project, if all Sitecore content items have already been migrated to the destination Sitecore master database (for example using the Sitecore Express Migration tool), then the WFFM forms content items should be deleted from the destination Sitecore instance before running the WFFM Conversion Tool. 

1) Launch a Command Prompt window application.
2) Browse to the root folder where the tool has been extracted to.
3) The tool is executed running the `WFFM.ConversionTool.exe` executable file. Available options are:
    - `WFFM.ConversionTool.exe` (no input parameters) - Use this option to execute a dry run of the forms item conversion process, without writing any data in the destination databases. The *conversion analysis report* will be generated and stored in the `Analysis` folder.
    - `WFFM.ConversionTool.exe help` or `WFFM.ConversionTool.exe ?` - Use this option to get a list of available input parameters.
    - `WFFM.ConversionTool.exe -convert` - Use this option to execute the conversion and migration of both forms items and their stored data to the destination databases.
    - `WFFM.ConversionTool.exe -convert -nodata` - Use this option to execute the conversion and migration of forms items only. Stored forms data will not be converted and migrated.
    - `WFFM.ConversionTool.exe -convert -onlydata` - Use this option to execute the conversion and migration of forms data only. Only data of forms previously converted and migrated on the destination instance will be converted and migrated by this process. Data of forms that don't exist on the destination instance will not be converted and migrated.
4) After the tool execution is finished successfully, if the destination Sitecore instance was running in IIS while the tool was executed, recycle the app pool of the destination Sitecore web application to clear the Sitecore Items cache.
5) Once you login in the destination Sitecore instance, the migrated forms will not be listed in the Forms section (accessible from the Sitecore Desktop) until the `sitecore_master_index` index is rebuilt. Use the Indexing Manager in the Sitecore Control Panel to rebuild it.

NOTE: The *conversion analysis report* is always generated, independently if it is a dry run or a real conversion being executed.

## Conversion Behaviors
The tool enforces some specific behaviors during the conversion process, following best practices. Some of them are:
- *Page Creation* - For each form, the tool creates a new *Page* item under the parent Form item.
- *Text Fields Creation* - Any decorator fields, like for example the form title, the form introduction or the form footer, are converted in *Text* fields and positioned in the form in the correct location.
- *Success Message Creation in Second Page* - For each form that shows a success message when a submit action is successful, the tool creates a second *Page* item with a *Text* field that contains the success message. The full approach has been implemented following the answer in [this](https://sitecore.stackexchange.com/questions/14834/how-to-show-success-message-in-sitecore-9-forms) Sitecore StackExchange question.
 
## Metadata Configuration
The tool uses mapping configuration rules for each destination form item entity (form, fields, buttons, text,...) to control the conversion of form items and their fields. These mapping rules are defined and stored in *Metadata Template* files in JSON format, all stored under the `Metadata` folder. All *Metadata Template* files are validated using the `metadata-schema.json` file stored in the `Schemas` folder.

*Metadata Template* objects can inherit some properties from other metadata files, avoiding the repetition of mapping rules across multiple *Metadata Template* objects. The base metadata file that doesn't inherit from any other metadata file is the `baseTemplate.json` metadata file.

### Metadata Properties
The valid properties available in the *Metadata Template* files are the following:
- `sourceTemplateId` - Template Id of the source item.
- `sourceTemplateName` - Template Name of the source item.
- `destTemplateId` - Template Id of the destination item.
- `destTemplateName` - Template Name of the destination item.
- `baseTemplateMetadataFileName` - Filename of the inherited base *Metadata Template*.
- `sourceMappingFieldId` - Id of the field of the source item used to map the Form Field type (applicable to field items only).
- `sourceMappingFieldValue` - Value of the field of the source item used to map the Form Field type (applicable to field items only).
- `dataValueType` - Value type of the data stored for a particular form field type. The value type is stored in the records of the `FieldData` table of the Sitecore Experience Forms destination database.
- `dataValueConverter` - Name of converter used to convert the data value of a particular form field type. All converters are defined in the `AppSettings.json` configuration file and referred by name.
- `fields` - This properties contains the conversion mappings for the fields of a specific item. The fields are organized in three categories:
    - `newFields` - Array of fields that don't exist in the source Sitecore item, but are required in the Sitecore destination item. Each *newField* object can have the following properties:
        - `fieldType` - Type of Sitecore field (shared, versioned, unversioned).
        - `destFieldId` - Id of the field of the destination Sitecore item.
        - `value` - Default value to assign to the new field when created.
        - `valueType` - Used to create a dynamic value of a specific type for the new field.
    - `existingFields` - Array of fields that exist in both the source Sitecore item and the destination Sitecore item and that don't require any convertion. These are Sitecore base fields, usually named using a double underscore (`__`) prefix.
        - `fieldId` - Id of the existing field to migrate.
    - `convertedField` - Array of fields of the source Sitecore item that are converted and mapped to different fields in the destination Sitecore item.
        - `fieldConverter` - Name of the converter used to convert the value of the source field.
        - `sourceFieldId` - Id of the field of the source Sitecore item.
        - `destFieldId` - Id of the field of the destination Sitecore item.
        - `destFields` - Array of destinaton fields used to map values stored in a single source field that are converted in multiple different destination fields. The values in the source field are organized in a pseudo XML document object. Each *destField* object can have the following properties:
            - `sourceElementName` - The name of the XML element stored in the source field value.
            - `destFieldId` - Id of the field of the destination Sitecore item.
            - `fieldConverter` - Name of the converter used to convert the value of the XML element.

## Analysis Conversion Report
WFFM form items and Sitecore Forms items are very different and even if the tool is capable of converting most of the entites, there are some item fields that cannot be converted and migrated. The *analysis conversion report* is a report in CSV format generated by the tool, that lists the Sitecore item fields that the tool cannot convert, because they are not mapped or not supported.

The *analysis convertion report* contains the following columns for each record:
- `ItemId` - Id of the source item.
- `ItemName` - Name of the source item.
- `ItemPath` - Path of the source item.
- `ItemVersion` - Version of the source item.
- `ItemLanguage` - Language of the source item.
- `ItemTemplateId` - Template Id of the source item.
- `ItemTemplateName` - Template Name of the source item.
- `FieldId` - Id of the field of the souce item.
- `FieldName` - Name of the field of the source item.
- `FieldType` - Type of the field of the source item.
- `FieldValue` - Value of the field of the source item.
- `FieldValueElementName` - Name of the XML element stored in the field of the source item.
- `FieldValueElementValue` - Value of the XML element stored in the field of the source item.
- `FieldValueReferencedItemId` - Id of the item referenced as value of the XML value element.
- `FieldValueReferencedItemName` - Name of the item referenced as value of the XML value element.
- `Message` - Analysis result message. Possible values are:
    - *Source Field Not Mapped* - The source field is not mapped in the *Metadata Template* file or a conversion for that particular field is not possible.
    - *Form Save Action Not Mapped* - The form save action is not mapped.
    - *Source Field Element Value Not Mapped* - The source field XML element value is not mapped.
    - *Form Field Item Not Mapped - Form Field Type Name = field-type-name* - The form field type is not mapped, because the field is a custom field type. Form fields that are not mapped are still migrated and converted using the default destination *Input* form field type. 

## WFFM Conversion Tool Extensions - NEW !!!
The v1.1.0 release of the WFFM Conversion Tool introduces the availability of the *WFFM Conversion Tool Extensions*, a group of plugins that add support for the conversion of WFFM forms items that don't exist in the Sitecore Forms out-of-the-box solution, but that are extended using popular Sitecore modules available on the Sitecore Marketplace.

The WFFM Conversion Tool Extensions plugins are available in the `Extensions` folder in the tool root folder. Each *extension* plugin has its own subfolder that contains the plugin files and a `readme_<module_name>.txt` file that describes step-by-step instructions to install each plugin. 

### Sitecore Forms Extensions
The *extension* plugin for the [Sitecore Forms Extensions](https://github.com/bartverdonck/Sitecore-Forms-Extensions) module developed by Bart Verdonck adds the support for the conversion of the following two form field types:
- Captcha field
- File Upload field

NOTE: The data of the File Upload field is converted and migrated using the storage format of the File System storage provider (`FileSystemFileUploadStorageProvider` class).

### Sitecore Forms Send Email Submit Action
The *extension* plugin for the [Sitecore Forms Send Email Submit Action](https://marketplace.sitecore.net/Modules/S/Sitecore_Forms_Send_Email_Submit_Action.aspx) module developed by Byron Calisto adds the support for the conversion of the *Send Email* submit action.

## How to Expand the WFFM Conversion Tool to Convert Custom Entities
Some of the items or fields that cannot be mapped could be custom items created to expand the out-of-the-box functionality of the Sitecore WFFM module. For example, custom entities could be custom form field types or custom save actions. To help developers to automate the conversion and migration of custom entities, the tool allows to expand its default mapping capabilities, as described in the next three sections.

### How to Map a Custom Form Field Type
Any metadata .json file stored under the `Metadata` folder is parsed and validated by the tool when its execution starts. A custom form field type will requires its own *Metadata Template* file to describe its conversion fields mappings.

Usually custom field types are customization of a default WFFM form field type. The customization adds additional parameters in the *Parameters* field or in the *Localized Parameters* field of the form field item. An easy way to create a new metadata file for the custom form field is copying the metadata file of the default WFFM form field type that the custom form field is based on and modifying its properties.

The `baseTemplateMetadataFileName` property should contain the name of the metadata file of the inherited default WFFM form field type. 

A useful online resource to validate the content of a metadata file is the [JSON Schema Validator](https://www.jsonschemavalidator.net/). The JSON schema of the metadata file is included in the tool `Schemas` folder and also available [here](https://github.com/afaniuolo/WFFM-Conversion-Tool/blob/master/src/WFFM.ConversionTool/Schemas/metadata-schema.json).

### How to Customize the Conversion of a Field Value
The *fieldConverter* property of *convertedField* mapping objects in metadata files allows to specify the name of a converter to process the source value of a field. Converters are defined in the `AppSettings.json` configuratio file in the *converters* property. This property is an array of *converter* objects that have the following properties:
- `name` - Name of the converter, used as value in the *fieldConverter* property in metadata files.
- `converterType` - Type of the class object that defines the converter.

The tool allows the loading of converter types defined in external libraries at runtime. The converter class needs to be created inheriting the `BaseFieldConverted` class defined in the `WFFM.ConversionTool.Library.dll` (that should be referenced in the custom class library project), and overriding the `ConvertValue` method. This method takes a string as input and return a string. The base method just returns the input, without applying any conversion.

The new custom converter will need to be added in the list of converters in the `AppSettings.json` configuration file. Its library dll file should be copied in the `libs` folder of the tool.

### How to Map a Custom Submit Action
Custom submit actions can be defined and mapped in the `AppSettings.json` file in the `submitActions` property. This property contains an array of `submitAction` objects, that have the following properties:
- `sourceSaveActionId` - The Id of the source save action item.
- `destSubmitActionItemName` - The name of the destination submit action item.
- `destSubmitActionFieldValue` - The value to store in the destination Submit Action field value.
- `destParametersConverterType` - The name of the field converter to use to convert the save action parameters.

The custom field converter should be created as described in the previous section, overriding the `ConvertValue` method defined in the inherited `BaseFieldConverted` class.

## Bugs and Feature Requests
Please open new Issue [here](https://github.com/afaniuolo/WFFM-Conversion-Tool/issues) to report any bug that you might encounter in the execution of the tool or to suggest a modification or a new feature for the tool.

Thank you!

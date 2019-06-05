# WFFM Conversion Tool
The WFFM Conversion Tool is a console application that provides an automated solution to convert and migrate Sitecore Web Forms For Marketers (WFFM) forms items and their data to Sitecore Forms.

The tool provides the ability to analyze the source items in the source Sitecore instance, before executing the actual conversion and migration of items to the destination Sitecore instance.

The tool offers to the user the choice to migrate saved WFFM forms data from a SQL database or from a MongoDB database source to the destination Sitecore Experience Forms SQL database.

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
    - `WFFM` - The connection string to the source Sitecore WFFM database (if applicable).
    - `SitecoreForms` - The connection string to the destination Sitecore Experience Forms database.
    - `SourceMasterDb` - The connection string to the source Sitecore Master database.
    - `DestMasterDb` - The connection string to the destination Sitecore Master database.
    - `mongodb_analytics` - The connection string to the source Sitecore Analytics MongoDB database (if applicable)

    NOTE: *Only one between the WFFM and the mongodb_analytics connection strings is required, based on the available data source.*

2) Open the `AppSettings.json` file and modify the following settings, if needed:
    - `formsDataProvider` - This setting is used to configure the data provider used to read the stored forms data to migrate. Available values:
        - `sqlFormsDataProvider` - Used to select the SQL Sitecore WFFM database source (default value).
        - `analyticsFormsDataProvider` - Used to select the MongoDB Analytics database source.
    - `invalidItemNameChars` - This settings is used to configure the list of invalid characters to be excluded when creating a new item's name.
    - `enableReferencedItemCheck` - This setting is used to enable or disable the check existence of a referenced item in any form fields value. When enabled, the tool will not populate fields that refers an item that doesn't exist in the destination Sitecore master database.
    - `analysis_ExcludeBaseStandardFields` - This setting is used to exclude base standard fields of Sitecore items from the list of fields that are reported about in the *conversion analysis report* output. The exclusion of the base standard fields from the analysis reporting process doesn't exclude them from the conversion process.

## How to Use the Tool
The tool should be executed in a Command Prompt window application in order to control its input parameters and visualize the execution progress.

1) Launch a Command Prompt window application.
2) Browse to the folder where the tool has been extracted to.
3) The tool is executed running the `WFFM.ConversionTool.exe` executable file. Available options:
    - `WFFM.ConversionTool.exe` (no input parameters) - Use this option to execute a dry-run of the forms item conversion process, without writing any data in the destination databases. The *conversion analysis report* will be generated and stored in the `Analysis` folder.
    - `WFFM.ConversionTool.exe help` or `WFFM.ConversionTool.exe ?` - Use this option to get a list of available input parameters.
    - `WFFM.ConversionTool.exe -convert` - Use this option to execute the conversion and migration of both forms items and their stored data to the destination databases.
    - `WFFM.ConversionTool.exe -convert -nodata` - Use this option to execute the conversion and migration of forms items only. Stored forms data will not be converted and migrated.

NOTE: The *conversion analysis report* is always generated, independently if it is a dry-run or a real conversion being executed.



 

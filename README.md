# Deployment of a Search backend based on Azure Search, Azure Function, Azure Cosmos, Azure Storage, Azure Data Lake

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FTestSearchService%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FTestSearchService%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>

This template allows you to deploy from Github a Search backend based on Azure Search, Azure Function, Azure Cosmos, Azure Storage and AZure Data Lake.
The Azure Functions are Python Azure Function. The Web App is based on ASP.NET core version 3.0 with the support of an Azure Active Directory Authentication.


![](https://raw.githubusercontent.com/flecoqui/TestSearchService/master/Docs/1-architecture.png)


Once the backend is deployed, it's possible to test the AZure Function using the following curl commands:

			curl -d "{\"param1\":\"0123456789\",\"param2\":\"abcdef\"}" -H "Content-Type: application/json"  -X POST   "https://<prefixName>function.azurewebsites.net/api/HttpTriggerPythonFunction"
			
			curl -H "Content-Type: application/json"  -X GET   "https://<prefixName>function.azurewebsites.net/api/HttpTriggerPythonFunction?param1=0123456789&param2=abcdef" 



# DEPLOYING THE SEARCH BACKEND ON AZURE 

This chapter describes how to deploy Search backend using Azure CLI, the following services will be deployed with few command lines:</p>
* **Azure App Service**</p>
* **Azure Function**</p>
* **Azure Search**</p>
* **Azure Storage**</p>
* **Azure Data Lake**</p>
* **Azure Cosmos**</p>

## PRE-REQUISITES
First you need an Azure subscription.</p>
You can subscribe here:  https://azure.microsoft.com/en-us/free/ . </p>
Moreover, we will use Azure CLI v2.0 to deploy the resources in Azure.</p>
You can install Azure CLI on your machine running Linux, MacOS or Windows from here: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest 

The first Azure CLI command will create a resource group.</p>
The second  Azure CLI command will deploy an Azure Function, an Azure App Service and a Virtual Machine using an Azure Resource Manager Template.
In order to deploy the source code of the Python Azure Functions from github, unfortunately as today it's not possible to an Azure Resource Manager template to deploy Python source code from github to an Azure Function without using Azure DevOps.
* You need to clone the code locally using the following command:


			C:\git\Me> git clone https://github.com/flecoqui/TestSearchService.git

			
* Install Python 3.6.8. (https://www.python.org/downloads/) This version of Python is verified with Functions. 3.7 and later versions are not yet supported.
* Install Azure Functions Core Tools version 2.7.1575 (https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#v2) or a later version.


## CREATE RESOURCE GROUP:
First you need to create the resource group which will be associated with this deployment. For this step, you can use Azure CLI v1 or v2.

* **Azure CLI 1.0:** azure group create "ResourceGroupName" "RegionName"

* **Azure CLI 2.0:** az group create an "ResourceGroupName" -l "RegionName"

For instance:

    azure group create TestSearchServicerg eastus2

    az group create -n TestSearchServicerg -l eastus2

## DEPLOY THE SERVICES:

### DEPLOY REST API ON AZURE FUNCTION, APP SERVICE, VIRTUAL MACHINE:
You can deploy Azure Function, Azure App Service and Virtual Machine using ARM (Azure Resource Manager) Template and Azure CLI v1 or v2

* **Azure CLI 1.0:** azure group deployment create "ResourceGroupName" "DeploymentName"  -f azuredeploy.json -e azuredeploy.parameters.json*

* **Azure CLI 2.0:** az group deployment create -g "ResourceGroupName" -n "DeploymentName" --template-file "templatefile.json" --parameters @"templatefile.parameter..json"  --verbose -o json

For instance:

    azure group deployment create TestSearchServicerg TestSearchServicedep -f azuredeploy.json -e azuredeploy.parameters.json -vv

    az group deployment create -g TestSearchServicerg -n TestSearchServicedep --template-file azuredeploy.json --parameter @azuredeploy.parameters.json --verbose -o json


When you deploy the service you can define the following parameters:</p>
* **namePrefix:** The name prefix which will be used for all the services deployed with this ARM Template</p>
* **storageSku:** The Azure Storage SKU. Default value: Standard_LRS</p>
* **searchSku:** The Azure Search SKU, by default basic</p>
* **azFunctionAppSku:** The Azure Function App Sku Capacity, by default Y1 (Consumption Plan)</p>
* **WebAppSku:** The WebApp Sku Capacity, by default S1</p>
* **repoURL:** The github repository url</p>
* **branch:** The branch name in the repository</p>
* **repoFunctionPath:** The path to the Azure Function code in the main reposotory, by default "TestFunctionApp"</p>
* **repoWebAppPath:** The path to the Web Application code, by default "TestWebApp"</p>


### DEPLOY AZURE FUNCTION PYTHON SOURCE CODE FROM GITHUB TO AZURE:

In order to deploy the source code of the Python Azure Function, you need to deploy the Python code from your machine.
</p>

Below the command lines for Windows to deploy the Python Source code for Azure Function:


			C:\git\Me> cd TestSearchService\TestFunctionApp
			C:\git\Me\TestSearchService\TestFunctionApp> python -m venv .venv
			C:\git\Me\TestSearchService\TestFunctionApp> .venv\scripts\activate
			(.env) C:\git\Me\TestSearchService\TestFunctionApp> func azure functionapp publish <prefixName>function --build remote



Once deployed, the following services are available in the resource group:


![](https://raw.githubusercontent.com/flecoqui/TestSearchService/master/Docs/1-deploy.png)


The services has been deployed with 3 command lines.

If you want to deploy one single service, you can use the resources below:</p>

* **Azure Function:** Template ARM to deploy Azure Function https://github.com/flecoqui/TestSearchService/tree/master/Azure/101-functions </p>
* **Azure App Service:** Template ARM to deploy Azure App Service  https://github.com/flecoqui/TestSearchService/tree/master/Azure/101-appservice </p>


# TEST THE SERVICES:

## TEST THE SERVICES WITH CURL
Once the services are deployed, you can test the REST API using Curl. You can download curl from here https://curl.haxx.se/download.html 
For instance :

			curl -d "{\"param1\":\"0123456789\",\"param2\":\"abcdef\"}" -H "Content-Type: application/json"  -X POST   "https://<prefixName>function.azurewebsites.net/api/HttpTriggerPythonFunction"
			curl -H "Content-Type: application/json"  -X GET   "https://<prefixName>function.azurewebsites.net/api/HttpTriggerPythonFunction?param1=0123456789&param2=abcdef" 

</p>

## TEST THE APPLICATION SERVICE WITH YOUR BROWSER
Moreover, you can open the folliwing url with your favorite browser to open the Web Application:

			https://<prefixName>web.azurewebsites.net/ 

# Next Steps

1. Improve the ASP.NET Web App
2. Support an architecture with a VNET between the Azure App Service, Azure Storage, Azure Cosmos and Azure Data Lake.
3. Currently this template only support Python Azure Function running in a consumption plan, the architecture could be improved in supporting other Azure Function Plan.

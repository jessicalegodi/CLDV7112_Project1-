 CLDV7112 Project 2 - Azure Storage Solution

 Module

Cloud Development B - CLDV7112

 Project

Project 2

 Description

This project demonstrates the integration of Microsoft Azure services into a web application for ABC Retail.

The solution uses Azure cloud storage services and Azure Functions to store, process and manage different types of information.

The project demonstrates the use of:

- Azure Table Storage
- Azure Blob Storage
- Azure Queue Storage
- Azure File Storage
- Azure App Service
- Azure functions
- ASP.NET Core MVC
- Azure App service
- Github

Application Features

The project contains four Azure Functions that interact with Azure Storage services.
The Table Function stores product information in Azure Table Storage.

The function stores information such as:

Product Name
Price
Stock Quantity
2. Blob Function

The Blob Function uploads product images and other files to Azure Blob Storage.

The project uses the following Blob container:

product-images

Function Name

BlobFunction

The function receives a file name and uploads the request content to the Blob container.

The information is stored in the following Azure Table:

Products
3. Queue Function

The Queue Function allows messages to be written to and read from Azure Queue Storage.

The project uses the following queue:

order-processing

The function supports:

POST - Adds a message to the queue
GET - Reads available queue messages
4. Azure Files Function

The Azure Files Function uploads files to Azure File Storage.

The project uses the following file share:

logs

The function is used to store system log files.

Function Name

FileFunction

Web Application

The project contains an ASP.NET Core MVC web application.

The web application includes functionality for:

Customer management
Product management
Product image management
Order processing
System log management

Technologies Used
C#
.NET 8
ASP.NET Core MVC
Azure Functions
Azure Table Storage
Azure Blob Storage
Azure Queue Storage
Azure File Storage
Azure App Service
Microsoft Azure
GitHub
Bootstrap
ST10367784

Module Code: CLDV7112



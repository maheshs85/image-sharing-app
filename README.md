# **Image Sharing Web Application with Cloud & Serverless Integration**

## **Overview**
This project is a scalable, cloud-based image-sharing web application developed using **ASP.NET Core**, **Azure Cloud Services**, and **Serverless Computing**. It allows users to upload, store, and retrieve images while ensuring high availability, security, and performance.

## **Features**
- **User Authentication & Role Management** (SQL Database, ASP.NET Identity)
- **Image Uploading & Metadata Storage** (Azure Blob Storage, Cosmos DB)
- **Asynchronous Processing** (Azure Functions for background tasks)
- **Approval Workflow** (Queue-based image approval system)
- **Logging & Monitoring** (Azure Table Storage for logs)
- **Scalability** (Dockerized deployment on Azure App Service)

## **Technology Stack**
- **Backend:** ASP.NET Core 8.0 (C#)
- **Frontend:** Razor Pages with Bootstrap
- **Database:** Azure SQL Database & Cosmos DB
- **Cloud Storage:** Azure Blob Storage & Table Storage
- **Serverless Functions:** Azure Functions (Queue and Blob triggers)
- **Authentication:** ASP.NET Identity & Azure Key Vault
- **Deployment:** Docker, Azure Container Registry, App Service

## **Project Structure**
```
/ImageSharingWithCloud
│── /Controllers       # Handles requests for users, images, and authentication
│── /Models            # Data models for users and images
│── /Views             # Razor views for UI
│── /wwwroot           # Static files and CSS
│── /AzureFunctions    # Serverless functions for async processing
│── appsettings.json   # Configuration (Keys stored in Azure Key Vault)
│── Dockerfile         # Containerization script
│── README.md          # Project documentation
```

## **Deployment & Running Locally**
### **1. Local Setup**
```bash
# Build & Run
dotnet build
dotnet run --environment "Development"
```
### **2. Running in Docker**
```bash
docker build -t imagesharing .
docker run -p 8080:80 imagesharing
```
### **3. Deploying to Azure**
```bash
# Build and push Docker image
docker tag imagesharing <your-registry>.azurecr.io/imagesharing
docker push <your-registry>.azurecr.io/imagesharing

# Deploy web app
az webapp create --resource-group <group> --plan <plan> --name <app-name> --deployment-container-image-name <your-registry>.azurecr.io/imagesharing
```

## **Key Takeaways & Achievements**
- **Optimized cloud resource utilization** by leveraging **Azure Serverless Functions** to handle image processing asynchronously.
- **Implemented secure authentication & secrets management** using **Azure Key Vault & Managed Identity**.
- **Achieved high availability and scalability** by deploying the application in **Azure App Service** with **containerized deployment**.
- **Improved performance** by using **message queues for processing uploads** instead of synchronous API calls.

## **Next Steps**
- Implement real-time notifications for image approvals.
- Introduce AI-based image categorization using Azure Cognitive Services.
- Improve front-end experience with React/Angular.

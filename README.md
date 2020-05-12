# Containerize and run Azure functions on Kubernetes with Keda

This demo shows how to containerize Azure Functions with the new Azure Function Core tools and how to run them on Kubernetes using Keda (in this case GKE)

* Azure Functions Runtime Core Tools
* Containerization of Azure Functions Apps
* Deployment of containerized functions apps to Azure Functions Linux Premium
* Deployment to containerized functions apps to Kubernetes with KEDA

## Prerequisites
For MacOS 
```
brew cask install dotnet-sdk
install docker
brew install node
brew tap azure/functions
brew install azure-functions-core-tools@3
brew install httpie
brew cask install visual-studio-code
brew cast install microsoft-azure-storage-explorer
```

## Using azure-functions-core-tools to scaffold a function app

Documentation: 
* https://www.npmjs.com/package/azure-functions-core-tools
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=macos%2Ccsharp%2Cbash


```
mkdir azfuncapp && cd azfuncapp
func init --worker-runtime dotnet --docker 
func new -l c# -t HttpTrigger -n "httpNordine"
func new -l c# -t QueueTrigger -n "queueNordine"
```

### Fix a couple of things manually in the generated code
* Change the package versions to floating version (e.g 3.0.*)
* Change dockerfile to .net sdk 3.1
* Change authentication to anonymous

### Other things (not necessary)
* Swashbuckle for swagger and the necessary startup classes 
* FunctionInvocationFilter demo

### Compile and run locally
````
dotnet build
func host start
http -v -f POST localhost:7071/api/httpnordine hello=World
````

## 
Queue function: Add an item to the queue
Retreive the endpoint for the storage, it looks like
````
DefaultEndpointsProtocol=https;AccountName=xxx;AccountKey=xxxxxxxx;EndpointSuffix=core.windows.net
````

### References
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-app-settings#azurewebjobsstorage
* authentication is ignored when running locally

## Containerizing the function app
```
docker build --no-cache --tag nordineb/azfuncapp:v1.0.0 .
docker run --rm -it -p 8081:80   \
-e AZURE_FUNCTIONS_ENVIRONMENT="Development"   \
-e AzureWebJobsSecretStorageType="Files"   \
-e StorageConnectionAppSetting=""  \
nordineb/azfuncapp:v1.0.0  
```

## Deploying the container to Azure Functions Linux Premium

```
az login
az storage account create  \
--name techfridaystorage1 \
--location westeurope \
--resource-group AzureFunctionsContainers-rg \
--sku Standard_LRS storageConnectionString=$(az storage account show-connection-string --resource-group AzureFunctionsContainers-rg --name <storage_name> --query connectionString --output tsv)

az functionapp plan create  \
--resource-group AzureFunctionsContainers-rg  \
--name myPremiumPlan  \
--location westeurope  \
--number-of-workers 1  \
--sku EP1  \
--is-linux

az functionapp create --name techfridayfuncapp  \
--storage-account techfridaystorage1  \
--resource-group AzureFunctionsContainers-rg  \
--plan myPremiumPlan 
--deployment-container-image-name nordineb/azurefunctionsimage:v1.0.0

az functionapp config appsettings  \
set --name techfridayfuncapp  \
--resource-group AzureFunctionsContainers-rg  \
--settings AzureWebJobsStorage=$storageConnectionString

az functionapp create --name nordineazfuncapp  \
--functions-version 3  \
--storage-account storageaccounttechfb373  \
--resource-group techfriday  \
--plan techfriday-ep  \
--deployment-container-image-name nordineb/azfuncapp:v1.0.0
```


### Load testing with Apache Bench

```
ab -n 1000 https://nordineazfuncapp.azurewebsites.net/
```


## Deploying the container to Kubernetes 

### Installing Keda on GKE

Azure function core tools didn't managed to install Keda on the cluster automatically. I had to generate the YAML file first, and then fix a coupld of things.

```
# Prerequisites: Create a K8s cluster and use gcloud sdk to authenticate
kubectl create namespace techfriday
kubectl config set-context --current --namespace=techfriday

func kubernetes install --namespace techfriday --dry-run > /tmp/keda.yaml

# Edit the /tmp/keda.yaml to add --- between line 4833 and 4834
# Edit the /tmp/keda.yaml to change all references to kube-system to techfriday

kubectl apply -f /tmp/keda.yaml --namespace techfriday
```

### Deploy
```
func kubernetes deploy --name AzFuncApp --namespace techfriday --registry nordineb
```

### Testing

Add 1000 elements to Azure Storage Queue and observe how pods get dynamically created to handle the load


## References

* https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-function-linux-custom-image?tabs=bash%2Cportal&pivots=programming-language-csharp

* https://medium.com/@asavaritayal/azure-functions-on-kubernetes-75486225dac0

* https://developers.de/2019/08/13/hot-to-create-docker-image-from-azure-function/

* https://developers.de/2019/05/28/running-azure-functions-everywhere/

* https://github.com/Azure/azure-functions-host/issues/4147

* https://github.com/Azure/azure-functions-host/pull/4462





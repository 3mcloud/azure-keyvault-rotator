# Deploy the Rotator
az deployment group create --name StorageAccountRotatorFunction --resource-group project-breathe-3m --template-file function.json  --parameters keyVaultName=kv-projectbreathe-3m functionAppName=breathekeyrotator secretName=projectbreathe3mdata-sas

# deploy the function app code
func azure functionapp publish breathekeyrotator

# Deploy the Event listener for the secret you want to rotate
az deployment group create --name StorageAccountRotatorEventSubscription --resource-group project-breathe-3m --template-file events.json  --parameters keyVaultName=kv-projectbreathe-3m functionAppName=breathekeyrotator secretName=projectbreathe3mdata-sas



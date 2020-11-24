# Deploy the Rotator
az deployment group create --name RotatorFunction --resource-group <rg> --template-file function.json  --parameters keyVaultName=<keyVaultName> functionAppName=<functionAppName> secretName=<secretName>

# deploy the function app code
func azure functionapp publish <functionAppName>

# Deploy the Event listener for the secret you want to rotate
az deployment group create --name RotatorEventSubscription --resource-group <rg> --template-file events.json  --parameters keyVaultName=<keyVaultName> functionAppName=<functionAppName> secretName=<secretName>



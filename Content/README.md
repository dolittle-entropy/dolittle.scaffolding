# Instructions
In order to use these files correctly, ensure that you follow the steps in this readme fil. 

## Start by setting up the required configuration

### 1. Ensure that your project is referencing a `secrets.json` file
The broker is assuming that a specific set of configuration values is in place and accessible through [.Net Core App Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows). 
Managing secrets is beyond the scope of this document. If you do not yet have a secrets file, and need to add one quickly, you can do the following from the command line: 
```cmd
$ cd <folder where the project .csproj file is>
$ dotnet user-secrets init
```


### 2. Copy the `PEM` files to a secure location on your computer
Like the secrets file, the PEM files are digitally signed secrets that you should NOT keep in source control. 
Instead, copy them to a safe location on your disk, i.e. next to your `secrets.json` file so that they never run the risk of accidentally being committed to source control.

### 3. Create a Kafka section in your `secrets.json` file

#### **3.1 Copy the configuration section and paste it into your `secrets.json`**
Start by copying the configuration section named "Kafka" into your `secrets.json` file. 
These values have been pre-filled with values from the Dolittle Portal. 
Paste the `Kafka` configuration section as a child of the root json structure. 

```json
$(config)
```

> **Important** <br />
> Look over the configuration to ensure that things like the `GroupId` have no spaces (replace spaces with dashes)



#### **3.2 Replace the `<path>` under the `Ssl` section with your true path, i.e:**

```json
"Ssl": {
    "Authority" : "C:\\dev\\secrets\\mcdonalds\\dev\\ca.pem",
    "Certificate" : "C:\\dev\\secrets\\mcdonalds\\dev\\certificate.pem",
    "Key" : "C:\\dev\\secrets\\mcdonalds\\dev\\accessKey.pem"
}
```

### 4. (Optional) Copy the file `KafkaConfigurationBuilder.cs` into your project
Copy the file into your project, adjust its namespace and read through the comments to be able to quickly create kafka consumers and publishers.
The `KafkaConfigurationBuilder` is a static class that leverages the configuration section from the `secrets.json` directly, allowing you to 
quickly set up publishers and consumers.

----
End of README.md



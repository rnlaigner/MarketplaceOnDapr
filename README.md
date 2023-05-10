# MarketplaceDapr

## How to run

You can execute the microservices individually through the following template command:

```diff
dapr run --app-id <MICROSERVICE_NAME> --app-port <PORT> -- dotnet run [--project <CSPROJ_FILEPATH>]
```

Example:

```diff
dapr run --app-id cart --app-port 5001 -- dotnet run --project CartMS.csproj
```

However, for each application you have to execute a separate dapr run command.

However, Dapr offers in Linux/MacOS a "Multi-App Run" feature. Through a description file, information about all applications are processed and executed by the [Dapr runtime](https://docs.dapr.io/developing-applications/local-development/multi-app-dapr-run/multi-app-overview/).

In the root of this project here a dapr.yaml file, which describes a multi-app dapr execution. You can execute all applications through the following command (make sure iou are in the root of the project):

```diff
dapr run -f dapr.yaml
```

As a preview feature, it shows a performance gap: it takes some time for all listed applications  to get up running.

In contrast, there is a bash script that simply creates several terminals, one for each application.
# MarketplaceDapr



## How to setup the environment

Make sure you have Dapr CLI installed
```diff
https://github.com/dapr/cli
```

An observation: if you have arm64 architecture, you may need to install Dapr with the following command:

```diff
arch -arm64 brew install dapr/tap/dapr-cli
```

After installing Dapr cli, you can initiate it.

If Docker is available in your environment, just execute:
```diff
dapr init
```

Otherwise:
```diff
dapr init --slim
```

This version will not install default configuration files and set up the Dapr services.
In this case, make sure you have a Redis running in the default port to serve as a pubsub for the Dapr apps.
You can achieve this using Docker:
```diff
docker run -d --name redis -p 6379:6379 redis:latest
```

## How to run

You can execute the microservices individually through the following template command:

```diff
dapr run --app-id <MICROSERVICE_NAME> --app-port <PORT> -- dotnet run [--project <CSPROJ_FILEPATH>]
```

Example:

```diff
dapr run --app-id cart --app-port 5001 -- dotnet run --project CartMS.csproj
```

However, you have to execute a separate dapr run command for every application.

In this sense, Dapr offers in Linux/MacOS a "Multi-App Run" feature. Through a description file, information about all applications are processed and executed by the [Dapr runtime](https://docs.dapr.io/developing-applications/local-development/multi-app-dapr-run/multi-app-overview/).

In the root of this project here a dapr.yaml file, which describes a multi-app dapr execution. You can execute all applications through the following command (make sure iou are in the root of the project):

```diff
dapr run -f dapr.yaml
```

On the other hand, as a preview feature, it shows a performance gap: it takes some time for all listed applications to get up running.

Therefore, there is a bash script that simply creates several terminals, one for each application.

if you are in MacOs, run:

```diff
cmhmod 775 deploy_macos.sh
./deploy_macos.sh
```

## How to trigger transactions


## Useful links
https://www.ibm.com/garage/method/practices/manage/health-check-apis/
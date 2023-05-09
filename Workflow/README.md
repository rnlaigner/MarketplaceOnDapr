How to run:

dapr run --app-id workflow --app-port 5000 -- dotnet run --project Workflow/Workflow.csproj
 
To run app and workflow separated:

In Workflow folder:
dotnet run

In Workflow folder, but another prompt:
dapr run --enable-api-logging --app-id wfapp --dapr-grpc-port 4001 --dapr-http-port 3500

To start a workflow:
curl -i -X GET http://localhost:5000/test

https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-features-concepts/#workflow-activities

"The Dapr Workflow engine guarantees that each called activity is executed at least once as part of a workflow’s
execution. Because activities only guarantee at-least-once execution, it’s recommended that activity logic be
implemented as idempotent whenever possible."

About running several apps:
https://docs.dapr.io/developing-applications/local-development/multi-app-dapr-run/multi-app-overview/

Setting another name resolution:
https://github.com/dapr/components-contrib/pull/1380
https://forum.arduino.cc/t/allow-mdns-discovery-on-macos/1089144
https://docs.dapr.io/reference/components-reference/supported-state-stores/setup-consul/
https://docs.dapr.io/operations/configuration/configuration-overview/
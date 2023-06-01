The current workflow is alpha. A bug is affecting proper execution
https://github.com/dapr/dapr/issues/6373
It appears a fix has been provided but it not released yet
https://github.com/dapr/dapr/pull/6377
Which makes it challenging to use it in experiments.

==================

How to run:

dapr run --enable-api-logging --app-id workflow --app-port 5000 --dapr-http-port 3500 -- dotnet run --project Workflow/Workflow.csproj


To test whether a workflow starts:
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


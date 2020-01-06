#ToDo

  \/ create defect
* when creating relationships using content picker and picked content is not already in the graph, then the sync process fails silently. need to flag an error
* bulk publishing skips graph sync
* don't like new disabled select appearing once selected in single select scenario - nasty!
* need to add dependencies into manifest?
* use predefined list editor for node label?
* handle graph down/not running better (& quicker)
* looks like might be some useful code in the [client](https://github.com/Readify/Neo4jClient), e.g. working with results

#Templates

dotnet new -i OrchardCore.ProjectTemplates::1.0.0-rc1-* --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json

#Links

[Orchard Training Demo](https://github.com/Lombiq/Orchard-Training-Demo-Module/blob/orchard-core/StartLearningHere.md)

[Develop a custom widget](https://www.davidhayden.me/blog/develop-a-custom-widget-in-orchard-core-cms)

# Integration Testing

Unit tests will test our code, but without integration tests, we'd have no coverage checking that the graph database is left in the expected state after mutating it.

Unfortunately, whilst Neo4j supports embedding a database for testing when working in Java (Neo4j runs in a JVM), no such support exists for .Net. (The current support in Java is also [deprecated](https://neo4j.com/docs/java-reference/current/tutorials-java-embedded/unit-testing/index.html), with a new mechanism introduced in v4.0, which isn't finalised yet.) [Other mechanisms](https://neo4j.com/blog/dark-side-neo4j-worst-practices/) exist for integration testing, but again they are all Java based.

As there is great benefit in tests that check the database state, our integration tests will have to work against an externally hosted graph. We don't want to connect to and leave changed the graph we use for running the editor, so we need a mechanism to run our tests without mutating the main graph. Options include:

#### Connect to the main graph, run tests in transactions that get rolled back

[Example](https://neo4j.com/blog/integration-testing-neo4j-c-sharp/)

Cons of this approach:
* a bug in our tests' transaction handling could leave the graph mutated
* node ids are 'used up' (but later reused) and increase the size of the graph after each test run [citation](where'd it go?)

#### Use a separate graph

We can run our integration tests against a separate graph to the main graph.

Cons of this approach:
* Neo4j v3 only supports running a single graph on an instance at once. Therefore, we'd need DevOps to create and run a new instance for each environment for integration tests. Also, each developer would need to [set up and run multiple instances of Neo4j](https://stackoverflow.com/questions/32548590/multiple-standalone-neo4j-instances-on-a-single-machine) on their dev box.

Fortunately, v4 of Neo4j, which is currently in rc, supports multiple graphs per instance. This means we can have an integration testing graph, alongside and separate to the main graph. If graph creation and destruction is fast enough, we could create and destroy a graph wrapped around a test run, which would give maximum isolation between test runs and be the cleanest option.

 ## Conclusion

 We'll develop integration tests against an externally hosted graph and when v4 is released, we'll add support for provisioning a graph for each run. Until then, we won't run the tests as part of the build/release pipeline, and we'll leave it to the developer to manage the test graph (either running against the main graph, manually switching graphs in their instance, or running multiple instances).

V4 is currently at [rc1](https://neo4j.com/release-notes/neo4j-4-0-0-rc01/), with the final release due soon.

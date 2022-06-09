# Example of Integration Testing with ASP.NET Core including a database

This repo contains code that demonstrates one way to do integration testing with ASP.NET Core applications that include databases. 

The code is explained in my blog post ()[], so if you are curious, I suggest having a look at that!

## Background

Having a database often introduces some complexity when it comes to running integration tests, as the data in the database needs to be consistent. This is obviously something that might become complicated when you have tests that add or delete data. Not to mention that the data needs to contain all possible permutations needed to perform the required tests. Something that often leads to complications when new areas are tested, requiring changes to old tests as the data needs to be updated to include the data required for the new tests.

In this sample, the code uses a transaction around each test, allowing the database to be populated with required data before each test, and then rolled back to its original "empty" state when completed. This also allows you to validate the contents in the database as part of the test if needed. Without seeing data from other tests that are being run.

__Caveat:__ There might be edge cases where the transactions cause deadlocks. However, so far, this has not been observed while using this way of working.

## Source code

The source code consists of 2 applications, a Web API that is to be tested, and a test project using xUnit to run the tests.

The test project contains 3 different versions of tests. The first and most simple, is the [UsersControllerTests](./AspNetCoreTesting.Api.Tests/UsersControllerTests.cs) class. This class does all set up inside each test method. 

However, as this becomes tedious and annoying, there is a cleaner version called [UsersControllerTestsWithTestBase](./AspNetCoreTesting.Api.Tests/UsersControllerTestsWithTestBase.cs), which uses 2 base classes to perform the set-up. This allows each test method to be much cleaner, while still being able to perform everything it needs.

Finally, there one version called [UsersControllerTestsWithTestHelper](AspNetCoreTesting.Api.Tests/UsersControllerTestsWithTestHelper.cs) that uses a helper class to do the set-up. The helper class is using a fluid syntax that gives the test methods an easy way to set up and run the tests as needed. 

__Note:__ The [TestHelper](AspNetCoreTesting.Api.Tests/Infrastructure/TestHelper.cs) is a quick simple implementation that probably needs a bit more work. But it is there for you to see.

## Running the tests

The tests are dependent on a database (obviously). The easiest way to set this up is to run the following command, and start a Docker container

```bash
> docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=P@ssword123" \
   -p 14331:1433 --name sql_test --hostname sql_test \
   -d mcr.microsoft.com/mssql/server:2019-latest
```

This creates a new SQL Server Docker container that exposes the server on port 14331 instead of the default 1433. This way, it won't interfere with any existing SQL Server on your machine.

Before each test run, EF Core is used to ensure the database is created, and then the applications migrations are applied to the database. This is done in the [TestRunStart](AspNetCoreTesting.Api.Tests/TestRunStart.cs) class, which uses the Xunit.TestFramework attribute to get it to run during each test run.

## Feedback

I am always interested in feedback, so if you have any, feel free to reach out on Twitter. I'm available at [@ZeroKoll](https://twitter.com/ZeroKoll).

When it comes to this code, I'm curious about your preference when it comes to using a base class or a helper. I'm torn myself, so feedback would be very much appreciated!
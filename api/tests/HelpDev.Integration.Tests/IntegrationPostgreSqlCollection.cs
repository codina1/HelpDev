using HelpDev.Testing.PostgreSQL;
using Xunit;

namespace HelpDev.Integration.Tests;

[CollectionDefinition(PostgreSqlCollection.Name)]
public sealed class IntegrationPostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>;

﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NSaga;
using NSaga.SimpleInjector;
using SimpleInjector;
using Tests.Stubs;
using Xunit;
using System.Reflection;

namespace Tests.SimpleInjector
{
    public class SimpleInjectorNSagaIntegrationTests
    {
        [Fact]
        public void Basic_Registration_ValidContainer()
        {
            //Arrange
            var container = new Container();

            // Act
            container.RegisterNSagaComponents(Assembly.GetExecutingAssembly());

            // Assert
            container.Verify();
        }


        [Theory]
        [InlineData(typeof(ISagaMediator), typeof(SagaMediator))]
        [InlineData(typeof(ISagaRepository), typeof(InMemorySagaRepository))]
        [InlineData(typeof(ISagaFactory), typeof(SimpleInjectorSagaFactory))]
        [InlineData(typeof(IMessageSerialiser), typeof(JsonNetSerialiser))]
        [InlineData(typeof(ISaga<MySagaData>), typeof(MySaga))]
        [InlineData(typeof(InitiatedBy<MySagaInitiatingMessage>), typeof(MySaga))]
        [InlineData(typeof(ConsumerOf<MySagaConsumingMessage>), typeof(MySaga))]
        [InlineData(typeof(InitiatedBy<MySagaAdditionalInitialser>), typeof(MySaga))]
        public void DefaultRegistration_Resolves_DefaultComponents(Type requestedType, Type expectedImplementation)
        {
            //Arrange
            var container = new Container().RegisterNSagaComponents(Assembly.GetExecutingAssembly());

            // Act
            var result = container.GetInstance(requestedType);

            // Assert
            result.Should().NotBeNull()
                       .And.BeOfType(expectedImplementation);
        }


        [Fact]
        public void OverrideGeneric_Repository_Complies()
        {
            //Arrange
            var container = new Container().RegisterNSagaComponents(Assembly.GetExecutingAssembly())
                                           .UseSagaRepository<NullSagaRepository>();

            // Act
            var repository = container.GetInstance<ISagaRepository>();

            // Assert
            repository.Should().NotBeNull()
                .And.BeOfType<NullSagaRepository>();
        }


        [Fact]
        public void OverrideByType_Repository_Complies()
        {
            //Arrange
            var container = new Container()
                                .RegisterNSagaComponents(Assembly.GetExecutingAssembly())
                                .UseSqlServer()
                                .WithConnectionStringName("TestingConnectionString")
                                .UseSagaRepository<NullSagaRepository>();

            // Act
            var repository = container.GetInstance<ISagaRepository>();

            // Assert
            repository.Should().NotBeNull()
                .And.BeOfType<NullSagaRepository>();
        }

        [Fact]
        public void RegisterSqlServer_ByConnectionString_Works()
        {
            //Arrange
            var container = new Container()
                                .RegisterNSagaComponents(Assembly.GetExecutingAssembly())
                                .UseSqlServer()
                                .WithConnectionString(@"Server=(localdb)\v12.0;Database=NSaga-Testing");

            // Act
            var repository = container.GetInstance<ISagaRepository>();

            // Assert
            repository.Should().NotBeNull()
                .And.BeOfType<SqlSagaRepository>();
        }

        [Fact]
        public void Default_ResolvePiplineHooks_ResolvesMetadataHook()
        {
            //Arrange
            var container = new Container().RegisterNSagaComponents(Assembly.GetExecutingAssembly());

            // Act
            var collection = container.GetInstance<IEnumerable<IPipelineHook>>();

            // Assert
            collection.Should().NotBeNull()
                               .And.HaveCount(1)
                               .And.Contain(h => h is MetadataPipelineHook);
        }

        [Fact]
        public void AddPipline_Adds_ToCollection()
        {
            //Arrange
            var container = new Container().RegisterNSagaComponents(Assembly.GetExecutingAssembly())
                                           .AddSagaPipelineHook<NullPipelineHook>();

            // Act
            var collection = container.GetInstance<IEnumerable<IPipelineHook>>();

            // Assert
            collection.Should().NotBeNull()
                       .And.HaveCount(2)
                       .And.Contain(h => h is MetadataPipelineHook)
                       .And.Contain(h => h.GetType() == typeof(NullPipelineHook));
        }


        [Fact]
        public void UseMessageSerialiser_Overrides_Default()
        {
            //Arrange
            var container = new Container().RegisterNSagaComponents(Assembly.GetExecutingAssembly())
                                           .UseMessageSerialiser<NullMessageSerialiser>();

            // Act
            var result = container.GetInstance<IMessageSerialiser>();

            // Assert
            result.Should().NotBeNull().And.BeOfType<NullMessageSerialiser>();
        }
    }
}

﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Arcus.Messaging.Abstractions.MessageHandling;
using Arcus.Messaging.Abstractions.ServiceBus.MessageHandling;
using Arcus.Messaging.Pumps.ServiceBus;
using Arcus.Messaging.Tests.Unit.Fixture;
using Arcus.Security.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Arcus.Messaging.Tests.Unit.ServiceBus
{
    public class IServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddServiceBusTopicMessagePump_WithSubscriptionNameIndirectSecretProvider_WiresUpCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var spySecretProvider = new Mock<ISecretProvider>();
            services.AddSingleton(serviceProvider => spySecretProvider.Object);
            services.AddSingleton(serviceProvider => Mock.Of<IConfiguration>());
            services.AddLogging();

            // Act
            ServiceBusMessageHandlerCollection result = 
                services.AddServiceBusTopicMessagePump(
                    "topic name", "subscription name", "secret name", options => options.AutoComplete = true);
            
            // Assert
            Assert.NotNull(result);
            ServiceProvider provider = result.Services.BuildServiceProvider();

            var messagePump = provider.GetService<IHostedService>();
            Assert.IsType<AzureServiceBusMessagePump>(messagePump);

            await Assert.ThrowsAnyAsync<Exception>(() => messagePump.StartAsync(CancellationToken.None));
            spySecretProvider.Verify(spy => spy.GetRawSecretAsync("secret name"), Times.Once);
        }

        [Fact]
        public async Task AddServiceBusTopicMessagePump_WithTopicNameAndSubscriptionNameIndirectSecretProvider_WiresUpCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var spySecretProvider = new Mock<ISecretProvider>();
            services.AddSingleton(serviceProvider => spySecretProvider.Object);
            services.AddSingleton(serviceProvider => Mock.Of<IConfiguration>());
            services.AddLogging();

            // Act
            ServiceBusMessageHandlerCollection result = 
                services.AddServiceBusTopicMessagePump(
                    "topic name", "subscription name", "secret name", configureMessagePump: options => options.AutoComplete = true);

            // Assert
            // Assert
            Assert.NotNull(result);
            ServiceProvider provider = result.Services.BuildServiceProvider();

            var messagePump = provider.GetService<IHostedService>();
            Assert.IsType<AzureServiceBusMessagePump>(messagePump);

            await Assert.ThrowsAnyAsync<Exception>(() => messagePump.StartAsync(CancellationToken.None));
            spySecretProvider.Verify(spy => spy.GetRawSecretAsync("secret name"), Times.Once);
        }

        [Fact]
        public async Task AddServiceBusQueueMessagePump_IndirectSecretProviderWithQueueName_WiresUpCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var spySecretProvider = new Mock<ISecretProvider>();
            services.AddSingleton(serviceProvider => spySecretProvider.Object);
            services.AddSingleton(serviceProvider => Mock.Of<IConfiguration>());
            services.AddLogging();

            // Act
            ServiceBusMessageHandlerCollection result =
                services.AddServiceBusQueueMessagePump(
                    "queue name", "secret name", configureMessagePump: options => options.AutoComplete = true);

            // Assert
            Assert.NotNull(result);
            ServiceProvider provider = result.Services.BuildServiceProvider();

            var messagePump = provider.GetService<IHostedService>();
            Assert.IsType<AzureServiceBusMessagePump>(messagePump);

            try
            {
                await messagePump.StartAsync(CancellationToken.None);
            }
            finally
            {
                spySecretProvider.Verify(spy => spy.GetRawSecretAsync("secret name"), Times.Once);
            }
        }

        [Fact]
        public async Task AddServiceBusQueueMessagePump_IndirectSecretProviderWithoutQueueName_WiresUpCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var spySecretProvider = new Mock<ISecretProvider>();
            services.AddSingleton(serviceProvider => spySecretProvider.Object);
            services.AddSingleton(serviceProvider => Mock.Of<IConfiguration>());
            services.AddLogging();

            // Act
            ServiceBusMessageHandlerCollection result =
                services.AddServiceBusQueueMessagePump("secret name", configureMessagePump: options => options.AutoComplete = true);

            // Assert
            Assert.NotNull(result);
            ServiceProvider provider = result.Services.BuildServiceProvider();

            var messagePump = provider.GetService<IHostedService>();
            Assert.IsType<AzureServiceBusMessagePump>(messagePump);

            try
            {
                await messagePump.StartAsync(CancellationToken.None);
            }
            finally
            {
                spySecretProvider.Verify(spy => spy.GetRawSecretAsync("secret name"), Times.Once);
            }
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutContextFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(messageBodyFilter: null));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithBodyFilterWithoutContextFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    messageContextFilter: null,
                    messageBodyFilter: body => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutBodyFilterWithContextFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    messageBodyFilter: null,
                    messageContextFilter: context => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutImplementationFactory_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: null));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutImplementationFactoryWithContextFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: null,
                    messageContextFilter: context => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithImplementationFactoryWithoutContextFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: serviceProvider => new TestServiceBusMessageHandler(), 
                    messageContextFilter: null));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutImplementationFactoryWithBodyFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: null,
                    messageBodyFilter: body => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithImplementationFactoryWithoutBodyFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: serviceProvider => new TestServiceBusMessageHandler(), 
                    messageBodyFilter: null));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithoutImplementationFactoryWithContextFilterWithBodyFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: null,
                    messageContextFilter: context => true,
                    messageBodyFilter: body => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithImplementationFactoryWithoutContextFilterWithBodyFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: servivceProvider => new TestServiceBusMessageHandler(), 
                    messageContextFilter: null,
                    messageBodyFilter: body => true));
        }

        [Fact]
        public void WithServiceBusMessageHandler_WithImplementationFactoryWithContextFilterWithoutBodyFilter_Throws()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => collection.WithServiceBusMessageHandler<TestServiceBusMessageHandler, TestMessage>(
                    implementationFactory: serviceProvider => new TestServiceBusMessageHandler(), 
                    messageContextFilter: context => true,
                    messageBodyFilter: null));
        }

        [Fact]
        public void WithServiceBusFallbackMessageHandler_WithValidType_RegistersInterface()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act
            collection.WithServiceBusFallbackMessageHandler<PassThruServiceBusFallbackMessageHandler>();

            // Assert
            IServiceProvider provider = collection.Services.BuildServiceProvider();
            var messageHandler = provider.GetRequiredService<IAzureServiceBusFallbackMessageHandler>();

            Assert.IsType<PassThruServiceBusFallbackMessageHandler>(messageHandler);
        }

        [Fact]
        public void WithServiceBusFallbackMessageHandler_WithValidImplementationFunction_RegistersInterface()
        {
            // Arrange
            var collection = new ServiceBusMessageHandlerCollection(new ServiceCollection());
            var expected = new PassThruServiceBusFallbackMessageHandler();

            // Act
            collection.WithServiceBusFallbackMessageHandler(serviceProvider => expected);

            // Assert
            IServiceProvider provider = collection.Services.BuildServiceProvider();
            var actual = provider.GetRequiredService<IAzureServiceBusFallbackMessageHandler>();

            Assert.Same(expected, actual);
        }

        [Fact]
        public void WithServiceBusFallbackMessageHandlerType_WithoutServices_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(
                () => ((ServiceBusMessageHandlerCollection) null).WithServiceBusFallbackMessageHandler<PassThruServiceBusFallbackMessageHandler>());
        }

        [Fact]
        public void WithServiceBusFallbackMessageHandlerImplementationFunction_WithoutServices_Throws()
        {
            Assert.ThrowsAny<ArgumentException>(
                () => ((ServiceBusMessageHandlerCollection) null).WithServiceBusFallbackMessageHandler(serviceProvider => new PassThruServiceBusFallbackMessageHandler()));
        }

        [Fact]
        public void WithServiceBusFallbackMessageHandlerImplementationFunction_WithoutImplementationFunction_Throws()
        {
            // Arrange
            var services = new ServiceBusMessageHandlerCollection(new ServiceCollection());

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => services.WithServiceBusFallbackMessageHandler(createImplementation: (Func<IServiceProvider, PassThruServiceBusFallbackMessageHandler>)null));
        }
    }
}

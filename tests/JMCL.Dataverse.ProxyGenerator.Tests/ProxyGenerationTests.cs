using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using JMCL.Dataverse.Sdk.Metadata;

namespace JMCL.Dataverse.ProxyGenerator.Tests
{
    /// <summary>
    /// Runs hand-built metadata through the model builder and the shipped
    /// ProxyTemplate.t4, i.e. the whole generation pipeline minus the Dataverse
    /// connection, so a broken template, directive processor or Mono.TextTemplating
    /// upgrade fails here instead of on a developer's first proxybuilder run.
    /// </summary>
    [TestClass]
    public class ProxyGenerationTests
    {
        private string outputPath;

        [TestInitialize]
        public void Init()
        {
            outputPath = Path.Combine(Path.GetTempPath(), "jmcl-proxygen-tests", Guid.NewGuid().ToString("N"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }
        }

        [TestMethod]
        public void ShippedTemplate_GeneratesEntityProxy()
        {
            var settings = new Settings
            {
                Namespace = "Test.Proxies",
                TemplateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProxyTemplate.t4"),
                OutputPath = outputPath,
            };

            var model = new ProxyModelService(new TypeConverterFactory().Create(settings.TemplateLanguage))
                .BuildModel(new[] { BuildAccountMetadata() }, Enumerable.Empty<SdkMessageMetadata>());

            var errors = new List<string>();
            var generator = new ProxyGeneratorService(settings);
            generator.Message += (s, e) =>
            {
                if (e.MessageType == eMessageType.Error) errors.Add(e.Message);
            };

            generator.BuildProxies(model);

            Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors));

            var generated = string.Join(Environment.NewLine,
                Directory.GetFiles(outputPath, "*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));

            StringAssert.Contains(generated, "namespace Test.Proxies");
            StringAssert.Contains(generated, "public partial class Account : JMCL.Dataverse.Sdk.EarlyBound.EntityProxy");
            StringAssert.Contains(generated, "public static class Fields");
            StringAssert.Contains(generated, "\"primarycontactid\"");
            StringAssert.Contains(generated, "Accounting=1");
        }

        private static EntityMetadata BuildAccountMetadata()
        {
            var id = new UniqueIdentifierAttributeMetadata { LogicalName = "accountid", SchemaName = "AccountId" };
            SetInternal(id, nameof(AttributeMetadata.IsPrimaryId), (bool?)true);
            SetInternal(id, nameof(AttributeMetadata.AttributeType), (AttributeTypeCode?)AttributeTypeCode.Uniqueidentifier);

            var name = new StringAttributeMetadata { LogicalName = "name", SchemaName = "Name", MaxLength = 160 };
            SetInternal(name, nameof(AttributeMetadata.IsPrimaryName), (bool?)true);

            var contact = new LookupAttributeMetadata { LogicalName = "primarycontactid", SchemaName = "PrimaryContactId", Targets = new[] { "contact" } };

            var industry = new PicklistAttributeMetadata
            {
                LogicalName = "industrycode",
                SchemaName = "IndustryCode",
                OptionSet = new OptionSetMetadata(new OptionMetadataCollection
                {
                    Option("Accounting", 1),
                    Option("Agriculture", 2),
                })
                {
                    Name = "account_industrycode",
                    IsGlobal = false,
                },
            };

            var entity = new EntityMetadata { LogicalName = "account", SchemaName = "Account" };
            SetInternal(entity, nameof(EntityMetadata.PrimaryIdAttribute), "accountid");
            SetInternal(entity, nameof(EntityMetadata.PrimaryNameAttribute), "name");
            SetInternal(entity, nameof(EntityMetadata.Attributes), new AttributeMetadata[] { id, name, contact, industry });
            SetInternal(entity, nameof(EntityMetadata.OneToManyRelationships), new OneToManyRelationshipMetadata[0]);
            SetInternal(entity, nameof(EntityMetadata.ManyToOneRelationships), new OneToManyRelationshipMetadata[0]);
            SetInternal(entity, nameof(EntityMetadata.ManyToManyRelationships), new ManyToManyRelationshipMetadata[0]);
            return entity;
        }

        private static OptionMetadata Option(string label, int value)
        {
            var localized = new LocalizedLabel(label, 1033);
            return new OptionMetadata(new Label(localized, new[] { localized }), value);
        }

        // Metadata properties Dataverse populates on retrieval have non-public setters.
        private static void SetInternal(object target, string property, object value)
        {
            target.GetType().GetProperty(property).GetSetMethod(true).Invoke(target, new[] { value });
        }
    }
}

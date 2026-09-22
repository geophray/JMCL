using System.IO;


namespace JMCL.Dataverse.ProxyGenerator.Cmd.Extensions
{
    public static class SettingsExtensions
    {

        public static IConfigSettings SetTemplateRelativeTo(this IConfigSettings settings, string path)
        {
            if (Path.IsPathRooted(settings.TemplateFilePath))
                return settings;

            return new ConfigSettings
            {
                ActionsToExclude = settings.ActionsToExclude,
                ActionsToInclude = settings.ActionsToInclude,
                EntitiesToExclude = settings.EntitiesToExclude,
                EntitiesToInclude = settings.EntitiesToInclude,
                TargetEndPoint = settings.TargetEndPoint,
                TemplateFilePath = Path.Combine(path, settings.TemplateFilePath),
                TemplateLanguage = settings.TemplateLanguage,
                Namespace = settings.Namespace,
                OutputPath = settings.OutputPath,
                ConfigurationPath = settings.ConfigurationPath
            };
        }

        public static IConfigSettings SetOutputRelativeTo(this IConfigSettings settings, string path)
        {
            if (Path.IsPathRooted(settings.OutputPath))
                return settings;

            return new ConfigSettings
            {
                ActionsToExclude = settings.ActionsToExclude,
                ActionsToInclude = settings.ActionsToInclude,
                EntitiesToExclude = settings.EntitiesToExclude,
                EntitiesToInclude = settings.EntitiesToInclude,
                TargetEndPoint = settings.TargetEndPoint,
                TemplateFilePath = settings.TemplateFilePath,
                TemplateLanguage = settings.TemplateLanguage,
                Namespace = settings.Namespace,
                OutputPath = Path.Combine(path, settings.OutputPath),
                ConfigurationPath = settings.ConfigurationPath
            };
        }
    }
}

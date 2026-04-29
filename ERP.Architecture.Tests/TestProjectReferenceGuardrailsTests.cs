using System.Xml.Linq;
using Xunit;

namespace ERP.Architecture.Tests;

public sealed class TestProjectReferenceGuardrailsTests
{
    [Fact]
    public void BlackBoxTests_DoNotReferenceModules()
    {
        var projects = ProjectCatalog.Load();
        var blackBox = projects.GetProject("ERP.Api.BlackBox.Tests");
        var moduleReferences = blackBox.ProjectReferences
            .Where(reference => reference.StartsWith("ERP.Modules.", StringComparison.Ordinal))
            .ToList();

        Assert.True(moduleReferences.Count == 0,
            $"ERP.Api.BlackBox.Tests should not reference modules directly. Found: {string.Join(", ", moduleReferences)}");
    }

    [Fact]
    public void WhiteBoxTests_OnlyReferenceTheirModuleAndShared()
    {
        var projects = ProjectCatalog.Load();
        var whiteBoxProjects = projects.All
            .Where(project => project.Name.StartsWith("ERP.Modules.", StringComparison.Ordinal)
                && project.Name.EndsWith(".WhiteBox.Tests", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(whiteBoxProjects);

        foreach (var project in whiteBoxProjects)
        {
            var moduleName = project.Name
                .Substring("ERP.Modules.".Length)
                .Replace(".WhiteBox.Tests", string.Empty, StringComparison.Ordinal);

            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                $"ERP.Modules.{moduleName}",
                $"ERP.Modules.{moduleName}.Contracts",
                "ERP.Shared"
            };

            var invalidReferences = project.ProjectReferences
                .Where(reference => !allowed.Contains(reference))
                .ToList();

            Assert.True(invalidReferences.Count == 0,
                $"{project.Name} has invalid references: {string.Join(", ", invalidReferences)}");
        }
    }

    [Fact]
    public void Tests_DoNotReferenceMultipleModules()
    {
        var projects = ProjectCatalog.Load();
        var testProjects = projects.All
            .Where(project => project.Name.EndsWith(".Tests", StringComparison.Ordinal))
            .ToList();

        foreach (var project in testProjects)
        {
            var moduleNames = project.ProjectReferences
                .Select(ProjectCatalog.GetModuleName)
                .Where(name => name is not null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Assert.True(moduleNames.Count <= 1,
                $"{project.Name} references multiple modules: {string.Join(", ", moduleNames)}");
        }
    }

    private sealed record ProjectInfo(string Name, string Path, IReadOnlyList<string> ProjectReferences);

    private sealed class ProjectCatalog
    {
        private ProjectCatalog(IReadOnlyList<ProjectInfo> projects)
        {
            All = projects;
        }

        public IReadOnlyList<ProjectInfo> All { get; }

        public static ProjectCatalog Load()
        {
            var root = FindSolutionRoot();
            var projectFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            var projects = projectFiles
                .Select(CreateProjectInfo)
                .ToList();

            return new ProjectCatalog(projects);
        }

        public ProjectInfo GetProject(string name)
        {
            var project = All.SingleOrDefault(item => item.Name == name);
            if (project is null)
            {
                throw new InvalidOperationException($"Project '{name}' not found. Ensure it is included in the repository.");
            }

            return project;
        }

        public static string? GetModuleName(string projectName)
        {
            if (!projectName.StartsWith("ERP.Modules.", StringComparison.Ordinal))
            {
                return null;
            }

            var moduleName = projectName.Substring("ERP.Modules.".Length);
            if (moduleName.EndsWith(".Contracts", StringComparison.Ordinal))
            {
                moduleName = moduleName[..^".Contracts".Length];
            }

            return moduleName;
        }

        private static ProjectInfo CreateProjectInfo(string path)
        {
            var document = XDocument.Load(path);
            var references = document
                .Descendants()
                .Where(element => element.Name.LocalName == "ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => Path.GetFileNameWithoutExtension(value!))
                .ToList();

            var name = Path.GetFileNameWithoutExtension(path);
            return new ProjectInfo(name, path, references);
        }

        private static string FindSolutionRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ERP.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new InvalidOperationException("Could not locate ERP.sln from the test output directory.");
        }
    }
}

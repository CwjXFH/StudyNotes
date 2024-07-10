## 基于Sonar和.editorconfig的代码lint

在项目根目录下创建[Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022)文件，在里面引入[SonarAnalyzer.CSharp](https://www.nuget.org/packages/SonarAnalyzer.CSharp)包：

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.28.0.94264">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

在项目根目录下创建[.editorconfig](https://learn.microsoft.com/en-us/visualstudio/ide/create-portable-custom-editor-options?view=vs-2022)，里面定义好代码规范，然后执行`dotnet build`即可。

Sonar中有些[默认规则](https://rules.sonarsource.com/csharp/)可能不符合组内规范，可以在**.editorconfig**中就行自定义配置。这两个工具结合起来就可以在本地执行lint也可以集成到CI流程中。



## 推荐阅读

[enable code-style analysis on build](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8#enable-on-build)  

[NET code style rule options](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options)

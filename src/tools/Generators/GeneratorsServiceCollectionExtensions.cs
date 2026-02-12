using Microsoft.Extensions.DependencyInjection;

namespace Zss.BilliardHall.Generators;

/// <summary>
/// ServiceCollection 扩展，用于注册 Generators 相关服务
/// </summary>
public static class GeneratorsServiceCollectionExtensions
{
    /// <summary>
    /// 注册所有 Generators 服务到 DI 容器
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合，支持链式调用</returns>
    public static IServiceCollection AddGenerators(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IAdrDecisionGenerator, AdrDecisionGenerator>();
        services.AddSingleton<IAgentInstructionGenerator, AgentInstructionGenerator>();
        services.AddSingleton<IAdrDocumentMerger, AdrDocumentMerger>();
        services.AddSingleton<IArchitectureTestGenerator, ArchitectureTestGenerator>();
        
        return services;
    }

    /// <summary>
    /// 注册 ADR Decision Generator
    /// </summary>
    public static IServiceCollection AddAdrDecisionGenerator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IAdrDecisionGenerator, AdrDecisionGenerator>();
        return services;
    }

    /// <summary>
    /// 注册 Agent Instruction Generator
    /// </summary>
    public static IServiceCollection AddAgentInstructionGenerator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IAgentInstructionGenerator, AgentInstructionGenerator>();
        return services;
    }

    /// <summary>
    /// 注册 ADR Document Merger
    /// </summary>
    public static IServiceCollection AddAdrDocumentMerger(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IAdrDecisionGenerator, AdrDecisionGenerator>();
        services.AddSingleton<IAdrDocumentMerger, AdrDocumentMerger>();
        return services;
    }

    /// <summary>
    /// 注册 Architecture Test Generator
    /// </summary>
    public static IServiceCollection AddArchitectureTestGenerator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IArchitectureTestGenerator, ArchitectureTestGenerator>();
        return services;
    }
}

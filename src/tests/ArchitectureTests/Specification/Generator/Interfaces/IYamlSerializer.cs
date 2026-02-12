namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generator.Interfaces;

/// <summary>
/// YAML 序列化器接口
/// 负责将对象序列化为 YAML 格式字符串
/// </summary>
public interface IYamlSerializer
{
    /// <summary>
    /// 序列化对象为 YAML 字符串
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>YAML 格式字符串</returns>
    string Serialize<T>(T obj) where T : class;
}

namespace Zss.BilliardHall.BuildingBlocks.Contracts;

/// <summary>
/// 手机号脱敏工具类
/// Phone number masking utility for logging
/// </summary>
public static class PhoneMasker
{
    /// <summary>
    /// 对手机号进行脱敏，保留后4位，前面用星号替换
    /// Mask phone number, keeping last 4 digits, replacing the rest with asterisks
    /// </summary>
    /// <param name="phone">原始手机号 / Original phone number</param>
    /// <returns>脱敏后的手机号（如：*******8000）/ Masked phone number (e.g., *******8000)</returns>
    public static string Mask(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        // 如果手机号长度小于等于4位，全部用星号替换
        // If phone number is 4 digits or less, replace all with asterisks
        if (phone.Length <= 4)
            return new string('*', phone.Length);

        // 保留后4位，前面用星号替换
        // Keep last 4 digits, replace the rest with asterisks
        return new string('*', phone.Length - 4) + phone.Substring(phone.Length - 4);
    }

    /// <summary>
    /// 对手机号进行脱敏，自定义保留的后缀长度
    /// Mask phone number with custom suffix length
    /// </summary>
    /// <param name="phone">原始手机号 / Original phone number</param>
    /// <param name="keepLastDigits">保留的后缀长度 / Number of digits to keep at the end</param>
    /// <returns>脱敏后的手机号 / Masked phone number</returns>
    public static string Mask(string phone, int keepLastDigits)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        if (keepLastDigits <= 0)
            return new string('*', phone.Length);

        if (phone.Length <= keepLastDigits)
            return new string('*', phone.Length);

        return new string('*', phone.Length - keepLastDigits) + phone.Substring(phone.Length - keepLastDigits);
    }
}

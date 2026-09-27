namespace Scada.ModBus;

/// <summary>
/// 面向 Scada 命名体系的 ModbusTcp 客户端兼容层。
/// </summary>
/// <remarks>
/// 当前通过继承方式桥接到 Byoe 实现，避免一次性重命名导致的大范围破坏。
/// </remarks>
public class ModbusTcpNet : Byoe.ModBus.ModbusTcpNet
{
    public ModbusTcpNet()
    {
    }

    public ModbusTcpNet(string ipAddress, int port = 502, byte station = 1)
        : base(ipAddress, port, station)
    {
    }
}

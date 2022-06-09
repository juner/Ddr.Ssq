using System;
using System.Runtime.Serialization;

namespace Ddr.Ssq;

/// <summary>
/// Normal Step Types 
/// </summary>
[Flags]
public enum StepType : byte
{
    /// <summary>
    /// Player1 Left Step ←
    /// </summary>
    [StepType(StepPlayers.Player1, StepArrows.Left)]
    Player1Left = 0b_0000_0001,
    /// <summary>
    /// Player1 Down Step ↓
    /// </summary>
    [StepType(StepPlayers.Player1, StepArrows.Down)]
    Player1Down = 0b_0000_0010,
    /// <summary>
    /// Player1 Up Step ↑
    /// </summary>
    [StepType(StepPlayers.Player1, StepArrows.Up)]
    Player1Up = 0b_0000_0100,
    /// <summary>
    /// Player1 Right Step →
    /// </summary>
    [StepType(StepPlayers.Player1, StepArrows.Right)]
    Player1Right = 0b_0000_1000,
    /// <summary>
    /// Player1 Special Step
    /// </summary>
    Player1Special = 0b_0000_1111,
    /// <summary>
    /// Player2 Left Step ←
    /// </summary>
    [StepType(StepPlayers.Player2, StepArrows.Left)]
    Player2Left = 0b_0001_0000,
    /// <summary>
    /// Player2 Down Step ↓
    /// </summary>
    [StepType(StepPlayers.Player2, StepArrows.Down)]
    Player2Down = 0b_0010_0000,
    /// <summary>
    /// Player2 Up Step ↑
    /// </summary>
    [StepType(StepPlayers.Player2, StepArrows.Up)]
    Player2Up = 0b_0100_0000,
    /// <summary>
    /// Player2 Right Step 
    /// </summary>
    [StepType(StepPlayers.Player2, StepArrows.Right)]
    Player2Right = 0b_1000_0000,
    /// <summary>
    /// Player2 Special Step
    /// </summary>
    Player2Special = 0b_1111_0000,

}
/// <summary>
/// Solo Player Step Type
/// </summary>
public enum SoloStepType : byte
{
    /// <summary>
    /// Solo Player Left Step ←
    /// </summary>
    SoloPlayerLeft = StepType.Player1Left,
    /// <summary>
    /// Solo Player Down Step ↓
    /// </summary>
    SoloPlayerDown = StepType.Player1Down,
    /// <summary>
    /// Solo Player Up Step ↑
    /// </summary>
    SoloPlayerUp = StepType.Player1Up,
    /// <summary>
    /// Solo Player Right Step →
    /// </summary>
    SoloPlayerRight = StepType.Player1Right,
    /// <summary>
    /// Solo Player North West Step ↖
    /// </summary>
    SoloPlayerNorthWest = StepType.Player2Left,
    /// <summary>
    /// Solo Player North East Step ↗
    /// </summary>
    SoloPlayerNorthEast = StepType.Player2Up,
}
/// <summary>
/// Step Type Extensions
/// </summary>
public static class StepTypeExtensions
{
    /// <summary>
    /// <see cref="StepType"/> -&gt; <see cref="StepPlayers"/> and <see cref="StepArrows"/>
    /// </summary>
    /// <param name="StepType"></param>
    /// <param name="StepPlayer"></param>
    /// <param name="StepArrow"></param>
#if false
        public static void Deconstruct(this StepType StepType, out StepPlayers StepPlayer, out StepArrows StepArrow)
        {
            var Attr = StepType.GetAttribute<StepTypeAttribute>(ThrowNotFoundFiled: false);
            if (Attr is { })
                Attr.Deconstruct(out StepPlayer, out StepArrow);
            else
                (StepPlayer, StepArrow) = (default, default);
#else
    public static void Deconstruct(this StepType StepType, out StepPlayers StepPlayer, out StepArrows StepArrow)
    {
        var _StepType = (byte)StepType;
        StepPlayer = default;
        if ((_StepType & (byte)StepPlayers.Player1) > 0)
            StepPlayer |= StepPlayers.Player1;
        if ((_StepType & (byte)StepPlayers.Player2) > 0)
            StepPlayer |= StepPlayers.Player2;
        StepArrow = default;
        if ((_StepType & (byte)StepArrows.Left) > 0)
            StepArrow |= StepArrows.Left;
        if ((_StepType & (byte)StepArrows.Down) > 0)
            StepArrow |= StepArrows.Down;
        if ((_StepType & (byte)StepArrows.Up) > 0)
            StepArrow |= StepArrows.Up;
        if ((_StepType & (byte)StepArrows.Right) > 0)
            StepArrow |= StepArrows.Right;
#endif
    }
    /// <summary>
    /// <see cref="StepTypeAttribute"/> to <see cref="StepPlayers"/> and <see cref="StepArrows"/>
    /// </summary>
    /// <param name="StepType"></param>
    /// <param name="StepPlayer"></param>
    /// <param name="StepArrow"></param>
    public static void Deconstruct(this StepTypeAttribute StepType, out StepPlayers StepPlayer, out StepArrows StepArrow)
        => (StepPlayer, StepArrow) = (StepType.StepPlayer, StepType.StepArrow);

}
/// <summary>
/// Step Arrows Mask
/// </summary>
public enum StepArrows : byte
{
    /// <summary>
    /// Left Mask
    /// </summary>
    [EnumMember(Value = "←")]
    Left = 0b_0001_0001,
    /// <summary>
    /// Down Mask
    /// </summary>
    [EnumMember(Value = "↓")]
    Down = 0b_0010_0010,
    /// <summary>
    /// Up Mask
    /// </summary>
    [EnumMember(Value = "↑")]
    Up = 0b_0100_0100,
    /// <summary>
    /// /Right Mask
    /// </summary>
    [EnumMember(Value = "→")]
    Right = 0b_1000_1000,
}
/// <summary>
/// Step Players Mask
/// </summary>
public enum StepPlayers : byte
{
    /// <summary>
    /// Player1 Mask
    /// </summary>
    Player1 = 0b_0000_1111,
    /// <summary>
    /// Player2 Mask
    /// </summary>
    Player2 = 0b_1111_0000,
}
/// <summary>
/// Step Type Attribute
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class StepTypeAttribute : Attribute
{
    /// <summary>
    /// Step Player Type
    /// </summary>
    public readonly StepPlayers StepPlayer;
    /// <summary>
    /// Step Arrow Type
    /// </summary>
    public readonly StepArrows StepArrow;
    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="StepPlayer"></param>
    /// <param name="StepArrow"></param>
    public StepTypeAttribute(StepPlayers StepPlayer, StepArrows StepArrow)
        => (this.StepArrow, this.StepPlayer) = (StepArrow, StepPlayer);
}

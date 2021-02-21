using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Ddr.Ssq.Printing
{
    public sealed class OutputOptions : ICloneable
    {
        /// <summary>
        /// <see cref="IOtherDataBody"/> を表示する
        /// </summary>
        public bool ViewOtherBinary { get; set; } = false;
        /// <summary>
        /// ステップのマッピング設定
        /// </summary>
        public CharMappingType MappingType { get; set; }
        /// <summary>
        /// ←
        /// </summary>
        public string LeftArrow { get; set; } = string.Empty;
        /// <summary>
        /// →
        /// </summary>
        public string RightArrow { get; set; } = string.Empty;
        /// <summary>
        /// ↑
        /// </summary>
        public string UpArrow { get; set; } = string.Empty;
        /// <summary>
        /// ↓
        /// </summary>
        public string DownArrow { get; set; } = string.Empty;
        /// <summary>
        /// ↗
        /// </summary>
        public string NorthEastArrow { get; set; } = string.Empty;
        /// <summary>
        /// ↖
        /// </summary>
        public string NorthWestArrow { get; set; } = string.Empty;
        /// <summary>
        /// ◆
        /// </summary>
        public string ShockArrow { get; set; } = string.Empty;
        /// <summary>
        /// ＃
        /// </summary>
        public string FreezeArrowSign { get; set; } = string.Empty;
        /// <summary>
        /// 長
        /// </summary>
        public string FreezeArrowWord { get; set; } = string.Empty;
        /// <summary>
        /// L
        /// </summary>
        public string FreezeArrowCharactor { get; set; } = string.Empty;
        /// <summary>
        /// 衝
        /// </summary>
        public string ShockArrowWord { get; set; } = string.Empty;
        /// <summary>
        /// …
        /// </summary>
        public string EmptyArrow { get; set; } = string.Empty;
        /// <summary>
        /// "　"
        /// </summary>
        internal string NoArrow { get; set; } = string.Empty;

        static readonly Dictionary<Encoding, CharMappingType> MappingTypes = new();
        public OutputOptions AutoMapping(Encoding Encoding)
        {
            var Result = Clone();
            if (MappingType is CharMappingType.Auto)
            {
                if (!MappingTypes.TryGetValue(Encoding, out var Type))
                {
                    if (Encoding.GetString(Encoding.GetBytes("↗")) == "↗")
                    {
                        Type = CharMappingType.Unicode;
                    }
                    else if (Encoding.GetString(Encoding.GetBytes("／")) == "／")
                    {
                        Type = CharMappingType.ANSI;
                    }
                    else
                    {
                        Type = CharMappingType.ASCII;
                    }
                    MappingTypes.TryAdd(Encoding, Type);
                }
                Result.MappingType = Type;
            }
            Debug.Assert(Result.MappingType is not CharMappingType.Auto);
            switch (Result.MappingType)
            {
                default:
                    break;
                case CharMappingType.Unicode:
                    Result.LeftArrow = "←";
                    Result.RightArrow = "→";
                    Result.UpArrow = "↑";
                    Result.DownArrow = "↓";
                    Result.NorthEastArrow = "↗";
                    Result.NorthWestArrow = "↖";
                    Result.ShockArrow = "◆";
                    Result.FreezeArrowSign = "＃";
                    Result.FreezeArrowWord = "長";
                    Result.FreezeArrowCharactor = "L";
                    Result.ShockArrowWord = "衝";
                    Result.EmptyArrow = "…";
                    Result.NoArrow = "　";
                    break;
                case CharMappingType.ANSI:
                    Result.LeftArrow = "←";
                    Result.RightArrow = "→";
                    Result.UpArrow = "↑";
                    Result.DownArrow = "↓";
                    Result.NorthEastArrow = "／";
                    Result.NorthWestArrow = "＼";
                    Result.ShockArrow = "◆";
                    Result.FreezeArrowSign = "＃";
                    Result.FreezeArrowWord = "長";
                    Result.FreezeArrowCharactor = "L";
                    Result.ShockArrowWord = "衝";
                    Result.EmptyArrow = "…";
                    Result.NoArrow = "　";
                    break;
                case CharMappingType.ASCII:
                    Result.LeftArrow = "<";
                    Result.RightArrow = ">";
                    Result.UpArrow = "^";
                    Result.DownArrow = "_";
                    Result.NorthEastArrow = "/";
                    Result.NorthWestArrow = "\\";
                    Result.ShockArrow = "*";
                    Result.FreezeArrowSign = "#";
                    Result.FreezeArrowWord = "F";
                    Result.FreezeArrowCharactor = "L";
                    Result.ShockArrowWord = "S";
                    Result.EmptyArrow = ".";
                    Result.NoArrow = " ";
                    break;
            }
            return Result;
        }
        public OutputOptions Clone()
        {
            return (OutputOptions)MemberwiseClone();
        }
        object ICloneable.Clone() => Clone();
    }
}

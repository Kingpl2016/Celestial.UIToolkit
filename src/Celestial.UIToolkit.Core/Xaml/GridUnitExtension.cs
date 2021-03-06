﻿using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace Celestial.UIToolkit.Xaml
{

    /// <summary>
    /// A <see cref="MarkupExtension"/> which is being used by the toolkit to align
    /// visual elements in a grid-like fashion.
    /// The idea is that each control uses margins, paddings, sizes, etc. which can be fit
    /// into grid cells. Thus, when positioned correctly, all controls will have a unified appearance.
    /// 
    /// This extension provides unified values, multiplied with the bounds of a single grid cell.
    /// See the example for details.
    /// </summary>
    /// <example>
    /// 1gu = 1 grid unit(s) -> Multiplier = 1
    /// 2gu = 2 grid unit(s) -> Multiplier = 2
    /// 
    /// Thus, for a <see cref="GridCellSize"/> of 4:
    /// 
    /// 1gu = 4 * 1 = 4
    /// 2gu = 4 * 2 = 8
    /// 3gu = 4 * 3 = 12
    /// ...
    /// 
    /// 
    /// In XAML, the extension can be used like this:
    /// Width="{c:GridUnit 1.0}"
    /// Margin="{c:GridUnit 3}"
    /// Margin="{c:GridUnit '3,0'}"
    /// Margin="{c:GridUnit '3,0,2,0'}"
    /// </example>
    [ContentProperty(nameof(MultiplierString))]
    public class GridUnitExtension : MarkupExtension
    {

        private static readonly ThicknessConverter _thicknessConverter;
        private static readonly CornerRadiusConverter _cornerRadiusConverter;
        private static readonly SizeConverter _sizeConverter;
        private static readonly PointConverter _pointConverter;
        private static readonly RectConverter _rectConverter;
        private static readonly GridLengthConverter _gridLengthConverter;
        private static readonly double _dipMultiplier;
        private string _multiplierString;
        private string _formattedMultiplierString;
        private double? _gridCellSize;

        /// <summary>
        /// Gets or sets the default value of a single grid unit.
        /// This value equals the width and height of a single cell
        /// in the fictional grid.
        /// </summary>
        public static double DefaultGridCellSize { get; set; } = 4d;

        /// <summary>
        /// Gets or sets the value with which the <see cref="GridCellSize"/>
        /// will be multiplied, as string.
        /// Depending on the target property type, different formats will be allowed.
        /// 
        /// For numeric targets (<see cref="double"/>, <see cref="int"/>, ...), 
        /// a numeric string is valid, e.g. <c>"0"</c>, <c>"1.3"</c>, ...
        /// 
        /// For 4-way targets like <see cref="Thickness"/>, the above is allowed, in addition to the following example formats:
        /// <c>"1 2"</c>, <c>"4.4 1 2 0"</c>, ...
        /// </summary>
        [ConstructorArgument("multiplierString")]
        public string MultiplierString
        {
            get { return _multiplierString; }
            set
            {
                _multiplierString = value;
                FormatMultiplierString();
            }
        }
        
        /// <summary>
        /// Gets or sets a type to which the unit will be converted,
        /// if possible.
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the
        /// calculation of the final value should include a multiplication
        /// with the device independent pixel multiplier.
        /// This is <c>false</c>, by default, since most conversion targets (like <see cref="Thickness"/>)
        /// do the calculation by themselves.
        /// </summary>
        public bool MultiplyWithDip { get; set; }

        /// <summary>
        /// Gets the value of a single grid unit.
        /// This value equals the width and height of a single cell
        /// in the fictional grid.
        /// By default, this returns the value of the static <see cref="DefaultGridCellSize"/> property.
        /// By setting this property, you can override this default value for specific cases.
        /// </summary>
        public double GridCellSize
        {
            get { return _gridCellSize ?? DefaultGridCellSize; }
            set { _gridCellSize = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUnitExtension"/> class.
        /// </summary>
        public GridUnitExtension()
            : this(null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUnitExtension"/> class
        /// with the specified <paramref name="multiplierString"/>.
        /// </summary>
        /// <param name="multiplierString">
        /// The value with which the <see cref="GridCellSize"/>
        /// will be multiplied.
        /// </param>
        public GridUnitExtension(string multiplierString)
            : this(multiplierString, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUnitExtension"/> class
        /// with the specified <paramref name="targetType"/>.
        /// </summary>
        /// <param name="targetType">
        /// A type to which the unit will be converted, if possible.
        /// </param>        
        public GridUnitExtension(Type targetType)
            : this(null, targetType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridUnitExtension"/> class
        /// with the specified <paramref name="multiplierString"/> and 
        /// <paramref name="targetType"/>.
        /// </summary>
        /// <param name="multiplierString">
        /// The value with which the <see cref="GridCellSize"/>
        /// will be multiplied.
        /// </param>
        /// <param name="targetType">
        /// A type to which the unit will be converted, if possible.
        /// </param>
        public GridUnitExtension(string multiplierString, Type targetType)
        {
            TargetType = targetType;
            MultiplierString = multiplierString;
            MultiplyWithDip = false;
        }

        static GridUnitExtension()
        {
            _dipMultiplier = DipHelper.GetDipMultiplier();

            _thicknessConverter = new ThicknessConverter();
            _cornerRadiusConverter = new CornerRadiusConverter();
            _sizeConverter = new SizeConverter();
            _pointConverter = new PointConverter();
            _rectConverter = new RectConverter();
            _gridLengthConverter = new GridLengthConverter();
        }

        private void FormatMultiplierString()
        {
            _formattedMultiplierString = _multiplierString?.Replace(',', ' ')
                                                          ?.Replace(';', ' ');
        }

        /// <summary>
        /// Multiplies the <see cref="MultiplierString"/> with the <see cref="GridCellSize"/>,
        /// converts it to device independent pixels and finally returns that result.
        /// </summary>
        /// <param name="serviceProvider">
        /// An <see cref="IServiceProvider"/> to be used.
        /// Can be null.
        /// </param>
        /// <returns>
        /// The <see cref="double"/> which is the result of the grid unit multiplication.
        /// </returns>
        /// <exception cref="NotSupportedException" />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(_multiplierString))
                throw new InvalidOperationException(
                    $"The {MultiplierString} was null or empty. The {nameof(GridUnitExtension)} " +
                    $"is unable to convert such a string.");

            var provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            Type targetType = DetermineConversionTargetType(provideValueTarget);
            return CalculateValue(targetType);
        }

        private object CalculateValue(Type targetType)
        {
            if (targetType == typeof(double))
            {
                return CalculateDouble();
            }
            else if (typeof(IConvertible).IsAssignableFrom(targetType))
            {
                return Convert.ChangeType(CalculateDouble(), targetType);
            }
            else if (targetType == typeof(Thickness))
            {
                return CalculateThickness();
            }
            else if (targetType == typeof(CornerRadius))
            {
                return CalculateCornerRadius();
            }
            else if (targetType == typeof(Size))
            {
                return CalculateSize();
            }
            else if (targetType == typeof(Point))
            {
                return CalculatePoint();
            }
            else if (targetType == typeof(Rect))
            {
                return CalculateRect();
            }
            else if (targetType == typeof(GridLength))
            {
                return CalculateGridLength();
            }
            else
            {
                throw new NotSupportedException(
                    $"The {nameof(GridUnitExtension)} does not support the " +
                    $"type '{targetType.FullName}'.");
            }
        }

        private Type DetermineConversionTargetType(IProvideValueTarget target)
        {
            if (TargetType != null) return TargetType;
            if (target == null) return typeof(double);
            
            return (target.TargetObject is Setter setter ? setter.Property?.PropertyType : null) ??
                   (target.TargetProperty as PropertyInfo)?.PropertyType ??
                   (target.TargetProperty as DependencyProperty)?.PropertyType ??
                   typeof(double);
        }

        private double GetFinalLengthMultiplier() =>
            (MultiplyWithDip ? _dipMultiplier : 1) * GridCellSize;
        
        private double CalculateDouble() => 
            GetFinalLengthMultiplier() * 
            Convert.ToDouble(MultiplierString, CultureInfo.InvariantCulture);

        private Thickness CalculateThickness()
        {
            var thickness = (Thickness)_thicknessConverter
                .ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();
            return new Thickness(
                thickness.Left * multiplier,
                thickness.Top * multiplier,
                thickness.Right * multiplier,
                thickness.Bottom * multiplier);
        }

        private CornerRadius CalculateCornerRadius()
        {
            var cornerRadius = (CornerRadius)_cornerRadiusConverter
                .ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();
            return new CornerRadius(
                cornerRadius.TopLeft * multiplier,
                cornerRadius.TopRight * multiplier,
                cornerRadius.BottomRight * multiplier,
                cornerRadius.BottomLeft * multiplier);
        }

        private Size CalculateSize()
        {
            var size = (Size)_sizeConverter
                .ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();
            return new Size(size.Width * multiplier, size.Height * multiplier);
        }

        private Point CalculatePoint()
        {
            var point = (Point)_pointConverter
                .ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();
            return new Point(point.X * multiplier, point.Y * multiplier);
        }

        private Rect CalculateRect()
        {
            var rect = (Rect)_rectConverter.ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();
            rect.Scale(multiplier, multiplier);
            return rect;
        }

        private GridLength CalculateGridLength()
        {
            var gridLength = (GridLength)_gridLengthConverter
                .ConvertFromInvariantString(_formattedMultiplierString);
            double multiplier = GetFinalLengthMultiplier();

            return new GridLength(
                gridLength.Value * multiplier,
                gridLength.GridUnitType);
        }

    }

    internal static class DipHelper
    {

        public static double GetDipMultiplier()
        {
            // Abuse the LengthConverter to get the value of 1dip (device independent pixel).
            // This is the converter which is also used in XAML, so if we convert "1.0",
            // we will get the "real" size of one pixel on the current device.
            return (double)new LengthConverter().ConvertFromString("1.0");
        }

    }

}

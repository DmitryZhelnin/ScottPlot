﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;

namespace ScottPlot.Plottable
{
    /// <summary>
    /// A function plot displays a curve using a function (Y as a function of X)
    /// </summary>
    public class FunctionPlot : IPlottable, IHasLine, IHasColor
    {
        /// <summary>
        /// The function to translate an X to a Y (or null if undefined)
        /// </summary>
        public Func<double, double?> Function;

        // customizations
        public bool IsVisible { get; set; } = true;
        public int XAxisIndex { get; set; } = 0;
        public int YAxisIndex { get; set; } = 0;
        public double LineWidth { get; set; } = 1;
        public LineStyle LineStyle { get; set; } = LineStyle.Solid;
        public string Label { get; set; }
        public Color Color { get; set; } = Color.Black;
        public Color LineColor { get; set; } = Color.Black;
        public double XMin { get; set; } = double.NegativeInfinity;
        public double XMax { get; set; } = double.PositiveInfinity;

        public AxisLimits GetAxisLimits() => AxisLimits.NoLimits;

        public FunctionPlot(Func<double, double?> function)
        {
            Function = function;
        }

        public int PointCount { get; private set; }

        public void Render(PlotDimensions dims, Bitmap bmp, bool lowQuality = false)
        {
            List<double> xList = new List<double>();
            List<double> yList = new List<double>();

            double xStart = XMin.IsFinite() ? XMin : dims.XMin;
            double xEnd = XMax.IsFinite() ? XMax : dims.XMax;
            double width = xEnd - xStart;

            PointCount = (int)(width * dims.PxPerUnitX) + 1;

            for (int columnIndex = 0; columnIndex < PointCount; columnIndex++)
            {
                double x = columnIndex * dims.UnitsPerPxX + xStart;
                double? y = Function(x);

                if (y is null)
                {
                    Debug.WriteLine($"Y({x}) failed because y was null");
                    continue;
                }

                if (double.IsNaN(y.Value) || double.IsInfinity(y.Value))
                {
                    Debug.WriteLine($"Y({x}) failed because y was not a real number");
                    continue;
                }

                xList.Add(x);
                yList.Add(y.Value);
            }

            // create a temporary scatter plot and use it for rendering
            double[] xs = xList.ToArray();
            double[] ys = yList.ToArray();
            var scatter = new ScatterPlot(xs, ys)
            {
                Color = Color,
                LineWidth = LineWidth,
                MarkerSize = 0,
                Label = Label,
                MarkerShape = MarkerShape.none,
                LineStyle = LineStyle
            };
            scatter.Render(dims, bmp, lowQuality);
        }

        public void ValidateData(bool deepValidation = false)
        {
            if (Function is null)
                throw new InvalidOperationException("function cannot be null");
        }

        public override string ToString()
        {
            string label = string.IsNullOrWhiteSpace(this.Label) ? "" : $" ({this.Label})";
            return $"PlottableFunction{label} displaying {PointCount} points";
        }

        public LegendItem[] GetLegendItems()
        {
            var singleLegendItem = new LegendItem(this)
            {
                label = Label,
                color = Color,
                lineStyle = LineStyle,
                lineWidth = LineWidth,
                markerShape = MarkerShape.none
            };
            return new LegendItem[] { singleLegendItem };
        }
    }
}

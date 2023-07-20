// Copyright QUANTOWER LLC. © 2017-2022. All rights reserved.

using System;
using System.Drawing;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace VolumeBreadthLine
{
    public class VolumeBreadthLine : Indicator
    {

        private HistoricalData uvolData;
        private HistoricalData dvolData;

        public VolumeBreadthLine() : base()
        {
            this.Name = "VolumeBreadthLine";
            this.Description = "My indicator's annotation";
            this.SeparateWindow = true;
            this.AddLineSeries("Green Ratio", Color.Lime, 1, LineStyle.Histogramm);
            this.AddLineSeries("Red Ratio", Color.Red, 1, LineStyle.Histogramm);
            this.AddLineLevel(2.0, "Upper Strength", Color.Lime, 1, LineStyle.Dash);
            this.AddLineLevel(0.0, "Midline", Color.WhiteSmoke, 1, LineStyle.Dash);
            this.AddLineLevel(-5.0, "Lower Strength", Color.Red, 1, LineStyle.Dash);
        }

        protected override void OnInit()
        {
            this.uvolData = Core.Instance.Symbols.FirstOrDefault(s => s.Name == "$UVOL" && s.Connection.Name == "dxFeed").GetHistory(Period.MIN15, Core.TimeUtils.DateTimeUtcNow.AddDays(-5));
            this.dvolData = Core.Instance.Symbols.FirstOrDefault(s => s.Name == "$DVOL" && s.Connection.Name == "dxFeed").GetHistory(Period.MIN15, Core.TimeUtils.DateTimeUtcNow.AddDays(-5));
        }

        protected override void OnUpdate(UpdateArgs args)
        {

            var time = this.Time();
            int offset = (int)this.uvolData.GetIndexByTime(time.Ticks);

            if (offset < 0)
                return;

            double bv = GetClose(this.uvolData, offset);
            double sv = GetClose(this.dvolData, offset);
            double ratio = (bv > sv) ? bv / sv : (sv / bv) * (-1.0);

            if (ratio > 0)
            {
                this.SetValue(ratio, 0);
                this.SetValue(0, 1);
            }
            else
            {
                this.SetValue(0, 0);
                this.SetValue(ratio, 1);
            }
        }

        private static double GetClose(HistoricalData hd, int offset)
        {
            try
            {
                var bar = hd[offset];
                return bar[PriceType.Close];
            }
            catch (Exception e)
            {
                Core.Loggers.Log("Exception: " + e);
                return 0.0;
            }
        }

        protected override void OnClear()
        {
            //this.uvolData?.Dispose();
            //this.dvolData?.Dispose();
        }

        public override void OnPaintChart(PaintChartEventArgs args)
        {
            base.OnPaintChart(args);

            if (this.CurrentChart == null)
                return;

            Graphics graphics = args.Graphics;
            Font font = new Font("Arial", 10, FontStyle.Bold);
            this.Digits = 2;

            double num = this.GetValue(0, 0);
            if (num == 0.0)
                num = this.GetValue(0, 1);

            graphics.DrawString("NYSE:   " + this.FormatPrice(num), font, num > 0.0 ? Brushes.Green : Brushes.Red, 50, 50);
        }
    }
}
